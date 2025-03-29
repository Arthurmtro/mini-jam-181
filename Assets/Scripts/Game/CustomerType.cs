using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    [Serializable]
    public struct CustomerType
    {
        public string Id;
        public string Name;

        [Tooltip("in seconds")]
        public int MinWaitTime;
        [Tooltip("in seconds")]
        public int MaxWaitTime;

        public Product.Quality MinQuality;
        public CustomerController Controller;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "BunnyCoffee/CustomerType")]
    public class CustomerTypeCollection : ScriptableObject
    {
        [SerializeField] private CustomerType[] types = Array.Empty<CustomerType>();

        public int Count => types.Length;
        public CustomerType[] All => types;
        public CustomerType AtIndex(int index) => types[index];
    }

}