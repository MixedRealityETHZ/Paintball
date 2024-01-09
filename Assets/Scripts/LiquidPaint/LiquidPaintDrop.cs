using System;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class LiquidPaintDrop : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private bool isImmortal;
    private LiquidPaintDropSystem liquidPaintDropSystem;
    private SphereCollider myCollider;
    private Rigidbody myRigidbody;
    private MaterialPropertyBlock materialPropertyBlock;

    private Vector3 lastPaintPosition;
    private float paintCapacity;
    private float lifeTime;
    private Color color = Color.white;

    public SphereCollider SphereCollider => this.myCollider;

    public Rigidbody Rigidbody => this.myRigidbody;

    public Color Color
    {
        get => this.color;
        set
        {
            this.color = value;
            this.UpdateColorInRenderer();
        }
    }
    
    private static readonly Collider[] NearbyColliders = new Collider[16];

    void Awake()
    {
        this.myCollider = this.GetComponent<SphereCollider>();
        this.myRigidbody = this.GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        this.liquidPaintDropSystem = LiquidPaintDropSystem.GetInstance();
        if (this.liquidPaintDropSystem == null)
        {
            this.enabled = false;
            throw new InvalidOperationException("There is no LiquidPaintDropSystem present in the scene.");
        }

        this.liquidPaintDropSystem.RegisterPaintDrop(this);
        this.paintCapacity = this.liquidPaintDropSystem.MaxPaintCapacity;
        this.lifeTime = this.liquidPaintDropSystem.MaxPaintLifetime;
        this.UpdateColorInRenderer();
    }

    void OnDisable()
    {
        this.liquidPaintDropSystem.UnregisterPaintDrop(this);
    }

    private void Update()
    {
        if (this.isImmortal)
        {
            return;
        }

        this.lifeTime -= Time.deltaTime;

        if (this.lifeTime <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!PaintUtility.IsOnSurfaceLayer(other.gameObject))
        {
            return;
        }

        this.Rigidbody.velocity *= 0.1f;
    }

    private void OnCollisionStay(Collision collision)
    {
        try
        {
            if (collision.contactCount == 0 || !PaintUtility.IsOnSurfaceLayer(collision.gameObject))
            {
                return;
            }

            var myPosition = this.transform.position;
            var sqDistToLastPaintPosition = (myPosition - this.lastPaintPosition).sqrMagnitude;
            var minDist = this.liquidPaintDropSystem.MinPaintBlobDistance;

            if (sqDistToLastPaintPosition < minDist * minDist)
            {
                return;
            }

            this.lastPaintPosition = myPosition;

            if (collision.contactCount == 1)
            {
                var contact = collision.GetContact(0);
                this.AddPaint(contact.point, contact.normal);
            }

            var averageContactPoint = Vector3.zero;
            var averageContactNormal = Vector3.zero;

            for (var i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                averageContactPoint += contact.point;
                averageContactNormal += contact.normal;
            }
        
            var invContactCount = 1f / collision.contactCount;
            averageContactPoint *= invContactCount;
            averageContactNormal.Normalize();

            this.AddPaint(in averageContactPoint, in averageContactNormal);
        }
        catch (Exception e)
        {
            ErrorMessageUtility.ShowError(e.Message);
        }
    }

    private void OnValidate()
    {
        this.UpdateColorInRenderer();
    }

    private void UpdateColorInRenderer()
    {
        var meshRenderer = this.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
        {
            return;
        }

        if (this.materialPropertyBlock == null)
        {
            this.materialPropertyBlock = new MaterialPropertyBlock();
        }

        this.materialPropertyBlock.SetColor(BaseColorId, this.color);
        meshRenderer.SetPropertyBlock(this.materialPropertyBlock);
    }

    private void AddPaint(in Vector3 position, in Vector3 normal)
    {
        PaintUtility.AddPaint(in position, in normal, in this.color, this.liquidPaintDropSystem.PaintBlobRadius);

        if (this.isImmortal)
        {
            return;
        }

        this.paintCapacity--;

        if (this.paintCapacity <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
