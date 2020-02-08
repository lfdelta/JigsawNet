using UnityEngine;

public class ClientDisconnect : MonoBehaviour
{
    public void Disconnect()
    {
        JigsawNetworkManager netManager = FindObjectOfType<JigsawNetworkManager>();
        if (netManager.IsClientConnected())
        {
            netManager.StopClient();
        }
    }
}
