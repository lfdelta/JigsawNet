using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Sample code from https://docs.unity3d.com/Manual/UNetManager.html


public class ClientInfoMsg : MessageBase
{
    public string Username;
};


public class JigsawNetworkManager : NetworkManager
{
    public GameObject GameWorldPrefab;

    private TextureTransfer TexTransfer;
    private NetworkWorldState NetWorldState;
    private UniqueObjectManager ObjectManager;

    private Dictionary<int, ClientInfoMsg> DeferredClientInfo;


    private void Awake()
    {
        ObjectManager = gameObject.AddComponent<UniqueObjectManager>();
    }


    public void StartJigsawHost()
    {
        if (StaticJigsawData.PuzzleTexture == null)
        {
            StaticJigsawData.ErrorHUD.DisplayMessage("You must load a valid puzzle image in order to host a game", 5.0f);
            return;
        }
        JigsawMenu menu = FindObjectOfType<JigsawMenu>();
        if (menu.PlayerNameInput.text.Length < 4)
        {
            StaticJigsawData.ErrorHUD.DisplayMessage("You must have a valid display name, with at least 4 characters", 5.0f);
            return;
        }
        StaticJigsawData.LocalPlayerName = menu.PlayerNameInput.text;
        StaticJigsawData.PuzzleWidth = (uint)menu.PuzzleWidthSlider.value;
        StaticJigsawData.PuzzleHeight = (uint)menu.PuzzleHeightSlider.value;

        NetworkWorldState oldWorldState = FindObjectOfType<NetworkWorldState>();
        if (oldWorldState)
        {
            Destroy(oldWorldState);
        }

        ObjectManager.ResetState();
        networkPort = 7777;
        StartHost();
    }


    public void StartJigsawClient()
    {
        JigsawMenu menu = FindObjectOfType<JigsawMenu>();
        if (menu.PlayerNameInput.text.Length < 4)
        {
            StaticJigsawData.ErrorHUD.DisplayMessage("You must have a valid display name, with at least 4 characters", 5.0f);
            return;
        }
        string hostAddr = menu.HostAddressInput.text;
        if (!NetworkUtils.IsValidHexAddr(hostAddr))
        {
            StaticJigsawData.ErrorHUD.DisplayMessage("Provided host ID is invalid. It must be 8 characters long, using 0-9 and A-B", 5.0f);
            return;
        }
        StaticJigsawData.LocalPlayerName = menu.PlayerNameInput.text;
        

        NetworkWorldState oldWorldState = FindObjectOfType<NetworkWorldState>();
        if (oldWorldState)
        {
            Destroy(oldWorldState);
        }

        StaticJigsawData.PuzzleTexture = null; // Clear the client's texture to prevent auto-populating the puzzle pieces with their own image
        ObjectManager.ResetState();
        networkAddress = NetworkUtils.HexToIPv4(hostAddr);
        networkPort = 7777;
        StartClient();
    }


