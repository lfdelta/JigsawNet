using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicTexturePreview : MonoBehaviour
{
    public RawImage PreviewImage;
    public GridOverlay Overlay;

    private Rect rect;


    public void Awake()
    {
        rect = GetComponent<RectTransform>().rect;

        // Update Overlay based on initial dimensions
        Overlay.SetTextureScreenSize(JigsawUtilities.RectTransformToScreenSpace(PreviewImage.rectTransform).size);
    }


    public void UpdatePreviewImage(Texture2D PreviewTexture)
    {
        PreviewImage.texture = PreviewTexture;
        float texAspect = (float)PreviewTexture.width / (float)PreviewTexture.height;
        if (texAspect > 1.0f)
        {
            PreviewImage.rectTransform.sizeDelta = new Vector2(rect.width, rect.height / texAspect);
        }
        else
        {
            PreviewImage.rectTransform.sizeDelta = new Vector2(texAspect * rect.width, rect.height);
        }

        // Update Overlay based on new dimensions
        Overlay.SetTextureScreenSize(JigsawUtilities.RectTransformToScreenSpace(PreviewImage.rectTransform).size);
    }


    public void EnableGridOverlay(bool Enabled)
    {
        Overlay.SetEnabled(Enabled);
    }
}
