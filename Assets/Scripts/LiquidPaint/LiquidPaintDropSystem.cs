using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

public class LiquidPaintDropSystem : MonoBehaviour
{
    private const int MaxOverlapSphereHitCount = 2;
    
    [Header("Paint")]
    [SerializeField] private float maxPaintCapacity = 200f;
    [SerializeField] private float maxPaintLifetime = 3f;
    [SerializeField] private float minPaintBlobDistance = 0.05f;
    [SerializeField] private float paintBlobRadius = 0.1f;

    [Header("Stickiness")]
    [SerializeField] private float airDrag = 0.05f;
    [SerializeField] private float stickyDrag = 1f;
    [SerializeField] private float stickyForce = 1f;
    [SerializeField] private float stickyRadius = 0.03f;
    
    
    [Header("Debug")]
    [SerializeField]private int numLiquidDropsAlive;
    [SerializeField]private int overlapSphereResultsSize;
    [SerializeField]private int closestPointResultsSize;
    [SerializeField]private int overlapSphereCommandsSize;
    [SerializeField]private int jobHandlesSize;

    private static LiquidPaintDropSystem instance;
    
    private readonly List<LiquidPaintDrop> allLiquidPaintDrops = new();
    private readonly Dictionary<int, MeshVertexTriangleData> colliderIdToMeshData = new();
    private readonly List<JobHandle> jobHandles = new();
    private readonly List<MeshVertexTriangleData> usedMeshData = new();

    private MeshingSubsystemComponent meshingSubsystemComponent;

    private List<Vector3> vertexCache = new();
    private List<ushort> indexCache = new();

    private NativeList<ColliderHit> overlapSphereResults;
    private NativeList<OverlapSphereCommand> overlapSphereCommands;
    private NativeList<float3> closestPointResults;
    
    public float MaxPaintCapacity => this.maxPaintCapacity;
    public float MaxPaintLifetime => this.maxPaintLifetime;
    public float MinPaintBlobDistance => this.minPaintBlobDistance;
    public float PaintBlobRadius => this.paintBlobRadius;

    private bool isInFixedUpdate;

    public static LiquidPaintDropSystem GetInstance()
    {
        return instance;
    }

    public void RegisterPaintDrop(LiquidPaintDrop paintDrop)
    {
#if UNITY_EDITOR
        if (this.allLiquidPaintDrops.Contains(paintDrop))
        {
            throw new InvalidOperationException("The liquid paint drop is already registered.");
        }
#endif

        this.allLiquidPaintDrops.Add(paintDrop);
        this.numLiquidDropsAlive = this.allLiquidPaintDrops.Count;
    }

    public void UnregisterPaintDrop(LiquidPaintDrop paintDrop)
    {
        this.allLiquidPaintDrops.Remove(paintDrop);
        this.numLiquidDropsAlive = this.allLiquidPaintDrops.Count;
    }

