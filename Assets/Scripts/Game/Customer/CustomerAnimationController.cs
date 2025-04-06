using UnityEngine;

namespace BunnyCoffee
{
    public class CustomerAnimationController : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] Animator animator;
        [SerializeField] BubbleController bubble;

        void Start()
        {
            HideBubble();
        }

        public void SetWalking(bool value)
        {
            animator.SetBool("IsWalking", value);
        }

        public void SetSitting(bool value)
        {
            animator.SetBool("IsSitting", value);
        }

        public void SetValues(bool isWalking, bool isSitting)
        {
            SetWalking(isWalking);
            SetSitting(isSitting);
        }

        public void ShowBubble(BubbleType type)
        {
            bubble.ShowBubble(type);
            animator.SetBool("ShowBubble", true);
        }

        public void HideBubble()
        {
            animator.SetBool("ShowBubble", false);
        }
    }
}