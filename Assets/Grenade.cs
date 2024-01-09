using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private GameObject paintPrefab;
    [SerializeField] private bool released = false;
    [SerializeField] private float triggerTime = 3f;
    [SerializeField] private float spawnRadius = 0.03f;
    [SerializeField] private int numberOfSplatters = 200;

    private MeshRenderer renderer;

    public Color Color
    {
        get => this.color;
        set
        {
            this.color = value;
            this.renderer.material.color = this.color;
        }
    }
    
    public void Release()
    {
        this.released = true;
    }

    void Awake()
    {
        this.renderer = this.GetComponentInChildren<MeshRenderer>();
    }

    void Update()
    {
        if (!released)
        {
            return;
        }

        if (this.transform.position.y < -10)
        {
            Destroy(this.gameObject);
            return;
        }

        this.triggerTime -= Time.deltaTime;

        if (this.triggerTime > 0)
        {
            return;
        }

        for (int i = 0; i < this.numberOfSplatters; i++)
        {
            var dir = Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y);

            var spawnedObject = Instantiate(this.paintPrefab, this.transform.position + dir * this.spawnRadius, Quaternion.identity);

            var spawnedRigidbody = spawnedObject.GetComponent<Rigidbody>();
            var spawnedLiquidPaint = spawnedObject.GetComponent<LiquidPaintDrop>();

            spawnedRigidbody.AddForce(dir * 1000);
            spawnedLiquidPaint.Color = this.color;
        }
        Destroy(this.gameObject);
    }
}
