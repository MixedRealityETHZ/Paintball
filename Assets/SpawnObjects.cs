using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnObjects : MonoBehaviour
{
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public XRRayInteractor ri;
    public GameObject spawnLocation;
    //public GameObject model;
    public Rigidbody objectToSpawn;
    //private Animation animation;
    private int strengthModifier;
    

    private Color [] colors;
    private int i;
    public Transform spawnPosition;

    void Start()
    {
        this.strengthModifier = 50;
        // set up controller inputs
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);

        // Setup Colors
        this.colors = new Color[] { Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow };
        this.i = 0;

        // subscribe to bumper event
        this.controllerActions.Bumper.performed += this.HandleOnBumper;
        this.controllerActions.Trigger.performed += this.HandleOnTrigger;

        //animation = model.GetComponent<Animation>();
    }

    private void HandleOnBumper(InputAction.CallbackContext obj) {
        this.i = (this.i + 1) % this.colors.Length;
    }

    private void HandleOnTrigger(InputAction.CallbackContext obj)
    {
        Vector3 origin;
        Vector3 direction;
        this.ri.GetLineOriginAndDirection(out origin, out direction);
        int strength = (int)(this.strengthModifier / 2 * obj.ReadValue<float>()) + (this.strengthModifier / 2);

        
        Rigidbody spawnedObject = Instantiate(this.objectToSpawn, this.spawnLocation.transform.position, this.spawnLocation.transform.rotation);
        var liquidPaintDrop = spawnedObject.GetComponent<LiquidPaintDrop>();

        liquidPaintDrop.Color = this.colors[this.i++ % this.colors.Length];
        spawnedObject.AddForce(direction.normalized * strength * 10);
    }

    /*void OnDestroy() {
        controllerActions.Bumper.performed -= HandleOnBumper;
        controllerActions.Trigger.performed -= HandleOnTrigger;
        mlInputs.Dispose();
    }*/
}
