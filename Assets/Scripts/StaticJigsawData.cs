using UnityEngine;

public static class StaticJigsawData
{
    public static Texture2D PuzzleTexture;

    public static uint PuzzleWidth = 0;
    public static uint PuzzleHeight = 0;

    public static bool IsHost = false;

    public static string HostPublicIP = "";

    public static string LocalPlayerName = "";

    public static ErrorDisplayer ErrorHUD;

    public static UniqueObjectManager ObjectManager;
}