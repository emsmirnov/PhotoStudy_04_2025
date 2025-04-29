using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Linq;
using System;

public class QrGeneratorOnline : MonoBehaviour
{
    private string API_URL = "https://devdeployment.ru/2025.2.ar/api/v1/ar-link/upload";
    private string AUTH_TOKEN = "AXh3gf4MYS9wcnsCm8HeQ6";

    [System.Serializable]
    public class LinkData
    {
        public string value;
        public string qrUrl;
        public string qr;
    }

    [System.Serializable]
    public class ResponseData
    {
        public LinkData link;
        public int status;
    }

    [System.Serializable]
    public class ApiResponse
    {
        public bool success;
        public ResponseData data;
    }

    public void Init()
    {
        try
        {
            DirectoryInfo d = new DirectoryInfo(Application.streamingAssetsPath);
            var files = d.GetFiles("*.txt");
            if (files == null)
            {
                throw new System.Exception("NO CONFIGS IN STREAMING ASSETS");
            }
            var config = files.First((x) => x.Name.Contains("qronlineconfig"));
            var sr = config.OpenText();
            var yandexConfig = sr.ReadToEnd();
            var splitPathes = yandexConfig.Split(new char[] { '|' });
            API_URL = splitPathes[0];
            AUTH_TOKEN = splitPathes[1];
        }
        catch (Exception e)
        {
            Debug.Log("CHECK CONFIG QR_ONLINE" + e.Message);
        }

    }

    public void UploadPhotoLink(string photoUrl, System.Action<ApiResponse> callback = null)
    {
        Debug.Log(API_URL+ AUTH_TOKEN);
        StartCoroutine(UploadPhotoLinkCoroutine(photoUrl, callback));
    }

    private IEnumerator UploadPhotoLinkCoroutine(string photoUrl, System.Action<ApiResponse> callback)
    {
        // Создаем форму для multipart/form-data
        WWWForm form = new WWWForm();
        form.AddField("link", photoUrl);

        using (UnityWebRequest request = UnityWebRequest.Post(API_URL, form))
        {
            // Устанавливаем заголовки
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Upload-Token", AUTH_TOKEN);

            // Для POST с формой Content-Type устанавливается автоматически

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка: {request.error}");
                Debug.LogError($"Ответ: {request.downloadHandler.text}");
                callback?.Invoke(null);
                yield break;
            }

            ApiResponse response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);

            if (!response.success)
            {
                Debug.LogError("Сервер вернул success=false");
                callback?.Invoke(response);
                yield break;
            }

            // Загружаем QR-код если есть ссылка
            if (!string.IsNullOrEmpty(response.data.link.qr))
            {

                callback?.Invoke(response);
            }
            else
            {
                callback?.Invoke(null);
            }
        }
    }
}