﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzlePiece : NetworkBehaviour
{
    [HideInInspector]
    public int PlayerControllerId = -1; // The owning player, only valid on the server

    [HideInInspector]
    [SyncVar(hook = "OnChangeGravity")]
    public bool UseGravity = true;

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
        Rbody.useGravity = UseGravity;
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


    public void OnChangeGravity(bool Gravity)
    {
        Rbody.useGravity = Gravity;
    }
}
