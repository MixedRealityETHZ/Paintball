using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{

    private PaintManager _instance;

    public Shader texturePaint;
    public Shader extendIslands;

    int prepareUVID = Shader.PropertyToID("_PrepareUV");
    int positionID = Shader.PropertyToID("_PainterPosition");
    int hardnessID = Shader.PropertyToID("_Hardness");
    int strengthID = Shader.PropertyToID("_Strength");
    int radiusID = Shader.PropertyToID("_Radius");
    int blendOpID = Shader.PropertyToID("_BlendOp");
    int colorID = Shader.PropertyToID("_PainterColor");
    int textureID = Shader.PropertyToID("_MainTex");
    int uvOffsetID = Shader.PropertyToID("_OffsetUV");
    int uvIslandsID = Shader.PropertyToID("_UVIslands");

    Material paintMaterial;
    Material extendMaterial;

    CommandBuffer command;


    public PaintManager instance() {
        return this._instance;
    }

    public void Start()
    {
        //base.Awake();
        this._instance = new PaintManager();

        this.paintMaterial = new Material(this.texturePaint);
        this.extendMaterial = new Material(this.extendIslands);
        this.command = new CommandBuffer();
        this.command.name = "CommmandBuffer - " + this.gameObject.name;
    }

    public void initTextures(Paintable paintable)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        this.command.SetRenderTarget(mask);
        this.command.SetRenderTarget(extend);
        this.command.SetRenderTarget(support);

        this.paintMaterial.SetFloat(this.prepareUVID, 1);
        this.command.SetRenderTarget(uvIslands);
        this.command.DrawRenderer(rend, this.paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(this.command);
        this.command.Clear();
    }


    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        this.paintMaterial.SetFloat(this.prepareUVID, 0);
        this.paintMaterial.SetVector(this.positionID, pos);
        this.paintMaterial.SetFloat(this.hardnessID, hardness);
        this.paintMaterial.SetFloat(this.strengthID, strength);
        this.paintMaterial.SetFloat(this.radiusID, radius);
        this.paintMaterial.SetTexture(this.textureID, support);
        this.paintMaterial.SetColor(this.colorID, color ?? Color.red);
        this.extendMaterial.SetFloat(this.uvOffsetID, paintable.extendsIslandOffset);
        this.extendMaterial.SetTexture(this.uvIslandsID, uvIslands);

        this.command.SetRenderTarget(mask);
        this.command.DrawRenderer(rend, this.paintMaterial, 0);

        this.command.SetRenderTarget(support);
        this.command.Blit(mask, support);

        this.command.SetRenderTarget(extend);
        this.command.Blit(mask, extend, this.extendMaterial);

        Graphics.ExecuteCommandBuffer(this.command);
        this.command.Clear();
    }

}
