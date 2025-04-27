using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ClampImageInsideFrame : MonoBehaviour
{
    private RectTransform imageRect;
    private RectTransform frameRect;

    void Awake()
    {
        imageRect = GetComponent<RectTransform>();
        frameRect = transform.parent as RectTransform;
    }

    void LateUpdate()
    {
        ClampImage();
    }

    private void ClampImage()
    {
        if (imageRect == null || frameRect == null) return;

        // Размеры рамки (родителя) и изображения (дочернего) с учетом масштаба
        Vector2 frameSize = frameRect.rect.size;
        Vector2 imageSize = imageRect.rect.size;
        Vector2 scale = imageRect.localScale;

        Vector2 scaledImageSize = Vector2.Scale(imageSize, scale);

        // Если изображение меньше рамки — не даём двигать вообще
        if (scaledImageSize.x <= frameSize.x && scaledImageSize.y <= frameSize.y)
        {
            imageRect.anchoredPosition = Vector2.zero;
            return;
        }

        // Допустимый диапазон смещения
        Vector2 maxOffset = (scaledImageSize - frameSize) * 0.5f;

        Vector2 clampedPosition = imageRect.anchoredPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -maxOffset.x, maxOffset.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -maxOffset.y, maxOffset.y);

        imageRect.anchoredPosition = clampedPosition;
    }
}
