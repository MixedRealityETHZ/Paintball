using UnityEngine;

public class stopOnCollision : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    private int counter;
    private Vector3 normal;
    public Mesh mesh;

    public Color paintColor;
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;
    public PaintBlob paintBlobPrefab;
    public Material[] variants;

    private void Start() {
        this.rb = this.GetComponent<Rigidbody>();
        this.counter = 0;
    }

    private void OnCollisionEnter(Collision collision) {
        PaintBlob paintBlob = Instantiate(this.paintBlobPrefab, collision.contacts[0].point + collision.contacts[0].normal * 0.003f, Quaternion.LookRotation(collision.contacts[0].normal));
        paintBlob.GetComponent<MeshRenderer>().material = this.variants[Random.Range(0, this.variants.Length)];
        paintBlob.GetComponent<MeshRenderer>().material.color = this.paintColor;
        paintBlob.mass = paintBlob.transform.localScale.magnitude;

        if(paintBlob.GetComponent<Transform>().localScale.magnitude > 0.001f){
            for(int i = 0; i <= Random.Range(0, 10); i++)
            {
                var x = Random.Range(-1f, 1f);
                var y = Random.Range(-Mathf.Sqrt(1 - (x * x)), Mathf.Sqrt(1 - (x * x)));
                x *= 0.05f;
                y *= 0.05f;
                var z = (-collision.contacts[0].normal.x * x - collision.contacts[0].normal.y * y) / collision.contacts[0].normal.z;
                PaintBlob splatter = Instantiate(this.paintBlobPrefab, collision.contacts[0].point + collision.contacts[0].normal * 0.003f + new Vector3(x, y, z), Quaternion.LookRotation(collision.contacts[0].normal));
                splatter.GetComponent<MeshRenderer>().material = this.variants[Random.Range(0, this.variants.Length)];
                splatter.GetComponent<MeshRenderer>().material.color = this.paintColor;
                splatter.GetComponent<Transform>().localScale = paintBlob.GetComponent<Transform>().localScale * Random.Range(0.01f, 0.5f - Mathf.Abs(x * 4) - Mathf.Abs(y * 4));
                splatter.mass = splatter.transform.localScale.magnitude;
            }
        }
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update() {
        if (this.counter < 2) {
            this.velocity = this.rb.velocity;
            this.counter++;
        }
        if (this.rb.velocity != this.velocity && !this.rb.isKinematic) {
            this.GetComponent<MeshFilter>().mesh = this.mesh;
            this.rb.isKinematic = true;
            this.rb.transform.rotation *= Quaternion.FromToRotation(this.rb.transform.up, this.normal);
            this.rb.transform.localScale = new Vector3(this.rb.transform.localScale.x * 100, this.rb.transform.localScale.y * 10, this.rb.transform.localScale.z * 100);
        }
    }
}
