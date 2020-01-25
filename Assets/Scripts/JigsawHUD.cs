using UnityEngine;
using UnityEngine.UI;

public class JigsawHUD : MonoBehaviour
{
    public GameObject HostHUD;
    public GameObject HostHUDHiddenTab;
    public Text HostIPText;


    public void Awake()
    {
        if (StaticJigsawData.IsHost)
        {
            EnableHostHUD();
        }
    }


    public void ToggleHostHUD()
    {
        if (HostHUD.activeInHierarchy)
        {
            HideHostHUD();
        }
        else
        {
            EnableHostHUD();
        }
    }


    private void EnableHostHUD()
    {
        HostHUDHiddenTab.SetActive(false);
        HostHUD.SetActive(true);
        HostIPText.text = "Lobby ID: " + NetworkUtils.IPv4toHex(NetworkUtils.GetPublicIPAddress());
    }


    private void HideHostHUD()
    {
        HostHUD.SetActive(false);
        HostHUDHiddenTab.SetActive(true);
    }
}
