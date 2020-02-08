using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JigsawMenu : MonoBehaviour
{
    public Slider PuzzleWidthSlider;
    public Slider PuzzleHeightSlider;
    public InputField HostAddressInput;
    public InputField PlayerNameInput;

    public DynamicTexturePreview TexturePreview;
    //public GridOverlay TextureOverlay;
    public ErrorDisplayer ErrorHUD;


    private void Start()
    {
        if (StaticJigsawData.LocalPlayerName.Length > 0)
        {
            PlayerNameInput.text = StaticJigsawData.LocalPlayerName;
        }
        if (StaticJigsawData.PuzzleTexture != null)
        {
            TexturePreview.UpdatePreviewImage(StaticJigsawData.PuzzleTexture);
        }
        if (StaticJigsawData.PuzzleWidth > 0 && StaticJigsawData.PuzzleHeight > 0)
        {
            PuzzleWidthSlider.value = StaticJigsawData.PuzzleWidth;
            PuzzleHeightSlider.value = StaticJigsawData.PuzzleHeight;
        }
    }
}
