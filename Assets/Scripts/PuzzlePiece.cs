using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzlePiece : NetworkBehaviour
{
    [SyncVar]
    private PuzzleMeshRandomizer.PuzzleShape ShapeEnum;

    [SyncVar]
    private float Rotation;
    
    [SyncVar]
    private float MatScale;

    [SyncVar]
    private float MatOffsetX;

    [SyncVar]
    private float MatOffsetY;


    // Updates mesh, rotates UV coordinates, and updates world rotation to match
    public void SetMesh(PuzzleMeshRandomizer.PuzzleShape Shape, float CW_Rotation = 0.0f)
    {
        ShapeEnum = Shape;
        Rotation = CW_Rotation;
        InternalSetMesh(PuzzleMeshRandomizer.GetPuzzleMesh(Shape), CW_Rotation);
        transform.rotation = Quaternion.AngleAxis(CW_Rotation, Vector3.up) * transform.rotation;
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


    public void SetMaterialInfo(Texture2D PuzzleTexture, float Scale, float OffsetX, float OffsetY)
    {
        MatScale = Scale;
        MatOffsetX = OffsetX;
        MatOffsetY = OffsetY;
        InternalSetMaterialInfo(PuzzleTexture, Scale, OffsetX, OffsetY);
    }


    public void ClientSetPuzzleTexture(Texture2D PuzzleTexture)
    {
        InternalSetMesh(PuzzleMeshRandomizer.GetPuzzleMesh(ShapeEnum), Rotation);
        InternalSetMaterialInfo(PuzzleTexture, MatScale, MatOffsetX, MatOffsetY);
    }


    private void InternalSetMaterialInfo(Texture2D PuzzleTexture, float Scale, float OffsetX, float OffsetY)
    {
        Material PuzzleMat = GetComponent<MeshRenderer>().material;
        PuzzleMat.SetTexture("_MainTex", PuzzleTexture);
        PuzzleMat.SetFloat("_Scale", Scale);
        PuzzleMat.SetFloat("_OffsetX", OffsetX);
        PuzzleMat.SetFloat("_OffsetY", OffsetY);
    }
}
