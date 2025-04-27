using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Threading.Tasks;

public class QrCodeScreen : ScreenBase
{
    [SerializeField] private Transform _photosContainer;
    [SerializeField] private GameObject _photoPrefab;
    [SerializeField] private Button _makePosterButton;
    [SerializeField] private Button _toMainScreenButton;
    [SerializeField] private Loader _loader;
    [SerializeField] private Image _qrCodeImage;
    [SerializeField] private CanvasGroup _qrCodeImagePreloaderCG;
    private List<Texture2D> _selectedPhotos = new List<Texture2D>();
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;

    public void SetSelectedPhotos(List<Texture2D> selectedPhotos)
    {
        _selectedPhotos = selectedPhotos;
    }

    public override void Initialize()
    {
        _makePosterButton.onClick.AddListener(OnMakePosterPressed);
        _toMainScreenButton.onClick.AddListener(OnMainScreenPressed);
    }

    public override IEnumerator AnimateShow()
    {
        PreparePhotos();
        DisplayPhotos();
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
    }
    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
    }

    public async Task setupQr(Texture2D tex)
    {
        _qrCodeImage.sprite = TextureConverter.ConvertTextureToSprite(tex);
        await Task.Yield();
        _qrCodeImagePreloaderCG.DOFade(0, 1);
    }
    private void DisplayPhotos()
    {
        foreach (Transform child in _photosContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _selectedPhotos.Count; i++)
        {
            int index = i;
            var photoObj = Instantiate(_photoPrefab, _photosContainer);
            var image = photoObj.transform.Find("Photo").GetComponent<SmartImageFitter>();
            var button = photoObj.GetComponent<Button>();

            image.SetImage(TextureConverter.ConvertTextureToSprite(_selectedPhotos[i]));
        }
    }

    private void PreparePhotos()
    {
        _qrCodeImage.sprite = null;
        _qrCodeImagePreloaderCG.DOFade(1, 0);
        _loader.UploadSelectedPhotosToDisk(_selectedPhotos.ToArray(), true);
    }


    private void OnMakePosterPressed()
    {
        ScreenManager.Instance.ShowScreen<DataInputScreen>();
    }

    private void OnMainScreenPressed()
    {
        _loader.CleanFolder();
        ScreenManager.Instance.StartScreens();
        // ScreenManager.Instance.ShowScreen<ScreensaverScreen>();
    }

}