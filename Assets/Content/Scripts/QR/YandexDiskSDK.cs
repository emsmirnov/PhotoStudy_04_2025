using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Project
{
    public class YandexDiskSDK : MonoBehaviour
    {
        private string token;
        private string sourcePath = "";
        private string pathDisk = "";
        private string pathDiskName;// = "Photo-Posters";
        private string urlToDownloadedFile = "";
        private bool folderCreated, fileUploaded, metaReceived;

        private string GetPathDisk(string folder, string date, string nameFile) => folder + "/" + date + "/" + nameFile;

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
                var config = files.First((x) => x.Name.Contains("yandex"));
                var sr = config.OpenText();
                var yandexConfig = sr.ReadToEnd();
                var splitPathes = yandexConfig.Split(new char[] { '|' });
                token = splitPathes[0];
                pathDiskName = splitPathes[1];
            }
            catch (Exception e)
            {
                Debug.Log("CHECK CONFIG YA_DISK " + e.Message);
            }

        }

        public async Task SaveOneFileToDisk(string sourcePath, string folderName, string nameFile, Action<string> callbackResultURL)
        {
            folderCreated = false;
            fileUploaded = false;
            metaReceived = false;

            urlToDownloadedFile = "";

            this.sourcePath = $"{sourcePath}/{nameFile}";
            // var folderName = DateTime.Now.ToSafeString().Replace(" ", "_").Replace(":", "-").Replace(".", "-");
            pathDisk = GetPathDisk(pathDiskName, folderName, nameFile);
            // Debug.Log("---> путь к файлу на диске, sourcePath: " + sourcePath + "      nameFile: " + nameFile);
            // Debug.Log("путь к файлу на диске, filePath: " + pathDisk);
            await CreateFolder(folderName);
            while (!folderCreated)
            {
                await Task.Yield();
            }
            await UploadingFileToDisk(pathDisk, folderName);
            while (!fileUploaded)
            {
                await Task.Yield();
            }
            Debug.Log(urlToDownloadedFile);
            callbackResultURL?.Invoke(urlToDownloadedFile);
        }

        public async Task SaveFilesToDisk(string sourcePath, string folderName, string[] nameFiles, Action<string> callbackResultURL)
        {
            folderCreated = false;
            fileUploaded = false;
            metaReceived = false;

            urlToDownloadedFile = "";
            if (!GlobalChosesDataContainer.Instance.YDFolderCreated)
            {
                await CreateFolder(folderName);
            }
            else
            {
                folderCreated = true;
            }
            while (!folderCreated)
            {
                await Task.Yield();
            }
            GlobalChosesDataContainer.Instance.YDFolderCreated = true;
            for (int i = 0; i < nameFiles.Length; i++)
            {

                this.sourcePath = $"{sourcePath}/{nameFiles[i]}";
                // var folderName = DateTime.Now.ToSafeString().Replace(" ", "_").Replace(":", "-").Replace(".", "-");
                pathDisk = GetPathDisk(pathDiskName, folderName, nameFiles[i]);
                // Debug.Log("---> путь к файлу на диске, sourcePath: " + sourcePath + "      nameFile: " + nameFile);
                // Debug.Log("путь к файлу на диске, filePath: " + pathDisk);
                fileUploaded = false;
                await UploadingFileToDisk(pathDisk, folderName);
                while (!fileUploaded)
                {
                    await Task.Yield();
                }
            }
            await GetMeta(pathDiskName + "/" + folderName);
            while (!metaReceived)
            {
                await Task.Yield();
            }

            callbackResultURL?.Invoke(urlToDownloadedFile);
        }

        private async Task CreateFolder(string folderName)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage createFolder = new HttpRequestMessage(HttpMethod.Put, "https://cloud-api.yandex.net/v1/disk/resources?path=" + pathDiskName + "/" + folderName);
                createFolder.Headers.Add("Accept", "application/json");
                createFolder.Headers.Add("Authorization", $"OAuth {token}");
                HttpResponseMessage responseFolder = await client.SendAsync(createFolder);
                responseFolder.EnsureSuccessStatusCode();

                await PublishFolder(folderName);
                folderCreated = true; ;
            }
            catch (Exception e)
            {
                Debug.Log("CreateFolder exc " + e);
                ScreenManager.Instance.EnableError();
                // DownloadPhotoScreen.EnableNetworkError.Invoke();
            }
        }

        private async Task PublishFolder(string folderName)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage createFolder = new HttpRequestMessage(HttpMethod.Put, "https://cloud-api.yandex.net/v1/disk/resources/publish?path=" + pathDiskName + "/" + folderName + "&overwrite=true");
                // HttpRequestMessage createFolder = new HttpRequestMessage(HttpMethod.Put, "https://cloud-api.yandex.net/v1/disk/resources?path=" + pathDiskName + "/" + folderName);
                createFolder.Headers.Add("Accept", "application/json");
                createFolder.Headers.Add("Authorization", $"OAuth {token}");
                HttpResponseMessage responseFolder = await client.SendAsync(createFolder);
                responseFolder.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Debug.Log("PublishFolder exc " + e);
                ScreenManager.Instance.EnableError();
            }
        }



        private async Task UploadingFileToDisk(string path, string folderName)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://cloud-api.yandex.net/v1/disk/resources/upload?path=" + path + "&overwrite=true");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"OAuth {token}");

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log(responseBody);
                int indexOfSubstring = responseBody.IndexOf("https");
                responseBody = responseBody.Substring(indexOfSubstring);
                int indexOfSubstring2 = responseBody.IndexOf("\"");
                responseBody = responseBody.Substring(0, indexOfSubstring2);

                await HTTP_PUT(responseBody, sourcePath);
                await Task.Delay(100);
                // await PublishingFile(path);
                fileUploaded = true;

            }
            catch (Exception e)
            {
                Debug.Log("UploadingFileToDisk exc " + e);
                ScreenManager.Instance.EnableError();
            }
        }

        private async Task PublishingFile(string path)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "https://cloud-api.yandex.net/v1/disk/resources/publish?path=" + path + "&overwrite=true");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"OAuth {token}");
                Debug.Log(request);
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                await Task.Yield();
                await response.Content.ReadAsStringAsync();
                await Task.Delay(200);
            }
            catch (Exception e)
            {
                Debug.Log("PublishingFile exc " + e.Message);
                ScreenManager.Instance.EnableError();
            }
        }

        private async Task GetMeta(string path)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://cloud-api.yandex.net/v1/disk/resources?path=" + path);// "&fields=public_url");

                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"OAuth {token}");
                Debug.Log(request);
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log(responseBody);
                int indexOfSubstring = responseBody.IndexOf("https");
                responseBody = responseBody.Substring(indexOfSubstring);
                int indexOfSubstring2 = responseBody.IndexOf("\"");
                responseBody = responseBody.Substring(0, indexOfSubstring2);
                urlToDownloadedFile = responseBody;
                metaReceived = true;
            }
            catch (Exception e)
            {
                Debug.Log("GetMeta " + e);
                ScreenManager.Instance.EnableError();
            }
        }

        // HTTP_PUT Function ����� �������� ����� �� url
        private async Task HTTP_PUT(string Url, string filePath)
        {
            try
            {

                using (WebClient client = new WebClient())
                {
                    var resp = await client.UploadFileTaskAsync(new Uri(Url), filePath);
                }
            }
            catch (Exception e)
            {
                Debug.Log("HTTP_PUT --> " + e);
                // DownloadPhotoScreen.EnableNetworkError.Invoke();
            }
        }

        /*private async Task testAsync()
        {
            // ���������� � �����
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://cloud-api.yandex.net/v1/disk");

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"OAuth {_token}");

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log("���������� � �����");
            Debug.Log(responseBody);
        }*/

        public async Task<bool> CheckConnection()
        {
            Debug.Log("Start check " + DateTime.Now.ToString());
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Not connected " + DateTime.Now.ToString());
                return false;
            }
            else
            {

                WWW www = new WWW("http://ya.ru");
                // yield return www;
                while (!www.isDone)
                    await Task.Yield();
                if (www.error != null)
                {
                    Debug.Log("Request failed " + DateTime.Now.ToString());
                    return false;
                }
                else
                {
                    Debug.Log("Request done " + DateTime.Now.ToString());
                    return true;
                }
                // bool connection = false;
                // StartCoroutine(checkInternetConnection((isConnected) =>
                // {
                //     connection = isConnected;
                // }));
                // return connection;
            }
        }

        IEnumerator checkInternetConnection(Action<bool> action)
        {
            WWW www = new WWW("http://ya.ru");
            yield return www;
            if (www.error != null)
            {
                action(false);
            }
            else
            {
                action(true);
            }
        }
    }
}
