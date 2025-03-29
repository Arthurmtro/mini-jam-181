using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BunnyCoffee
{

    public enum CustomerStatus
    {
        Idle,
        MovingToQueue,
        InQueue,
        MovingToBar,
        ThinkingOrder,
        WaitingEmployee,
        ExplainingOrder,
        WaitingOrder,
        ReceivingOrder,
        WaitingTable,
        MovingToTable,
        EnjoyingOrder,
        Leaving,
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerController : MonoBehaviour
    {
        const float TimeToThinkOrder = 1;
        const float TimeToExplainOrder = 1;
        const float TimeToReceiveOrder = 1;
        const float TimeToEnjoyOrder = 1;

        public bool IsActive { get; private set; }
        public CustomerStatus Status;
        public bool NeedsTimerUpdate => Status == CustomerStatus.ThinkingOrder || Status == CustomerStatus.ExplainingOrder || Status == CustomerStatus.EnjoyingOrder;
        public bool IsAttended { get; private set; }
        public bool CanBeAttended => Status == CustomerStatus.WaitingEmployee && !IsAttended;
        public float RemainingTime { get; private set; }
        public bool TimeHasFinished => RemainingTime == 0;
        public BarPosition BarPosition { get; private set; }
        Vector3 inactivePosition;

        public string ProductId { get; private set; }
        public TableController Table { get; private set; }

        NavMeshAgent agent;
        public bool HasReachedDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            inactivePosition = transform.position;
        }

        public void Reset()
        {
            Status = CustomerStatus.Idle;
            IsAttended = false;
            RemainingTime = 0;
        }

        public void Deactivate()
        {
            Reset();
            IsActive = false;
            agent.isStopped = true;
        }

        public void ActivateToQueue(Vector3 queuePosition, string productId)
        {
            ProductId = productId;
            Reset();
            IsActive = true;
            agent.isStopped = false;
            Status = CustomerStatus.MovingToQueue;
            MoveToTarget(queuePosition);
        }

        public void ActivateToBar(BarPosition barPosition, string productId)
        {
            ProductId = productId;
            Reset();
            IsActive = true;
            Status = CustomerStatus.MovingToBar;
            agent.isStopped = false;
            BarPosition = barPosition;
            BarPosition.Reserve();
            MoveToTarget(barPosition.Customer);
        }

        public void Attend()
        {
            if (CanBeAttended)
            {
                IsAttended = true;
            }
        }

        // State init

        public void StartMovingToBar()
        {
            Status = CustomerStatus.MovingToBar;
        }

        public void StartThinkingOrder()
        {
            Status = CustomerStatus.ThinkingOrder;
            RemainingTime = TimeToThinkOrder;
        }

        public void StartWaitingEmployee()
        {
            Status = CustomerStatus.WaitingEmployee;
        }

        public void StartExplainingOrder()
        {
            Status = CustomerStatus.ExplainingOrder;
            RemainingTime = TimeToExplainOrder;
        }

        public void StartWaitingOrder()
        {
            Status = CustomerStatus.WaitingOrder;
        }

        public void StartReceivingOrder()
        {
            Status = CustomerStatus.ReceivingOrder;
            RemainingTime = TimeToReceiveOrder;
        }

        public void StartWaitingTable()
        {
            Status = CustomerStatus.WaitingTable;
        }

        public void StartMovingToTable()
        {
            if (Table == null)
            {
                Debug.LogError("Cannot start moving to table, table is null");
                return;
            }

            BarPosition.Free();
            BarPosition = null;

            Status = CustomerStatus.MovingToTable;
            MoveToTarget(Table.CustomerPosition.position);
        }

        public void StartEnjoyingOrder()
        {
            Status = CustomerStatus.EnjoyingOrder;
            RemainingTime = TimeToEnjoyOrder;
        }

        public void StartLeaving()
        {
            Status = CustomerStatus.Leaving;
            MoveToTarget(inactivePosition);

            if (Table != null)
            {
                Table.Free();
            }
        }

        // update by status

        public void UpdateController(float deltaTime, GameManager game)
        {
            if (!IsActive)
            {
                return;
            }

            switch (Status)
            {
                case CustomerStatus.Idle:
                    UpdateWithStatusIdle(deltaTime);
                    break;
                case CustomerStatus.MovingToQueue:
                    UpdateWithStatusMovingToQueue(deltaTime);
                    break;
                case CustomerStatus.InQueue:
                    UpdateWithStatusInQueue(deltaTime);
                    break;
                case CustomerStatus.MovingToBar:
                    UpdateWithStatusMovingToBar(deltaTime);
                    break;
                case CustomerStatus.ThinkingOrder:
                    UpdateWithStatusThinkingOrder(deltaTime);
                    break;
                case CustomerStatus.WaitingEmployee:
                    UpdateWithStatusWaitingEmployee(deltaTime);
                    break;
                case CustomerStatus.ExplainingOrder:
                    UpdateWithStatusExplainingOrder(deltaTime);
                    break;
                case CustomerStatus.WaitingOrder:
                    UpdateWithStatusWaitingOrder(deltaTime);
                    break;
                case CustomerStatus.ReceivingOrder:
                    UpdateWithStatusReceivingOrder(deltaTime);
                    break;
                case CustomerStatus.WaitingTable:
                    UpdateWithStatusWaitingTable(game);
                    break;
                case CustomerStatus.MovingToTable:
                    UpdateWithStatusMovingToTable(deltaTime);
                    break;
                case CustomerStatus.EnjoyingOrder:
                    UpdateWithStatusEnjoyingOrder(deltaTime);
                    break;
                case CustomerStatus.Leaving:
                    UpdateWithStatusLeaving();
                    break;
            }
        }

        public void UpdateWithStatusIdle(float deltaTime)
        {
        }

        public void UpdateWithStatusMovingToQueue(float deltaTime)
        {
            if (HasReachedDestination)
            {
                Status = CustomerStatus.InQueue;
            }
        }

        public void UpdateWithStatusInQueue(float deltaTime)
        {
            // Logic for InQueue status
        }

        public void UpdateWithStatusMovingToBar(float deltaTime)
        {
            if (HasReachedDestination)
            {
                StartThinkingOrder();
            }
        }

        public void UpdateWithStatusThinkingOrder(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartWaitingEmployee();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusWaitingEmployee(float deltaTime)
        {
        }

        public void UpdateWithStatusExplainingOrder(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartWaitingOrder();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusWaitingOrder(float deltaTime)
        {
        }

        public void UpdateWithStatusReceivingOrder(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartWaitingTable();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusWaitingTable(GameManager game)
        {
            Table = game.FindFreeTable();
            if (Table == null)
            {
                return;
            }

            Table.Reserve();
            StartMovingToTable();
        }

        public void UpdateWithStatusMovingToTable(float deltaTime)
        {
            if (HasReachedDestination)
            {
                StartEnjoyingOrder();
            }
        }

        public void UpdateWithStatusEnjoyingOrder(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartLeaving();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusLeaving()
        {
            if (HasReachedDestination)
            {
                Deactivate();
            }
        }

        void MoveToTarget(Vector3 target)
        {
            agent.destination = target;
        }

        void UpdateTimer(float deltaTime)
        {
            RemainingTime = Mathf.Max(0, RemainingTime - deltaTime);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Handles.Label(transform.position + 1f * Vector3.up, Status.ToString());
        }
    }
}