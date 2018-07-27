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

public class Main : MonoBehaviour
{

    private ResourceManager mResourceManager = ResourceManager.Instance;
    private GameTimerManager mGameTimerManager = GameTimerManager.Instance;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Game.Scene.AddComponent<NetOuterComponent>();
        Game.Scene.AddComponent<OpcodeTypeComponent>();
        Game.Scene.AddComponent<MessageDispatherComponent>();
        this.CreatConnect();

    }
    void CreatConnect()
    {
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000);
        Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(iPEndPoint);
        

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
        await SyncResourceManager.Init(new CallBackDelegate(() => UIManager.Instance.Init()
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
