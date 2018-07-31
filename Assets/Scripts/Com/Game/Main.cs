using UnityEngine;
using Com.Game.Manager;
using Com.Game.Utils.Timers;
using Assets.Scripts.Com.Game.Utils;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Assets.Scripts.Com.Game.Manager;
using static Com.Game.Manager.SyncResourceManager;
using Assets.Scripts.Com.Manager;
using ETModel;
using System.Net;
using System.Threading;

public class Main : MonoBehaviour
{

    private ResourceManager mResourceManager = ResourceManager.Instance;
    private GameTimerManager mGameTimerManager = GameTimerManager.Instance;
    private void Awake()
    {
        Debug.Log(SynchronizationContext.Current);
        SynchronizationContext unityContext = SynchronizationContext.Current; 
        SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
        Game.EventSystem.Add(DLLType.Model, typeof(Main).Assembly);
        DontDestroyOnLoad(this.gameObject);
        Game.Scene.AddComponent<NetOuterComponent>();
        Game.Scene.AddComponent<OpcodeTypeComponent>();
        Game.Scene.AddComponent<MessageDispatherComponent>();
        GameObject gameLooepr = new GameObject();
        gameLooepr.name = "GameLooper";
        gameLooepr.AddComponent<GameLooper>();
        DontDestroyOnLoad(gameLooepr);
        this.CreatConnect();
        SynchronizationContext.SetSynchronizationContext(unityContext);
    }
    async void CreatConnect()
    {
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10002);
        Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(iPEndPoint);
        Game.Scene.AddComponent<SessionComponent>().Session = session;
    //    Debug.Log("Send");
        R2C_Ping r2C_Ping = (R2C_Ping)await session.Call(new C2R_Ping() { });
        Debug.Log(r2C_Ping.Message);
    }
    void Start()
    {
        Init();
    }

    async void Init()
    {
        await LoadInit();
    }

    async Task LoadInit()
    {
        await AsyncResourceManager.Init();
        await SyncResourceManager.Init(new CallBackDelegate(() => {
            UIManager.Instance.Init();
        }
        ));
        SceneManager.Instance.EnterSceneById(0);
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
