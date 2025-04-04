using System;
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

  [CreateAssetMenu(fileName = "Products", menuName = "BunnyCoffee/Product")]
  public class ProductResource : ScriptableObject
  {
    [SerializeField] private Product[] types = Array.Empty<Product>();

    public int Count => types.Length;
    public Product[] All => types;
    public Product AtIndex(int index) => types[index];
    public Product ById(string id) => Array.Find(types, type => type.Id == id);
  }
}