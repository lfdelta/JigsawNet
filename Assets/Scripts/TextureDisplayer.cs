using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureDisplayer : MonoBehaviour
{
    public string ImageFileDiskLocation;

    private bool DoneLoading = false;
    private Texture2D LoadedTexture;
    private TextureLoader Loader;

    void Start()
    {
        Loader = (TextureLoader)gameObject.AddComponent(typeof(TextureLoader));
        Loader.OnTextureLoaded += HandleOnTextureLoaded;
        Loader.RequestTexture(ImageFileDiskLocation);
    }


    void HandleOnTextureLoaded(Texture2D Texture)
    {
        Loader.OnTextureLoaded -= HandleOnTextureLoaded;
        LoadedTexture = Texture;
        DoneLoading = true;
    }


    void OnGUI()
    {
        if (DoneLoading)
        {
            GUI.DrawTexture(Camera.current.pixelRect, LoadedTexture);
        }
    }
}
