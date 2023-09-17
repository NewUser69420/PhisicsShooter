using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using LiteNetLib;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    NetworkManager _networkManager;
    PlayerControlls playerControlls;

    public string ip = "";
    public ushort serverHostPort = 0;
    public ushort clientConnectPort = 0;

    private string layer = "home";
    int connectedPlayers;

    [SerializeField]
    public Transform HomeScreen;
    [SerializeField]
    private Transform ServerSelectionScreen;
    [SerializeField]
    private Transform Background;
    [SerializeField]
    public Transform LoadingScreen;
    [SerializeField]
    private Transform DedicatedServerScreen;

    private void Awake()
    {
        if (FindObjectsByType<MainMenu>(FindObjectsSortMode.None).Count() > 1) Destroy(gameObject);

        string[] args = System.Environment.GetCommandLineArgs();

        foreach (var arg in args)
        {
            if(arg.Length == 4)
            {
                serverHostPort = Convert.ToUInt16(arg);
            }
        }

        if (serverHostPort == 0)
        {
            serverHostPort = 7777;
        }
        if (clientConnectPort == 0)
        {
            clientConnectPort = 7777;
        }
        //local play
        ip = "localhost";

        //online play
        //ip = "86.83.234.112";


        Debug.Log("serverHostPort = " + serverHostPort);
        Debug.Log("clientConnectPort = " + clientConnectPort);
        Debug.Log("Server Ip = " + ip);

        foreach (var arg in args)
        {
            if(arg == "-launchAsServer")
            {
                StartServer(serverHostPort);
            }
        }
    }

    private void Start()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        _networkManager = FindObjectOfType<NetworkManager>();

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (playerControlls.MainMenu.GoBackOne.WasPressedThisFrame()) GoBackOne(layer);
    }

    public void Play()
    {
        HomeScreen.gameObject.SetActive(false);
        ServerSelectionScreen.gameObject.SetActive(true);
        layer = "serverSelection";
    }

    public void Quit()
    {
        Debug.Log("Quiting...");
        Application.Quit();
    }

    private void GoBackOne(string _layer)
    {
        switch(_layer)
        {
            case "home":
                break;
            case "serverSelection":
                ServerSelectionScreen.gameObject.SetActive(false);
                HomeScreen.gameObject.SetActive(true);
                break;
        }
    }

    public void Server1Click()
    {
        //activate loading screen
        ServerSelectionScreen.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(true);

        ConnectClient(ip, 7777);
        Debug.Log("Connecting with: " + ip + ":7777");
    }
    public void Server2Click()
    {
        //activate loading screen
        ServerSelectionScreen.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(true);

        ConnectClient(ip, 7778);
        Debug.Log("Connecting with: " + ip + ":7778");
    }

    public void OnSceneLoaded(SceneLoadEndEventArgs obj)
    {
        //deactive loading screen
        Background.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(false);
    }

    public void ConnectClient(string _ip, ushort _port)
    {
        _networkManager.ClientManager.StartConnection(_ip, _port);
    }

    public void StartServer(ushort _port)
    {
        HomeScreen.gameObject.SetActive(false);
        DedicatedServerScreen.gameObject.SetActive(true);

        InstanceFinder.ServerManager.StartConnection(_port);

        DedicatedServerScreen.transform.Find("ip + port").GetComponent<TMP_Text>().text = ip + ":" + _port;
    }

    public void TestServerSolution()
    {
        StartServer(serverHostPort);
    }
}