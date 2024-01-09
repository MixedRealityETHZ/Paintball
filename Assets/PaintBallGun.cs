using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PaintBallGun : MonoBehaviour
{
    public Transform SpawnLocation;
    public GameObject ColorIndicator;
    public ColorPickerTriangle ColorMenu;
    public int strengthModifier = 50;
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public XRRayInteractor ri;
    public GameObject ObjectToSpawn;
    // Start is called before the first frame update

    private Material colorIndicatorMaterial;

    void Awake()
    {
        // set up controller inputs
        this.mlInputs = new MagicLeapInputs();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);

        var meshRenderer = this.ColorIndicator.GetComponent<MeshRenderer>();
        this.colorIndicatorMaterial = meshRenderer.material;
    }

    void OnEnable()
    {
        this.mlInputs.Enable();
    }

    private void OnDisable()
    {
        this.mlInputs.Disable();
    }

    private void HandleOnTrigger()
    {
        this.ri.GetLineOriginAndDirection(out var origin, out var direction);

        var strength = (int)(this.strengthModifier / 2) + (this.strengthModifier / 2);
        var spawnedObject = Instantiate(this.ObjectToSpawn, this.SpawnLocation.position, this.SpawnLocation.transform.rotation);

        var spawnedRigidbody = spawnedObject.GetComponent<Rigidbody>();
        var spawnedLiquidPaint = spawnedObject.GetComponent<LiquidPaintDrop>();

        spawnedRigidbody.AddForce(direction.normalized * strength);
        spawnedLiquidPaint.Color = this.ColorMenu.TheColor;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.colorIndicatorMaterial.color != this.ColorMenu.TheColor)
        {
            this.colorIndicatorMaterial.color = this.ColorMenu.TheColor;
        }
        if (this.controllerActions.Trigger.IsPressed())
        {
            this.HandleOnTrigger();
        }
    }
}
