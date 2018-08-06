using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraControl : MonoBehaviour
{

    // Use this for initialization
    public Transform followTarget;

    public Vector3 offset;

    public void SetFieldOfView(float value)
    {
        Camera camera = gameObject.GetComponent<Camera>();

        camera.fieldOfView = value;
    }

    void LateUpdate()
    {
        if (this.followTarget)
        {
            this.transform.position = this.followTarget.position + this.offset;
            this.transform.LookAt(this.followTarget.position + Vector3.up * 0.6f);
        }
    }
}
