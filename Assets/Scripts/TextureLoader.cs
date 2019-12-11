using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TextureLoader : MonoBehaviour
{
    public delegate void TextureLoadDelegate(Texture2D Texture);
    public event TextureLoadDelegate OnTextureLoaded;


    public void RequestTexture(string Filepath)
    {
        StartCoroutine(AsyncLoadTexture(Filepath));
    }


    private IEnumerator AsyncLoadTexture(string Filepath)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(Filepath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log("Loaded texture!");
                OnTextureLoaded(DownloadHandlerTexture.GetContent(uwr));
            }
        }
    }
}