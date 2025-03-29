using System;
using System.Linq;
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

        [Header("Resources")]
        [SerializeField] ResourceManager resources;

        [Header("Position")]
        public Transform EmployeePosition;

        public string TypeId;
        private ApplianceType type;
        public int Level;
        private ApplianceTypeLevel? currentLevel => type.Levels != null && Level < type.Levels.Length ? type.Levels[Level] : null;
        private ApplianceTypeLevel? nextLevel => Level + 1 < type.Levels.Length ? type.Levels[Level + 1] : null;
        public bool CanLevelUp => IsActive && nextLevel.HasValue;

        public bool IsActive { get; private set; }

        public ApplianceStatus Status { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsFree => IsActive && Status == ApplianceStatus.Idle && !IsBusy;

        public float RemainingTime { get; private set; }

        void Awake()
        {
            type = resources.ApplianceTypes.ById(TypeId);
        }

        public void Reset()
        {
            Status = ApplianceStatus.Idle;
            RemainingTime = 0;
        }

        public void Activate(int level = 0)
        {
            Level = Math.Min(type.Levels.Length - 1, level);
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

        public bool CanPrepare(string productId)
        {
            if (!IsFree)
            {
                return false;
            }

            return currentLevel?.Products.Any(product => product.ProductId == productId) ?? false;
        }

        public void LevelUp()
        {
            if (!CanLevelUp)
            {
                return;
            }

            Level++;
        }

        // start statuses

        public void StartIdle()
        {
            Status = ApplianceStatus.Idle;
        }

        public float StartPreparing(string productId)
        {
            ApplianceTypeProduct levelProduct = FindProduct(productId);
            Status = ApplianceStatus.Preparing;
            RemainingTime = levelProduct.Duration;

            return levelProduct.Duration;
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
            return Array.Find(currentLevel?.Products, product => product.ProductId == productId);
        }

        void OnDrawGizmos()
        {
            if (EmployeePosition == null)
            {
                return;
            }


            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            Handles.Label(transform.position + 1.25f * Vector3.up, Status.ToString(), centeredStyle);
            Handles.Label(transform.position - 1.25f * Vector3.up, $"{type.Name} ({currentLevel?.Name ?? "-"})", centeredStyle);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, 1.25f * Vector3.one);
            Gizmos.color = !IsFree ? Color.red : Color.magenta;
            Gizmos.DrawSphere(EmployeePosition.position, 1f);
        }
    }
}