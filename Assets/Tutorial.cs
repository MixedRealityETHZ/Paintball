using Unity.VisualScripting;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Buttons buttons;
    private TutorialSteps step = TutorialSteps.Start;
    private float time;
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public MenuManager menu;
    public GameObject[] buffer;
    public GameObject[] screens;

    private bool didSelectColor;

    void Start() 
    {
        // set up controller inputs
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch (this.step)
        {   
            case TutorialSteps.Start:
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ToolChanging);
                    this.buttons.Touchpad.SetActive(true);
                }
                break;
            case TutorialSteps.ToolChanging:
                if (this.controllerActions.TouchpadTouch.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ToolSelection);
                }
                break;
            case TutorialSteps.ToolSelection:
                if (!this.controllerActions.TouchpadTouch.IsPressed())
                {
                    if (this.menu.activeTool == null)
                    {
                        this.GoToStep(TutorialSteps.ToolChanging);
                        this.buttons.Touchpad.SetActive(true);
                    }
                    else
                    {
                        this.GoToTutorialForTool();
                    }
                }
                break;
            case TutorialSteps.PaintGun:
                this.ResetToolTutorialIfSwitched("p");
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ColorChanging);
                    this.buttons.Bumper.SetActive(true);
                }
                break;
            case TutorialSteps.SprayPaint:
                this.ResetToolTutorialIfSwitched("s");
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ColorChanging);
                    this.buttons.Bumper.SetActive(true);
                }
                break;
            case TutorialSteps.Grenade1:
                this.ResetToolTutorialIfSwitched("g");
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.Grenade2);
                }
                break;
            case TutorialSteps.Grenade2:
                this.ResetToolTutorialIfSwitched("g");
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ColorChanging);
                    this.buttons.Bumper.SetActive(true);
                }
                break;
            case TutorialSteps.Brush:
                this.ResetToolTutorialIfSwitched("b");
                if (this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ColorChanging);
                    this.buttons.Bumper.SetActive(true);
                }
                break;
            case TutorialSteps.ColorChanging:
                if (this.controllerActions.Bumper.IsPressed())
                {
                    this.GoToStep(TutorialSteps.ColorSelecting);
                    this.buttons.Touchpad.SetActive(true);
                }
                break;
            case TutorialSteps.ColorSelecting:
                if (this.controllerActions.TouchpadTouch.IsPressed())
                {
                    didSelectColor = true;
                }
                if (!this.controllerActions.Bumper.IsPressed())
                {
                    if (!this.didSelectColor)
                    {
                        this.GoToStep(TutorialSteps.ColorChanging);
                        this.buttons.Bumper.SetActive(true);
                    }
                    else
                    {
                        this.GoToStep(TutorialSteps.Ending);
                    }
                }
                break;
            case TutorialSteps.Ending:
                if (!this.controllerActions.Trigger.IsPressed())
                {
                    this.GoToStep(TutorialSteps.Closed);
                    this.gameObject.SetActive(false);
                }
                break;
        }
    }

    public void RestartTutorial()
    {
        this.gameObject.SetActive(true);
        this.GoToStep(TutorialSteps.Start);
    }

    private void ResetToolTutorialIfSwitched(string expectedToolTag)
    {
        if (this.menu.activeTool == null || this.menu.activeTool.tag != expectedToolTag)
        {
            this.GoToTutorialForTool();
        }
    }

    private void GoToTutorialForTool()
    {
        switch (this.menu.activeTool.tag)
        {
            case "p":
                this.GoToStep(TutorialSteps.PaintGun);
                this.buttons.Trigger.SetActive(true);
                break;
            case "s":
                this.GoToStep(TutorialSteps.SprayPaint);
                this.buttons.Trigger.SetActive(true);
                break;
            case "g":
                this.GoToStep(TutorialSteps.Grenade1);
                this.buttons.Trigger.SetActive(true);
                break;
            case "b":
                this.GoToStep(TutorialSteps.Brush);
                break;
        }
    }

    private void GoToStep(TutorialSteps nextStep)
    {
        this.DisableAllButtons();
        this.TrySetScreenActive((int)this.step, false);
        this.TrySetScreenActive((int)nextStep, true);
        this.step = nextStep;
    }

    private void DisableAllButtons()
    {
        this.buttons.Bumper.SetActive(false);
        this.buttons.Menu1.SetActive(false);
        this.buttons.Menu2.SetActive(false);
        this.buttons.Touchpad.SetActive(false);
        this.buttons.Trigger.SetActive(false);
    }

    private void TrySetScreenActive(int screenIndex, bool active)
    {
        if (0 <= screenIndex && screenIndex < this.screens.Length)
        {
            this.screens[screenIndex].gameObject.SetActive(active);
        }
    }

    private enum TutorialSteps : int
    {
        Start = 0,
        ToolChanging = 1,
        ToolSelection = 2,
        PaintGun = 3,
        SprayPaint = 4,
        Grenade1 = 5,
        Grenade2 = 6,
        Brush = 7,
        ColorChanging = 8,
        ColorSelecting = 9,
        Ending = 10,
        Closed = 11
    }
}
