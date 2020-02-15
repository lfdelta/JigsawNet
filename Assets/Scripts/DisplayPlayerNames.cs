using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerNames : MonoBehaviour
{
    public Text DisplayText;
    public RectTransform TextRect;
    public RectTransform PanelRect;

    private NetworkWorldState NetWorldState;
    private float TextRectLineSize;  // Height of a single line
    private float PanelRectBaseSize; // Height with no text


    private void Awake()
    {
        DisplayText.text = "";
        TextRectLineSize = (float)DisplayText.fontSize;
        PanelRectBaseSize = PanelRect.rect.height - TextRectLineSize;
    }


    private void Start()
    {
        StaticJigsawData.ObjectManager.RequestObject("NetworkWorldState", ReceiveNetworkWorldState);
    }

    private void ReceiveNetworkWorldState(GameObject Object)
    {
        NetWorldState = Object.GetComponent<NetworkWorldState>();
        NetWorldState.OnConnectedPlayersUpdated += UpdatePlayerNames;
        UpdatePlayerNames();
    }


    private void UpdatePlayerNames()
    {
        string output = "";
        int len = NetWorldState.ConnectedPlayers.Count;
        for (int i = 0; i < len; ++i)
        {
            JigsawPlayerState player = NetWorldState.ConnectedPlayers[i];
            Color clr = player.UserColor;
            output += string.Format("<color=#{0:X2}{1:X2}{2:X2}FF>{3}</color>{4}",
                (byte)(255.0f * clr.r), (byte)(255.0f * clr.g), (byte)(255.0f * clr.b),
                player.Username,
                (i < len - 1) ? "\n" : "");
        }
        DisplayText.text = output;

        Vector2 textRectSize = TextRect.sizeDelta;
        textRectSize.y = TextRectLineSize * len;
        TextRect.sizeDelta = textRectSize;

        Vector2 panelSize = PanelRect.sizeDelta;
        panelSize.y = PanelRectBaseSize + textRectSize.y;
        PanelRect.sizeDelta = panelSize;
    }
}
