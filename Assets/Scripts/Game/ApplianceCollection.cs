using System;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    [Serializable]
    public struct ApplianceTypeProduct
    {
        public string ProductId;
        public Product.Quality MinQuality;
        public Product.Quality MaxQuality;
        [Tooltip("in seconds")]
        public int Duration;
    }

    [Serializable]
    public struct ApplianceTypeLevel
    {
        public string Name;
        public ApplianceTypeProduct[] Products;
    }

    [Serializable]
    public struct ApplianceType
    {
        public string Id;
        public string Name;
        public ApplianceTypeLevel[] Levels;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "BunnyCoffee/ApplianceType")]
    public class ApplianceTypeCollection : ScriptableObject
    {
        [SerializeField] private ApplianceType[] types = Array.Empty<ApplianceType>();

        public int Count => types.Length;
        public ApplianceType[] All => types;
        public ApplianceType AtIndex(int index) => types[index];
        public ApplianceType ById(string id) => Array.Find(types, type => type.Id == id);
    }

}