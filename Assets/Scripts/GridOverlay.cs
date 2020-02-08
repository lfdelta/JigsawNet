using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOverlay : MonoBehaviour
{
    public uint GridWidth;  // Number of horizontal cells
    public uint GridHeight; // Number of vertical cells
    public uint TextureScreenWidth;
    public uint TextureScreenHeight;
    public Color OverlayColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);

    public float LineThickness = 1.0f;

    private RectTransform rectTransform;
    private bool DoDraw = true;
    private Color CachedOverlayColor;
    private Texture2D OverlayTexture;


    public void SetEnabled(bool Enabled)
    {
        DoDraw = Enabled;
    }

    public void SetGridWidth(float Width)
    {
        GridWidth = (uint)Width;
    }

    public void SetGridHeight(float Height)
    {
        GridHeight = (uint)Height;
    }

    public void SetTextureScreenSize(Vector2 ScreenDimensions)
    {
        TextureScreenWidth = (uint)ScreenDimensions.x;
        TextureScreenHeight = (uint)ScreenDimensions.y;
    }


    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        OverlayTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        OverlayTexture.SetPixel(0, 0, OverlayColor);
        OverlayTexture.Apply();

        CachedOverlayColor = OverlayColor;
    }


    private void OnGUI()
    {
        if (!DoDraw)
        {
            return;
        }

        if (OverlayColor != CachedOverlayColor)
        {
            OverlayTexture.SetPixel(0, 0, OverlayColor);
            OverlayTexture.Apply();
        }

        // Compute the scale and offset, cutting off edges from the texture as needed (rather than letterboxing)
        float wScale = (float)TextureScreenWidth / (float)GridWidth;
        float hScale = (float)TextureScreenHeight / (float)GridHeight;

        float cellSize;
        Vector2 gridRoot = JigsawUtilities.RectTransformToScreenSpace(rectTransform).min;

        // Rect coords are [0,0] in the top-left to [ScreenWidth,ScreenHeight] in the bottom-right
        if (wScale < hScale)
        {
            cellSize = wScale;
            gridRoot += new Vector2(0.0f, 0.5f * (TextureScreenHeight - GridHeight * cellSize));
        }
        else
        {
            cellSize = hScale;
            gridRoot += new Vector2(0.5f * (TextureScreenWidth - GridWidth * cellSize), 0.0f);
        }

        // Draw cells
        float halfLineThickness = 0.5f * LineThickness;
        Rect lineRect = new Rect();
        lineRect.xMin = gridRoot.x - halfLineThickness;
        lineRect.xMax = gridRoot.x + (GridWidth * cellSize) + halfLineThickness;
        for (int i = 0; i <= GridHeight; ++i)
        {
            lineRect.yMin = gridRoot.y + (i * cellSize) - halfLineThickness;
            lineRect.yMax = lineRect.yMin + LineThickness;
            GUI.DrawTexture(lineRect, OverlayTexture);
        }

        lineRect.yMin = gridRoot.y - halfLineThickness;
        lineRect.yMax = gridRoot.y + (GridHeight * cellSize) + halfLineThickness;
        for (int j = 0; j <= GridWidth; ++j)
        {
            lineRect.xMin = gridRoot.x + (j * cellSize) - halfLineThickness;
            lineRect.xMax = lineRect.xMin + LineThickness;
            GUI.DrawTexture(lineRect, OverlayTexture);
        }
    }
}
