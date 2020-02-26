using UnityEngine;
using UnityEngine.Networking;

public class PuzzlePiece : NetworkBehaviour
{
    [HideInInspector]
    [SyncVar(hook = "OnChangePlayerController")]
    public int PlayerControllerId = -1; // The owning player, if any

    [SyncVar]
    private int Id = -1; // Index into the server's PuzzleManager array
    
    [SyncVar]
    private PuzzleMeshRandomizer.PuzzleShape ShapeEnum;

    [SyncVar]
    private float Rotation;
    
    [SyncVar]
    private float MatScaleX;

    [SyncVar]
    private float MatScaleY;

    [SyncVar]
    private float MatOffsetX;

    [SyncVar]
    private float MatOffsetY;

    private Rigidbody Rbody;
    private Material OutlineMat;
    private NetworkWorldState NetWorldState;


    // Updates mesh, rotates UV coordinates, and updates world rotation to match
    public void SetMesh(PuzzleMeshRandomizer.PuzzleShape Shape, float CW_Rotation = 0.0f)
    {
        ShapeEnum = Shape;
        Rotation = CW_Rotation;
        InternalSetMesh(PuzzleMeshRandomizer.GetPuzzleMesh(Shape), CW_Rotation);
        transform.rotation = Quaternion.AngleAxis(CW_Rotation, Vector3.up) * transform.rotation;
    }


    public int GetId()
    {
        return Id;
    }


    public void SetId(int ID)
    {
        Id = ID;
        if (Id < 0)
        {
            Debug.LogErrorFormat("PuzzlePiece assigned a negative Id %d", Id);
        }
    }


    public void SelectedBy(int PlayerId)
    {
        PlayerControllerId = PlayerId;

        // Round yaw to the nearest 90 degrees
        float yaw = transform.eulerAngles.y;
        float finalYaw = Mathf.Round(yaw / 90.0f) * 90.0f;
        if (yaw != finalYaw)
        {
            transform.Rotate(Vector3.up, finalYaw - yaw, Space.World);
        }

        // Set the height appropriately
        Vector3 pos = transform.position;
        pos.y = GlobalJigsawSettings.Get().PuzzlePieceSelectedHeight;
        transform.position = pos;

        Rbody.useGravity = false;
    }


    public void Deselected()
    {
        PlayerControllerId = -1;

        // Snap to XZ grid
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x);
        pos.z = Mathf.Round(pos.z);
        transform.position = pos;

        Rbody.useGravity = true;
    }


    private void InternalSetMesh(Mesh PieceMesh, float CW_Rotation)
    {
        MeshFilter meshComp = GetComponent<MeshFilter>();
        meshComp.mesh = PieceMesh;

        if (CW_Rotation != 0.0f)
        {
            // Wrestle with the type coersion system >:(
            Vector2 center2D = new Vector2(0.5f, 0.5f);
            Vector3 center3D = new Vector3(0.5f, 0.5f, 0.0f);

            Quaternion rotation = Quaternion.AngleAxis(-CW_Rotation, Vector3.forward);
            Vector2[] rotatedUVs = meshComp.mesh.uv;
            for (int i = 0; i < rotatedUVs.Length; ++i)
            {
                rotatedUVs[i] = rotation * (rotatedUVs[i] - center2D) + center3D;
            }
            meshComp.mesh.uv = rotatedUVs;
        }
    }


    public void SetMaterialInfo(Texture2D PuzzleTexture, float ScaleX, float ScaleY, float OffsetX, float OffsetY)
    {
        MatScaleX = ScaleX;
        MatScaleY = ScaleY;
        MatOffsetX = OffsetX;
        MatOffsetY = OffsetY;
        InternalSetMaterialInfo(PuzzleTexture, ScaleX, ScaleY, OffsetX, OffsetY);
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (StaticJigsawData.PuzzleTexture)
        {
            ClientSetPuzzleTexture(StaticJigsawData.PuzzleTexture);
        }
        Rbody = GetComponent<Rigidbody>();
        Rbody.useGravity = (PlayerControllerId < 0);

        OutlineMat = GetComponent<MeshRenderer>().materials[1];

        StaticJigsawData.ObjectManager.RequestObject("NetworkWorldState", ReceiveNetworkWorldState);
    }


    private void ReceiveNetworkWorldState(GameObject Object)
    {
        NetWorldState = Object.GetComponent<NetworkWorldState>();
        if (PlayerControllerId >= 0)
        {
            OnChangePlayerController(PlayerControllerId);
        }
    }


    public void ClientSetPuzzleTexture(Texture2D PuzzleTexture)
    {
        InternalSetMesh(PuzzleMeshRandomizer.GetPuzzleMesh(ShapeEnum), Rotation);
        InternalSetMaterialInfo(PuzzleTexture, MatScaleX, MatScaleY, MatOffsetX, MatOffsetY);
    }


    private void InternalSetMaterialInfo(Texture2D PuzzleTexture, float ScaleX, float ScaleY, float OffsetX, float OffsetY)
    {
        Material PuzzleMat = GetComponent<MeshRenderer>().material;
        PuzzleMat.SetTexture("_MainTex", PuzzleTexture);
        PuzzleMat.SetFloat("_ScaleX", ScaleX);
        PuzzleMat.SetFloat("_ScaleY", ScaleY);
        PuzzleMat.SetFloat("_OffsetX", OffsetX);
        PuzzleMat.SetFloat("_OffsetY", OffsetY);
    }


    private void Update()
    {
        if (isServer)
        {
            GlobalJigsawSettings settings = GlobalJigsawSettings.Get();
            float newX = Mathf.Clamp(transform.position.x, settings.PuzzleBoardBoundsX.x, settings.PuzzleBoardBoundsX.y);
            float newY = Mathf.Clamp(transform.position.y, settings.PuzzleBoardBoundsY.x, settings.PuzzleBoardBoundsY.y);
            float newZ = Mathf.Clamp(transform.position.z, settings.PuzzleBoardBoundsZ.x, settings.PuzzleBoardBoundsZ.y);
            if (transform.position.x != newX || transform.position.y != newY || transform.position.z != newZ)
            {
                transform.position = new Vector3(newX, newY, newZ);
            }
        }
    }


    public void OnChangePlayerController(int PlayerId)
    {
        if (PlayerId >= 0)
        {
            Rbody.useGravity = false;
            if (NetWorldState)
            {
                if (NetWorldState.ConnectedPlayers.Count > PlayerId)
                {
                    EnableOutline(NetWorldState.ConnectedPlayers[PlayerId].UserColor);
                }
                else
                {
                    Debug.LogErrorFormat("PuzzlePiece.OnChangePlayerController({0}) found ConnectedPlayers with length {1}", PlayerId, NetWorldState.ConnectedPlayers.Count);
                }
            }
        }
        else
        {
            DisableOutline();
            Rbody.useGravity = true;
        }
        PlayerControllerId = PlayerId;
    }


    private void EnableOutline(Color PlayerColor)
    {
        OutlineMat.SetColor("_Color", PlayerColor);
        OutlineMat.SetFloat("_Alpha", 1.0f);
    }


    private void DisableOutline()
    {
        OutlineMat.SetFloat("_Alpha", 0.0f);
    }
}
