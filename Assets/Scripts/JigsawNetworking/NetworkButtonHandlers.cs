using UnityEngine;

/*
 * NetworkButtonHandlers - A layer of indirection between UI input handlers and actual game-side response.
 *                         Exists to prevent losing references (causing non-functional inputs) when changing scenes.
 */
public class NetworkButtonHandlers : MonoBehaviour
{
    public void StartClient()
    {
        JigsawNetworkManager netManager = FindObjectOfType<JigsawNetworkManager>();
        netManager.StartJigsawClient();
    }


    public void StartHost()
    {
        JigsawNetworkManager netManager = FindObjectOfType<JigsawNetworkManager>();
        netManager.StartJigsawHost();
    }


    public void Disconnect()
    {
        JigsawNetworkManager netManager = FindObjectOfType<JigsawNetworkManager>();
        if (StaticJigsawData.IsHost)
        {
            if (netManager.IsClientConnected())
            {
                netManager.StopHost();
            }
        }
        else
        {
            if (netManager.IsClientConnected())
            {
                StaticJigsawData.PuzzleTexture = null;
                netManager.StopClient();
            }
        }
    }
}