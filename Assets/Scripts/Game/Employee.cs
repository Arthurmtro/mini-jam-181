using UnityEngine;

namespace BunnyCoffee
{
    // public enum EmployeeStatus
    // {
    //     Idle,
    //     Waiting,
    //     MovingToCustomer,
    //     AskingCustomer,
    //     MovingToAppliance,
    //     Preparing,
    //     Delivering,
    // }

    public struct Employee
    {
        const float TimeAskingCustomer = 1;
        const float TimeDelivering = 1;

        public bool IsActive { get; private set; }
        public EmployeeStatus Status { get; private set; }
        public readonly bool NeedsTimerUpdate => Status == EmployeeStatus.AskingCustomer || Status == EmployeeStatus.Delivering;
        public float RemainingTime { get; private set; }
        public readonly bool TimeHasFinished => RemainingTime == 0;

        public void Reset()
        {
            Status = EmployeeStatus.Idle;
            RemainingTime = 0;
        }

        public void Activate()
        {
            Reset();
            IsActive = true;
        }

        public void Deactivate()
        {
            Reset();
            IsActive = false;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (!IsActive || !NeedsTimerUpdate)
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
                EmployeeStatus.Delivering => EmployeeStatus.Idle,
                _ => Status
            };
        }

        public void OnWaiting()
        {
            Status = EmployeeStatus.Waiting;
        }

        public void OnMovingToCustomer()
        {
            Status = EmployeeStatus.MovingToCustomer;
        }

        public void OnAskingCustomer()
        {
            Status = EmployeeStatus.AskingCustomer;
            RemainingTime = TimeAskingCustomer;
        }

        public void OnMovingToAppliance()
        {
            Status = EmployeeStatus.MovingToAppliance;
        }

        public void OnPreparing()
        {
            Status = EmployeeStatus.Preparing;
        }

        public void OnDelivering()
        {
            Status = EmployeeStatus.Delivering;
            RemainingTime = TimeDelivering;
        }
    }
}