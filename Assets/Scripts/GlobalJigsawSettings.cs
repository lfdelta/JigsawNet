using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalJigsawSettings : MonoBehaviour
{
    public float PuzzlePieceRestHeight = 0.0f;

    public float PuzzlePieceSelectedHeight = 0.2f;

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

        MouseWorldHeight = PuzzlePieceSelectedHeight + 0.2f;
    }
}
