using System;
using UnityEngine;

namespace BunnyCoffee
{
    public class ResourceManager : MonoBehaviour
    {
        [SerializeField] CustomerController bull1;
        [SerializeField] CustomerController bull2;
        [SerializeField] CustomerController sheep;


        [Header("Resources")]
        ApplianceType[] applianceTypes = new ApplianceType[1];
        Product[] productTypes = new Product[3];
        CustomerType[] customerTypes = new CustomerType[3];

        void Start()
        {
            // appliance types
            ApplianceTypeLevel[] levels = new ApplianceTypeLevel[5];
            levels[0] = new ApplianceTypeLevel
            {
                Price = 100,
                Products = new ApplianceTypeProduct[]
                {
                    new ApplianceTypeProduct { ProductId = "coffee-latte", Duration = 5 }
                }
            };
            levels[1] = new ApplianceTypeLevel
            {
                Price = 50,
                Products = new ApplianceTypeProduct[]
                {
                    new ApplianceTypeProduct { ProductId = "coffee-latte", Duration = 4 },
                    new ApplianceTypeProduct { ProductId = "coffee-americano", Duration = 3 }
                }
            };
            levels[2] = new ApplianceTypeLevel
            {
                Price = 100,
                Products = new ApplianceTypeProduct[]
                {
                    new ApplianceTypeProduct { ProductId = "coffee-latte", Duration = 3 },
                    new ApplianceTypeProduct { ProductId = "coffee-americano", Duration = 2 },
                    new ApplianceTypeProduct { ProductId = "coffee-expensive", Duration = 4 },
                }
            };
            levels[3] = new ApplianceTypeLevel
            {
                Price = 200,
                Products = new ApplianceTypeProduct[]
                {
                    new ApplianceTypeProduct { ProductId = "coffee-latte", Duration = 2 },
                    new ApplianceTypeProduct { ProductId = "coffee-americano", Duration = 1 },
                    new ApplianceTypeProduct { ProductId = "coffee-expensive", Duration = 3 },
                }
            };
            levels[4] = new ApplianceTypeLevel
            {
                Price = 400,
                Products = new ApplianceTypeProduct[]
                {
                    new ApplianceTypeProduct { ProductId = "coffee-latte", Duration = 1 },
                    new ApplianceTypeProduct { ProductId = "coffee-americano", Duration = 1 },
                    new ApplianceTypeProduct { ProductId = "coffee-expensive", Duration = 2 },
                }
            };
            applianceTypes[0] = new ApplianceType { Id = "coffee-machine-1", Price = 150, Levels = levels };
            applianceTypes[0].Levels = levels;

            // products
            productTypes[0] = new Product { Id = "coffee-latte", Price = 4 };
            productTypes[1] = new Product { Id = "coffee-americano", Price = 3 };
            productTypes[2] = new Product { Id = "coffee-expensive", Price = 10 };

            // customers
            customerTypes[0] = new CustomerType { Id = "bull-1", Name = "bull-1", Controller = bull1 };
            customerTypes[1] = new CustomerType { Id = "bull-2", Name = "bull-2", Controller = bull2 };
            customerTypes[2] = new CustomerType { Id = "sheep-1", Name = "sheep", Controller = sheep };

            Debug.Log(applianceTypes[0].Levels[0]);
        }

        public ApplianceType ApplianceAtIndex(int index) => applianceTypes[index];
        // public ApplianceType ApplianceById(string id) => applianceTypes == null ? null : Array.Find(applianceTypes, type => type.Id == id);
        public ApplianceType ApplianceById(string id) => applianceTypes[0];

        public Product ProductAtIndex(int index) => productTypes[index];
        public Product ProductById(string id) => Array.Find(productTypes, type => type.Id == id);

        public int CustomerCount => customerTypes.Length;
        public CustomerType CustomerAtIndex(int index) => customerTypes[index];
        public CustomerType CustomerById(string id) => Array.Find(customerTypes, type => type.Id == id);

        // public ApplianceTypeCollection ApplianceTypes => applianceTypes;
        // public ProductCollection ProductTypes => productTypes;
        // public CustomerTypeCollection CustomerTypes => customerTypes;
    }
}