    private void Awake()
    {
        this.meshingSubsystemComponent = FindObjectOfType<MeshingSubsystemComponent>();
        
        if (this.meshingSubsystemComponent == null)
        {
            throw new NullReferenceException("No MeshingSubsystemComponent was found in the scene.");
        }

        if (instance != null && instance != this)
        {
            this.enabled = false;
            throw new InvalidOperationException("Another instance of the liquid paint drop system already exists.");
        }

        instance = this;

        this.meshingSubsystemComponent.meshAdded += this.OnMeshAdded;
        this.meshingSubsystemComponent.meshRemoved += this.OnMeshRemoved;
        this.meshingSubsystemComponent.meshUpdated += this.OnMeshUpdated;

        this.overlapSphereResults = new NativeList<ColliderHit>(Allocator.Persistent);
        this.closestPointResults = new NativeList<float3>(Allocator.Persistent);
        this.overlapSphereCommands = new NativeList<OverlapSphereCommand>(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        this.meshingSubsystemComponent.meshAdded -= this.OnMeshAdded;
        this.meshingSubsystemComponent.meshRemoved -= this.OnMeshRemoved;
        this.meshingSubsystemComponent.meshUpdated -= this.OnMeshUpdated;

        if (this.overlapSphereResults.IsCreated)
        {
            this.overlapSphereResults.Dispose();
        }

        if (this.closestPointResults.IsCreated)
        {
            this.closestPointResults.Dispose();
        }

        if (this.overlapSphereCommands.IsCreated)
        {
            this.overlapSphereCommands.Dispose();
        }

        foreach (var meshData in this.colliderIdToMeshData.Values)
        {
            meshData.Indices.Dispose();
            meshData.Vertices.Dispose();
        }

        this.colliderIdToMeshData.Clear();
    }

    private void FixedUpdate()
    {
        this.isInFixedUpdate = true;

        try
        {
            var numDrops = this.allLiquidPaintDrops.Count;

            this.overlapSphereResults.ResizeUninitialized(numDrops * MaxOverlapSphereHitCount);
            this.closestPointResults.ResizeUninitialized(numDrops * MaxOverlapSphereHitCount);

            this.FindOverlappingColliders();

            // Schedule closest point jobs for all concave mesh collider, then await completion of them all
            this.FindClosestPointsToColliders();

            // Calculate resulting force for liquid paint and write back to individual paint blobs
            for (var i = 0; i < numDrops; i++)
            {
                var startIndex = i * MaxOverlapSphereHitCount;
                var paintDrop = this.allLiquidPaintDrops[i];
                var worldPoint = (float3)paintDrop.transform.position;
                var totalForce = float3.zero;
                var radius = paintDrop.SphereCollider.radius;
                var relevantColliderCount = 0;

                for (var j = 0; j < MaxOverlapSphereHitCount; j++)
                {
                    var result = this.overlapSphereResults[startIndex + j];

                    if (result.instanceID == 0 || result.collider == null)
                    {
                        break;
                    }

                    var closestPoint =
                        (float3)result.collider.transform.TransformPoint(this.closestPointResults[startIndex + j]);

                    var dir = closestPoint - worldPoint;
                    var distance = math.length(dir);

                    if (distance == 0)
                    {
                        continue;
                    }

                    var shellDistance = Mathf.Max(0f, distance - radius);
                    totalForce += math.lerp(dir * this.stickyForce / distance, Vector3.zero,
                        shellDistance / this.stickyRadius);
                    relevantColliderCount++;
                }

                if (relevantColliderCount > 0)
                {
                    if (float.IsNaN(totalForce.x + totalForce.y + totalForce.z))
                    {
                        ErrorMessageUtility.ShowError("Calculated nan force for liquid paint sphere.");
                        continue;
                    }

                    paintDrop.Rigidbody.AddForce(totalForce);
                    paintDrop.Rigidbody.drag = this.stickyDrag;
                }
                else
                {
                    paintDrop.Rigidbody.drag = this.airDrag;
                }
            }

            this.overlapSphereResultsSize = this.overlapSphereResults.Length;
            this.closestPointResultsSize = this.closestPointResults.Length;
            this.overlapSphereCommandsSize = this.overlapSphereCommands.Length;
            this.jobHandlesSize = this.jobHandles.Count;
        }
        finally
        {
            this.isInFixedUpdate = false;
        }
    }

    private void FindOverlappingColliders()
    {
        var numDrops = this.allLiquidPaintDrops.Count;
        var queryParameters = new QueryParameters(PaintUtility.SurfaceLayer);

        this.overlapSphereCommands.ResizeUninitialized(numDrops);

        for (var i = 0; i < numDrops; i++)
        {
            var liquidDrop = this.allLiquidPaintDrops[i];
            var position = liquidDrop.transform.position;
            var radius = liquidDrop.SphereCollider.radius + this.stickyRadius;

            var overlapSphereCommand = new OverlapSphereCommand(position, radius, queryParameters);
            this.overlapSphereCommands[i] = overlapSphereCommand;
        }

        var jobHandle = OverlapSphereCommand.ScheduleBatch(this.overlapSphereCommands.AsArray(), this.overlapSphereResults.AsArray(), 1, MaxOverlapSphereHitCount);
        jobHandle.Complete();
    }
    
    private void FindClosestPointsToColliders()
    {
        var numDrops = this.allLiquidPaintDrops.Count;
        this.jobHandles.Clear();
        this.usedMeshData.Clear();
        
        for (var i = 0; i < numDrops; i++)
        {
            var startIndex = i * MaxOverlapSphereHitCount;
            var worldPoint = this.allLiquidPaintDrops[i].transform.position;

            for (var j = 0; j < MaxOverlapSphereHitCount; j++)
            {
                var result = this.overlapSphereResults[startIndex + j];
                var colliderId = result.instanceID;

                if (colliderId == 0)
                {
                    break;
                }
                
                var collider = result.collider;
                
                if (!this.colliderIdToMeshData.TryGetValue(result.instanceID, out var meshData))
                {
                    ErrorMessageUtility.ShowError("There's no mesh data for a collider.");
                    this.closestPointResults[startIndex + j] = worldPoint;
                }
                else
                {
                    var localPoint = collider.transform.InverseTransformPoint(worldPoint);

                    var job = new FindClosestPointOnConcaveMeshJob
                    {
                        VertexPositions = meshData.Vertices,
                        TriangleIndices = meshData.Indices,
                        ClosestPointContainer = this.closestPointResults.AsArray(),
                        Point = localPoint,
                        ResultIndex = startIndex + j
                    };

                    var handle = job.ScheduleByRef();
                    this.jobHandles.Add(handle);

                    meshData.CurrentUsers.Add(handle);
                    this.usedMeshData.Add(meshData);
                }
            }
        }

        JobHandle.ScheduleBatchedJobs();

        foreach (var job in this.jobHandles)
        {
            job.Complete();
        }

        foreach (var meshData in this.usedMeshData)
        {
            meshData.CurrentUsers.Clear();
        }

        this.usedMeshData.Clear();
    }

    private void OnMeshAdded(MeshId meshId)
    {
        if (this.isInFixedUpdate)
        {
            ErrorMessageUtility.ShowError("A mesh was added during fixed update.");
        }

        if (!this.meshingSubsystemComponent.meshIdToGameObjectMap.TryGetValue(meshId, out var gameObject))
        {
            ErrorMessageUtility.ShowErrorAndThrow(new InvalidOperationException("Failed to get game object for newly added mesh."));
        }

        var collider = gameObject.GetComponent<MeshCollider>();
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        var meshData = this.ExtractDataFromMesh(mesh);
        this.colliderIdToMeshData.Add(collider.GetInstanceID(), meshData);
    }

    private void OnMeshRemoved(MeshId meshId)
    {
        if (this.isInFixedUpdate)
        {
            ErrorMessageUtility.ShowError("A mesh was removed during fixed update.");
        }

        if (!this.meshingSubsystemComponent.meshIdToGameObjectMap.TryGetValue(meshId, out var gameObject))
        {
            ErrorMessageUtility.ShowErrorAndThrow(new InvalidOperationException("Failed to get game object for removed mesh."));
        }

        var collider = gameObject.GetComponent<MeshCollider>();
        var colliderId = collider.GetInstanceID();

        if (this.colliderIdToMeshData.TryGetValue(colliderId, out var meshData))
        {
            if (meshData.CurrentUsers.Count == 0)
            {
                meshData.Indices.Dispose();
                meshData.Vertices.Dispose();
            }
            else
            {
                var handle = default(JobHandle);

                foreach (var user in meshData.CurrentUsers)
                {
                    handle = JobHandle.CombineDependencies(handle, user);
                }

                meshData.Indices.Dispose(handle);
                meshData.Vertices.Dispose(handle);
            }
        }

        this.colliderIdToMeshData.Remove(colliderId);
    }

    private void OnMeshUpdated(MeshId meshId)
    {
        if (this.isInFixedUpdate)
        {
            ErrorMessageUtility.ShowError("A mesh was updated during fixed update.");
        }

        this.OnMeshRemoved(meshId);
        this.OnMeshAdded(meshId);
    }

    private MeshVertexTriangleData ExtractDataFromMesh(Mesh mesh)
    {
        this.vertexCache.Clear();
        this.indexCache.Clear();

        mesh.GetVertices(this.vertexCache);
        mesh.GetIndices(this.indexCache, 0);

        var nativeVertices = new NativeArray<Vector3>(this.vertexCache.Count, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);
        var nativeIndices = new NativeArray<ushort>(this.indexCache.Count, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        for (var i = 0; i < this.vertexCache.Count; i++)
        {
            nativeVertices[i] = this.vertexCache[i];
        }

        for (var i = 0; i < this.indexCache.Count; i++)
        {
            nativeIndices[i]= this.indexCache[i];
        }

        return new MeshVertexTriangleData(nativeVertices, nativeIndices);
    }
    
    private class MeshVertexTriangleData
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<ushort> Indices;
        public readonly List<JobHandle> CurrentUsers = new();

        public MeshVertexTriangleData(in NativeArray<Vector3> vertices, in NativeArray<ushort> indices)
        {
            this.Vertices = vertices;
            this.Indices = indices;
        }
    }

    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
    private unsafe struct FindClosestPointOnConcaveMeshJob : IJob
    {
        [ReadOnly, NoAlias]
        public NativeSlice<Vector3> VertexPositions;
        
        [ReadOnly, NoAlias]
        public NativeArray<ushort> TriangleIndices;
        
        [WriteOnly, NoAlias, NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> ClosestPointContainer;

        public float3 Point;

        public int ResultIndex;

        /// <inheritdoc />
        public void Execute()
        {
            this.TestIfBurstCompiled();

            var closestPoint = float3.zero;
            var closestSqrDistance = 1e+31f;
            
            for (var i = 0; i < this.TriangleIndices.Length; i += 3)
            {
                var vertexIndex0 = this.TriangleIndices[i + 0];
                var vertexIndex1 = this.TriangleIndices[i + 1];
                var vertexIndex2 = this.TriangleIndices[i + 2];

                var p0 = this.VertexPositions[vertexIndex0];
                var p1 = this.VertexPositions[vertexIndex1];
                var p2 = this.VertexPositions[vertexIndex2];

                var closestInAabb = this.ClosestPointInTriangleAabb(p0, p1, p2, this.Point);
                var closestSqrDistInAabb = math.distancesq(this.Point, closestInAabb);
                if (closestSqrDistInAabb > closestSqrDistance)
                {
                    continue;
                }

                var closestPointToTri = this.ClosestPointTriangle(p0, p1, p2, this.Point);

                var sqrDist = math.distancesq(this.Point, closestPointToTri);

                if (sqrDist < closestSqrDistance)
                {
                    closestPoint = closestPointToTri;
                    closestSqrDistance = sqrDist;
                }
            }

            this.ClosestPointContainer[this.ResultIndex] = closestPoint;
        }

        [BurstDiscard]
        private void TestIfBurstCompiled()
        {
            ErrorMessageUtility.ShowError("The LiquidPaintJob is not burst compiled.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float3 ClosestPointTriangle(float3 a, float3 b, float3 c, float3 p)
        {
            float3 ab = b - a;
            float3 ac = c - a;
            float3 ap = p - a;

            float d1 = math.dot(ab, ap);
            float d2 = math.dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) return a; //#1

            float3 bp = p - b;
            float d3 = math.dot(ab, bp);
            float d4 = math.dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) return b; //#2

            float3 cp = p - c;
            float d5 = math.dot(ab, cp);
            float d6 = math.dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) return c; //#3

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
            {
                float v = d1 / (d1 - d3);
                return a + v * ab; //#4
            }

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
            {
                float v = d2 / (d2 - d6);
                return a + v * ac; //#5
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
            {
                float v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + v * (c - b); //#6
            }

            float denom = 1f / (va + vb + vc);
            float vd = vb * denom;
            float wd = vc * denom;
            return a + vd * ab + wd * ac; //#0
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float3 ClosestPointInTriangleAabb(float3 a, float3 b, float3 c, float3 p)
        {
            var min = math.min(math.min(a, b), c);
            var max = math.max(math.max(a, b), c);
            var closestInAabb = math.clamp(p, min, max);
            return closestInAabb;
        }
    }
}