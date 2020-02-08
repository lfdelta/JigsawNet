using UnityEngine;

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
                netManager.StopClient();
            }
        }
    }
}