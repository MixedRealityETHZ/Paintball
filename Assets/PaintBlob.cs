using UnityEngine;

public class PaintBlob : MonoBehaviour
{   
    private Vector3 direction;
    private Vector3 position;
    public Rigidbody particle;
    public PaintBlob pb;
    public float mass = 0;
    // Start is called before the first frame update
    void Start()
    {
        this.position = this.GetComponent<Transform>().position;
        this.direction = -this.GetComponent<Transform>().forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Physics.Raycast(this.position, this.direction, 0.05f, LayerMask.GetMask(new string[] { "Environment" })) && this.transform.localScale.magnitude > 0.001f)
        {
            Rigidbody newParticle = Instantiate(this.particle, this.transform);
            newParticle.mass = this.transform.localScale.magnitude;
            newParticle.transform.localScale = this.transform.localScale * 0.005f;
            newParticle.GetComponent<MeshRenderer>().material.color = this.GetComponent<MeshRenderer>().material.color;
            newParticle.GetComponent<stopOnCollision>().paintColor = this.GetComponent<MeshRenderer>().material.color;
            Destroy(this.gameObject);
        }
        /*if (Physics.SphereCast(position, 0.01f, -direction, out RaycastHit ray, LayerMask.GetMask(new string[] { "PaintBlob" })) && this.transform.localScale.magnitude > 0.001f)
        {
            if (ray.collider.gameObject.transform.localScale.magnitude < this.transform.localScale.magnitude) 
            { 
                print("hit!");
                mass = this.transform.localScale.magnitude;
                PaintBlob newb = Instantiate(pb, this.transform.position, this.transform.rotation);
                newb.transform.localScale = newb.transform.localScale + (ray.collider.gameObject.transform.localScale * 0.7f);
                newb.GetComponent<MeshRenderer>().material.color = GetComponent<MeshRenderer>().material.color;
                newb.mass = this.mass + ray.collider.gameObject.GetComponent<PaintBlob>().mass;
                print(newb.mass);
                Destroy(ray.collider.gameObject);
                Destroy(this.gameObject);
            }
        }*/
    }
}
