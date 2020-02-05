using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TogglableHUD
{
    public GameObject VisibleHUD;
    public GameObject HiddenHUD;
    public KeyCode ToggleKey;

    public void SetVisible(bool Visible)
    {
        VisibleHUD.SetActive(Visible);
        HiddenHUD.SetActive(!Visible);
    }

    public void ToggleVisible()
    {
        bool active = VisibleHUD.activeInHierarchy;
        VisibleHUD.SetActive(!active);
        HiddenHUD.SetActive(active);
    }
}


public class JigsawHUD : MonoBehaviour
{
    public GameObject HostHUD;
    public GameObject HostHUDHiddenTab;
    public Text HostIPText;

    public TogglableHUD[] HostToggles;
    public TogglableHUD[] ClientToggles;


    public void Awake()
    {
        foreach(TogglableHUD h in ClientToggles)
        {
            h.SetVisible(true);
        }

        if (StaticJigsawData.IsHost)
        {
            HostIPText.text = "Lobby ID: " + NetworkUtils.IPv4toHex(NetworkUtils.GetPublicIPAddress());
            foreach (TogglableHUD h in HostToggles)
            {
                h.SetVisible(true);
            }
        }
        else
        {
            foreach (TogglableHUD h in HostToggles)
            {
                h.VisibleHUD.SetActive(false);
                h.HiddenHUD.SetActive(false);
            }
        }
    }
}
