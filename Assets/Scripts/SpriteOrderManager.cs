using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    public class SpriteOrderManager : MonoBehaviour
    {
        [SerializeField] float precision = 100;
        [SerializeField] string[] layers;

        SpriteRenderer[] staticSprites;
        float[] staticSpritesOffsets;
        SpriteRenderer[] dynamicSprites;
        float[] dynamicSpritesOffsets;

        public void Initialize()
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>()
                .Where(sprite => layers.Contains(sprite.sortingLayerName))
                .ToArray();
            staticSprites = sprites.Where(c => c.gameObject.isStatic).ToArray();
            staticSpritesOffsets = new float[staticSprites.Length];

            for (int i = 0; i < staticSprites.Length; i++)
            {
                var sprite = staticSprites[i];
                staticSpritesOffsets[i] = GetSpriteOffset(sprite.gameObject);
                float position = sprite.transform.position.y + staticSpritesOffsets[i];

                UpdateSprite(sprite, position);
            }

            dynamicSprites = sprites.Where(c => !c.gameObject.isStatic).ToArray();
            dynamicSpritesOffsets = new float[dynamicSprites.Length];
            for (int i = 0; i < dynamicSprites.Length; i++)
            {
                dynamicSpritesOffsets[i] = GetSpriteOffset(dynamicSprites[i].gameObject);
            }
        }

        void Update()
        {
            for (int i = 0; i < dynamicSprites.Length; i++)
            {
                var sprite = dynamicSprites[i];
                float position = sprite.transform.position.y + dynamicSpritesOffsets[i];
                UpdateSprite(sprite, position);
            }
        }

        void UpdateSprite(SpriteRenderer sprite, float position)
        {
            int sortingOrder = (int)(-position * precision);
            sprite.sortingOrder = sortingOrder;
        }

        float GetSpriteOffset(GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.CompareTag("SpriteOrderPosition"))
                {
                    return child.localPosition.y;
                }
            }

            return 0;
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Initialize();
            }

            Gizmos.color = new Color(0.9f, 0.9f, 0.9f);
            for (int i = 0; i < staticSprites.Length; i++)
            {
                var sprite = staticSprites[i];
                float spriteWidth = sprite.bounds.size.x;
                float position = sprite.transform.position.y + staticSpritesOffsets[i];
                Gizmos.DrawLine(new(sprite.transform.position.x - spriteWidth / 2, position, sprite.transform.position.z),
                                 new(sprite.transform.position.x + spriteWidth / 2, position, sprite.transform.position.z));
                int sortingOrder = (int)(-position * precision);
                GizmosUtils.DrawCenteredText(new Vector3(sprite.transform.position.x, position - 0.25f, sprite.transform.position.z), sortingOrder.ToString());
            }

            for (int i = 0; i < dynamicSprites.Length; i++)
            {
                var sprite = dynamicSprites[i];
                float spriteWidth = sprite.bounds.size.x;
                float position = sprite.transform.position.y + dynamicSpritesOffsets[i];
                Gizmos.DrawLine(new(sprite.transform.position.x - spriteWidth / 2, position, sprite.transform.position.z),
                                 new(sprite.transform.position.x + spriteWidth / 2, position, sprite.transform.position.z));
                int sortingOrder = (int)(-position * precision);
                GizmosUtils.DrawCenteredText(new Vector3(sprite.transform.position.x, position - 0.25f, sprite.transform.position.z), sortingOrder.ToString());
            }
        }
    }
}