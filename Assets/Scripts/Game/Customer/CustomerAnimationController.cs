using UnityEngine;

namespace BunnyCoffee
{
    public class CustomerAnimationController : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] Animator body;

        public void SetWalking(bool value)
        {
            body.SetBool("IsWalking", value);
        }

        public void SetSitting(bool value)
        {
            body.SetBool("IsSitting", value);
        }

        public void SetValues(bool isWalking, bool isSitting)
        {
            SetWalking(isWalking);
            SetSitting(isSitting);
        }
    }
}