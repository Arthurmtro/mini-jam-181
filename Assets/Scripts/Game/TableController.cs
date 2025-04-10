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
        [SerializeField] SpriteRenderer productCoffee1;
        [SerializeField] SpriteRenderer productCoffee2;
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
                    SpriteUtils.Show(productCoffee1);
                    break;
                case TableProduct.Coffee2:
                    SpriteUtils.Show(productCoffee2);
                    break;
            }

            particles.Play();
        }

        public void HideProduct()
        {
            SpriteUtils.Hide(productCoffee1);
            SpriteUtils.Hide(productCoffee2);
            particles.Stop();
        }

        void OnDrawGizmos()
        {
            if (CustomerPosition == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(CustomerPosition.position, 0.25f);
        }
    }
}