using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public string playerName;

    private string layer = "home";
    int connectedPlayers;

    [SerializeField]
    public Transform HomeScreen;
    [SerializeField]
    private GameObject Cam;
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

            if(arg == "-dediServ")
            {
                ip = "86.83.234.112";
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
        if(ip == "" || ip == null || ip == "-hubSessionId")
        {
            ip = "localhost";
        }

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

        foreach(NetworkObject obj in InstanceFinder.ClientManager.Objects.Spawned.Values)
        {
            if(obj.gameObject.tag == "Player")
            {
                Cam.SetActive(false);
            }
            else
            {
                Cam.SetActive(true);
            }
        }
    }

    public void Play()
    {
        HomeScreen.gameObject.SetActive(false);
        ServerSelectionScreen.gameObject.SetActive(true);
        layer = "serverSelection";
        FindAnyObjectByType<AudioManger>().Play("click1");
    }

    public void Quit()
    {
        Debug.Log("Quiting...");
        Application.Quit();
        FindAnyObjectByType<AudioManger>().Play("click3");
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
        FindAnyObjectByType<AudioManger>().Play("click2");
    }

    public void OnNameChange()
    {
        playerName = ServerSelectionScreen.transform.Find("InputNameValue/Text Area/Text").GetComponent<TMP_Text>().text;
    }  

    public void Server1Click()
    {
        //activate loading screen
        ServerSelectionScreen.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(true);
        FindObjectOfType<ClientManager>()._port = 7777;

        ConnectClient(ip, 7777);
        Debug.Log("Connecting with: " + ip + ":7777");
        FindAnyObjectByType<AudioManger>().Play("click1");
    }
    public void Server2Click()
    {
        //activate loading screen
        ServerSelectionScreen.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(true);
        FindObjectOfType<ClientManager>()._port = 7779;

        ConnectClient(ip, 7779);
        Debug.Log("Connecting with: " + ip + ":7779");
        FindAnyObjectByType<AudioManger>().Play("click1");
    }

    public void OnSceneLoaded(SceneLoadEndEventArgs obj)
    {
        //deactive loading screen
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);

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