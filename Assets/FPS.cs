using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FPS : MonoBehaviour
{
    private TMP_Text _fpsText;

    private void Start()
    {
        this._fpsText = this.GetComponent<TMP_Text>();
        this.StartCoroutine(this.FramesPerSecond());
    }

    private IEnumerator FramesPerSecond()
    {
        while (true)
        {
            int fps = (int)(1f / Time.deltaTime);
            this.DisplayFPS(fps);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void DisplayFPS(float fps)
    {
        this._fpsText.text = $"{fps} FPS";
    }
}