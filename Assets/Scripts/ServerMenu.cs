using UnityEngine;
using TMPro;
using FishNet.Object;
using BayatGames.SaveGameFree;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using BayatGames.SaveGameFree.Examples;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Shapes;
using FishNet.Example.Scened;
using FishNet.Managing.Client;
using System;
using System.Collections;

public class ServerMenu : NetworkBehaviour
{
    [SerializeField] private GameObject LoginScreen;
    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private GameObject ScoreScreen;
    [SerializeField] private GameObject PlayerPrefab;

    public string playerName = "not set";
    public float score = -1;

    public void OnLoginClick()
    {
        //get inputs
        string tempName = LoginScreen.transform.Find("InputName").GetComponent<TMP_InputField>().text;
        string tempPassword = LoginScreen.transform.Find("InputPassword").GetComponent<TMP_InputField>().text;
        Debug.Log($"name: {tempName} , Pass: {tempPassword}");

        RequestLogin(tempName, tempPassword, LocalConnection);
    }

    public void OnClickPlay()
    {
        FindObjectOfType<AudioManger>().Play("click2");

        foreach (var obj in LocalConnection.Objects)
        {
            if (obj.CompareTag("Player")) obj.GetComponent<InitializePlayer>().DoStart();
        }

        AskServerToGoLobby(LocalConnection);
    }

    public void OnClickScore()
    {
        ScoreScreen.SetActive(true);
        MenuScreen.SetActive(false);
        ScoreScreen.transform.Find("ScoreVal").GetComponent<TMP_Text>().text = Math.Round(SaveGame.Load<PlayerData>(playerName).score, 2).ToString();
    }

    public void OnClickScoreBack()
    {
        ScoreScreen.SetActive(false);
        MenuScreen.SetActive(true);
    }

    public void OnClickMainMenu()
    {
        if (base.IsServerStarted) return;
        Debug.Log("Going Back To MainMenu");
        FindObjectOfType<AudioManger>().Play("click2");

        //stopping connecion
        FindObjectOfType<ClientManager>().loadingMM = true;
        base.ClientManager.StopConnection();

        //setting mm ui active
        GameObject MM = FindObjectOfType<MainMenu>().gameObject;
        MM.SetActive(true);
        //UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Lobbies");
        //UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("ServerMenu");

        //delete player
        foreach (var obj in FindObjectsOfType<InitializePlayer>(true))
        {
            Destroy(obj.gameObject);
        }

        //setup mm
        MM.transform.Find("ServerSelectionScreen").gameObject.SetActive(false);
        MM.transform.Find("LoadingScreen").gameObject.SetActive(false);
        MM.transform.Find("HomeScreen").gameObject.SetActive(true);

        GameObject serverMenu = FindObjectOfType<ServerMenu>(true).gameObject;
        serverMenu.transform.Find("LoginScreen").gameObject.SetActive(true);
        serverMenu.transform.Find("MenuScreen").gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestLogin(string _name, string _pass, NetworkConnection conn)
    {
        //check for availability
        if (SaveGame.Exists(_name))
        {
            //try login
            PlayerData pData = SaveGame.Load<PlayerData>(_name);
            if (pData.password == _pass)
            {
                //login
                LoginScreen.transform.Find("WrongPassScreen").gameObject.SetActive(false);

                InitializePlayer player = null;
                ScoreTracker playerScore = null;
                foreach (var obj in conn.Objects) { if (obj.CompareTag("Player")) { playerScore = obj.GetComponent<ScoreTracker>(); player = obj.GetComponent<InitializePlayer>(); } }

                player.playerName = pData.name;
                playerScore.playerName = pData.name;
                playerScore.score = pData.score;
                playerScore.wins = pData.wins;
                playerScore.losses = pData.losses;
                LoginClient(conn, pData.name, pData.score, pData.wins, pData.losses, player);
                Debug.Log("Logged in");
            }
            else
            {
                //say wrong password
                DoWrongPassClient(conn);
                Debug.Log("Wrong Pass");
            }
        }
        else
        {
            //make new acc
            PlayerData pData = new();
            pData.password = _pass;
            pData.name = _name;
            pData.score = 0;
            pData.wins = 0;
            pData.losses = 0;

            SaveGame.Save<PlayerData>(_name, pData);

            //login
            InitializePlayer player = null;
            ScoreTracker playerScore = null;
            foreach (var obj in conn.Objects) { if (obj.CompareTag("Player")) { playerScore = obj.GetComponent<ScoreTracker>(); player = obj.GetComponent<InitializePlayer>(); } }

            player.playerName = pData.name;
            playerScore.playerName = pData.name;
            playerScore.score = pData.score;
            playerScore.wins = pData.wins;
            playerScore.losses = pData.losses;
            LoginClient(conn, pData.name, pData.score, pData.wins, pData.losses, player);
            Debug.Log("Created Acc");
        }
    }

    [TargetRpc]
    private void DoWrongPassClient(NetworkConnection conn)
    {
        LoginScreen.transform.Find("WrongPassScreen").gameObject.SetActive(true);
    }

    [TargetRpc]
    private void LoginClient(NetworkConnection conn, string _name, float _score, int _wins, int _losses, InitializePlayer _pobj)
    {
        playerName = _name;
        score = _score;
        _pobj.playerName = _name;
        _pobj.GetComponent<ScoreTracker>().playerName = _name;
        _pobj.GetComponent<ScoreTracker>().score = _score;
        _pobj.GetComponent<ScoreTracker>().wins = _wins;
        _pobj.GetComponent<ScoreTracker>().losses = _losses;

        LoginScreen.SetActive(false);
        MenuScreen.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskServerToGoLobby(NetworkConnection conn)
    {
        foreach (var obj in conn.Objects)
        {
            if (obj.CompareTag("Player")) obj.GetComponent<InitializePlayer>().DoStart();
        }

        List<NetworkObject> objs = new();
        foreach(var obj in conn.Objects) if(obj.CompareTag("Player")) objs.Add(obj);
        
        SceneLookupData lookup = new SceneLookupData(0, "Lobbies");
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = false;
        sld.MovedNetworkObjects = objs.ToArray();
        //sld.Options.LocalPhysics = UnityEngine.SceneManagement.LocalPhysicsMode.Physics3D;
        sld.ReplaceScenes = ReplaceOption.None;
        base.SceneManager.LoadConnectionScenes(conn, sld);
    }

    public void StartPlayerReset(string name, NetworkConnection conn)
    {
        StartCoroutine(ResetPlayer(name, conn));
    }
    
    IEnumerator ResetPlayer(string _name, NetworkConnection playerConn)
    {
        yield return new WaitForSeconds(0.5f);

        GameObject Player = Instantiate(PlayerPrefab);
        base.ServerManager.Spawn(Player, playerConn, gameObject.scene);

        FixName(Player.GetComponent<NetworkObject>().Owner, Player, _name);
    }

    [TargetRpc]
    private void FixName(NetworkConnection conn, GameObject pobj, string _name)
    {
        pobj.GetComponent<InitializePlayer>().playerName = _name;
    }
}

public class PlayerData
{
    public string password = null;
    public string name = null;
    public float score = -1;
    public int wins = -1;
    public int losses = -1;
}
