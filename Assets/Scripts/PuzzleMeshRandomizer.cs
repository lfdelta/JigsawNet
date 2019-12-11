using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// NOTE: X increases from left to right; Y increases from bottom to top

public class PuzzleMeshRandomizer : MonoBehaviour
{
    public PuzzleMapping[] MeshLookup;

    private PuzzleMapping[] SortedMeshLookup;
    private bool[,] HorizontalPolarities; // True if pointing rightward
    private bool[,] VerticalPolarities;   // True if pointing downward
    private bool Initialized = false;
    private uint Width;
    private uint Height;


    void Start()
    {
        if (MeshLookup.Length != (uint)PuzzleShape.MAX_VAL)
        {
            Debug.LogError("PuzzleMeshRandomizer given MeshLookup value of size " + MeshLookup.Length.ToString() + "; expected " + ((int)PuzzleShape.MAX_VAL).ToString());
            return;
        }

        // Sort and verify MeshLookup
        SortedMeshLookup = new PuzzleMapping[(uint)PuzzleShape.MAX_VAL];
        foreach (PuzzleMapping map in MeshLookup)
        {
            if (map.PuzzleMesh == null)
            {
                Debug.LogError("PuzzleMeshRandomizer given MeshLookup key " + map.ShapeEnum.ToString() + " with null value");
            }

            uint ind = (uint)map.ShapeEnum;
            if (ind >= (uint)PuzzleShape.MAX_VAL)
            {
                Debug.LogError("PuzzleMeshRandomizer given MeshLookup key " + ((uint)map.ShapeEnum).ToString() + " exceeding MAX_VAL " + ((uint)PuzzleShape.MAX_VAL).ToString());
            }
            else
            {
                if (SortedMeshLookup[ind].PuzzleMesh != null)
                {
                    Debug.LogError("PuzzleMeshRandomizer found duplicate MeshLookup key " + ind.ToString());
                }
                SortedMeshLookup[ind] = map;
            }
        }

        for (uint i = 0; i < (uint)PuzzleShape.MAX_VAL; ++i)
        {
            if ((uint)(SortedMeshLookup[i].ShapeEnum) != i)
            {
                Debug.LogError("PuzzleMeshRandomizer MeshLookup is missing key " + ((PuzzleShape)i).ToString());
            }
        }
    }


    // Generates a randomized pattern of puzzle cuts (e.g. which pieces stick out and which ones have gaps)
    public void InitializeRandomizer(int RandomSeed, uint GridWidth, uint GridHeight)
    {
        // Don't interrupt any other random behaviors occurring before/after this execution
        Random.State oldRandState = Random.state;

        // Fill in arrays with random polarity values
        HorizontalPolarities = new bool[GridWidth - 1, GridHeight];
        VerticalPolarities = new bool[GridWidth, GridHeight - 1];

        Random.InitState(RandomSeed);

        for (uint y = 0; y < GridHeight; ++y)
        {
            for (uint x = 0; x < GridWidth - 1; ++x)
            {
                HorizontalPolarities[x, y] = (Random.Range(0, 2) == 0);
            }
        }
        for (uint y = 0; y < GridHeight - 1; ++y)
        {
            for (uint x = 0; x < GridWidth; ++x)
            {
                VerticalPolarities[x, y] = (Random.Range(0, 2) == 0);
            }
        }

        Random.state = oldRandState;
        Width = GridWidth;
        Height = GridHeight;
        Initialized = true;
    }


    // Sets up the mesh and UVs for a single puzzle piece given its grid position.
    public void SetupPiece(ref PuzzlePiece Piece, uint X, uint Y)
    {
        if (!Initialized)
        {
            Debug.LogError("PuzzleMeshRandomizer::SetupPiece called before InitializeRandomizer was called");
            return;
        }

        float rotationCW = 0.0f;
        PuzzleShape shapeEnum;

        bool isHorizEdge = (X == 0 || X == Width - 1);
        bool isVertEdge = (Y == 0 || Y == Height - 1);
        if (isHorizEdge && isVertEdge)
        {
            shapeEnum = GetCornerShape(X, Y, ref rotationCW);
        }
        else if (isHorizEdge || isVertEdge)
        {
            shapeEnum = GetEdgeShape(X, Y, ref rotationCW);
        }
        else
        {
            shapeEnum = GetFillerShape(X, Y, ref rotationCW);
        }

        Piece.SetMesh(SortedMeshLookup[(uint)shapeEnum].PuzzleMesh, rotationCW);
    }



    // BELOW: Helper functions for assigning enums and rotations from a tile coordinate's polarity values

