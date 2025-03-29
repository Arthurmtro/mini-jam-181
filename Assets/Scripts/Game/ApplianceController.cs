using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    public enum ApplianceStatus
    {
        Idle,
        Preparing,
        Finished,
    }

    public class ApplianceController : MonoBehaviour
    {
        const float TimeToFinish = 1.0f;

        [Header("Position")]
        public Transform EmployeePosition;

        public bool IsActive { get; private set; }
        public ApplianceType Type;
        private int level;
        public ApplianceTypeLevel Level => Type.Levels[level];

        public ApplianceStatus Status { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsFree => Status == ApplianceStatus.Idle && !IsBusy;

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
            IsBusy = true;
        }

        public void Free()
        {
            IsBusy = false;
        }

        // start statuses

        public void StartIdle()
        {
            Status = ApplianceStatus.Idle;
        }

        public void StartPreparing(string productId)
        {
            Status = ApplianceStatus.Preparing;
            RemainingTime = 3f;
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

        public void StartFinished()
        {
            Status = ApplianceStatus.Finished;
            RemainingTime = TimeToFinish;
        }

        public void UpdateController(float deltaTime)
        {

            switch (Status)
            {
                case ApplianceStatus.Idle:
                    UpdateWithStatusIdle();
                    break;
                case ApplianceStatus.Preparing:
                    UpdateWithStatusPreparing(deltaTime);
                    break;
                case ApplianceStatus.Finished:
                    UpdateWithStatusFinished(deltaTime);
                    break;
            }
        }

        public void UpdateWithStatusIdle()
        {
        }

        public void UpdateWithStatusPreparing(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartFinished();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusFinished(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartIdle();
                return;
            }

            UpdateTimer(deltaTime);
        }

        void UpdateTimer(float deltaTime)
        {
            RemainingTime = Mathf.Max(0, RemainingTime - deltaTime);
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

            Handles.Label(transform.position + 1.25f * Vector3.up, Status.ToString());
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, 1.25f * Vector3.one);
            Gizmos.color = !IsFree ? Color.red : Color.magenta;
            Gizmos.DrawSphere(EmployeePosition.position, 1f);
        }
    }
}