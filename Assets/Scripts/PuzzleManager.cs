﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzleManager : MonoBehaviour
{
    public GameObject PuzzlePiecePrefab;
    public uint GridWidth = 2;
    public uint GridHeight = 2;

    private PuzzleMeshRandomizer PuzzleRandomizer;
    private PuzzlePiece[] Pieces;
    private int LastAssignedPlayerId = -1;


    void Start()
    {
        if (StaticJigsawData.PuzzleWidth >= 2)
        {
            GridWidth = StaticJigsawData.PuzzleWidth;
        }
        if (StaticJigsawData.PuzzleHeight >= 2)
        {
            GridHeight = StaticJigsawData.PuzzleHeight;
        }

        PuzzleRandomizer = (PuzzleMeshRandomizer)FindObjectOfType(typeof(PuzzleMeshRandomizer));
        GeneratePuzzlePieces(StaticJigsawData.PuzzleTexture);
    }


    public int GetNextPlayerId()
    {
        ++LastAssignedPlayerId;
        return LastAssignedPlayerId;
    }


    public PuzzlePiece GetPiece(int Id)
    {
        if (Id < 0 || Id >= Pieces.Length)
        {
            return null;
        }
        return Pieces[Id];
    }


    void GeneratePuzzlePieces(Texture2D PuzzleTexture)
    {
        if (Pieces != null)
        {
            Debug.LogWarning("GeneratePuzzlePieces was called after Pieces array was already initialized");
        }
        Pieces = new PuzzlePiece[GridWidth * GridHeight];

        // TODO: choose a different random seed, possibly from user input
        PuzzleRandomizer.InitializeRandomizer(0, GridWidth, GridHeight);

        // Compute the scale and offset, cutting off edges from the texture as needed (rather than letterboxing)
        // Texture UV coords are [0,0] in the lower-left to [1,1] in the upper-right
        float pieceScaleX;
        float pieceScaleY;
        Vector2 puzzleRootUV;
        float wScale = (float)PuzzleTexture.width / (float)GridWidth;
        float hScale = (float)PuzzleTexture.height / (float)GridHeight;
        if (wScale < hScale)
        {
            pieceScaleX = 1.0f / (float)GridWidth;
            pieceScaleY = ((float)PuzzleTexture.width / (float)(PuzzleTexture.height)) * pieceScaleX;
            puzzleRootUV = new Vector2(0.0f, 0.5f * (1.0f - GridHeight * pieceScaleY)); // TODO
        }
        else
        {
            pieceScaleY = 1.0f / (float)GridHeight;
            pieceScaleX = ((float)PuzzleTexture.height / (float)(PuzzleTexture.width)) * pieceScaleY;
            puzzleRootUV = new Vector2(0.5f * (1.0f - GridWidth * pieceScaleX), 0.0f); // TODO
        }

        // Spawn and initialize puzzle pieces
        for (uint y = 0; y < GridHeight; ++y)
        {
            for (uint x = 0; x < GridWidth; ++x)
            {
                int ind = (int)(y * GridWidth + x);
                Quaternion spawnRot = new Quaternion();
                spawnRot.eulerAngles = new Vector3(270, 0, 0);

                PuzzlePiece piece = Instantiate(PuzzlePiecePrefab, Vector3.zero, spawnRot).GetComponent<PuzzlePiece>();
                PuzzleRandomizer.SetupPiece(ref piece, x, y);
                piece.SetMaterialInfo(PuzzleTexture, pieceScaleX, pieceScaleY, puzzleRootUV.x + x * pieceScaleX, puzzleRootUV.y + y * pieceScaleY);
                piece.SetId(ind);
                Pieces[ind] = piece;
            }
        }

        AlignPiecesNeatly(Vector3.zero, Pieces);
        foreach (PuzzlePiece piece in Pieces)
        {
            NetworkServer.Spawn(piece.gameObject);
        }
    }


    void AlignPiecesNeatly(Vector3 PuzzleWorldOrigin, PuzzlePiece[] PuzzlePieces, float TilingDistance = 1.0f)
    {
        if (PuzzlePieces.Length != GridWidth * GridHeight)
        {
            Debug.LogError("PuzzleGenerator::AlignPiecesNeatly given array of size " + PuzzlePieces.Length.ToString() + " but has GridWidth " + GridWidth.ToString() + " and GridHeight " + GridHeight.ToString());
            return;
        }

        for (int y = 0; y < GridHeight; ++y)
        {
            for (int x = 0; x < GridWidth; ++x)
            {
                PuzzlePieces[y * GridWidth + x].transform.position = PuzzleWorldOrigin + new Vector3(TilingDistance * x, 0, TilingDistance * y);
            }
        }
    }
}
