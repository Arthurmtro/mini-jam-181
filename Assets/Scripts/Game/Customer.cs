using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace BunnyCoffee
{
    [Serializable]
    public readonly struct CustomerOrder
    {
        public readonly string[] ProductIds;
        public readonly int Total;

        public CustomerOrder(string[] productIds, int total)
        {
            ProductIds = productIds;
            Total = total;
        }
    }
}
