using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

namespace BunnyCoffee
{
    public enum BubbleType
    {
        Thinking,
        Coffee1,
        Coffee2,
        Coin,
        Happy,
    }

    public class CustomerAnimationController : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer spriteBubbleIcon;
        [SerializeField] Transform iconThinking;
        [SerializeField] Transform iconCoffee1;
        [SerializeField] Transform iconCoffee2;
        [SerializeField] Transform iconCoin;
        [SerializeField] Transform iconHappy;

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

        public void ShowBubble(Sprite image)
        {
            spriteBubbleIcon.sprite = image;
            animator.SetBool("ShowBubble", true);
        }

        public void ShowBubble(BubbleType type)
        {
            spriteBubbleIcon.sprite = null;
            iconThinking.gameObject.SetActive(false);
            iconCoffee1.gameObject.SetActive(false);
            iconCoffee2.gameObject.SetActive(false);
            iconCoin.gameObject.SetActive(false);
            iconHappy.gameObject.SetActive(false);
            switch (type)
            {
                case BubbleType.Thinking:
                    iconThinking.gameObject.SetActive(true);
                    break;
                case BubbleType.Coffee1:
                    iconCoffee1.gameObject.SetActive(true);
                    break;
                case BubbleType.Coffee2:
                    iconCoffee2.gameObject.SetActive(true);
                    break;
                case BubbleType.Coin:
                    iconCoin.gameObject.SetActive(true);
                    break;
                case BubbleType.Happy:
                    iconHappy.gameObject.SetActive(true);
                    break;
            }
            animator.SetBool("ShowBubble", true);
        }

        public void HideBubble()
        {
            spriteBubbleIcon.sprite = null;
            animator.SetBool("ShowBubble", false);
        }
    }
}