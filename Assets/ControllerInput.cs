using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : MonoBehaviour {

    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    public GameObject myCube;
    public Transform controllerTransform;
    private GameObject currentCube;


    void Start() {
        this.magicLeapInputs = new MagicLeapInputs();
        this.magicLeapInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.magicLeapInputs);

        this.controllerActions.Bumper.performed += this.Bumper_performed;
        this.controllerActions.TouchpadPosition.performed += this.TouchpadPositionOnperformed;
    }

    private void Bumper_performed(InputAction.CallbackContext obj) {
        this.currentCube = Instantiate(this.myCube, this.controllerTransform.position, this.controllerTransform.rotation);
    }

    private void TouchpadPositionOnperformed(InputAction.CallbackContext obj) {
        var touchPosition = this.controllerActions.TouchpadPosition.ReadValue<Vector2>();
        var touchValue = Mathf.Clamp((touchPosition.y + 1) / 1.8f, 0, 1);

        this.currentCube.transform.localScale = new Vector3(touchValue, touchValue, touchValue);
    }
}
