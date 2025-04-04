using System;
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

  [CreateAssetMenu(fileName = "Customers", menuName = "BunnyCoffee/Customer")]
  public class CustomerResource : ScriptableObject
  {
    [SerializeField] private CustomerType[] types = Array.Empty<CustomerType>();

    public int Count => types.Length;
    public CustomerType[] All => types;
    public CustomerType AtIndex(int index) => types[index];
    public CustomerType ById(string id) => Array.Find(types, type => type.Id == id);
  }
}
