using System;
using UnityEngine;

namespace BunnyCoffee
{
    public enum TableProduct
    {
        Coffee1,
        Coffee2,
    }
    public class TableController : MonoBehaviour
    {

        [Header("Position")]
        public Transform CustomerPosition;

        [Header("Products")]
        [SerializeField] Transform productCoffee1;
        [SerializeField] Transform productCoffee2;
        [SerializeField] ParticleSystem particles;

        public bool IsBusy { get; private set; }

        public void Reserve()
        {
            IsBusy = true;
        }

        public void Free()
        {
            IsBusy = false;
        }

        public void ShowProduct(TableProduct product)
        {
            switch (product)
            {
                case TableProduct.Coffee1:
                    productCoffee1.gameObject.SetActive(true);
                    break;
                case TableProduct.Coffee2:
                    productCoffee2.gameObject.SetActive(true);
                    break;
            }

            particles.Play();
        }

        public void HideProduct()
        {
            productCoffee1.gameObject.SetActive(false);
            productCoffee2.gameObject.SetActive(false);
            particles.Stop();
        }

        void OnDrawGizmos()
        {
            if (CustomerPosition == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, 2f * Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(CustomerPosition.position, 0.5f);
        }
    }
}