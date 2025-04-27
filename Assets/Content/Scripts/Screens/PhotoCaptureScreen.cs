using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

public class PhotoCaptureScreen : ScreenBase
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Text _countdownText;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _captureButton;
    [SerializeField] private Image _screenImage;
    [SerializeField] private Loader _loader;

    [SerializeField] private float _countdownDuration = 10f;
    [SerializeField] private int _photosToTake = 8;
    [SerializeField] private float _photoInterval = 0.5f;
    [SerializeField] private float _fadeDuration = 0.8f;
    [SerializeField] private string cameraAppAddress = "http://127.0.0.1:8080";
    [SerializeField] private string cameraAppShotCount = "12";

    private bool _isCapturing, _shotingStarted;
    private Coroutine _captureRoutine;

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
            var cameraAppCongig = files.First((x) => x.Name.Contains("cameraApp"));
            var sr = cameraAppCongig.OpenText();
            var cameraAppData = sr.ReadToEnd();
            var splitPathes = cameraAppData.Split(new char[] { '|' });
            cameraAppAddress = splitPathes[0];
            cameraAppShotCount = splitPathes[1];
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        _backButton.onClick.RemoveAllListeners();
        _backButton.onClick.AddListener(OnBackPressed);
        _captureButton.onClick.RemoveAllListeners();
        _captureButton.onClick.AddListener(StartCapture);
    }

    public override IEnumerator AnimateShow()
    {
        SetupCaptureScreen();
        yield return AnimateFadeIn(_canvasGroup, _fadeDuration);
    }

    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, _fadeDuration);
    }

    private void SetupCaptureScreen()
    {
        _screenImage.sprite = GlobalChosesDataContainer.Instance.LightingMode.diagramSprite;
        _countdownText.gameObject.SetActive(false);
        _isCapturing = false;
    }

    public void StartCapture()
    {
        if (!_isCapturing)
        {
            _loader.CleanFolder();
            _loader.CleanProcessedFiles();
            _loader.StartMonitoring();
            ScreenManager.Instance.GetScreen<PhotoReviewScreen>().CleanPhotosContainer();
            _captureRoutine = StartCoroutine(CaptureRoutine());
        }
    }

    private IEnumerator CaptureRoutine()
    {
        _isCapturing = true;
        if (GlobalChosesDataContainer.Instance.ShootingMode == ShootingModeSelectionScreen.ShootingMode.SingleParticipant)
        {
            _countdownText.gameObject.SetActive(true);

            float timer = _countdownDuration;
            while (timer > 0)
            {
                _countdownText.text = Mathf.CeilToInt(timer).ToString();
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        _shotingStarted = false;
        pushCommandToShot();
        yield return new WaitUntil(() => _shotingStarted == true);
        // for (int i = 0; i < _photosToTake; i++)
        // {
        //     TakePhoto();
        //     yield return new WaitForSeconds(_photoInterval);
        // }

        ScreenManager.Instance.ShowScreen<PhotoReviewScreen>();
        _isCapturing = false;
    }

    private void TakePhoto()
    {
        Debug.Log("Photo taken!");
    }

    private async void pushCommandToShot()
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(cameraAppAddress + $"/camera?action=multishot&count={cameraAppShotCount}"))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
                ScreenManager.Instance.EnableError();
                return;
            }
            else
            {
                _shotingStarted = true;
                return;
            }
        }
    }

    private void OnBackPressed()
    {
        if (_isCapturing)
        {
            if (_captureRoutine != null)
                StopCoroutine(_captureRoutine);
            _isCapturing = false;
        }
        //ScreenManager.Instance.ShowPreviousScreen();
        ScreenManager.Instance.ShowScreen<LightingModeSelectionScreen>();
    }
}