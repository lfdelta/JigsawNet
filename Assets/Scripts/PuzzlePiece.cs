using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    // Updates mesh, rotates UV coordinates, and updates world rotation to match
    public void SetMesh(Mesh PuzzleMesh, float CW_Rotation = 0.0f)
    {
        MeshFilter meshComp = GetComponent<MeshFilter>();
        meshComp.mesh = PuzzleMesh;

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
            transform.rotation = Quaternion.AngleAxis(CW_Rotation, Vector3.up) * transform.rotation;
        }
    }


    public void SetMaterialInfo(Texture2D PuzzleTexture, float Scale, float OffsetX, float OffsetY)
    {
        Material PuzzleMat = GetComponent<MeshRenderer>().material;
        PuzzleMat.SetTexture("_MainTex", PuzzleTexture);
        PuzzleMat.SetFloat("_Scale", Scale);
        PuzzleMat.SetFloat("_OffsetX", OffsetX);
        PuzzleMat.SetFloat("_OffsetY", OffsetY);
    }
}
