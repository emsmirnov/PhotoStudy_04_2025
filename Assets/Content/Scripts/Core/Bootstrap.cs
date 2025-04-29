using Project;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public Loader loader;
    public ScreenManager screenManager;
    public YandexDiskSDK yandex;
    public QrGeneratorOnline qrGeneratorOnline;
    void Awake()
    {
        // screenManager.InitScreens();
    }
    void Start()
    {
        loader.Init();
        screenManager.StartScreens();
        yandex.Init();
        qrGeneratorOnline.Init();
    }
}
