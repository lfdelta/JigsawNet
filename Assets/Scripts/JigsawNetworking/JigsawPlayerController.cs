using UnityEngine;
using UnityEngine.Networking;

public struct JigsawPlayerState
{
    public uint Id;
    public string Username;
    public Color UserColor;
}


public class JigsawPlayerController : NetworkBehaviour
{
    [HideInInspector]
    [SyncVar]
    public JigsawPlayerState PlayerState;

    [SyncVar(hook = "OnRepSelectedPieceId")]
    private int SelectedPieceId = -1;
    private int LastSelectedPieceId = -1;

    [SyncVar]
    private int PlayerId = -1;

    private ServerPuzzleManager PuzzleManagerS;

    private Vector3 LastMousePosition;
    private Vector3 LastMouseWorldPosition;

    private ClientPuzzleManager PuzzleManagerC;
    private JigsawHUD HUD;


    public override void OnStartServer()
    {
        base.OnStartServer();
        PuzzleManagerS = FindObjectOfType<ServerPuzzleManager>();
        PlayerId = PuzzleManagerS.GetNextPlayerId();
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        StaticJigsawData.ObjectManager.RequestObject("ClientPuzzleManager", ReceiveClientPuzzleManager);
    }


    private void ReceiveClientPuzzleManager(GameObject Manager)
    {
        PuzzleManagerC = Manager.GetComponent<ClientPuzzleManager>();
        OnRepSelectedPieceId(SelectedPieceId);
    }


    public override void OnStartLocalPlayer()
    {
        Debug.Log("Local player started!");

        HUD = FindObjectOfType<JigsawHUD>();
    }


    // Should be called on the server when this player disconnects
    public void CleanupPlayer()
    {
        // Deselect any selected puzzle pieces
        if (SelectedPieceId >= 0)
        {
            PuzzlePiece piece = PuzzleManagerS.GetPiece(SelectedPieceId);
            if (piece != null && piece.PlayerControllerId == PlayerId)
            {
                piece.Deselected();
            }
            SelectedPieceId = -1;
        }
    }


    void Update()
    {
        if (isLocalPlayer)
        {
            ClientUpdate();

            // HUD controls
            foreach (TogglableHUD h in HUD.AvailableToggles)
            {
                if (Input.GetKeyDown(h.ToggleKey))
                {
                    h.ToggleVisible();
                }
            }

            if (isServer)
            {
                HostUpdate();
            }
        }
    }


    [Command]
    void CmdDebugRotateAllPieces(float Yaw)
    {
        PuzzlePiece[] pieces = (PuzzlePiece[])FindObjectsOfType<PuzzlePiece>();
        foreach(PuzzlePiece p in pieces)
        {
            RotatePiece(p, Yaw);
        }
    }


    [Command]
    void CmdSelectPuzzlePiece(int Id)
    {
        if (SelectedPieceId >= 0)
        {
            return;
        }
        PuzzlePiece piece = PuzzleManagerS.GetPiece(Id);
        if (piece != null && piece.PlayerControllerId < 0)
        {
            piece.SelectedBy(PlayerId);
            SelectedPieceId = Id;
        }
    }


