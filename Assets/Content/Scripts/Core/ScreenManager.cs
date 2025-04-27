using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    [SerializeField] private Transform _screensContainer;
    [SerializeField] private ScreensaverScreen _initialScreen;
    [SerializeField] private float _screenTransitionDuration = 0.5f;
    [SerializeField] private float _activityTimer = 60f;
    [SerializeField] private float _currentTimer = 0f;
    private Dictionary<Type, ScreenBase> _screens = new Dictionary<Type, ScreenBase>();
    private ScreenBase _currentScreen;
    private ScreenBase _previousScreen;
    private bool _isTransitioning;
    private bool _canCount;
    [SerializeField] private GameObject _networkError;
    [SerializeField] private Loader _loader;

    public GlobalChosesDataContainer container;


    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        container.Init();
        InitializeScreens();
    }

    public void StartScreens()
    {
        if (_initialScreen != null)
        {
            ShowScreen<ScreensaverScreen>();
        }
    }

    private void InitializeScreens()
    {
        DirectoryInfo d = new DirectoryInfo(Application.streamingAssetsPath);
        var files = d.GetFiles("*.txt");
        if (files == null)
        {
            throw new System.Exception("NO CONFIGS IN STREAMING ASSETS");
        }
        try
        {
            var lightAppCongig = files.First((x) => x.Name.Contains("activityTimer"));
            var sr = lightAppCongig.OpenText();
            _activityTimer = float.Parse(sr.ReadToEnd());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }


        foreach (Transform child in _screensContainer)
        {
            var screen = child.GetComponent<ScreenBase>();
            if (screen != null)
            {
                _screens.Add(screen.GetType(), screen);
                screen.Initialize();
                screen.gameObject.SetActive(false);
            }
        }
        InitErrors();
    }

    private void InitErrors()
    {
        _networkError.GetComponent<Button>().onClick.AddListener(DisableError);
        DisableError();
    }

    public T GetScreen<T>() where T : ScreenBase
    {
        Debug.Log(typeof(T));
        if (_screens.TryGetValue(typeof(T), out ScreenBase screen))
        {
            return (T)screen;
        }
        return null;
    }

    public void ShowScreen<T>(Action onComplete = null) where T : ScreenBase
    {
        if (_isTransitioning) return;
        if (typeof(T) == typeof(ScreensaverScreen))
        {
            _canCount = false;
            _currentTimer = 0;
        }
        else
        {
            _canCount = true;
        }
        if (_screens.TryGetValue(typeof(T), out ScreenBase screen))
        {
            StartCoroutine(TransitionToScreen(screen, onComplete));
        }
        else
        {
            Debug.LogError($"Screen of type {typeof(T)} not found!");
        }
    }

    private IEnumerator TransitionToScreen(ScreenBase newScreen, Action onComplete)
    {
        _isTransitioning = true;

        if (_currentScreen != null)
        {
            _previousScreen = _currentScreen;
            yield return StartCoroutine(_currentScreen.AnimateHide());
            _currentScreen.gameObject.SetActive(false);
        }

        _currentScreen = newScreen;
        _currentScreen.gameObject.SetActive(true);
        yield return StartCoroutine(_currentScreen.AnimateShow());

        _isTransitioning = false;
        onComplete?.Invoke();
    }

    public void ShowPreviousScreen()
    {
        if (_previousScreen != null && !_isTransitioning)
        {
            StartCoroutine(TransitionToScreen(_previousScreen, null));
        }
    }

    public void EnableError()
    {
        _networkError.SetActive(true);
    }

    public void DisableError()
    {
        _networkError.SetActive(false);
    }
    void Update()
    {
        if (_canCount)
        {
            _currentTimer += Time.deltaTime;
        }
        if (_currentTimer >= _activityTimer)
        {
            resetScreens();
        }
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            _currentTimer = 0f;
        }
    }

    private void resetScreens()
    {
        _currentTimer = 0;
        _loader.CleanFolder();
        StartScreens();
        _loader.StartMonitoring();
    }
}