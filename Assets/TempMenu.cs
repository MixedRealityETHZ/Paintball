using UnityEngine;
using UnityEngine.InputSystem;
using static MagicLeapInputs;

public class TempMenu : MonoBehaviour
{
    public Tool[] tools;
    private int i = 0;
    private MagicLeapInputs mlInputs;
    private ControllerActions controllerActions;
    private Color[] colors;
    private int j = 0;

    void Start()
    {
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new ControllerActions(this.mlInputs);
        this.controllerActions.Bumper.performed += this.HandleOnBumper;
        this.controllerActions.TouchpadTouch.performed += this.HandleOnTouchpadClick;
        this.colors = new Color[] { Color.black, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow };
        for(int k = 0; k < this.tools.Length; k++)
        {
            this.tools[k].color = this.colors[this.j];
        }
    }

    void HandleOnBumper(InputAction.CallbackContext obj)
    {
        this.j = (this.j + 1) % this.colors.Length;
        this.tools[this.i].color = this.colors[this.j];
    }

    void HandleOnTouchpadClick(InputAction.CallbackContext obj)
    {
        this.tools[this.i].gameObject.SetActive(false);
        this.i = (this.i + 1) % this.tools.Length;
        this.tools[this.i].GetComponent<Tool>().color = this.colors[this.j];
        this.tools[this.i].gameObject.SetActive(true);
    }

}
