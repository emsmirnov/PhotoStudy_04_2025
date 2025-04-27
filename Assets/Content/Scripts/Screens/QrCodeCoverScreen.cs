using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using DG.Tweening;
using System.Threading.Tasks;

public class QrCodeCoverScreen : ScreenBase
{
    [SerializeField] private Transform _photosContainer;
    [SerializeField] private Image _posterImage;
    [SerializeField] private Button _toMainScreenButton;
    [SerializeField] private Loader _loader;
    [SerializeField] private CanvasGroup _qrCodeImagePreloaderCG, _posterImagePreloaderCG;
    [SerializeField] private Image _qrCodeImage;
    private List<Texture2D> _selectedPhotos = new List<Texture2D>();
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;

    public void SetSelectedPhotos(List<Texture2D> selectedPhotos)
    {
        _selectedPhotos = selectedPhotos;
    }

    public override void Initialize()
    {
        _toMainScreenButton.onClick.AddListener(OnMainScreenPressed);
    }

    public override IEnumerator AnimateShow()
    {
        PreparePhotos();
        _posterImage.sprite = TextureConverter.ConvertTextureToSprite(GlobalChosesDataContainer.Instance.Poster);
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
        await Task.Yield();
        _posterImagePreloaderCG.DOFade(0, 1);
        await Task.Yield();
        _posterImage.GetComponent<CanvasGroup>().DOFade(1, 1);
    }
    private async void PreparePhotos()
    {
        _qrCodeImage.sprite = null;
        _posterImage.GetComponent<CanvasGroup>().DOFade(0, 0);
        _qrCodeImagePreloaderCG.DOFade(1, 0);
        _posterImagePreloaderCG.DOFade(1, 0);
        // _loader.CaptureAreaAndSave(_posterImage.rectTransform, _loader.GetFilePath(), DateTime.Now.ToSafeString().Replace(" ", "").Replace(":", "").Replace(".", "").Replace("_", ""));
        while (!_loader.posterCreationFinished)
        {
            await Task.Delay(500);
        }
        _loader.UploadSelectedPhotosToDisk(new Texture2D[1] { GlobalChosesDataContainer.Instance.Poster }, true, true);
    }

    private void OnMainScreenPressed()
    {
        _loader.CleanFolder();
        ScreenManager.Instance.StartScreens();
        _loader.StartMonitoring();
        // ScreenManager.Instance.ShowScreen<ScreensaverScreen>();
    }

}