    PuzzleShape GetCornerShape(uint X, uint Y, ref float RotationCW)
    {
        if (X == 0)
        {
            if (Y == 0)
            {
                // Bottom-left -> _RT
                RotationCW = -90.0f;
                if (HorizontalPolarities[0, 0])
                {
                    return (VerticalPolarities[0, 0]) ? PuzzleShape.TL_Corner_10 : PuzzleShape.TL_Corner_11;
                }
                else
                {
                    return (VerticalPolarities[0, 0]) ? PuzzleShape.TL_Corner_00 : PuzzleShape.TL_Corner_01;
                }
            }
            else
            {
                // Top-left -> _BR
                RotationCW = 0.0f;
                if (VerticalPolarities[0, Height - 2])
                {
                    return (HorizontalPolarities[0, Height - 1]) ? PuzzleShape.TL_Corner_11 : PuzzleShape.TL_Corner_10;
                }
                else
                {
                    return (HorizontalPolarities[0, Height - 1]) ? PuzzleShape.TL_Corner_01 : PuzzleShape.TL_Corner_00;
                }
            }
        }
        else
        {
            if (Y == 0)
            {
                // Bottom-right -> _TL
                RotationCW = 180.0f;
                if (VerticalPolarities[Width - 1, 0])
                {
                    return (HorizontalPolarities[Width - 2, 0]) ? PuzzleShape.TL_Corner_00 : PuzzleShape.TL_Corner_01;
                }
                else
                {
                    return (HorizontalPolarities[Width - 2, 0]) ? PuzzleShape.TL_Corner_10 : PuzzleShape.TL_Corner_11;
                }
            }
            else
            {
                // Top-right -> _LB
                RotationCW = 90.0f;
                if (HorizontalPolarities[Width - 2, Height - 1])
                {
                    return (VerticalPolarities[Width - 1, Height - 2]) ? PuzzleShape.TL_Corner_01 : PuzzleShape.TL_Corner_00;
                }
                else
                {
                    return (VerticalPolarities[Width - 1, Height - 2]) ? PuzzleShape.TL_Corner_11 : PuzzleShape.TL_Corner_10;
                }
            }
        }
    }


    PuzzleShape GetEdgeShape(uint X, uint Y, ref float RotationCW)
    {
        if (X == 0)
        {
            // Left edge -> _BRT
            RotationCW = 0.0f;
            if (VerticalPolarities[0, Y - 1]) // B
            {
                if (HorizontalPolarities[0, Y]) // R
                {
                    return (VerticalPolarities[0, Y]) ? PuzzleShape.L_Edge_110 : PuzzleShape.L_Edge_111;
                }
                else // !R
                {
                    return (VerticalPolarities[0, Y]) ? PuzzleShape.L_Edge_100 : PuzzleShape.L_Edge_101;
                }
            }
            else // !B
            {
                if (HorizontalPolarities[0, Y]) // R
                {
                    return (VerticalPolarities[0, Y]) ? PuzzleShape.L_Edge_010 : PuzzleShape.L_Edge_011;
                }
                else // !R
                {
                    return (VerticalPolarities[0, Y]) ? PuzzleShape.L_Edge_000 : PuzzleShape.L_Edge_001;
                }
            }
        }
        else if (X == Width - 1)
        {
            // Right edge -> _TLB
            RotationCW = 180.0f;
            if (VerticalPolarities[Width - 1, Y]) // T
            {
                if (HorizontalPolarities[Width - 2, Y]) // L
                {
                    return (VerticalPolarities[Width - 1, Y - 1]) ? PuzzleShape.L_Edge_001 : PuzzleShape.L_Edge_000;
                }
                else // !L
                {
                    return (VerticalPolarities[Width - 1, Y - 1]) ? PuzzleShape.L_Edge_011 : PuzzleShape.L_Edge_010;
                }
            }
            else // !T
            {
                if (HorizontalPolarities[Width - 2, Y]) // L
                {
                    return (VerticalPolarities[Width - 1, Y - 1]) ? PuzzleShape.L_Edge_101 : PuzzleShape.L_Edge_100;
                }
                else // !L
                {
                    return (VerticalPolarities[Width - 1, Y - 1]) ? PuzzleShape.L_Edge_111 : PuzzleShape.L_Edge_110;
                }
            }
        }
        else if (Y == 0)
        {
            // Bottom edge -> _RTL
            RotationCW = -90.0f;
            if (HorizontalPolarities[X, 0]) // R
            {
                if (VerticalPolarities[X, 0]) // T
                {
                    return (HorizontalPolarities[X - 1, 0]) ? PuzzleShape.L_Edge_100 : PuzzleShape.L_Edge_101;
                }
                else // !T
                {
                    return (HorizontalPolarities[X - 1, 0]) ? PuzzleShape.L_Edge_110 : PuzzleShape.L_Edge_111;
                }
            }
            else // !R
            {
                if (VerticalPolarities[X, 0]) // T
                {
                    return (HorizontalPolarities[X - 1, 0]) ? PuzzleShape.L_Edge_000 : PuzzleShape.L_Edge_001;
                }
                else // !T
                {
                    return (HorizontalPolarities[X - 1, 0]) ? PuzzleShape.L_Edge_010 : PuzzleShape.L_Edge_011;
                }
            }
        }
        else // Y == Height - 1
        {
            // Top edge -> _LBR
            RotationCW = 90.0f;
            if (HorizontalPolarities[X - 1, Height - 1]) // L
            {
                if (VerticalPolarities[X, Height - 2]) // B
                {
                    return (HorizontalPolarities[X, Height - 1]) ? PuzzleShape.L_Edge_011 : PuzzleShape.L_Edge_010;
                }
                else // !B
                {
                    return (HorizontalPolarities[X, Height - 1]) ? PuzzleShape.L_Edge_001 : PuzzleShape.L_Edge_000;
                }
            }
            else // !L
            {
                if (VerticalPolarities[X, Height - 2]) // B
                {
                    return (HorizontalPolarities[X, Height - 1]) ? PuzzleShape.L_Edge_111 : PuzzleShape.L_Edge_110;
                }
                else // !B
                {
                    return (HorizontalPolarities[X, Height - 1]) ? PuzzleShape.L_Edge_101 : PuzzleShape.L_Edge_100;
                }
            }
        }
    }


