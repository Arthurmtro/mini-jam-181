using UnityEngine;

namespace BunnyCoffee
{
    public class DecorationController : MonoBehaviour
    {
        private readonly Vector3 hiddenPosition = new(1000, 1000, 1000);

        [SerializeField] ParticleSystem particles;

        [SerializeField] int price;
        public int Price => price;

        public bool IsActive { get; private set; }

        Vector3 realPosition;

        void Awake()
        {
            realPosition = transform.position;
            transform.position = hiddenPosition;
        }

        public void Activate()
        {
            transform.position = realPosition;
            IsActive = true;

            if (particles != null)
            {
                particles.Play();
            }
        }

        public void Deactivate()
        {
            transform.position = hiddenPosition;
            IsActive = false;

            if (particles != null)
            {
                particles.Stop();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(realPosition, 0.15f);
        }
    }
}