    [Command]
    void CmdDeselectPuzzlePiece()
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = PuzzleManagerS.GetPiece(SelectedPieceId);
        if (piece != null && piece.PlayerControllerId == PlayerId)
        {
            piece.Deselected();
        }
        SelectedPieceId = -1;
    }


    [Command]
    void CmdMouseDrag(float WorldDeltaX, float WorldDeltaZ)
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = PuzzleManagerS.GetPiece(SelectedPieceId);
        if (piece != null)
        {
            piece.transform.position = piece.transform.position + new Vector3(WorldDeltaX, 0, WorldDeltaZ);
        }

        // TODO: clamp position to defined game board boundary
    }


    void RotatePiece(PuzzlePiece Piece, float Angle)
    {
        Piece.transform.Rotate(Vector3.up, Angle, Space.World);
    }


    [Command]
    void CmdRotateLeft()
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = PuzzleManagerS.GetPiece(SelectedPieceId);
        if (piece != null && piece.PlayerControllerId == PlayerId)
        {
            RotatePiece(piece, -90.0f);
        }
    }


    [Command]
    void CmdRotateRight()
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = PuzzleManagerS.GetPiece(SelectedPieceId);
        if (piece != null && piece.PlayerControllerId == PlayerId)
        {
            RotatePiece(piece, 90.0f);
        }
    }


    // Handle client inputs, including local controls and RPC events
    private void ClientUpdate()
    {
        // Puzzle piece interactions
        if (Input.GetButtonDown("Select"))
        {
            RaycastHit hitInfo;
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out hitInfo))
            {
                PuzzlePiece piece = hitInfo.transform.gameObject.GetComponent<PuzzlePiece>();
                if (piece != null)
                {
                    CmdSelectPuzzlePiece(piece.GetId());
                    LastMousePosition = Input.mousePosition;
                    LastMouseWorldPosition = mouseRay.origin + ((GlobalJigsawSettings.Get().MouseWorldHeight - mouseRay.origin.y) / mouseRay.direction.y) * mouseRay.direction;
                }
            }
        }
        else if (Input.GetButtonUp("Select"))
        {
            CmdDeselectPuzzlePiece();
        }
        else if (Input.GetButton("Select") && SelectedPieceId >= 0)
        {
            // TODO: test to verify correctness in case of delayed SelectedPieceId syncVar
            // Handle mouse drag
            if (Input.mousePosition != LastMousePosition)
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 mouseWorld = mouseRay.origin + ((GlobalJigsawSettings.Get().MouseWorldHeight - mouseRay.origin.y) / mouseRay.direction.y) * mouseRay.direction;
                Vector3 diff = mouseWorld - LastMouseWorldPosition;
                LastMousePosition = Input.mousePosition;
                LastMouseWorldPosition = mouseWorld;
                CmdMouseDrag(diff.x, diff.z);
            }

            // Handle piece rotation
            if (Input.GetButtonDown("RotateLeft"))
            {
                CmdRotateLeft();
            }
            else if (Input.GetButtonDown("RotateRight"))
            {
                CmdRotateRight();
            }
        }

        // DEBUG commands below
        //if (Input.GetKeyDown(KeyCode.RightBracket))
        //{
        //    CmdDebugRotateAllPieces(10.0f);
        //}
        //else if (Input.GetKeyDown(KeyCode.LeftBracket))
        //{
        //    CmdDebugRotateAllPieces(-10.0f);
        //}
        //else if (Input.GetKeyDown(KeyCode.P))
        //{
        //    // Temp unit tests for IP conversion
        //    string ipv4 = NetworkUtils.GetPublicIPAddress();
        //    string hex = NetworkUtils.IPv4toHex(ipv4);
        //    Debug.Log(ipv4 + " -> " + hex + " -> " + NetworkUtils.HexToIPv4(hex));

        //    ipv4 = "0.0.0.0";
        //    hex = NetworkUtils.IPv4toHex(ipv4);
        //    Debug.Log(ipv4 + " -> " + hex + " -> " + NetworkUtils.HexToIPv4(hex));

        //    ipv4 = "14.0.255.127";
        //    hex = NetworkUtils.IPv4toHex(ipv4);
        //    Debug.Log(ipv4 + " -> " + hex + " -> " + NetworkUtils.HexToIPv4(hex));
        //}
    }


    void HostUpdate()
    {
        // Empty for now
    }


    public void OnRepSelectedPieceId(int NewSelectedPieceId)
    {
        if (LastSelectedPieceId >= 0)
        {
            PuzzlePiece piece = PuzzleManagerC.GetPiece(LastSelectedPieceId);
            if (piece != null)
            {
                piece.DisableOutline();
            }
        }
        if (NewSelectedPieceId >= 0)
        {
            PuzzlePiece piece = PuzzleManagerC.GetPiece(NewSelectedPieceId);
            if (piece != null)
            {
                piece.EnableOutline(PlayerState.UserColor);
            }
            else
            {
                PuzzleManagerC.RequestPiece(NewSelectedPieceId, PieceRegisteredCallback);
            }
        }
        SelectedPieceId = NewSelectedPieceId;
        LastSelectedPieceId = NewSelectedPieceId;
    }


    private void PieceRegisteredCallback(PuzzlePiece Piece)
    {
        if (SelectedPieceId == Piece.GetId())
        {
            Piece.EnableOutline(PlayerState.UserColor);
        }
    }
}
