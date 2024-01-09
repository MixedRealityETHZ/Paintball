using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TexturePainter : MonoBehaviour
{
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    public XRRayInteractor ri;

    public Shader paintShader;
    public RenderTexture targetTexture;
    public Material paintMaterial;

    private Material brushMaterial;

    void Start()
    {
        this.brushMaterial = new Material(this.paintShader);

        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
    }

    void Update()

    {
        if (this.controllerActions.Trigger.triggered)
        {
            Debug.Log("triggered");
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(this.ri.transform.position), out hit))
            {
                Debug.Log("hit");
                Debug.Log(hit.textureCoord);
                this.brushMaterial.SetVector("_UV", hit.textureCoord);
                RenderTexture.active = this.targetTexture;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, this.targetTexture.width, this.targetTexture.height, 0);
                this.brushMaterial.SetPass(0);
                GL.Begin(GL.QUADS);
                GL.TexCoord2(0, 0); GL.Vertex3(0, 0, 0);
                GL.TexCoord2(1, 0); GL.Vertex3(this.targetTexture.width, 0, 0);
                GL.TexCoord2(1, 1); GL.Vertex3(this.targetTexture.width, this.targetTexture.height, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(0, this.targetTexture.height, 0);
                GL.End();
                GL.PopMatrix();
            }
        }
    }
}
