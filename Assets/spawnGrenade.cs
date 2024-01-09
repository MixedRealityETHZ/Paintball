using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class spawnGrenade : MonoBehaviour
{
    // Start is called before the first frame update
    public Grenade grenade;
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public XRRayInteractor ri;
    public Color color;
    private Grenade spawnedObject;
    private Vector3 velocity;
    private Vector3 lastPosition;
    public ColorPickerTriangle ColorMenu;
    [FormerlySerializedAs("forceMultiplier")]
    public float velocityOnReleaseMultiplier = 1f;

    void Awake()
    {
        this.mlInputs = new MagicLeapInputs();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
    }

    void OnEnable()
    {
        this.mlInputs.Enable();
        this.controllerActions.Trigger.started += this.HandleOnTrigger;
        this.controllerActions.Trigger.canceled += this.HandleOnRelease;
    }

    private void OnDisable()
    {
        this.mlInputs.Disable();
        this.controllerActions.Trigger.started -= this.HandleOnTrigger;
        this.controllerActions.Trigger.canceled -= this.HandleOnRelease;
    }

    private void HandleOnTrigger(InputAction.CallbackContext obj)
    {
        if (this.spawnedObject != null)
        {
            return;
        }

        this.spawnedObject = Instantiate(this.grenade, this.transform.position, this.transform.rotation);
        this.spawnedObject.Color = this.color;

        var spawnedRigidBody = this.spawnedObject.GetComponent<Rigidbody>();
        spawnedRigidBody.isKinematic = true;

        this.lastPosition = this.transform.position;
    }

    private void HandleOnRelease(InputAction.CallbackContext obj)
    {
        if (this.spawnedObject == null)
        {
            return;
        }

        var spawnedRigidBody = this.spawnedObject.GetComponent<Rigidbody>();
        spawnedRigidBody.position = this.spawnedObject.transform.position;
        spawnedRigidBody.isKinematic = false;
        spawnedRigidBody.velocity = this.velocity * this.velocityOnReleaseMultiplier;

        this.spawnedObject.Release();
        this.spawnedObject = null;
    }
    
    void Update()
    {
        if (this.color != this.ColorMenu.TheColor)
        {
            this.color = this.ColorMenu.TheColor;
        }

        if (this.spawnedObject != null)
        {
            this.spawnedObject.Color = this.color;
            this.spawnedObject.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            
            this.velocity = Vector3.Lerp(this.velocity, (this.transform.position - this.lastPosition) / Time.deltaTime, 1/2f);
            this.lastPosition = this.transform.position;
        }
    }
}