    //~ Begin server callbacks
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("A client connected to the server: " + conn);
        if (conn.hostId != -1)
        {
            if (!TexTransfer)
            {
                TexTransfer = (TextureTransfer)gameObject.AddComponent(typeof(TextureTransfer));
            }
            TexTransfer.SendTextureToClient(conn.connectionId, 0, StaticJigsawData.PuzzleTexture);
        }
    }


    public override void OnServerDisconnect(NetworkConnection conn)
    {
        for (int i = 0; i < conn.playerControllers.Count; ++i)
        {
            PlayerController player = conn.playerControllers[i];
            if (player.gameObject != null)
            {
                JigsawPlayerController jigsawController = player.gameObject.GetComponent<JigsawPlayerController>();
                if (jigsawController)
                {
                    jigsawController.CleanupPlayer();
                    NetWorldState.DeregisterPlayer(jigsawController.PlayerState);
                }
            }
        }

        NetworkServer.DestroyPlayersForConnection(conn);
        if (conn.lastError != NetworkError.Ok && LogFilter.logError)
        {
            Debug.LogError("ServerDisconnected due to error: " + conn.lastError);
        }
        Debug.Log("A client disconnected from the server: " + conn);
    }


    public override void OnServerReady(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
        Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);
    }


    private void GenerateWorldState()
    {
        if (GameWorldPrefab != null)
        {
            GameObject gameWorldInst = GameObject.Instantiate(GameWorldPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(gameWorldInst);
            NetWorldState = gameWorldInst.GetComponent<NetworkWorldState>();
            if (NetWorldState == null)
            {
                Debug.LogError("JigsawNetworkManager has GameWorldPrefab with missing NetworkWorldState");
            }
        }
        else
        {
            Debug.LogError("JigsawNetworkManager has null GameWorldPrefab property");
        }
    }


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (NetWorldState == null)
        {
            GenerateWorldState();
        }

        var player = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        Debug.Log("Client has requested to get his player added to the game");

        ClientInfoMsg clientMsg;
        if (DeferredClientInfo.TryGetValue(conn.connectionId, out clientMsg))
        {
            JigsawPlayerState playerState = NetWorldState.RegisterPlayer(clientMsg.Username);
            JigsawPlayerController jigsawController = player.GetComponent<JigsawPlayerController>();
            jigsawController.PlayerState = playerState;
            DeferredClientInfo.Remove(conn.connectionId);
        }
    }


    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        // TODO: figure out why deregister isn't getting called
        Debug.Log("OnServerRemovePlayer executed");
        if (player.gameObject != null)
        {
            JigsawPlayerController jigsawController = player.gameObject.GetComponent<JigsawPlayerController>();
            if (jigsawController)
            {
                NetWorldState.DeregisterPlayer(jigsawController.PlayerState);
            }

            NetworkServer.Destroy(player.gameObject);
        }
    }


    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("Server network error occurred: " + (NetworkError)errorCode);
        StaticJigsawData.ErrorHUD.DisplayMessage("Network error: " + ((NetworkError)errorCode).ToString(), 5.0f);
    }


    public override void OnStartHost()
    {
        Debug.Log("Host has started");

        StaticJigsawData.IsHost = true;
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server has started");

        TexTransfer = (TextureTransfer)gameObject.AddComponent(typeof(TextureTransfer));
        NetworkServer.RegisterHandler(JigsawNetworkMsg.ClientInfo, OnServerReceiveClientInfo);
        DeferredClientInfo = new Dictionary<int, ClientInfoMsg>();
    }


    public override void OnStopServer()
    {
        Debug.Log("Server has stopped");
    }


    public override void OnStopHost()
    {
        Debug.Log("Host has stopped");
        StaticJigsawData.IsHost = false;
    }


    private void OnServerReceiveClientInfo(NetworkMessage Msg)
    {
        if (Msg.conn.playerControllers.Count > 0)
        {
            PlayerController player = Msg.conn.playerControllers[0];
            if (player != null && player.gameObject != null)
            {
                JigsawPlayerController jigsawController = player.gameObject.GetComponent<JigsawPlayerController>();
                if (jigsawController != null)
                {
                    if (NetWorldState == null)
                    {
                        GenerateWorldState();
                    }
                    ClientInfoMsg clientMsg = Msg.ReadMessage<ClientInfoMsg>();
                    JigsawPlayerState playerState = NetWorldState.RegisterPlayer(clientMsg.Username);
                    jigsawController.PlayerState = playerState;
                    return;
                }
            }
        }
        ClientInfoMsg tmp = Msg.ReadMessage<ClientInfoMsg>();
        DeferredClientInfo.Add(Msg.conn.connectionId, tmp);
    }
    //~ End server callbacks


    //~ Begin client callbacks
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected successfully to server, now to set up other stuff for the client...");

        ClientInfoMsg msg = new ClientInfoMsg();
        msg.Username = StaticJigsawData.LocalPlayerName;
        client.Send(JigsawNetworkMsg.ClientInfo, msg);
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError)
            {
                Debug.LogError("ClientDisconnected due to error: " + conn.lastError);
            }
            StaticJigsawData.ErrorHUD.DisplayMessage("Network error: " + ((NetworkError)conn.lastError).ToString(), 10.0f);
        }
        Debug.Log("Client disconnected from server: " + conn);
    }


    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("Client network error occurred: " + (NetworkError)errorCode);
        StaticJigsawData.ErrorHUD.DisplayMessage("Network error: " + ((NetworkError)errorCode).ToString(), 10.0f);
    }


    public override void OnClientNotReady(NetworkConnection conn)
    {
        Debug.Log("Server has set client to be not-ready (stop getting state updates)");
    }


    public void OnClientTextureMeta(NetworkMessage msg)
    {
        TexTransfer.OnClientReceiveTexMeta(msg);
    }


    public void OnClientTextureChunk(NetworkMessage msg)
    {
        TexTransfer.OnClientReceiveTexChunk(msg);
    }


    public override void OnStartClient(NetworkClient client)
    {
        TexTransfer = (TextureTransfer)gameObject.AddComponent(typeof(TextureTransfer));

        TexTransfer.SetupClient(client);
        Debug.Log("Client has started");
    }


    public override void OnStopClient()
    {
        Debug.Log("Client has stopped");

        NetworkWorldState oldWorldState = FindObjectOfType<NetworkWorldState>();
        if (oldWorldState)
        {
            Destroy(oldWorldState);
        }
    }


    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");
    }
    //~ End client callbacks
}