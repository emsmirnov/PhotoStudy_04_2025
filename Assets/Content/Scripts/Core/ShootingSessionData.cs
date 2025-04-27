// using System.Collections.Generic;
// using UnityEngine;

// public class ShootingSessionData : MonoBehaviour
// {
//     public static ShootingSessionData Instance { get; private set; }

//     public ShootingModeSelectionScreen.ShootingMode CurrentMode { get; private set; }
//     public LightingScheme CurrentLightingScheme { get; private set; }
//     public LightingSchemeData CurrentScheme { get; private set; }
//     public List<Texture2D> CapturedPhotos { get; set; }

//     public void AddCapturedPhoto(Texture2D photo)
//     {
//         if (CapturedPhotos == null)
//             CapturedPhotos = new List<Texture2D>();

//         CapturedPhotos.Add(photo);
//     }

//     public void SetLightingScheme(LightingSchemeData scheme)
//     {
//         CurrentScheme = scheme;
//     }

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//     }

//     public void SetShootingMode(ShootingModeSelectionScreen.ShootingMode mode)
//     {
//         CurrentMode = mode;
//     }

//     public void SetLightingScheme(LightingScheme scheme)
//     {
//         CurrentLightingScheme = scheme;
//     }
// }