using UnityEngine;

public class GlobalJigsawSettings : MonoBehaviour
{
    public float PuzzlePieceRestHeight = 0.0f;

    public float PuzzlePieceSelectedHeight = 0.2f;

    public Vector2 PuzzleBoardBoundsX = new Vector2(-25.0f, 75.0f);
    public Vector2 PuzzleBoardBoundsY = new Vector2(-25.0f, 75.0f);
    public Vector2 PuzzleBoardBoundsZ = new Vector2(-25.0f, 75.0f);

    // Puzzle piece height when selected, plus puzzle piece thickness.
    [HideInInspector]
    public float MouseWorldHeight;


    private static GlobalJigsawSettings Instance = null;

    public static GlobalJigsawSettings Get()
    {
        return Instance;
    }

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GlobalJigsawSettings was constructed with an instance already in existence.");
            return;
        }
        Instance = this;

        MouseWorldHeight = PuzzlePieceSelectedHeight + 0.2f; // Add the thickness of the puzzle piece mesh
    }
}
