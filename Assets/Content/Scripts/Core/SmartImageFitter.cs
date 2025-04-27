using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class SmartImageFitter : MonoBehaviour
{
    public enum FitMode
    {
        Crop,      // Обрезать лишнее (масштабировать с заполнением)
        Fit        // Вписать целиком (с возможными полями)
    }

    [SerializeField] private FitMode _fitMode = FitMode.Crop;
    
    private Image _image;
    [SerializeField] private RectTransform _rectTransform;
    private FitMode? _runtimeFitMode = null;

    /// <summary>
    /// Устанавливает изображение с возможностью указания режима
    /// </summary>
    /// <param name="sprite">Спрайт для отображения</param>
    /// <param name="mode">Опционально: режим отображения (если null - используется режим из инспектора)</param>
    public void SetImage(Sprite sprite, FitMode? mode = null)
    {
        if (_image == null) _image = GetComponent<Image>();
        _image.sprite = sprite;
        
        if (mode.HasValue)
        {
            _runtimeFitMode = mode;
        }
        
        UpdateImageFit();
    }

    /// <summary>
    /// Устанавливает режим отображения во время выполнения
    /// </summary>
    public void SetFitMode(FitMode mode)
    {
        _runtimeFitMode = mode;
        UpdateImageFit();
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        UpdateImageFit();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateImageFit();
    }

    private void UpdateImageFit()
    {
        if (_image == null || _image.sprite == null || _rectTransform == null)
            return;

        FitMode currentMode = _runtimeFitMode ?? _fitMode;

        float containerWidth = _rectTransform.rect.width;
        float containerHeight = _rectTransform.rect.height;

        float imageWidth = _image.sprite.rect.width;
        float imageHeight = _image.sprite.rect.height;

        _image.rectTransform.localScale = Vector3.one;
        _image.rectTransform.localRotation = Quaternion.identity;

        if (currentMode == FitMode.Crop)
        {
            float scale = Mathf.Max(containerWidth / imageWidth, containerHeight / imageHeight);
            _image.rectTransform.sizeDelta = new Vector2(imageWidth * scale, imageHeight * scale);
        }
        else // FitMode.Fit
        {
            float scale = Mathf.Min(containerWidth / imageWidth, containerHeight / imageHeight);
            _image.rectTransform.sizeDelta = new Vector2(imageWidth * scale, imageHeight * scale);
        }
    }
}