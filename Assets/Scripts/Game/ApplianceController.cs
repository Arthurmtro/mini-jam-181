using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{

    public class ApplianceController : MonoBehaviour
    {
        const float TimeToIdle = 5.0f;

        [Header("Position")]
        public Transform EmployeePosition;

        public bool IsActive { get; private set; }
        public ApplianceType Type;
        private int level;
        public ApplianceTypeLevel Level => Type.Levels[level];

        public ApplianceStatus Status { get; private set; }
        public bool IsFree => Status == ApplianceStatus.Idle;

        public string ProductId;
        public float RemainingTime { get; private set; }

        public void Reset()
        {
            Status = ApplianceStatus.Idle;
            ProductId = "";
            RemainingTime = 0;
        }

        public void Activate(ApplianceType type, int level)
        {
            Type = type;
            this.level = level;
            Reset();
            IsActive = true;
        }

        public void Deactivate()
        {
            Reset();
            IsActive = false;
        }

        public void Reserve()
        {
            Status = ApplianceStatus.Reserved;
        }

        public void StartPreparing(string productId)
        {
            Status = ApplianceStatus.Preparing;
            // if (Status != ApplianceStatus.Reserved)
            // {
            //     Debug.LogError("Cannot start preparing: invalid status: " + Status);
            //     return;
            // }

            // product = FindProduct(productId);
            // if (productOptional is not ApplianceTypeProduct product)
            // {
            //     Debug.LogError("Product not found for ID: " + productId);
            //     return;
            // }

            // ProductId = productId;
            // RemainingTime = product.Duration;
            // Status = ApplianceStatus.Preparing;
        }

        public void Free()
        {
            Status = ApplianceStatus.Idle;
        }

        public void UpdateController(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            RemainingTime = Mathf.Max(0, RemainingTime - deltaTime);
            if (RemainingTime != 0)
            {
                return;
            }

            Status = Status switch
            {
                ApplianceStatus.Preparing => ApplianceStatus.Finished,
                ApplianceStatus.Finished => ApplianceStatus.Idle,
                _ => Status
            };
            if (Status == ApplianceStatus.Finished)
            {
                RemainingTime = TimeToIdle;
            }
        }

        ApplianceTypeProduct FindProduct(string productId)
        {
            return Array.Find(Level.Products, product => product.ProductId == productId);
        }

        void OnDrawGizmos()
        {
            if (EmployeePosition == null)
            {
                return;
            }

            Handles.Label(transform.position, $"AP: {Status}");
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, 2f * Vector3.one);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(EmployeePosition.position, 1.5f);
        }
    }
}