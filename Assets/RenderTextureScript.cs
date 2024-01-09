using UnityEngine;

public class RenderTextureScript : MonoBehaviour
{
    public RenderTexture renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Camera>().targetTexture = this.renderTexture;
    }
}
