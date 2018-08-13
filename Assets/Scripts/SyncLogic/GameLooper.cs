using ETModel;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameLooper : MonoBehaviour {
    static int s_intervalTime = 200;
    static float s_UpdateTimer = 0; //ms

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Game.Scene.AddComponent<PlayerComponent>();
        Game.Scene.AddComponent<UnitComponent>();
    }

    //TODO 操作重计算流程，核对到来帧操作，PASS继续，否则重算
    // Update is called once per frame
    void Update () {
      //  s_UpdateTimer += Time.deltaTime * 1000; //换算成ms
        OneThreadSynchronizationContext.Instance.Update();
        Game.EventSystem.Update();
      /*  if (s_UpdateTimer > s_intervalTime)
        {
            s_UpdateTimer -= s_intervalTime;
            Game.EventSystem.FrameUpdate();
        }*/
    }
    private void LateUpdate()
    {
        Game.EventSystem.LateUpdate();
    }

    private void OnApplicationQuit()
    {
        Game.Close();
    }
}
