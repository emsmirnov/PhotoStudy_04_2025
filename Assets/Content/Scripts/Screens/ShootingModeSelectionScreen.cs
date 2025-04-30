using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShootingModeSelectionScreen : ScreenBase
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _singleParticipantBtn;
    [SerializeField] private Button _twoParticipantsBtn;
    [SerializeField] private float _fadeDuration = 0.8f;
    [SerializeField] private Animator _transitionAnimator;

    [SerializeField] private Button _backButton;

    public enum ShootingMode
    {
        SingleParticipant,
        TwoParticipants
    }

    public override void Initialize()
    {
        
        _backButton.onClick.RemoveAllListeners();
        _backButton.onClick.AddListener(OnBackPressed);
        _singleParticipantBtn.onClick.AddListener(() => OnModeSelected(ShootingMode.SingleParticipant));
        _twoParticipantsBtn.onClick.AddListener(() => OnModeSelected(ShootingMode.TwoParticipants));
    }

    public override IEnumerator AnimateShow()
    {
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);

        if (_transitionAnimator != null)
        {
            _transitionAnimator.Play("ModeSelectionEnter");
            yield return new WaitForSeconds(_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    public override IEnumerator AnimateHide()
    {
        if (_transitionAnimator != null)
        {
            _transitionAnimator.Play("ModeSelectionExit");
            yield return new WaitForSeconds(_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
        }
    }

    private void OnModeSelected(ShootingMode mode)
    {
        GlobalChosesDataContainer.Instance.ShootingMode = mode;
        ScreenManager.Instance.ShowScreen<LightingModeSelectionScreen>();
    }

    private void OnBackPressed()
    {
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<ScreensaverScreen>();
    }
}