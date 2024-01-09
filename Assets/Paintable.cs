using UnityEngine;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;

    public float extendsIslandOffset = 1;

    public GameObject paintManager;
    private PaintManager _paintManager;

    RenderTexture extendIslandsRenderTexture;
    RenderTexture uvIslandsRenderTexture;
    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;

    Renderer rend;

    int maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => this.maskRenderTexture;
    public RenderTexture getUVIslands() => this.uvIslandsRenderTexture;
    public RenderTexture getExtend() => this.extendIslandsRenderTexture;
    public RenderTexture getSupport() => this.supportTexture;
    public Renderer getRenderer() => this.rend;

    void Start()
    {
        this.maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        this.maskRenderTexture.filterMode = FilterMode.Bilinear;

        this.extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        this.extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        this.uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        this.uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        this.supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        this.supportTexture.filterMode = FilterMode.Bilinear;

        this.rend = this.GetComponent<Renderer>();
        this.rend.material.SetTexture(this.maskTextureID, this.extendIslandsRenderTexture);

        this._paintManager = this.paintManager.GetComponent<PaintManager>().instance();
        this._paintManager.initTextures(this);
    }

    void OnDisable()
    {
        this.maskRenderTexture.Release();
        this.uvIslandsRenderTexture.Release();
        this.extendIslandsRenderTexture.Release();
        this.supportTexture.Release();
    }
}