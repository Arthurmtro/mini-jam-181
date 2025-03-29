using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace BunnyCoffee
{
    [Serializable]
    public readonly struct CustomerOrder
    {
        public readonly string[] ProductIds;
        public readonly int Total;

        public CustomerOrder(string[] productIds, int total)
        {
            ProductIds = productIds;
            Total = total;
        }
    }

    public enum CustomerStatus
    {
        InQueue,
        MovingToBar,
        ThinkingOrder,
        WaitingEmployee,
        ExplainingOrder,
        WaitingOrder,
        MovingToTable,
        EnjoyingOrder,
        Leaving,
    }

    [Serializable]
    public struct Customer
    {
        const float TimeThinkingOrder = 1;
        const float TimeExplainingOrder = 1;
        const float TimeEnjoyingOrder = 1;

        public bool IsActive { get; private set; }
        public CustomerStatus Status;
        public readonly bool NeedsTimerUpdate => Status == CustomerStatus.ThinkingOrder || Status == CustomerStatus.ExplainingOrder || Status == CustomerStatus.EnjoyingOrder;
        public bool IsAttended { get; private set; }
        public readonly bool CanBeAttended => Status == CustomerStatus.WaitingEmployee && !IsAttended;
        public float RemainingTime { get; private set; }
        public readonly bool TimeHasFinished => RemainingTime == 0;

        public string ProductId { get; private set; }

        public void Reset()
        {
            Status = CustomerStatus.InQueue;
            IsAttended = false;
            RemainingTime = 0;
        }

        public void Activate(string productId)
        {
            ProductId = productId;
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
                CustomerStatus.ThinkingOrder => CustomerStatus.WaitingEmployee,
                CustomerStatus.ExplainingOrder => CustomerStatus.WaitingOrder,
                CustomerStatus.EnjoyingOrder => CustomerStatus.Leaving,
                _ => Status
            };
        }

        public void OnMovingToBar()
        {
            Status = CustomerStatus.MovingToBar;
        }

        public void OnThinkingOrder()
        {
            Status = CustomerStatus.ThinkingOrder;
            RemainingTime = TimeThinkingOrder;
        }

        public void OnWaitingEmployee()
        {
            Status = CustomerStatus.WaitingEmployee;
        }

        public void OnExplainingOrder()
        {
            Status = CustomerStatus.ExplainingOrder;
            RemainingTime = TimeExplainingOrder;
        }

        public void OnWaitingOrder()
        {
            Status = CustomerStatus.WaitingOrder;
        }

        public void OnMovingToTable()
        {
            Status = CustomerStatus.MovingToTable;
        }

        public void OnEnjoyingOrder()
        {
            Status = CustomerStatus.EnjoyingOrder;
            RemainingTime = TimeEnjoyingOrder;
        }

        public void OnLeaving()
        {
            Status = CustomerStatus.Leaving;
        }
    }

}
