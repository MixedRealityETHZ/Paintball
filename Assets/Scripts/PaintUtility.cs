using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintUtility : MonoBehaviour
{
    private static PaintUtility instance;

    private static readonly int MaskId = Shader.PropertyToID("_Mask");
    private static readonly int VertexBufferId = Shader.PropertyToID("_VertexBuffer");
    
    [SerializeField] private Texture2D paintMask;
    [SerializeField] private LayerMask surfaceLayer;
    [SerializeField] private LayerMask paintLayer;

    [Header("Debug")]
    [SerializeField] private int paintBufferSize;
    [SerializeField] private int paintBufferCapacity;

    private ComputeBuffer paintDropsBuffer;
    private ComputeBuffer argBuffer;
    private Shader pointCloudShader;
    private Material pointCloudStereoMaterial;
    private Material pointCloudMonoMaterial;

    //private readonly List<PaintDrop> paintDrops = new();
    private readonly List<PaintVertex> paintVertices = new();
    private int lastAddedIndex = 0;

    public static LayerMask SurfaceLayer => instance.surfaceLayer;
    public static LayerMask PaintLayer => instance.paintLayer;

    public static void AddPaint(in Vector3 position, in Vector3 normal, in Color color, float radius)
    {
        if (instance == null)
        {
            ErrorMessageUtility.ShowError("Paint utility isn't present in scene.");
            return;
        }

        var rotation = Quaternion.LookRotation(normal, Vector3.up);
        var left = rotation * (new Vector3(radius, 0, 0));
        var up = rotation * (new Vector3(0, radius, 0));

        var p0 = position - left - up;
        var p1 = position - left + up;
        var p2 = position + left - up;
        var p3 = position + left + up;

        var v0 = new PaintVertex(p0, color, new Vector2(0, 0));
        var v1 = new PaintVertex(p1, color, new Vector2(0, 1));
        var v2 = new PaintVertex(p2, color, new Vector2(1, 0)); 
        var v3 = new PaintVertex(p3, color, new Vector2(1, 1));

        //instance.paintDrops.Add(new PaintDrop(position + normal * 0.003f, in normal, in color, radius));
        instance.paintVertices.Add(v0);
        instance.paintVertices.Add(v2);
        instance.paintVertices.Add(v1);
        instance.paintVertices.Add(v1);
        instance.paintVertices.Add(v2);
        instance.paintVertices.Add(v3);

        instance.paintBufferSize = instance.paintVertices.Count;
    }

    public static bool IsOnSurfaceLayer(GameObject go)
    {
        var mask = SurfaceLayer.value;
        return (mask & (1 << go.layer)) != 0;
    }

    public static bool IsOnPaintLayer(GameObject go)
    {
        var mask = PaintLayer.value;
        return (mask & (1 << go.layer)) != 0;
    }
    
    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            ErrorMessageUtility.ShowError("Another instance of the paint utility already exists.");
            throw new InvalidOperationException("Another instance of the paint utility already exists.");
        }

        if (this.paintMask == null)
        {
            ErrorMessageUtility.ShowError("Paint blob mask was not provided.");
            Debug.LogError("Paint blob mask was not provided.");
        }

        this.pointCloudShader = Shader.Find("Unlit/PointCloudShader");

        if (this.pointCloudShader == null)
        {
            ErrorMessageUtility.ShowError("Couldn't find Unlit/PointCloudShader.shader");
            Debug.LogError("Couldn't find Unlit/PointCloudShader.shader");
        }

        this.pointCloudStereoMaterial = new Material(this.pointCloudShader); 
        this.pointCloudStereoMaterial.EnableKeyword("STEREO_INSTANCING_ON");
        this.pointCloudMonoMaterial = new Material(this.pointCloudShader); 
        this.pointCloudMonoMaterial.DisableKeyword("STEREO_INSTANCING_ON");
        this.paintDropsBuffer = new ComputeBuffer(6*1024, UnsafeUtility.SizeOf<PaintVertex>());
        RenderPipelineManager.endCameraRendering += this.OnEndCameraRendering;

        this.paintBufferCapacity = this.paintDropsBuffer.count;
        
        instance = this;
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (this.paintDropsBuffer != null)
        {
            this.paintDropsBuffer.Dispose();
        }

        if (this.argBuffer != null)
        {
            this.argBuffer.Dispose();
        }

        if (this.pointCloudStereoMaterial != null)
        {
            Destroy(this.pointCloudStereoMaterial);
        }

        RenderPipelineManager.endCameraRendering -= this.OnEndCameraRendering;
    }

    public void ResetPaint()
    {
        this.paintVertices.Clear();
        this.lastAddedIndex = 0;
    }
    
    private void OnEndCameraRendering(ScriptableRenderContext scriptableRenderContext, Camera camera)
    {
        if (this.paintVertices.Count == 0)
        {
            return;
        }

        if (this.argBuffer == null)
        {
            this.argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        }

        //var material = camera.stereoEnabled ? this.pointCloudStereoMaterial : this.pointCloudMonoMaterial;
        
        this.PreparePaintDropsBuffer(); 

        var args = new int[] { this.paintVertices.Count, 1, 0, 0 };
        this.argBuffer.SetData(args);

        this.pointCloudMonoMaterial.SetTexture(MaskId, this.paintMask);
        this.pointCloudMonoMaterial.SetBuffer(VertexBufferId, this.paintDropsBuffer);
        this.pointCloudMonoMaterial.SetPass(0);
        
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, this.argBuffer);
    }

    private void PreparePaintDropsBuffer() 
    {
        if (this.paintDropsBuffer.count < this.paintVertices.Count)
        {
            var newCount = this.paintDropsBuffer.count * 2;
            while (newCount < this.paintVertices.Count)
            {
                newCount *= 2;
            }
            
            this.paintDropsBuffer.Dispose();
            this.paintDropsBuffer = new ComputeBuffer(newCount, UnsafeUtility.SizeOf<PaintVertex>());
            this.lastAddedIndex = 0;
            this.paintBufferCapacity = this.paintDropsBuffer.count;
        }

        this.paintDropsBuffer.SetData(this.paintVertices, this.lastAddedIndex, this.lastAddedIndex, this.paintVertices.Count - this.lastAddedIndex);
        this.lastAddedIndex = this.paintVertices.Count;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintDrop
    {
        public float4 Position;
        public quaternion Rotation;
        public float4 ColorAndRadius;

        public PaintDrop(in Vector3 position, in Vector3 normal, in Color color, float radius)
        {
            this.Position = new float4(position.x, position.y, position.z, 1f);
            this.Rotation = Quaternion.LookRotation(normal, Vector3.up);
            this.ColorAndRadius = new float4(color.r, color.g, color.b, radius);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintVertex
    {
        public float4 Vertex;
        public float4 Color;
        public float2 UV;

        public PaintVertex(in Vector3 position, in Color color, in Vector2 uv)
        {
            this.Vertex = new float4(position, 1);
            this.Color = new float4(color.r, color.g, color.b, 1f);
            this.UV = uv;
        }
    }
}
