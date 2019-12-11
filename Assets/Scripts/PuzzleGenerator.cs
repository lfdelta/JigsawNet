using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PuzzleMeshRandomizer))]
public class PuzzleGenerator : MonoBehaviour
{
    public string ImageFileDiskLocation;
    public GameObject PuzzlePiecePrefab;
    public uint GridWidth = 1;
    public uint GridHeight = 1;

    private TextureLoader Loader;
    private PuzzleMeshRandomizer PuzzleRandomizer;


    void Start()
    {
        Loader = (TextureLoader)gameObject.AddComponent(typeof(TextureLoader));
        Loader.OnTextureLoaded += HandleOnTextureLoaded;
        Loader.RequestTexture(ImageFileDiskLocation);

        PuzzleRandomizer = GetComponent<PuzzleMeshRandomizer>();
    }


    void HandleOnTextureLoaded(Texture2D PuzzleTexture)
    {
        Loader.OnTextureLoaded -= HandleOnTextureLoaded;
        PuzzlePiece[] pieceArr = new PuzzlePiece[GridWidth * GridHeight];

        // TODO: choose a different random seed
        PuzzleRandomizer.InitializeRandomizer(0, GridWidth, GridHeight);

        // Compute the scale and offset, cutting off edges from the texture as needed (rather than letterboxing)
        float wScale = (float)PuzzleTexture.width / (float)GridWidth;
        float hScale = (float)PuzzleTexture.height / (float)GridHeight;
        float pieceScale;
        Vector2 puzzleRoot;
        // Texture UV coords are [0,0] in the lower-left to [1,1] in the upper-right
        if (wScale < hScale)
        {
            pieceScale = (1.0f / (float)GridWidth);
            puzzleRoot = new Vector2(0.0f, 0.5f * (1 - GridHeight * pieceScale));
        }
        else
        {
            pieceScale = (1.0f / (float)GridHeight);
            puzzleRoot = new Vector2(0.5f * (1 - GridWidth * pieceScale), 0.0f);
        }

        // Spawn and initialize puzzle pieces
        for (uint y = 0; y < GridHeight; ++y)
        {
            for (uint x = 0; x < GridWidth; ++x)
            {
                Quaternion spawnRot = new Quaternion();
                spawnRot.eulerAngles = new Vector3(270, 0, 0);

                PuzzlePiece piece = Instantiate(PuzzlePiecePrefab, Vector3.zero, spawnRot).GetComponent<PuzzlePiece>();
                PuzzleRandomizer.SetupPiece(ref piece, x, y);
                piece.SetMaterialInfo(PuzzleTexture, pieceScale, puzzleRoot.x + x * pieceScale, puzzleRoot.y + y * pieceScale);
                pieceArr[y * GridWidth + x] = piece;
            }
        }

        AlignPiecesNeatly(Vector3.zero, pieceArr);
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
