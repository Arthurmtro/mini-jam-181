using System;
using UnityEngine;

namespace BunnyCoffee
{
    public class ResourceManager : MonoBehaviour
    {
        ApplianceResource appliances;
        ProductResource products;
        CustomerResource customers;

        void Awake()
        {
            appliances = Resources.Load<ApplianceResource>("Appliances");
            products = Resources.Load<ProductResource>("Products");
            customers = Resources.Load<CustomerResource>("Customers");
        }

        public ApplianceType ApplianceAtIndex(int index) => appliances.AtIndex(index);
        public ApplianceType ApplianceById(string id) => appliances.ById(id);

        public Product ProductAtIndex(int index) => products.AtIndex(index);
        public Product ProductById(string id) => products.ById(id);

        public int CustomerCount => customers.Count;
        public CustomerType CustomerAtIndex(int index) => customers.AtIndex(index);
        public CustomerType CustomerById(string id) => customers.ById(id);
    }
}