using UnityEngine;

public class ColorPickerTriangle : MonoBehaviour {

    public Color TheColor = Color.cyan;
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    //set the colortriangle (might change it)
    const float MainRadius = .6f;
    const float CRadius = .5f;
    const float CWidth = .1f;
    const float TRadius = .4f;


    public GameObject Triangle;
    //the pointer indicating which color to choose with the controller
    public GameObject PointerColor;
    //pointer for the outer color circle
    public GameObject PointerMain;

    private Mesh TMesh;
    private Plane MyPlane;
    private Vector3[] RPoints;
    private Vector3 CurLocalPos;
    private Vector3 CurBary = Vector3.up;
    private Color CircleColor = Color.red;
    

    void Start()
    {
        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
    }

	// Use this for initialization
	void Awake () {
        float h, s, v;
        Color.RGBToHSV(this.TheColor, out h, out s, out v);
        //Debug.Log("HSV = " + v.ToString() + "," + h.ToString() + "," + v.ToString() + ", color = " + TheColor.ToString());
        this.MyPlane = new Plane(this.transform.TransformDirection(Vector3.forward), this.transform.position);
        this.RPoints = new Vector3[3];
        this.SetTrianglePoints();
        this.TMesh = this.Triangle.GetComponent<MeshFilter>().mesh;
        this.SetNewColor(this.TheColor);
    }

    Vector2 touchpadposition;

    // Update is called once per frame
    void Update () {
        if (this.controllerActions.TouchpadTouch.IsPressed())
        {
            this.touchpadposition = this.controllerActions.TouchpadPosition.ReadValue<Vector2>();
        }
        Vector3 vec = new Vector3(this.touchpadposition.x, this.touchpadposition.y, 0.0f);

        this.PointerColor.transform.localPosition = vec*CRadius;

        if (this.isintriangle(this.PointerColor.transform.localPosition, this.RPoints[0], this.RPoints[1], this.RPoints[2]))
        {
            this.CurLocalPos = this.PointerColor.transform.localPosition;
            this.CheckTrianglePosition();
            return;
        } else if (this.isincircle(this.PointerColor.transform.localPosition))
        {
            this.CurLocalPos = this.PointerColor.transform.localPosition;
            this.CheckCirclePosition();
            return;
        }
        
    }


    private bool isintriangle(Vector3 position,  Vector3 a, Vector3 b, Vector3 c) 
    {
        Vector3 res= this.Barycentric(position, a,b,c);
        if(res.x >= 0.0f && res.y >=0.0f && res.z >=0.0f)
        {
            return true;
        }
        return false;
    }

    private bool isincircle(Vector3 position)
    {
        if((this.PointerColor.transform.localPosition.magnitude - CRadius) > CWidth / 2f)
        {
            return false;
        }
        return true;
    }

    

    public void SetNewColor(Color NewColor)
    {
        this.TheColor = NewColor;
        float h, s, v;
        Color.RGBToHSV(this.TheColor, out h, out s, out v);
        this.CircleColor = Color.HSVToRGB(h, 1, 1);
        this.ChangeTriangleColor(this.CircleColor);
        this.PointerMain.transform.localEulerAngles = Vector3.back * (h * 360f);
        this.CurBary.y = 1f - v;
        this.CurBary.x = v * s;
        this.CurBary.z = 1f - this.CurBary.y - this.CurBary.x;
        this.CurLocalPos = this.RPoints[0] * this.CurBary.x + this.RPoints[1] * this.CurBary.y + this.RPoints[2] * this.CurBary.z;
        this.PointerColor.transform.localPosition = this.CurLocalPos;
    }

    private void CheckCirclePosition()
    {
        if (Mathf.Abs(this.CurLocalPos.magnitude - CRadius) > CWidth / 2f)
            return;

        float a = Vector3.Angle(Vector3.left, this.CurLocalPos);
        if (this.CurLocalPos.y < 0)
            a = 360f - a;

        this.CircleColor = Color.HSVToRGB(a / 360, 1, 1);
        this.ChangeTriangleColor(this.CircleColor);
        this.PointerMain.transform.localEulerAngles = Vector3.back * a;
        this.SetColor();
    }

    private void CheckTrianglePosition()
    {
        Vector3 b = this.Barycentric(this.CurLocalPos, this.RPoints[0], this.RPoints[1], this.RPoints[2]);
        if (b.x >= 0f && b.y >= 0f && b.z >= 0f)
        {
            this.CurBary = b;
            this.PointerColor.transform.localPosition = this.CurLocalPos;
            this.SetColor();
        }
    }

    private void SetColor()
    {
        float h, v, s;
        Color.RGBToHSV(this.CircleColor, out h, out v, out s);
        Color c = (this.CurBary.y > .9999) ? Color.black : Color.HSVToRGB(h, this.CurBary.x / (1f - this.CurBary.y), 1f - this.CurBary.y);
        this.TheColor = c;
        this.TheColor.a = 1f;
        Debug.Log(this.TheColor);
    }

    private void ChangeTriangleColor(Color c)
    {
        Color[] colors = new Color[this.TMesh.colors.Length];
        colors[0] = Color.black;
        colors[1] = c;
        colors[2] = Color.white;
        this.TMesh.colors = colors;
    }

    private Vector3 Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 bary = Vector3.zero;
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        bary.y = (d11 * d20 - d01 * d21) / denom;
        bary.z = (d00 * d21 - d01 * d20) / denom;
        bary.x = 1.0f - bary.y - bary.z;
        return bary;
    }


    private void SetTrianglePoints()
    {
        this.RPoints[0] = Vector3.up * TRadius;
        float c = Mathf.Sin(Mathf.Deg2Rad * 30);
        float s = Mathf.Cos(Mathf.Deg2Rad * 30);
        this.RPoints[1] = new Vector3 (s, -c, 0) * TRadius;
        this.RPoints[2] = new Vector3(-s, -c, 0) * TRadius;
    }
}
