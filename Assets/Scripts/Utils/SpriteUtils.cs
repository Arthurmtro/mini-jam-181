using System.Collections.Generic;
using UnityEngine;

namespace BunnyCoffee
{
  public enum SpriteSortingLayer
  {
    Default,
    Background1,
    Background2,
    Background3,
    Main1,
    Main2,
    Main3,
    Foreground1,
    Foreground2,
    Foreground3,
  }

  public static class SpriteUtils
  {
    public static void SetSortingLayer(SpriteRenderer sprite, SpriteSortingLayer sortingLayer)
    {
      sprite.sortingLayerName = sortingLayerNames[sortingLayer];
    }

    public static void Hide(SpriteRenderer sprite)
    {
      SetSortingLayer(sprite, SpriteSortingLayer.Default);
    }

    public static void Show(SpriteRenderer sprite, SpriteSortingLayer sortingLayer = SpriteSortingLayer.Main1)
    {
      SetSortingLayer(sprite, sortingLayer);
    }

    static readonly Dictionary<SpriteSortingLayer, string> sortingLayerNames = new()
    {
        { SpriteSortingLayer.Default, "Default" },
        { SpriteSortingLayer.Background1, "Background 1" },
        { SpriteSortingLayer.Background2, "Background 2" },
        { SpriteSortingLayer.Background3, "Background 3" },
        { SpriteSortingLayer.Main1, "Main 1" },
        { SpriteSortingLayer.Main2, "Main 2" },
        { SpriteSortingLayer.Main3, "Main 3" },
        { SpriteSortingLayer.Foreground1, "Foreground 1" },
        { SpriteSortingLayer.Foreground2, "Foreground 2" },
        { SpriteSortingLayer.Foreground3, "Foreground 3" },
    };
  }
}