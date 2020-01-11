using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JigsawPlayerController : NetworkBehaviour
{
    [SyncVar]
    private int SelectedPieceId = -1;

    [SyncVar]
    private int PlayerId = -1;

    private PuzzleManager puzzleManager;


    public override void OnStartServer()
    {
        base.OnStartServer();
        puzzleManager = FindObjectOfType<PuzzleManager>();
        PlayerId = puzzleManager.GetNextPlayerId();
    }


    public override void OnStartLocalPlayer()
    {
        Debug.Log("Local player started!");

        // TODO: attach to camera
    }


    void Update()
    {
        if (isLocalPlayer)
        {
            ClientUpdate();
        }
    }


    [Command]
    void CmdSelectPuzzlePiece(int Id)
    {
        if (SelectedPieceId >= 0)
        {
            return;
        }
        PuzzlePiece piece = puzzleManager.GetPiece(Id);
        if (piece != null && piece.PlayerControllerId < 0)
        {
            piece.PlayerControllerId = PlayerId;
            SelectedPieceId = Id;

            // TODO: set piece height to above the rest
            // TODO: reset pitch and roll; round yaw to nearest 90 degrees
        }
    }


    [Command]
    void CmdMouseDrag(float WorldDeltaX, float WorldDeltaZ)
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = puzzleManager.GetPiece(SelectedPieceId);
        if (piece != null)
        {
            piece.transform.position = piece.transform.position + new Vector3(WorldDeltaX, 0, WorldDeltaZ);
        }
    }


    [Command]
    void CmdDeselectPuzzlePiece()
    {
        if (SelectedPieceId < 0)
        {
            return;
        }
        PuzzlePiece piece = puzzleManager.GetPiece(SelectedPieceId);
        if (piece != null && piece.PlayerControllerId == PlayerId)
        {
            piece.PlayerControllerId = -1;

            // TODO: return to initial height; snap to grid XZ
        }
        SelectedPieceId = -1;
    }


    // Handle client inputs, including local controls and RPC events
    private void ClientUpdate()
    {
        // TODO: local camera controls (pan, rotate, reset to fixed camera orientation)
        
        if (Input.GetButtonDown("Select"))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                PuzzlePiece piece = hitInfo.transform.gameObject.GetComponent<PuzzlePiece>();
                if (piece != null)
                {
                    Debug.Log(piece.GetId());
                    CmdSelectPuzzlePiece(piece.GetId());
                }
            }
        }
        else if (Input.GetButtonUp("Select"))
        {
            CmdDeselectPuzzlePiece();
        }
        else if (Input.GetButton("Select"))
        {
            // TODO: if reasonable, only send updates if our SelectedPieceId is valid (will need testing to verify correctness in case of delayed SyncVar updates)

            float dx = Input.GetAxis("MouseX");
            float dy = Input.GetAxis("MouseY");
            if (dx != 0.0f && dy != 0.0f)
            {
                Debug.LogFormat("[{0}, {1}]", dx, dy);
                // TODO: track absolute Input.mousePosition for better reliability (store in ButtonDown, if changed then send update)
                // TODO: compute world-space XZ updates by projecting screen delta into horizontal plane delta (maybe cast both rays onto y=0 plane and subtract result)
                CmdMouseDrag(dx, dy);
            }

            // TODO: rotate and lock inputs/RPCs
        }
    }
}
