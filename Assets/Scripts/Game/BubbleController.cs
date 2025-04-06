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

    public class BubbleController : MonoBehaviour
    {

        [Header("Params")]
        [SerializeField] SpriteRenderer iconThinking;
        [SerializeField] SpriteRenderer iconCoffee1;
        [SerializeField] SpriteRenderer iconCoffee2;
        [SerializeField] SpriteRenderer iconCoin;
        [SerializeField] SpriteRenderer iconHappy;

        public void ShowBubble(BubbleType type)
        {
            SpriteUtils.Hide(iconThinking);
            SpriteUtils.Hide(iconCoffee1);
            SpriteUtils.Hide(iconCoffee2);
            SpriteUtils.Hide(iconCoin);
            SpriteUtils.Hide(iconHappy);
            switch (type)
            {
                case BubbleType.Thinking:
                    SpriteUtils.Show(iconThinking, SpriteSortingLayer.Foreground2);
                    break;
                case BubbleType.Coffee1:
                    SpriteUtils.Show(iconCoffee1, SpriteSortingLayer.Foreground2);
                    break;
                case BubbleType.Coffee2:
                    SpriteUtils.Show(iconCoffee2, SpriteSortingLayer.Foreground2);
                    break;
                case BubbleType.Coin:
                    SpriteUtils.Show(iconCoin, SpriteSortingLayer.Foreground2);
                    break;
                case BubbleType.Happy:
                    SpriteUtils.Show(iconHappy, SpriteSortingLayer.Foreground2);
                    break;
            }
        }

        public void HideBubble()
        {
        }
    }
}