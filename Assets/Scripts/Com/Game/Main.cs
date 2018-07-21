using UnityEngine;
using Com.Game.Manager;
using Com.Game.Utils.Timers;
using Assets.Scripts.Com.Game.Utils;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Assets.Scripts.Com.Game.Manager;
using static Com.Game.Manager.SyncResourceManager;

public class Main : MonoBehaviour
{

    private ResourceManager mResourceManager = ResourceManager.Instance;
    private GameTimerManager mGameTimerManager = GameTimerManager.Instance;

    void Start()
    {
        Init();
        DontDestroyOnLoad(this.gameObject);
    }

    async void Init()
    {
        await LoadInit();
    }

    async Task LoadInit()
    {
        await AsyncResourceManager.Init();
        await SyncResourceManager.Init(new CallBackDelegate(() => UIManager.Instance.Init()));

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
