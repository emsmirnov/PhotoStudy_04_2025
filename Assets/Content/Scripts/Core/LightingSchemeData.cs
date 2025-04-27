using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LightingScheme", menuName = "Photography/Lighting Scheme")]
public class LightingSchemeData : ScriptableObject
{
    [Header("Basic Info")]
    public string schemeName;
    // public Sprite previewIcon;
    // [TextArea(3, 5)] public string description;

    [Header("Visual References")]
    public Sprite diagramSprite;
    // public Sprite referencePhoto;
    // public Sprite fullSizeDiagram; 

    // [Header("OSC Parameters")]
    // public OSCLightingCommand[] oscCommands;

    // [Serializable]
    // public class OSCLightingCommand
    // {
    //     public string deviceAddress; 
    //     public string parameter;    
    //     public float value;        
    // }

    // [Header("Camera Settings")]
    // public string exposureSettings; 
    // public string aperture;
    // public string shutterSpeed;
    // public string iso;
}