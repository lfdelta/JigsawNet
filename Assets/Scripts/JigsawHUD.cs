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

    public TogglableHUD[] HostOnlyToggles;
    public TogglableHUD[] ClientOnlyToggles;
    public TogglableHUD[] SharedToggles;

    [HideInInspector] public TogglableHUD[] AvailableToggles;
    

    public void Awake()
    {
        int i = 0; // For copying arrays

        if (StaticJigsawData.IsHost)
        {
            AvailableToggles = new TogglableHUD[HostOnlyToggles.Length + SharedToggles.Length];

            HostIPText.text = "Lobby ID: " + NetworkUtils.IPv4toHex(NetworkUtils.GetPublicIPAddress());
            foreach (TogglableHUD h in HostOnlyToggles)
            {
                AvailableToggles[i++] = h;
                h.SetVisible(h.StartOpen);
            }
            foreach (TogglableHUD h in ClientOnlyToggles)
            {
                h.FullyHide();
            }
        }
        else
        {
            AvailableToggles = new TogglableHUD[ClientOnlyToggles.Length + SharedToggles.Length];
            foreach (TogglableHUD h in HostOnlyToggles)
            {
                h.FullyHide();
            }
            foreach (TogglableHUD h in ClientOnlyToggles)
            {
                AvailableToggles[i++] = h;
                h.SetVisible(h.StartOpen);
            }
        }

        foreach (TogglableHUD h in SharedToggles)
        {
            AvailableToggles[i++] = h;
            h.SetVisible(h.StartOpen);
        }
    }
}
