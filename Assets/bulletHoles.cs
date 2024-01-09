using UnityEngine;
using UnityEngine.InputSystem;

public class bulletHoles : MonoBehaviour
{
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public Transform controllerTransform;
    public GameObject myCube;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;

    void Start()

    {
        this.magicLeapInputs = new MagicLeapInputs();
        this.magicLeapInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.magicLeapInputs);

        this.controllerActions.Bumper.performed += this.Bumper_performed;
    }

    void Update()
    {
        //rayOrigin = controllerTransform.position;
        //rayDirection = controllerTransform.rotation * Vector3.forward;   
    }

    private void Bumper_performed(InputAction.CallbackContext obj)
    {
        //GameObject currentCube = Instantiate(myCube, .position, controllerTransform.rotation);
        this.rayOrigin = this.controllerTransform.position;
        this.rayDirection = this.controllerTransform.rotation * Vector3.forward;

        Debug.Log(this.rayOrigin);
        // Cast a ray from the controller's position along its forward direction
        RaycastHit hit;
        if (Physics.Raycast(this.rayOrigin, this.rayDirection, out hit, 10.0f))
        {
            // You can now access information about the hit point (hit.point) and interact with objects.
            // For example, you might change the color of a hit object:
            MeshRenderer meshRenderer = hit.collider.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.red;
            }
        }
    }

    void OnDestroy()
    {
    }
}
