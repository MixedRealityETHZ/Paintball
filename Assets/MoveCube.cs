using UnityEngine;

public class MoveCube : MonoBehaviour {
    // Start is called before the first frame update
    Vector3 initalPosition;

    void Start() {
        this.initalPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update() {
        this.transform.position = this.initalPosition + (Mathf.Sin(Time.time)+ 1.0f) * Vector3.up;
    }
}
