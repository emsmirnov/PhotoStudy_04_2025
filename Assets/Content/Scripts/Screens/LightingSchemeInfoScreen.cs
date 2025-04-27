using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightingSchemeInfoScreen : ScreenBase
{
    [SerializeField] private Image _schemeDiagram;
    [SerializeField] private Button _activateButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;
    [SerializeField] private LightingSchemeData _currentScheme;

    public override void Initialize()
    {
        _backButton.onClick.AddListener(OnBackPressed);
        _activateButton.onClick.AddListener(OnActivatePressed);
    }

    public void SetSchemeData(LightingSchemeData scheme)
    {
        _currentScheme = scheme;
        UpdateUI();
    }


    private void UpdateUI()
    {
        _schemeDiagram.sprite = _currentScheme.diagramSprite;
    }

    private void OnBackPressed()
    {//ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<LightingModeSelectionScreen>();
    }

    private void OnActivatePressed()
    {
        ScreenManager.Instance.ShowScreen<PhotoCaptureScreen>();
    }

    public override IEnumerator AnimateShow()
    {
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
    }

    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
    }
}