﻿using System;
using UnityEngine;
using UnityEngine.UI;
using GracesGames.SimpleFileBrowser.Scripts;

public class JigsawFileBrowser : MonoBehaviour {

    public GameObject FileBrowserPrefab;
    public string[] FileExtensions;
    public Text OutputField;
    public RawImage OutputImage;

    private TextureLoader Loader;


    private void Start()
    {
        Loader = (TextureLoader)gameObject.AddComponent(typeof(TextureLoader));
    }


    public void OpenFileBrowser()
    {
        OpenFileBrowser(FileBrowserMode.Load);
    }


    // Open a file browser to load files
    private void OpenFileBrowser(FileBrowserMode fileBrowserMode)
    {
        // Create the file browser and name it
        GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
        fileBrowserObject.name = "FileBrowser_Transient";

        // Set the mode to save or load
        FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
        fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);

        // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
        fileBrowserScript.OpenFilePanel(FileExtensions);
        fileBrowserScript.OnFileSelect += LoadFileUsingPath;
    }


    // Loads a file using a path
    private void LoadFileUsingPath(string Path)
    {
        if (Path.Length != 0)
        {
            OutputField.text = Path;
            Loader.OnTextureLoaded += HandleOnTextureLoaded;
            Loader.RequestTexture("file://" + Path);
        }
        else
        {
            Debug.Log("LoadFileUsingPath: empty path given");
        }
    }


    private void HandleOnTextureLoaded(Texture2D LoadedTexture)
    {
        OutputImage.texture = LoadedTexture;
        StaticJigsawData.PuzzleTexture = LoadedTexture;
        Loader.OnTextureLoaded -= HandleOnTextureLoaded;
    }
}