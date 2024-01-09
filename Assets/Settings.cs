using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Settings : MonoBehaviour
{
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    private bool menu = false;
    public SUI sui;

    // Start is called before the first frame update
    void Start()
    {
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
        this.controllerActions.Menu.performed += this.HandleOnMenu;
    }

    void HandleOnMenu(InputAction.CallbackContext obj)
    {
        if(this.menu)
        {
            this.menu = false;
            this.sui.gameObject.SetActive(false);
            this.GetComponent<XRInteractorLineVisual>().enabled = false;
        }
        else
        {
            this.sui.transform.position = this.transform.position;
            this.sui.transform.rotation = this.transform.rotation;
            this.sui.gameObject.SetActive(true);
            this.GetComponent<XRInteractorLineVisual>().enabled = true;
            this.menu = true;
        }
    }
    
}
