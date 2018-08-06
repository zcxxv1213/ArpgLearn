using UnityEngine;
using System.Collections;

public class ScreenShot : MonoBehaviour 
{
	// Use this for initialization
    public string fileName = "";
    public int FPS = 10;
    public bool enableCapture = false;
    public bool reset = false;

    private int mFrameCount = 0;
    private float mLastCaptureTime = 0;
    private float mIntervalTime = 0.1f;

	void Start () 
    {
        this.Reset();
	}

    void Reset()
    {
        this.mFrameCount = 0;
        this.reset = false;
        this.FPS = Mathf.Max(1, this.FPS);
        this.mIntervalTime = 1 / this.FPS;
    }
	// Update is called once per frame
	void Update () 
    {
        if (this.enableCapture)
        {
            if (Time.time - this.mLastCaptureTime > this.mIntervalTime)
            {
                if (!System.IO.Directory.Exists("Capture"))
                {
                    System.IO.Directory.CreateDirectory("Capture");
                }
                string file = string.Format("Capture/{0}_{1}.png", fileName, this.mFrameCount++);
                ScreenCapture.CaptureScreenshot(file);
                this.mLastCaptureTime = Time.time;
            }
        }
        if (this.reset)
        {
            this.Reset();
        }
	}
}
