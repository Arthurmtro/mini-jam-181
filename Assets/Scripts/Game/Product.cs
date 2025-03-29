using System;
using System.Data.Common;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{

    [Serializable]
    public struct Product
    {
        public string Id;
        public string Name;
        public int Price;
        public GameObject View;

        public enum Quality
        {
            Bad = 0,
            Medium = 1,
            Good = 2,
            Great = 4,
            Perfect = 5
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "BunnyCoffee/Product")]
    public class ProductCollection : ScriptableObject
    {
        [SerializeField] private Product[] products = Array.Empty<Product>();

        public int Count => products.Length;
        public Product[] All => products;
        public Product AtIndex(int index) => products[index];
        public Product ById(string id) => Array.Find(products, type => type.Id == id);
    }
}
