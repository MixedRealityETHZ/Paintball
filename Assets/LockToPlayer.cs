using UnityEngine;

public class LockToPlayer : MonoBehaviour
{
    public Transform camera;
    // Update is called once per frame
    void Update()
    {
        Vector3 direction = (this.camera.position - this.transform.position).normalized;
        this.transform.rotation = Quaternion.LookRotation(-direction);
    }
}
