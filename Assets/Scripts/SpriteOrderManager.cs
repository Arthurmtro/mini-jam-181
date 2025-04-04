using System.Linq;
using UnityEngine;

public class SpriteOrderManager : MonoBehaviour
{
    [SerializeField] float precision = 100;
    [SerializeField] string[] layers;

    SpriteRenderer[] staticSprites;
    SpriteRenderer[] dynamicSprites;

    public void Initialize()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>()
            .Where(sprite => layers.Contains(sprite.sortingLayerName))
            .ToArray();
        staticSprites = sprites.Where(c => c.gameObject.isStatic).ToArray();
        dynamicSprites = sprites.Where(c => !c.gameObject.isStatic).ToArray();
        Debug.Log(staticSprites.Length);
        Debug.Log(dynamicSprites.Length);

        foreach (var sprite in staticSprites)
        {
            UpdateSprite(sprite);
        }
    }

    void Update()
    {
        foreach (var sprite in dynamicSprites)
        {
            UpdateSprite(sprite);
        }
    }

    void UpdateSprite(SpriteRenderer sprite)
    {
        int sortingOrder = (int)(-sprite.transform.position.y * precision);
        sprite.sortingOrder = sortingOrder;
    }
}


