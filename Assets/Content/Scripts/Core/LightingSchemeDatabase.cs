// using System.Collections.Generic;
// using UnityEngine;

// public class LightingSchemeDatabase : MonoBehaviour
// {
//     public static LightingSchemeDatabase Instance { get; private set; }

//     [SerializeField] private LightingSchemeData[] _allSchemes;

//     private Dictionary<string, LightingSchemeData> _schemeDictionary;

//     private void Awake()
//     {
//         Instance = this;
//         InitializeDictionary();
//     }

//     private void InitializeDictionary()
//     {
//         _schemeDictionary = new Dictionary<string, LightingSchemeData>();
//         foreach (var scheme in _allSchemes)
//         {
//             _schemeDictionary.Add(scheme.schemeName, scheme);
//         }
//     }

//     public LightingSchemeData GetScheme(string schemeName)
//     {
//         return _schemeDictionary.TryGetValue(schemeName, out var scheme) ? scheme : null;
//     }

//     public LightingSchemeData[] GetAllSchemes()
//     {
//         return _allSchemes;
//     }
// }