using UnityEngine;
using UnityEngine.UI;

public class JigsawHUD : MonoBehaviour
{
    public GameObject HostHUD;
    public Text HostIPText;


    public void Awake()
    {
        if (StaticJigsawData.IsHost)
        {
            EnableHostHUD();
        }
    }


    public void EnableHostHUD()
    {
        HostHUD.SetActive(true);
        HostIPText.text = "Lobby ID: " + NetworkUtils.IPv4toHex(NetworkUtils.GetPublicIPAddress());
    }
}
