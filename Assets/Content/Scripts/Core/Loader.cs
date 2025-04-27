using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Project;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class Loader : MonoBehaviour
{
    [SerializeField] private string FilesPath;
    [SerializeField] private string OutFilesPath;
    public static Action<DateTime> filesLoaded;
    // [SerializeField] private PhotosContainer container;
    [SerializeField] private GlobalChosesDataContainer container;
    [SerializeField] private YandexDiskSDK yandex;
    public delegate void VoidDelegate(bool canCount);
    public static VoidDelegate CanCount;
    public VideoPlayer vp;
    public delegate void RepeatDelegate();
    // public static RepeatDelegate RepeatDrawQr;
    private bool canCount, fileCreated, fileUploaded;
    [SerializeField] private float checkDelay, timer;
    public delegate Task UploadDelegate();
    // public static UploadDelegate UploadPhotos;
    private string downloadResult;
    private List<string> downloadedFiles;
    private string photoFormat = "jpg";
    [SerializeField] private string folderName = "";
    public bool posterCreationFinished;
    public async void Init()
    {
        DirectoryInfo d = new DirectoryInfo(Application.streamingAssetsPath);
        var files = d.GetFiles("*.txt");
        if (files == null)
        {
            throw new System.Exception("NO CONFIGS IN STREAMING ASSETS");
        }
        var config = files.First((x) => x.Name.Contains("config"));
        var sr = config.OpenText();
        var pathes = sr.ReadToEnd();


        var format = Array.Find(files, (x) => x.Name.Contains("format"));
        if (format != null)
        {
            var formatFile = files.First((x) => x.Name.Contains("format"));
            var formatSr = config.OpenText();
            photoFormat = sr.ReadToEnd();
        }

        var splitPathes = pathes.Split(new char[] { '|' });
        FilesPath = splitPathes[0];
        OutFilesPath = splitPathes[1];
        timer = 0;
        canCount = true;
        fileCreated = false;
        downloadedFiles = new List<string>();
        posterCreationFinished = false;
        StartMonitoring();
    }

    ////////////////////////////////////////////////////////////////////NEW LOADER LOGIC///////////////////////////////////////////////////////////////////////////
    public event Action<Texture2D> OnNewPhotoLoaded;
    public event Action OnAllPhotosLoaded;

    [SerializeField] private int _expectedPhotoCount = 12;
    [SerializeField] private int _checkIntervalMs = 500;

    private HashSet<string> _processedFiles = new HashSet<string>();
    private bool _isMonitoring = false;

    public async void StartMonitoring()
    {
        if (_isMonitoring) return;

        _isMonitoring = true;
        _processedFiles.Clear();

        while (_isMonitoring && _processedFiles.Count < _expectedPhotoCount)
        {
            await CheckForNewPhotos();
            await Task.Delay(_checkIntervalMs);
        }

        _isMonitoring = false;
        OnAllPhotosLoaded?.Invoke();
    }

    public void StopMonitoring()
    {
        _isMonitoring = false;
    }

    public string GetFilePath()
    {
        return FilesPath;
    }
    public string GetOutFilePath()
    {
        return OutFilesPath;
    }

    private async Task CheckForNewPhotos()
    {
        try
        {
            var directory = new DirectoryInfo(FilesPath);
            var files = directory.GetFiles($"*.{photoFormat}");

            foreach (var file in files)
            {
                if (!_processedFiles.Contains(file.Name))
                {
                    var texture = await LoadTexture(file.FullName);
                    if (texture != null)
                    {
                        _processedFiles.Add(file.Name);
                        OnNewPhotoLoaded?.Invoke(texture);

                        if (_processedFiles.Count >= _expectedPhotoCount)
                        {
                            StopMonitoring();
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Photo monitoring error: {e.Message}");
        }
    }

    private async Task<Texture2D> LoadTexture(string filePath)
    {
        try
        {
            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load texture: {e.Message}");
            return null;
        }
    }

    public void CleanProcessedFiles()
    {
        _processedFiles.Clear();
    }


    public async Task UploadSelectedPhotosToDisk(Texture2D[] textures, bool hasConnection, bool isPoster = false)
    {
        var names = new string[textures.Length];
        if (string.IsNullOrEmpty(GlobalChosesDataContainer.Instance.CurrentFolderName))
        {
            folderName = DateTime.Now.ToSafeString().Replace(" ", "").Replace(":", "").Replace(".", "").Replace("_", ""); //$"{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            GlobalChosesDataContainer.Instance.CurrentFolderName = folderName;
        }
        if (!isPoster)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                var index = i;
                fileCreated = false;
                names[index] = index.ToString() + "." + photoFormat;
                await SaveScreenshotToFile(textures[i], names[index]);
            }
        }
        else
        {
            fileCreated = false;
            names = new string[1] { $"MagazinePoster.{photoFormat}" };
            await SaveScreenshotToFile(textures[0], names[0]);
        }
        while (!fileCreated)
        {
            await Task.Yield();
        }
        // await Task.Delay(2000);
        if (hasConnection)
        {
            fileUploaded = false;
            await UploadingSelectedFilesToYandexDisk(names);
            while (!fileUploaded)
            {
                await Task.Yield();
            }
        }
        else
        {
            // DownloadPhotoScreen.EnableFatalNetworkError(folderName);
            // ScreenManager.Instance.EnableError();
            // await FakeSaveFileToDiskCompleted("example.com");
            Debug.LogError(folderName);
        }
    }

    private async Task UploadingSelectedFilesToYandexDisk(string[] names)
    {
        await Task.Delay(1000);
        downloadResult = "";
        var pathFileNameOpen = $"{OutFilesPath}/{folderName}";
        // var folderName = $"{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
        // folderName = DateTime.Now.ToSafeString().Replace(" ", "").Replace(":", "").Replace(".", "").Replace("_", "");//.Substring(4);
        // String[] names = new string[2] { "Photo.jpg", "PhotoWithFrame.jpg" };
        // for (int i = 0; i < names.Length; i++)
        if (string.IsNullOrEmpty(GlobalChosesDataContainer.Instance.CurrentFolderName))
        {
            folderName = DateTime.Now.ToSafeString().Replace(" ", "").Replace(":", "").Replace(".", "").Replace("_", ""); //$"{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            GlobalChosesDataContainer.Instance.CurrentFolderName = folderName;
        }                                                                                                             // {
        Debug.Log(pathFileNameOpen);
        Debug.Log(folderName);
        await yandex.SaveFilesToDisk(pathFileNameOpen, folderName, names,
            async (result) =>
            {
                await SaveFileToDiskCompleted(result);
                downloadResult = result;
                fileUploaded = true;
            });
    }

    private async Task SaveFileToDiskCompleted(string url)
    {
        try
        {
            Debug.Log("----> " + url);
            ////////////////////////////////////////////////////////////////// ДОБАВИТЬ ОТПРАВКУ URL ЧТОБЫ ПОЛУЧИТЬ ТЕКСТУРУ QR КОДА ///////////////////////////////////////////////////////////////////////////
            // var qrCodeDrawing = new QRCodeDrawing();
            // await Task.Delay(2000);

            // Texture2D qr = null;
            // if (url != "")
            //     qr = qrCodeDrawing.DrowQRCode(url);

            // container.QRCode = qr;
            // DownloadPhotoScreen.SetupQr.Invoke();

            ////////////////////////////////// // ScreenManager.Instance.GetScreen<QrCodeScreen>().setupQr()
            /// 
            GetComponent<QrGeneratorOnline>().UploadPhotoLink(url, (response) =>
           {
               if (response != null && response.success)
               {


                   Debug.Log($"Получен QR-код: {response.data.link.qr}");

                   StartCoroutine(LoadQRCodeTexture(response.data.link.qr, async (texture) =>
                           {
                               if (texture != null)
                               {
                                   Debug.Log("QR-код успешно загружен как Texture2D");
                                   container.QrCodePhotos = texture;
                                   container.QrCodePoster = texture;
                                   // DownloadPhotoScreen.SetupQr.Invoke();

                                   ScreenManager.Instance.GetScreen<QrCodeScreen>().setupQr(texture);
                                   ScreenManager.Instance.GetScreen<QrCodeCoverScreen>().setupQr(texture);
                               }
                               else
                               {
                                   Debug.LogError("Не удалось загрузить QR-код");

                                   var qrCodeDrawing = new QRCodeDrawing();
                                   // await Task.Delay(2000);

                                   Texture2D qr = null;
                                   if (url != "")
                                       qr = qrCodeDrawing.DrowQRCode(url);

                                   container.QrCodePhotos = texture;
                                   container.QrCodePoster = texture;
                                   // DownloadPhotoScreen.SetupQr.Invoke();

                                   ScreenManager.Instance.GetScreen<QrCodeScreen>().setupQr(texture);
                                   ScreenManager.Instance.GetScreen<QrCodeCoverScreen>().setupQr(texture);
                               }
                           }));
               }
           });

        }
        catch (Exception e)
        {
            Debug.Log("qr error" + e.Message);
            // DownloadPhotoScreen.EnableQrError.Invoke();
        }
        // isUploadingFileToYandexDisk = false;
    }

    private async Task FakeSaveFileToDiskCompleted(string url)
    {
        try
        {
            Debug.Log("----> " + url);
            ////////////////////////////////////////////////////////////////// ДОБАВИТЬ ОТПРАВКУ URL ЧТОБЫ ПОЛУЧИТЬ ТЕКСТУРУ QR КОДА ///////////////////////////////////////////////////////////////////////////
            // var qrCodeDrawing = new QRCodeDrawing();
            // await Task.Delay(2000);

            // Texture2D qr = null;
            // if (url != "")
            //     qr = qrCodeDrawing.DrowQRCode(url);

            GetComponent<QrGeneratorOnline>().UploadPhotoLink("https://example.com/my-photo.jpg", (response) =>
            {
                if (response != null && response.success)
                {


                    Debug.Log($"Получен QR-код: {response.data.link.qr}");

                    StartCoroutine(LoadQRCodeTexture(response.data.link.qr, async (texture) =>
                            {
                                if (texture != null)
                                {
                                    Debug.Log("QR-код успешно загружен как Texture2D");
                                    container.QrCodePhotos = texture;
                                    container.QrCodePoster = texture;
                                    // DownloadPhotoScreen.SetupQr.Invoke();

                                    ScreenManager.Instance.GetScreen<QrCodeScreen>().setupQr(texture);
                                    ScreenManager.Instance.GetScreen<QrCodeCoverScreen>().setupQr(texture);
                                }
                                else
                                {
                                    Debug.LogError("Не удалось загрузить QR-код");

                                    var qrCodeDrawing = new QRCodeDrawing();
                                    // await Task.Delay(2000);

                                    Texture2D qr = null;
                                    if (url != "")
                                        qr = qrCodeDrawing.DrowQRCode(url);

                                    container.QrCodePhotos = texture;
                                    container.QrCodePoster = texture;
                                    // DownloadPhotoScreen.SetupQr.Invoke();

                                    ScreenManager.Instance.GetScreen<QrCodeScreen>().setupQr(texture);
                                    ScreenManager.Instance.GetScreen<QrCodeCoverScreen>().setupQr(texture);
                                }
                            }));
                }
            });



        }
        catch (Exception e)
        {
            Debug.Log("qr error" + e.Message);
            // DownloadPhotoScreen.EnableQrError.Invoke();
        }
        // isUploadingFileToYandexDisk = false;
    }


    private async Task SaveScreenshotToFile(Texture2D texture, string name)
    {
        try
        {
            byte[] bytes = texture.EncodeToJPG();
            // var folderName = $"{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";// ToSafeString().Replace(" ", "_").Replace(":", "-").Replace(".", "-").;
            string path = $"{OutFilesPath}/{folderName}";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            var pathFileScreenshot = $"{OutFilesPath}/{folderName}/{name}"; //Path.Combine(OutFilesPath, nameFileScreenshot);
            if (File.Exists(pathFileScreenshot))
            {
                Debug.Log($"Файл: {pathFileScreenshot}, уже существует. Удаляю его!");
                File.Delete(pathFileScreenshot);
            }
            await File.WriteAllBytesAsync(pathFileScreenshot, bytes);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            Debug.Log($"Done SaveScreenshotToFile: {pathFileScreenshot}");
            fileCreated = true;
        }
        catch (Exception ex)
        {
            Debug.Log($"Error SaveScreenshotToFile: {ex.Message}");
        }
    }

    async Task<Texture2D> DownloadTexture(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
                return null;
            }
            else
            {
                return DownloadHandlerTexture.GetContent(uwr);
            }
        }
    }

    public void CleanFolder()
    {
        var directory = new DirectoryInfo(FilesPath);
        // var files = directory.GetFiles($"*.{photoFormat}");
        var files = directory.GetFiles($"*.*");
        foreach (var file in files)
        {
            string FilePath = Path.Combine(FilesPath, file.Name);
            File.Delete(FilePath);
        }
    }

    public void CaptureAreaAndSave(RectTransform uiElement, string folderPath, string fileName,
                            int targetWidth = 1080, int targetHeight = 1920)
    {
        posterCreationFinished = false;
        StartCoroutine(CaptureUIAreaCoroutine(uiElement, folderPath, fileName, targetWidth, targetHeight));
    }

    private IEnumerator CaptureUIAreaCoroutine(RectTransform uiElement, string folderPath, string fileName,
                                             int targetWidth, int targetHeight)
    {
        yield return new WaitForEndOfFrame();

        Canvas canvas = uiElement.GetComponentInParent<Canvas>();
        Vector3[] worldCorners = new Vector3[4];
        uiElement.GetWorldCorners(worldCorners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, worldCorners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera ?? Camera.main, worldCorners[2]);
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            min = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]);
            max = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]);
        }

        Rect screenRect = new Rect(
            x: Mathf.Max(0, min.x),
            y: Mathf.Max(0, min.y),
            width: Mathf.Min(Screen.width, max.x - min.x),
            height: Mathf.Min(Screen.height, max.y - min.y)
        );

        Debug.Log($"Normalized screen rect: {screenRect}");

        if (screenRect.width <= 10 || screenRect.height <= 10)
        {
            Debug.LogError($"UI Element is too small or invalid: {screenRect}");
            yield break;
        }

        Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();
        Debug.Log(screenRect);
        Texture2D croppedTex = new Texture2D((int)screenRect.width, (int)screenRect.height, TextureFormat.RGBA32, false);
        Color[] pixels = screenTex.GetPixels((int)screenRect.x, (int)screenRect.y,
                                          (int)screenRect.width, (int)screenRect.height);
        croppedTex.SetPixels(pixels);
        croppedTex.Apply();
        Destroy(screenTex);

        Texture2D finalTex = croppedTex;
        if (targetWidth != croppedTex.width || targetHeight != croppedTex.height)
        {
            finalTex = ScaleTexture(croppedTex, croppedTex.width * 2, croppedTex.height * 2);//ScaleTexture(croppedTex, targetWidth, targetHeight);
            Destroy(croppedTex);
        }

        string fullPath = Path.Combine(folderPath, fileName + ".jpg");
        Directory.CreateDirectory(folderPath);
        File.WriteAllBytes(fullPath, finalTex.EncodeToJPG());
        Debug.Log($"Screenshot saved to: {fullPath}");

        GlobalChosesDataContainer.Instance.Poster = finalTex;
        posterCreationFinished = true;
        // Destroy(finalTex);
    }

    private Texture2D ScaleTexture(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D scaledTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        scaledTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        scaledTex.Apply();

        RenderTexture.ReleaseTemporary(rt);
        return scaledTex;
    }


    private static void SetAllCanvasesEnabled(bool enabled)
    {
        Canvas[] allCanvases = GameObject.FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
                canvas.enabled = enabled;
        }
    }


    private IEnumerator LoadQRCodeTexture(string svgUrl, System.Action<Texture2D> callback)
    {
        using (UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(svgUrl))
        {
            yield return textureRequest.SendWebRequest();

            if (textureRequest.result == UnityWebRequest.Result.ConnectionError ||
                textureRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Ошибка загрузки QR-кода: {textureRequest.error}");
                callback?.Invoke(null);
            }
            else
            {
                // Получаем текстуру из загрузчика
                Texture2D texture = DownloadHandlerTexture.GetContent(textureRequest);
                // Опционально: делаем текстуру читаемой (если нужно манипулировать пикселями)
                // texture = MakeTextureReadable(texture);

                callback?.Invoke(texture);
            }
        }
    }

    private Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
