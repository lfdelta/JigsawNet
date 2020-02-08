using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzleManager : MonoBehaviour
{
    public GameObject PuzzlePiecePrefab;
    public uint GridWidth = 2;
    public uint GridHeight = 2;

    public float PuzzleSpaceGridScale = 1.2f; // Scaling factor applied to (GridWidth, GridHeight) to determine board size
    public float SpawnSpaceGridScale = 1.8f;  // Scaling factor applied to (GridWidth, GridHeight) to determine puzzle piece spawning region


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
        FindObjectOfType<BoardScalingHandler>().SetBoardDimensions(new Vector2(PuzzleSpaceGridScale * GridWidth, PuzzleSpaceGridScale * GridHeight));
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

        GlobalJigsawSettings settings = FindObjectOfType<GlobalJigsawSettings>();
        Rect puzzleRegion = new Rect(
            -0.5f * PuzzleSpaceGridScale * GridWidth,
            -0.5f * PuzzleSpaceGridScale * GridHeight,
            PuzzleSpaceGridScale * GridWidth,
            PuzzleSpaceGridScale * GridHeight);
        float minX = Mathf.Max(settings.PuzzleBoardBoundsX.x, -SpawnSpaceGridScale * GridWidth);
        float maxX = Mathf.Min(settings.PuzzleBoardBoundsX.y, SpawnSpaceGridScale * GridWidth);
        float minZ = Mathf.Max(settings.PuzzleBoardBoundsZ.x, -SpawnSpaceGridScale * GridHeight);
        float maxZ = Mathf.Min(settings.PuzzleBoardBoundsZ.y, SpawnSpaceGridScale * GridHeight);
        Rect playRegion = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
        ScatterPiecesRandomly(puzzleRegion, playRegion, Pieces);

        //AlignPiecesNeatly(new Vector3(-0.5f * GridWidth, 0.0f, -0.5f * GridHeight), Pieces);
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


    // Places PuzzlePieces randomly throughout the area defined by TotalPlayRegion xor PuzzleBoardRegion, assuming TotalPlayRegion is a strict superset of PuzzleBoardRegion
    void ScatterPiecesRandomly(Rect PuzzleBoardRegion, Rect TotalPlayRegion, PuzzlePiece[] PuzzlePieces)
    {
        // Divide the spawnable region into four rects, weighted by area
        Rect bottomRegion = new Rect(TotalPlayRegion.xMin, TotalPlayRegion.yMin, TotalPlayRegion.width, PuzzleBoardRegion.yMin - TotalPlayRegion.yMin);
        Rect topRegion = new Rect(TotalPlayRegion.xMin, PuzzleBoardRegion.yMax, TotalPlayRegion.width, TotalPlayRegion.yMax - PuzzleBoardRegion.yMax);
        Rect leftRegion = new Rect(TotalPlayRegion.xMin, PuzzleBoardRegion.yMin, PuzzleBoardRegion.xMin - TotalPlayRegion.xMin, PuzzleBoardRegion.height);
        Rect rightRegion = new Rect(PuzzleBoardRegion.xMax, PuzzleBoardRegion.yMin, TotalPlayRegion.xMax - PuzzleBoardRegion.xMax, PuzzleBoardRegion.height);

        Rect[] regions = { bottomRegion, topRegion, leftRegion, rightRegion };
        float[] weightedAreas = { 0.0f, 0.0f, 0.0f, 0.0f };

        float totalArea = 0.0f;
        for (int i = 0; i < regions.Length; ++i)
        {
            totalArea += regions[i].size.x * regions[i].size.y;
            weightedAreas[i] = totalArea;
        }
        for (int i = 0; i < weightedAreas.Length - 1; ++i)
        {
            weightedAreas[i] /= totalArea;
        }
        weightedAreas[weightedAreas.Length - 1] = 1.0f;

        // Assign each puzzle piece to a spawnable subregion, and then to a position within that subregion
        foreach (PuzzlePiece piece in PuzzlePieces)
        {
            float rand = Random.value;
            int i = 0;
            while (weightedAreas[i] < rand)
            {
                ++i;
            }
            Rect spawnRegion = regions[i];

            Vector2 offset = spawnRegion.size;
            offset.Scale(new Vector2(Random.value, Random.value));
            Vector2 spawnPos = spawnRegion.position + offset;

            piece.transform.position = new Vector3(spawnPos.x, 0.0f, spawnPos.y);
            piece.transform.Rotate(Vector3.up, 90.0f * Random.Range(0, 4), Space.World);
        }
    }
}
