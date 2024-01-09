using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SprayPaint : MonoBehaviour
{
    public Transform SpawnLocation;
    public GameObject ColorIndicator;
    public ColorPickerTriangle ColorMenu;
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public XRRayInteractor ri;

    public int maxNumSplatters = 50;
    public float minSplatterSize = 0.001f;
    public float maxSplatterSize = 0.005f;
    
    void Start()
    {
        // set up controller inputs
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);

        // subscribe to bumper event
        //controllerActions.Trigger.performed += HandleOnTrigger;

        //animation = model.GetComponent<Animation>();
    }

    public void HandleOnTrigger()
    {
        var origin = this.SpawnLocation.position;
        var direction = this.SpawnLocation.forward;
        var right = this.SpawnLocation.right;
        var up = this.SpawnLocation.up;
        
        var power = (this.controllerActions.Trigger.ReadValue<float>() - 0.48f) * 2f;
        var numSplatter = Mathf.RoundToInt(this.maxNumSplatters * power);
        var color = this.ColorMenu.TheColor;

        for (int i = 0; i < numSplatter; i++)
        {
            var temp = Random.insideUnitCircle.x;   // Do a cos weighted random sampling
            var p = Random.insideUnitCircle * 10f * power * temp;

            if (Physics.Raycast(origin, Quaternion.AngleAxis(p.x, right) * Quaternion.AngleAxis(p.y, up) * direction, out RaycastHit hit))
            {
                var position = hit.point + hit.normal * 0.003f;
                var normal = hit.normal;
                var splatterSize = Random.Range(this.minSplatterSize, this.maxSplatterSize);
                PaintUtility.AddPaint(in position, in normal, in color, splatterSize);
            }
        }

        //animation.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.ColorIndicator.GetComponent<MeshRenderer>().material.color != this.ColorMenu.TheColor)
        {
            this.ColorIndicator.GetComponent<MeshRenderer>().material.color = this.ColorMenu.TheColor;
        }
        if (this.controllerActions.Trigger.IsPressed())
        {
            this.HandleOnTrigger();
        }
    }
}
