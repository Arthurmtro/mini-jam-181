using System;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    [Serializable]
    public class ApplianceTypeProduct
    {
        public string ProductId;
        public Product.Quality MinQuality;
        public Product.Quality MaxQuality;
        [Tooltip("in seconds")]
        public int Duration;
    }

    [Serializable]
    public class ApplianceTypeLevel
    {
        public string Name;
        public int Price;
        public ApplianceTypeProduct[] Products;
    }

    [Serializable]
    public class ApplianceType
    {
        public string Id;
        public string Name;
        public int Price;
        public ApplianceTypeLevel[] Levels;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(fileName = "Assets/Collections/Appliances.asset", menuName = "BunnyCoffee/ApplianceType")]
    public class ApplianceTypeCollection : ScriptableObject
    {
        [SerializeField] private ApplianceType[] types = Array.Empty<ApplianceType>();

        public int Count => types.Length;
        public ApplianceType[] All => types;
        public ApplianceType AtIndex(int index) => types[index];
        public ApplianceType ById(string id) => Array.Find(types, type => type.Id == id);
    }

}