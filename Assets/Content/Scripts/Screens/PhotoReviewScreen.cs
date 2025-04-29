using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PhotoReviewScreen : ScreenBase
{
    [SerializeField] private Transform _photosContainer;
    [SerializeField] private GameObject _photoPrefab;

    [SerializeField] private Button _downloadButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Loader _loader;
    private bool _canSelect;

    private List<Texture2D> _capturedPhotos = new List<Texture2D>();
    private List<int> _selectedPhotoIndices = new List<int>();

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;

    public bool TESTMODE;

    public override void Initialize()
    {
        CleanPhotosContainer();
        _canSelect = false;
        _downloadButton.onClick.AddListener(OnDownloadPressed);
        _backButton.onClick.AddListener(OnBackPressed);
        _loader.OnNewPhotoLoaded -= LoadCapturedPhotos;
        _loader.OnAllPhotosLoaded -= HandleAllPhotosLoaded;
        _loader.OnNewPhotoLoaded += LoadCapturedPhotos;
        _loader.OnAllPhotosLoaded += HandleAllPhotosLoaded;
    }

    public override IEnumerator AnimateShow()
    {
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
    }
    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
    }
    private void LoadCapturedPhotos(Texture2D tex)
    {
        _capturedPhotos.Add(tex);
        Debug.Log(_capturedPhotos.Count);
        GlobalChosesDataContainer.Instance.Photos = _capturedPhotos.ToList();
        DisplayOnePhoto(tex, _capturedPhotos.Count - 1);
    }

    private void DisplayOnePhoto(Texture2D tex, int index)
    {
        var photoObj = Instantiate(_photoPrefab, _photosContainer);
        var image = photoObj.transform.Find("Photo").GetComponent<Image>();
        var button = photoObj.GetComponent<Button>();

        image.gameObject.GetComponent<SmartImageFitter>().SetImage(TextureConverter.ConvertTextureToSprite(tex));

        button.onClick.AddListener(() => TogglePhotoSelection(index));

        UpdatePhotoSelectionVisual(photoObj, index);
    }

    private void HandleAllPhotosLoaded()
    {
        _canSelect = true;
    }

    private void TogglePhotoSelection(int index)
    {
        if (!_canSelect)
        {
            return;
        }
        if (_selectedPhotoIndices.Contains(index))
        {
            _selectedPhotoIndices.Remove(index);
        }
        else
        {
            _selectedPhotoIndices.Add(index);
        }

        var photoObj = _photosContainer.GetChild(index).gameObject;
        UpdatePhotoSelectionVisual(photoObj, index);
    }

    private void UpdatePhotoSelectionVisual(GameObject photoObj, int index)
    {
        var frame = photoObj.transform.Find("Frame");
        if (frame != null)
        {
            frame.gameObject.SetActive(_selectedPhotoIndices.Contains(index));
        }
    }

    public void CleanPhotosContainer()
    {
        foreach (Transform child in _photosContainer)
        {
            Destroy(child.gameObject);
        }
        _capturedPhotos = new List<Texture2D>();
        _canSelect = false;
        _selectedPhotoIndices = new List<int>();
    }

    private async void OnDownloadPressed()
    {
        if (_selectedPhotoIndices.Count == 0) return;
        _canSelect = false;
        var selectedPhotos = _capturedPhotos.Where(photo => _selectedPhotoIndices.Contains(_capturedPhotos.IndexOf(photo)));
        GlobalChosesDataContainer.Instance.SelectedPhotos = selectedPhotos.ToList();
        ScreenManager.Instance.GetScreen<QrCodeScreen>().SetSelectedPhotos(selectedPhotos.ToList());
        // await _loader.UploadSelectedPhotosToDisk(selectedPhotos.ToArray(), true);                                                    /////////////////// ПЕРЕДЕЛАТЬ ПРОВЕРКУ СОЕДИНЕНИЯ ИЛИ НЕ ПЕРЕДАВАТЬ ИЗ ЭТОГО СКРИПТА
        ScreenManager.Instance.ShowScreen<QrCodeScreen>();
    }

    private void OnBackPressed()
    {
        GlobalChosesDataContainer.Instance.SelectedPhotos = new List<Texture2D>();
        GlobalChosesDataContainer.Instance.Photos = new List<Texture2D>();
        if (!TESTMODE)
            _loader.CleanFolder();
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<PhotoCaptureScreen>();
    }

}