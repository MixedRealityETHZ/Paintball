using UnityEngine;

public class example : MonoBehaviour {

    public GameObject ColorPickedPrefab;
    private ColorPickerTriangle CP;
    private bool isPaint = false;
    private GameObject go;
    private Material mat;

    void Start()
    {
        this.mat = this.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        if (this.isPaint)
        {
            this.mat.color = this.CP.TheColor;
        }
    }

    void OnMouseDown()
    {
        if (this.isPaint)
        {
            this.StopPaint();
        }
        else
        {
            this.StartPaint();
        }
    }

    private void StartPaint()
    {
        this.go = (GameObject)Instantiate(this.ColorPickedPrefab, this.transform.position + Vector3.up * 1.4f, Quaternion.identity);
        this.go.transform.localScale = Vector3.one * 1.3f;
        this.go.transform.LookAt(Camera.main.transform);
        this.CP = this.go.GetComponent<ColorPickerTriangle>();
        this.CP.SetNewColor(this.mat.color);
        this.isPaint = true;
    }

    private void StopPaint()
    {
        Destroy(this.go);
        this.isPaint = false;
    }
}