    PuzzleShape GetFillerShape(uint X, uint Y, ref float RotationCW)
    {
        // _BRTL
        if (VerticalPolarities[X, Y - 1]) // B
        {
            if (HorizontalPolarities[X, Y]) // R
            {
                if (VerticalPolarities[X, Y]) // T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _1100
                        RotationCW = 180.0f;
                        return PuzzleShape.Center_0011;
                    }
                    else // !L
                    {
                        // _1101
                        RotationCW = 180.0f;
                        return PuzzleShape.Center_0111;
                    }
                }
                else // !T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _1110
                        RotationCW = 90.0f;
                        return PuzzleShape.Center_0111;
                    }
                    else // !L
                    {
                        // _1111
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_1111;
                    }
                }
            }
            else // !R
            {
                if (VerticalPolarities[X, Y]) // T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _1000
                        RotationCW = -90.0f;
                        return PuzzleShape.Center_0001;
                    }
                    else // !L
                    {
                        // _1001
                        RotationCW = -90.0f;
                        return PuzzleShape.Center_0011;
                    }
                }
                else // !T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _1010
                        RotationCW = 90.0f;
                        return PuzzleShape.Center_0101;
                    }
                    else // !L
                    {
                        // _1011
                        RotationCW = -90.0f;
                        return PuzzleShape.Center_0111;
                    }
                }
            }
        }
        else // !B
        {
            if (HorizontalPolarities[X, Y]) // R
            {
                if (VerticalPolarities[X, Y]) // T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _0100
                        RotationCW = 180.0f;
                        return PuzzleShape.Center_0001;
                    }
                    else // !L
                    {
                        // _0101
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_0101;
                    }
                }
                else // !T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _0110
                        RotationCW = 90.0f;
                        return PuzzleShape.Center_0011;
                    }
                    else // !L
                    {
                        // _0111
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_0111;
                    }
                }
            }
            else // !R
            {
                if (VerticalPolarities[X, Y]) // T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _0000
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_0000;
                    }
                    else // !L
                    {
                        // _0001
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_0001;
                    }
                }
                else // !T
                {
                    if (HorizontalPolarities[X - 1, Y]) // L
                    {
                        // _0010
                        RotationCW = 90.0f;
                        return PuzzleShape.Center_0001;
                    }
                    else // !L
                    {
                        // _0011
                        RotationCW = 0.0f;
                        return PuzzleShape.Center_0011;
                    }
                }
            }
        }
    }


    // BELOW: struct definitions for MeshLookup property

    public enum PuzzleShape : uint
    {
        // _BR (0 in, 1 out)
        TL_Corner_00 = 0,
        TL_Corner_01,
        TL_Corner_10,
        TL_Corner_11,

        // BRT (0 in, 1 out)
        L_Edge_000,
        L_Edge_001,
        L_Edge_010,
        L_Edge_011,
        L_Edge_100,
        L_Edge_101,
        L_Edge_110,
        L_Edge_111,

        // _BRTL (0 in, 1 out)
        Center_0000,
        Center_0001,
        Center_0011,
        Center_0101,
        Center_0111,
        Center_1111,

        MAX_VAL
    }

    [System.Serializable]
    public struct PuzzleMapping
    {
        public PuzzleShape ShapeEnum;
        public Mesh PuzzleMesh;
    }
}
