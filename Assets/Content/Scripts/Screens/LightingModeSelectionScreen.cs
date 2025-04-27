using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;

[System.Serializable]
public class LightingModeOption
{
    public string modeName;
    public Sprite icon;
    public LightingScheme scheme;
}

public enum LightingScheme
{
    LOOP,
    REMBRANDT,
    BUTTERFLY,
    SIDE_LIGHT,
    BACKLIGHT,
    CLASSIC_PORTRAIT,
    LOW_KEY_PORTRAIT
}


public class LightingModeSelectionScreen : ScreenBase
{

    [SerializeField] private Image _screenImage;
    [SerializeField] private List<Button> _lightSchemeButtons;
    [SerializeField] private Button _modeSelectedButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private List<Sprite> _lightModeScreenSprite;
    [SerializeField] private List<LightingSchemeData> _lightingModes;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;
    [SerializeField] private string lightAppAddress = "http://127.0.0.1:8080";
    private int _selectedIndex = 0;

    public override void Initialize()
    {
        DirectoryInfo d = new DirectoryInfo(Application.streamingAssetsPath);
        var files = d.GetFiles("*.txt");
        if (files == null)
        {
            throw new System.Exception("NO CONFIGS IN STREAMING ASSETS");
        }
        try
        {
            var lightAppCongig = files.First((x) => x.Name.Contains("lightApp"));
            var sr = lightAppCongig.OpenText();
            lightAppAddress = sr.ReadToEnd();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        CreateOptions();
    }

    public override IEnumerator AnimateShow()
    {
        _backButton.onClick.RemoveAllListeners();
        _backButton.onClick.AddListener(OnBackPressed);
        SelectOption(0);
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
    }

    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
    }

    private void CreateOptions()
    {
        if (_lightSchemeButtons.Count != _lightModeScreenSprite.Count)
        {
            Debug.LogError("WRONG BUTTONS OR SPRITES COUNT");
        }
        foreach (var button in _lightSchemeButtons)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectOption(button.transform.GetSiblingIndex()));
        }
    }

    public void SelectOption(int index)
    {
        setLight(index);
        if (index != 4)
        {
            _selectedIndex = index;
            _screenImage.sprite = _lightModeScreenSprite[index];
            GlobalChosesDataContainer.Instance.LightingMode = _lightingModes[index];

            _modeSelectedButton.onClick.RemoveAllListeners();
            _modeSelectedButton.onClick.AddListener(() => OnModeSelected(_lightingModes[index]));
        }
    }

    private IEnumerator AnimateSelection(Transform target)
    {
        float duration = 0.2f;
        float elapsed = 0;
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * 1.1f;

        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    public void ProceedToNextScreen()
    {
        ScreenManager.Instance.ShowScreen<LightingSchemeInfoScreen>();
    }

    private void OnModeSelected(LightingSchemeData mode)
    {
        ScreenManager.Instance.ShowScreen<PhotoCaptureScreen>();
    }

    private async void setLight(int index)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(lightAppAddress + $"/preset?id={index}"))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
                return;
            }
            else
            {
                return;
            }
        }
    }

    private void OnBackPressed()
    {
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<ShootingModeSelectionScreen>();
    }
}