using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private Image panel;
    Color temp1;
    Color temp2;

    // Start is called before the first frame update
    void Start()
    {
        this.temp1 = this.panel.color;
        this.temp1.a = 1.0f;
        this.temp2 = this.panel.color;
        this.temp2.a = 0.5f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        this.panel.color = this.temp1;
    }
    private void OnCollisionExit(Collision collision)
    {
        this.panel.color = this.temp2;
    }
}
