using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro;

public class CoverCategoryScreen : ScreenBase
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button[] _categories;
    [SerializeField] private float _videoPlayDuration = 2f;
    private int _selectedCategoryIndex = -1;
    private Coroutine _videoRoutine;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;

    public override void Initialize()
    {
        for (int i = 0; i < _categories.Length; i++)
        {
            int index = i;
            _categories[i].onClick.AddListener(() => SelectCategory(index));
            _categories[i].transform.Find("Highlight").gameObject.SetActive(false);
            _categories[i].transform.Find("Video").gameObject.SetActive(false);
        }
        _nextButton.onClick.AddListener(OnNextPressed);
        _backButton.onClick.AddListener(OnBackPressed);
        _mainMenuButton.onClick.AddListener(OnMainMenuPressed);

        _nextButton.interactable = false;
    }

    public override IEnumerator AnimateShow()
    {
        ResetSelection();
        yield return AnimateFadeIn(_canvasGroup, 0.3f);
    }

    private void SelectCategory(int index)
    {
        if (_selectedCategoryIndex >= 0 && _selectedCategoryIndex < _categories.Length)
        {
            _categories[_selectedCategoryIndex].transform.Find("Highlight").gameObject.SetActive(false);
            _categories[_selectedCategoryIndex].transform.Find("Video").gameObject.SetActive(false);
            _categories[_selectedCategoryIndex].transform.Find("Video").GetComponent<VideoPlayer>().targetTexture.Release();
        }

        _selectedCategoryIndex = index;
        _categories[index].transform.Find("Highlight").gameObject.SetActive(true);
        _categories[index].transform.Find("Video").gameObject.SetActive(true);
        _nextButton.interactable = true;
    }

    private void ResetSelection()
    {
        _selectedCategoryIndex = -1;
        foreach (var category in _categories)
        {
            category.transform.Find("Highlight").gameObject.SetActive(false);
            category.transform.Find("Video").gameObject.SetActive(false);
        }
        _nextButton.interactable = false;
    }

    private void OnNextPressed()
    {
        if (_selectedCategoryIndex >= 0)
        {
            GlobalChosesDataContainer.Instance.SelectedCategory = _selectedCategoryIndex;

            ScreenManager.Instance.ShowScreen<MakeCoverScreen>();
        }
    }

    private void OnBackPressed()
    {
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<DataInputScreen>();
    }

    private void OnMainMenuPressed()
    {
        ScreenManager.Instance.StartScreens();
        // ScreenManager.Instance.ShowScreen<ScreensaverScreen>();
    }

    public override IEnumerator AnimateHide()
    {
        if (_videoRoutine != null)
        {
            StopCoroutine(_videoRoutine);
            _videoRoutine = null;
        }

        yield return AnimateFadeOut(_canvasGroup, 0.3f);
    }
}