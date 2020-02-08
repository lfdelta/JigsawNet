using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TogglableHUD
{
    public GameObject VisibleHUD;
    public GameObject HiddenHUD;
    public KeyCode ToggleKey;
    public bool StartOpen;

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

    public void FullyHide()
    {
        VisibleHUD.SetActive(false);
        HiddenHUD.SetActive(false);
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
            h.SetVisible(h.StartOpen);
        }

        if (StaticJigsawData.IsHost)
        {
            HostIPText.text = "Lobby ID: " + NetworkUtils.IPv4toHex(NetworkUtils.GetPublicIPAddress());
            foreach (TogglableHUD h in HostToggles)
            {
                h.SetVisible(h.StartOpen);
            }
        }
        else
        {
            foreach (TogglableHUD h in HostToggles)
            {
                h.FullyHide();
            }
        }
    }
}
