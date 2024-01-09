using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    public GameObject menu;
    public GameObject colormenu;
    public Transform controllerTransform;
    public UnityEngine.UI.Button upButton;
    public UnityEngine.UI.Button downButton;
    public UnityEngine.UI.Button leftButton;
    public UnityEngine.UI.Button rightButton;
    private Color chosencolor=Color.red;
    public GameObject[] tools;
    public GameObject activeTool;
    private Vector2 touchpadposition;
    // Start is called before the first frame update
    void Start()
    {
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);

        foreach (var tool in this.tools)
        {
            tool.SetActive(false);
        }

        if (this.activeTool != null)
        {
            this.activeTool.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ColorPickerTriangle picker= this.colormenu.GetComponent<ColorPickerTriangle>();

        if (this.controllerActions.TouchpadTouch.IsPressed()) 
        {
            this.touchpadposition = this.controllerActions.TouchpadPosition.ReadValue<Vector2>(); 
        }
        
        if(this.controllerActions.Bumper.IsPressed())
        {
            this.colormenu.SetActive(true);
            this.chosencolor = picker.TheColor;
        } else
        {
            this.colormenu.SetActive(false);
            if (this.controllerActions.TouchpadTouch.IsPressed() && !this.menu.activeInHierarchy)
            {
                this.menu.SetActive(true);
            }
            else if(!this.controllerActions.TouchpadTouch.IsPressed() && this.menu.activeInHierarchy)
            {
                if (this.touchpadposition.y > 0.71)
                {
                    this.SetActiveTool(0);
                }
                else if (this.touchpadposition.y < -0.71)
                {
                    this.SetActiveTool(1);
                }
                else if (this.touchpadposition.x > 0.71)
                {
                    this.SetActiveTool(2);
                }
                else if (this.touchpadposition.x < -0.71)
                {
                    this.SetActiveTool(3);
                }

                this.menu.SetActive(false);
            }
        }

        if (this.touchpadposition.y>0.71)
        {
            this.upButton.image.color = Color.green;
            this.downButton.image.color = Color.white;
            this.rightButton.image.color = Color.white;
            this.leftButton.image.color = Color.white;
        } else if(this.touchpadposition.y<-0.71)
        {
            this.upButton.image.color = Color.white;
            this.downButton.image.color = Color.green;
            this.rightButton.image.color = Color.white;
            this.leftButton.image.color = Color.white;
        } else if(this.touchpadposition.x>0.71)
        {
            this.upButton.image.color = Color.white;
            this.downButton.image.color = Color.white;
            this.rightButton.image.color = Color.green;
            this.leftButton.image.color = Color.white;
        } else if(this.touchpadposition.x<-0.71)
        {
            this.upButton.image.color = Color.white;
            this.downButton.image.color = Color.white;
            this.rightButton.image.color = Color.white;
            this.leftButton.image.color = Color.green;
        }
    }

    private void SetActiveTool(int index)
    {
        for (var i = 0; i < this.tools.Length; i++)
        {
            this.tools[i].SetActive(i == index);
        }

        this.activeTool = index >= 0 && index < this.tools.Length ? this.tools[index] : null;
    }
}
