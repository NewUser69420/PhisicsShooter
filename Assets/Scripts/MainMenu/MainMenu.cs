using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    NetworkManager _networkManager;
    PlayerControlls playerControlls;

    public string ip;
    public ushort serverHostPort;
    public ushort clientConnectPort;

    private string layer = "home";

    [SerializeField]
    private Transform HomeScreen;
    [SerializeField]
    private Transform ServerSelectionScreen;
    [SerializeField]
    private Transform Background;
    [SerializeField]
    private Transform LoadingScreen;
    [SerializeField]
    private Transform TestServerSolutionButn;

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);

        if (serverHostPort == 0)
        {
            serverHostPort = _networkManager.ServerManager.GetComponent<Tugboat>().GetPort();
            Debug.Log("serverHostPort = " + serverHostPort);
        }
        if (clientConnectPort == 0)
        {
            clientConnectPort = _networkManager.ServerManager.GetComponent<Tugboat>().GetPort();
            Debug.Log("clientConnectPort = " + clientConnectPort);
        }
        if (ip == "")
        {
            ip = _networkManager.ServerManager.GetComponent<Tugboat>().GetClientAddress();
            Debug.Log("Server Ip = " + ip);
        }
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

    public void ServerClick(int _serverId)
    {
        //activate loading screen
        ServerSelectionScreen.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(true);

        //set ip
        //ip = "...."; 

        //Connect To Server
        switch (_serverId)
        {
            case 1:
                ConnectClient(ip, 7770);
                Debug.Log("Connecting with: " + ip + "7770");
                break;
            case 2:
                ConnectClient(ip, 7771);
                Debug.Log("Connecting with: " + ip + "7771");
                break;
            default:
                Debug.Log("Server Doesn't Exist");
                break;
        }
    }

    public void OnSceneLoaded(SceneLoadEndEventArgs obj)
    {
        //deactive loading screen
        Background.gameObject.SetActive(false);
        LoadingScreen.gameObject.SetActive(false);
        TestServerSolutionButn.gameObject.SetActive(false);
    }

    public void ConnectClient(string _ip, ushort _port)
    {
        _networkManager.ClientManager.StartConnection(_ip, _port);
    }

    public void StartServer(ushort _port)
    {
        _networkManager.ServerManager.StartConnection(_port);
    }

    public void TestServerSolution()
    {
        _networkManager.ServerManager.StartConnection(7770);
    }
}