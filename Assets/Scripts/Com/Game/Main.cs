using UnityEngine;
using Com.Game.Manager;
using Com.Game.Utils.Timers;
using Assets.Scripts.Com.Game.Utils;
using System.Reflection;
public class Main : MonoBehaviour
{

    private ResourceManager mResourceManager = ResourceManager.Instance;
    private GameTimerManager mGameTimerManager = GameTimerManager.Instance;

    void Start()
    {

        AsyncResourceManager.Init();
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        mResourceManager.Update();
        mGameTimerManager.Execute();

        AsyncResourceManager.OnUpdate();
        SyncResourceManager.OnUpdate();
          
    }
  
    void OnDisable()
    {
        print("main OnDisable");
    }
}
