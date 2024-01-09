using UnityEngine;

public class TexturePainter2 : MonoBehaviour
{
    public RenderTexture tt;
    private RenderTexture targetTexture;
    private Texture2D tempTexture;

    void Start()
    {
        this.targetTexture = new RenderTexture(this.tt);
        this.GetComponent<MeshRenderer>().material.mainTexture = this.targetTexture;
    }

    private void OnCollisionEnter(Collision collision)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(collision.gameObject.transform.position, collision.gameObject.transform.forward), out hit))
        {
            Debug.Log(hit.textureCoord);
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= this.targetTexture.width;
            pixelUV.y *= this.targetTexture.height;

            this.tempTexture = new Texture2D(this.targetTexture.width, this.targetTexture.height);
            RenderTexture.active = this.targetTexture;
            this.tempTexture.ReadPixels(new Rect(0, 0, this.targetTexture.width, this.targetTexture.height), 0, 0);
            this.tempTexture.Apply();

            Color paintColor = Color.green; // Set your desired paint color here

            this.tempTexture.SetPixel((int)pixelUV.x, (int)pixelUV.y, paintColor);
            this.tempTexture.Apply();

            Graphics.Blit(this.tempTexture, this.targetTexture);
        }
    }
}

