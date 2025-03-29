using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    #region resource

    [Serializable]
    public struct ApplianceTypeProduct
    {
        public string ProductId;
        public Product.Quality MinQuality;
        public Product.Quality MaxQuality;
        [Tooltip("in seconds")]
        public int Duration;
    }

    [Serializable]
    public struct ApplianceTypeLevel
    {
        public string Name;
        public ApplianceTypeProduct[] Products;
    }

    [Serializable]
    public struct ApplianceType
    {
        public string Id;
        public string Name;
        public ApplianceTypeLevel[] Levels;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "BunnyCoffee/ApplianceType")]
    public class ApplianceTypeCollection : ScriptableObject
    {
        [SerializeField] private ApplianceType[] types = Array.Empty<ApplianceType>();

        public int Count => types.Length;
        public ApplianceType[] All => types;
        public ApplianceType AtIndex(int index) => types[index];
    }

    #endregion

    // public enum ApplianceStatus
    // {
    //     Idle,
    //     Reserved,
    //     Preparing,
    //     Finished,
    // }

    // [Serializable]
    // public struct Appliance
    // {
    //     const float TimeToIdle = 5.0f;

    //     public bool IsActive { get; private set; }
    //     public ApplianceType Type;
    //     private int level;
    //     public readonly ApplianceTypeLevel Level => Type.Levels[level];

    //     public ApplianceStatus Status { get; private set; }
    //     public readonly bool NeedsTimerUpdate => Status != ApplianceStatus.Idle && Status != ApplianceStatus.Reserved;
    //     public readonly bool CanBeReserved => Status == ApplianceStatus.Reserved;

    //     public string ProductId;
    //     public float RemainingTime { get; private set; }

    //     // public Appliance()
    //     // {
    //     //     IsActive = false;
    //     //     Type = type;
    //     //     this.level = level;
    //     //     Status = ApplianceStatus.Idle;
    //     //     ProductId = "";
    //     //     RemainingTime = 0;
    //     // }

    //     public void Reset()
    //     {
    //         Status = ApplianceStatus.Idle;
    //         ProductId = "";
    //         RemainingTime = 0;
    //     }

    //     public void Activate(ApplianceType type, int level)
    //     {
    //         Type = type;
    //         this.level = level;
    //         Reset();
    //         IsActive = true;
    //     }

    //     public void Deactivate()
    //     {
    //         Reset();
    //         IsActive = false;
    //     }

    //     public void Reserve()
    //     {
    //         Status = ApplianceStatus.Reserved;
    //     }

    //     public void StartPreparing(string productId)
    //     {
    //         if (Status != ApplianceStatus.Reserved)
    //         {
    //             Debug.LogError("Cannot start preparing: invalid status: " + Status);
    //             return;
    //         }

    //         ApplianceTypeProduct? productOptional = FindProduct(productId);
    //         if (productOptional is not ApplianceTypeProduct product)
    //         {
    //             Debug.LogError("Product not found for ID: " + productId);
    //             return;
    //         }

    //         ProductId = productId;
    //         RemainingTime = product.Duration;
    //         Status = ApplianceStatus.Preparing;
    //     }

    //     public void UpdateTimer(float deltaTime)
    //     {
    //         if (!IsActive)
    //         {
    //             return;
    //         }

    //         RemainingTime = Mathf.Max(0, RemainingTime - deltaTime);
    //         if (RemainingTime != 0)
    //         {
    //             return;
    //         }

    //         Status = Status switch
    //         {
    //             ApplianceStatus.Preparing => ApplianceStatus.Finished,
    //             ApplianceStatus.Finished => ApplianceStatus.Idle,
    //             _ => Status
    //         };
    //         if (Status == ApplianceStatus.Finished)
    //         {
    //             RemainingTime = TimeToIdle;
    //         }
    //     }

    //     private readonly ApplianceTypeProduct? FindProduct(string productId)
    //     {
    //         return Level.Products.FirstOrDefault(product => product.ProductId == productId);
    //     }
    // }
}