using UnityEngine;

public static class JigsawUtilities
{
    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }


    public static void RecursiveDestroy(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }
        for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
        {
            RecursiveDestroy(gameObject.transform.GetChild(i).gameObject);
        }
        Object.Destroy(gameObject);
    }
}
