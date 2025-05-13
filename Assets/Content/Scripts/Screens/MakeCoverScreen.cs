using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class MakeCoverScreen : ScreenBase
{
    [SerializeField] private Transform _photosContainer;
    [SerializeField] private Image _posterImage;
    [SerializeField] private Texture2D[] _posterTextures;
    [SerializeField] private Font[] _posterFonts;
    [SerializeField] private int[] _fontSizes;
    [SerializeField] private float[] _fontLineSpacing;
    [SerializeField] private Text _titleText;
    [SerializeField] private GameObject _photoPrefab;
    [SerializeField] private Sprite _frameSprite;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Loader _loader;
    private bool _canSelect;
    private bool _photosLoaded;

    [SerializeField] private string _cloudFolderTemplate = "https://cloud.example.com/user1234/";

    private List<Texture2D> _capturedPhotos = new List<Texture2D>();
    private List<int> _selectedPhotoIndices = new List<int>();
    private string _cloudFolderUrl;

    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.8f;

    public override void Initialize()
    {
        _canSelect = false;
        _nextButton.onClick.AddListener(OnNextPressed);
        _backButton.onClick.AddListener(OnBackPressed);
    }

    public override IEnumerator AnimateShow()
    {
        _posterImage.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        _capturedPhotos = new List<Texture2D>();
        _selectedPhotoIndices = new List<int>();
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
        if (GlobalChosesDataContainer.Instance.isDoubleBuild)
        {
            _loader.StopMonitoring();
            foreach (Transform child in _photosContainer)
            {
                Destroy(child.gameObject);
            }
            yield return new WaitUntil(() => _photosContainer.childCount < 1);
            _loader.OnNewPhotoLoaded -= LoadCapturedPhotos;
            _loader.OnNewPhotoLoaded += LoadCapturedPhotos;
            _loader.StartMonitoring();
            // _photosLoaded = false;
            // LoadCapturedPhotos();
            yield return new WaitUntil(() => _photosContainer.childCount > 0);
            // DisplayPhotos();
        }
        if (!GlobalChosesDataContainer.Instance.isDoubleBuild)
        {
            for (int i = 0; i < GlobalChosesDataContainer.Instance.SelectedPhotos.Count; i++)
            {
                _capturedPhotos.Add(GlobalChosesDataContainer.Instance.SelectedPhotos[i]);
            }
            DisplayPhotos();
        }
        _titleText.text = GlobalChosesDataContainer.Instance.Name + "\n" + (GlobalChosesDataContainer.Instance.Surname.Contains('-') ? GlobalChosesDataContainer.Instance.Surname.Replace("-", "-\n") : GlobalChosesDataContainer.Instance.Surname);
        _posterImage.transform.Find("Overlay").GetComponent<Image>().sprite = TextureConverter.ConvertTextureToSprite(_posterTextures[GlobalChosesDataContainer.Instance.SelectedCategory]);
        _posterImage.transform.Find("Text").GetComponent<Text>().font = _posterFonts[GlobalChosesDataContainer.Instance.SelectedCategory];
        _posterImage.transform.Find("Text").GetComponent<Text>().fontSize = _fontSizes[GlobalChosesDataContainer.Instance.SelectedCategory];
        _posterImage.transform.Find("Text").GetComponent<Text>().lineSpacing = _fontLineSpacing[GlobalChosesDataContainer.Instance.SelectedCategory];
        yield return new WaitForEndOfFrame();

        TogglePhotoSelection(0);
        yield return new WaitForEndOfFrame();
        yield return AnimateFadeIn(_posterImage.gameObject.GetComponent<CanvasGroup>(), _fadeDuration);
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
        if (_capturedPhotos.Count > 0)
        {
            _canSelect = true;
        }
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
    private async void LoadCapturedPhotos()
    {
        _capturedPhotos = await _loader.CheckForExistingPhotos();
        _photosLoaded = true;
    }
    private void DisplayPhotos()
    {
        foreach (Transform child in _photosContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _capturedPhotos.Count; i++)
        {
            int index = i;
            var photoObj = Instantiate(_photoPrefab, _photosContainer);
            var image = photoObj.transform.Find("Photo").GetComponent<Image>();
            var button = photoObj.GetComponent<Button>();

            image.gameObject.GetComponent<SmartImageFitter>().SetImage(TextureConverter.ConvertTextureToSprite(_capturedPhotos[i]));

            button.onClick.AddListener(() => TogglePhotoSelection(index));

            UpdatePhotoSelectionVisual(photoObj, index);
        }
    }

    private void TogglePhotoSelection(int index)
    {
        if (_selectedPhotoIndices.Contains(index))
        {
            return;
        }
        var photoObj = _photosContainer.GetChild(index).gameObject;
        for (int i = 0; i < _photosContainer.childCount; i++)
        {
            var indexTiUnselect = i;
            var unselect = _photosContainer.GetChild(indexTiUnselect).gameObject;
            _selectedPhotoIndices.Remove(indexTiUnselect);
            UpdatePhotoSelectionVisual(unselect, indexTiUnselect);
        }
        _selectedPhotoIndices.Add(index);
        _posterImage.transform.Find("Photo").GetComponent<SmartImageFitter>().SetImage(photoObj.transform.Find("Photo").GetComponent<Image>().sprite);
        UpdatePhotoSelectionVisual(photoObj, index);
    }

    private void UpdatePhotoSelectionVisual(GameObject photoObj, int index)
    {
        var frame = photoObj.transform.Find("Frame");
        if (frame != null)
        {
            frame.gameObject.SetActive(_selectedPhotoIndices.Contains(index));
        }
        var toner = photoObj.transform.Find("Toner");
        if (toner != null)
        {
            toner.gameObject.SetActive(!_selectedPhotoIndices.Contains(index));
        }
    }

    private async void OnNextPressed()
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
               Camera.main,
               _posterImage.transform.position
           );

        Vector2 size = _posterImage.rectTransform.rect.size;
        _loader.CaptureAreaAndSave(_posterImage.rectTransform, _loader.GetFilePath(), DateTime.Now.ToSafeString().Replace(" ", "").Replace(":", "").Replace(".", "").Replace("_", ""));
        // await Task.Delay(1000);
        // await _loader.UploadSelectedPhotosToDisk(new Texture2D[1] { GlobalChosesDataContainer.Instance.Poster }, true, true);
        ScreenManager.Instance.ShowScreen<QrCodeCoverScreen>();
    }

    private void OnBackPressed()
    {
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<CoverCategoryScreen>();
    }

}