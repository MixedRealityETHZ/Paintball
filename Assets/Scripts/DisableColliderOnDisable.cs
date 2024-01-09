using UnityEngine;

public class DisableColliderOnDisable : MonoBehaviour
{
    void OnDisable()
    {
        var collider = this.GetComponent<Collider>();

        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}
