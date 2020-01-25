using System;
using UnityEngine;
using UnityEngine.UI;
using GracesGames.SimpleFileBrowser.Scripts;

public class JigsawFileBrowser : MonoBehaviour
{
    public GameObject FileBrowserPrefab;
    public string[] FileExtensions;
    public int MaxFileSize = -1;
    public Text OutputField;
    public DynamicTexturePreview ImagePreview;

    private TextureLoader Loader;


    private void Start()
    {
        Loader = (TextureLoader)gameObject.AddComponent(typeof(TextureLoader));
    }


    public void OpenFileBrowser()
    {
        OpenFileBrowser(FileBrowserMode.Load);
        ImagePreview.EnableGridOverlay(false);
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
        fileBrowserScript.OpenFilePanel(FileExtensions, MaxFileSize);
        fileBrowserScript.OnFileBrowserClose += HandleBrowserClosed;
        fileBrowserScript.OnFileSelect += LoadFileUsingPath;
    }


    private void HandleBrowserClosed()
    {
        ImagePreview.EnableGridOverlay(true);
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
        ImagePreview.EnableGridOverlay(true);
    }


    private void HandleOnTextureLoaded(Texture2D LoadedTexture)
    {
        ImagePreview.UpdatePreviewImage(LoadedTexture);
        Loader.OnTextureLoaded -= HandleOnTextureLoaded;

        // Preprocess the texture to reduce size
        Debug.Log("Initial texture size is " + LoadedTexture.GetRawTextureData().Length.ToString() + " with format " + LoadedTexture.graphicsFormat.ToString());
        LoadedTexture.Compress(true);
        Debug.Log("Compressed texture size is " + LoadedTexture.GetRawTextureData().Length.ToString() + " with format " + LoadedTexture.graphicsFormat.ToString());

        StaticJigsawData.PuzzleTexture = LoadedTexture;
    }
}