using System;
using UnityEngine;

public class PhotosContainer : MonoBehaviour
{
    public Texture2D[] InitPhotos;
    public Texture2D defaultQR;
    public Texture2D SelectedPhoto;
    public Texture2D SelectedPhotoWithFrame;
    public Texture2D QRCode;
    public DateTime DateTime;
    public delegate bool BoolDelegate();
    public delegate void VoidDelegate();
    public static BoolDelegate PhotosLoaded;
    private bool photosLoaded;

    public void Init()
    {
        DateTime = new DateTime();
        InitPhotos = new Texture2D[5];
        PhotosLoaded = () => { return photosLoaded; };
        Loader.filesLoaded += (s) =>
        {
            if (s > DateTime)
            {
                DateTime = s;
                photosLoaded = true;
            }

        };
    }



    public void Clean()
    {
        foreach (Texture2D photo in InitPhotos)
        {
            Destroy(photo);
        }
        Destroy(SelectedPhoto);
        Destroy(SelectedPhotoWithFrame);
        Destroy(QRCode);
        photosLoaded = false;
    }
}
