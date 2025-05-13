using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalChosesDataContainer : MonoBehaviour
{
    public static GlobalChosesDataContainer Instance { get; private set; }
    public ShootingModeSelectionScreen.ShootingMode ShootingMode { get; set; }
    public LightingSchemeData LightingMode { get; set; }
    public List<Texture2D> Photos { get; set; }
    public List<Texture2D> SelectedPhotos { get; set; }
    public Texture2D QrCodePhotos { get; set; }
    public Texture2D Poster { get; set; }
    public Texture2D QrCodePoster { get; set; }
    public string CurrentFolderName = "";
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public bool YDFolderCreated = false;
    public bool isDoubleBuild;

    public int SelectedCategory { get; set; }
    public void Init()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Clean()
    {
        Photos = new List<Texture2D>();
        SelectedPhotos = new List<Texture2D>();
        QrCodePhotos = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        QrCodePoster = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        Poster = new Texture2D(509, 720, TextureFormat.ARGB32, false);
        Name = "";
        Surname = "";
        CurrentFolderName = "";
        YDFolderCreated = false;
    }
}