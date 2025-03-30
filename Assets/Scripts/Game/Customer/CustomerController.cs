using Unity.VisualScripting;
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
        ConsumingOrder,
        ReviewingOrder,
        Leaving,
    }

    [RequireComponent(typeof(CustomerAnimationController))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerController : MonoBehaviour
    {
        [Header("Params")]
        float TimeToThinkOrder = 0.1f;
        [SerializeField] float TimeToExplainOrder = 2;
        [SerializeField] float TimeToReceiveOrder = 1;
        [SerializeField] float TimeToEnjoyOrder = 15;
        [SerializeField] float TimeToReviewOrder = 2;
        [SerializeField] Sprite spriteThinking;

        public bool IsActive { get; private set; }
        public CustomerStatus Status;
        public bool IsAttended { get; private set; }
        public bool CanBeAttended => Status == CustomerStatus.WaitingEmployee && !IsAttended;
        public float RemainingTime { get; private set; }
        public bool TimeHasFinished => RemainingTime == 0;
        public BarPosition BarPosition { get; private set; }
        public QueuePosition QueuePosition { get; private set; }
        Vector3 inactivePosition;

        Product? product;
        public TableController Table { get; private set; }

        NavMeshAgent agent;
        public bool HasReachedDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        CustomerAnimationController animations;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animations = GetComponent<CustomerAnimationController>();
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

        public void ActivateToQueue(QueuePosition queuePosition)
        {
            Reset();
            IsActive = true;
            agent.isStopped = false;
            Status = CustomerStatus.MovingToQueue;
            StartMovingToQueue(queuePosition);
        }

        public void ActivateToBar(BarPosition barPosition)
        {
            Reset();
            IsActive = true;
            Status = CustomerStatus.MovingToBar;
            agent.isStopped = false;
            StartMovingToBar(barPosition);
        }

        public void Attend()
        {
            if (CanBeAttended)
            {
                IsAttended = true;
            }
        }

        // State init

        public void StartMovingToQueue(QueuePosition queuePosition)
        {
            if (QueuePosition != null)
            {
                QueuePosition.Free();
                QueuePosition = null;
            }

            QueuePosition = queuePosition;
            QueuePosition.Reserve();
            Status = CustomerStatus.MovingToQueue;
            MoveToTarget(queuePosition.CustomerPosition);
            animations.SetValues(true, false);
        }

        public void StartInQueue()
        {
            Status = CustomerStatus.InQueue;
            animations.SetValues(false, false);
        }

        public void StartMovingToBar(BarPosition barPosition)
        {
            if (QueuePosition != null)
            {
                QueuePosition.Free();
                QueuePosition = null;
            }
            if (BarPosition != null)
            {
                BarPosition.Free();
                BarPosition = null;
            }

            BarPosition = barPosition;
            BarPosition.Reserve();
            Status = CustomerStatus.MovingToBar;
            MoveToTarget(barPosition.CustomerPosition);
            animations.SetValues(true, false);
        }

        public void StartThinkingOrder()
        {
            Status = CustomerStatus.ThinkingOrder;
            RemainingTime = TimeToThinkOrder;
            animations.SetValues(false, false);
            // animations.ShowBubble(BubbleType.Thinking);
        }

        public void StartWaitingEmployee()
        {
            Status = CustomerStatus.WaitingEmployee;
            animations.SetValues(false, false);
            animations.HideBubble();
        }

        public void StartExplainingOrder(Product product)
        {
            this.product = product;
            Status = CustomerStatus.ExplainingOrder;
            RemainingTime = TimeToExplainOrder;
            animations.SetValues(false, false);

            if (product.Id == "coffee-latte")
            {
                animations.ShowBubble(BubbleType.Coffee1);
            }
            else
            {
                animations.ShowBubble(BubbleType.Coffee2);
            }
        }

        public void StartWaitingOrder()
        {
            Status = CustomerStatus.WaitingOrder;
            animations.SetValues(false, false);
            animations.HideBubble();
        }

        public void StartReceivingOrder()
        {
            Status = CustomerStatus.ReceivingOrder;
            RemainingTime = TimeToReceiveOrder;
            animations.SetValues(false, false);
            animations.ShowBubble(BubbleType.Coin);
        }

        public void StartWaitingTable()
        {
            Status = CustomerStatus.WaitingTable;
            animations.SetValues(false, false);
            animations.HideBubble();
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
            animations.SetValues(true, false);
            animations.HideBubble();
        }

        public void StartConsumingOrder()
        {
            Status = CustomerStatus.ConsumingOrder;
            RemainingTime = TimeToEnjoyOrder;
            animations.SetValues(false, true);
            animations.HideBubble();
        }

        public void StartReviewingOrder()
        {
            Status = CustomerStatus.ReviewingOrder;
            RemainingTime = TimeToReviewOrder;
            animations.SetValues(false, false);
            animations.ShowBubble(BubbleType.Happy);
        }

        public void StartLeaving()
        {
            if (Table != null)
            {
                Table.Free();
            }

            Status = CustomerStatus.Leaving;
            MoveToTarget(inactivePosition);
            animations.SetValues(true, false);
            animations.HideBubble();
        }

        // update by status

        public void UpdateController(float deltaTime, GameManager game)
        {
            switch (Status)
            {
                case CustomerStatus.Idle:
                    UpdateWithStatusIdle(deltaTime);
                    break;
                case CustomerStatus.MovingToQueue:
                    UpdateWithStatusMovingToQueue(deltaTime);
                    break;
                case CustomerStatus.InQueue:
                    UpdateWithStatusInQueue(game);
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
                case CustomerStatus.ConsumingOrder:
                    UpdateWithStatusConsumingOrder(deltaTime);
                    break;
                case CustomerStatus.ReviewingOrder:
                    UpdateWithStatusReviewingOrder(deltaTime);
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
                StartInQueue();
            }
        }

        public void UpdateWithStatusInQueue(GameManager game)
        {
            // Check next queue positions
            QueuePosition nextQueuePosition = game.FindNextQueuePosition(QueuePosition);
            if (nextQueuePosition != null)
            {
                if (!nextQueuePosition.IsBusy)
                {
                    StartMovingToQueue(nextQueuePosition);
                }

                return;
            }

            // Check if bar is free
            BarPosition nextBarPosition = game.FindFreeBarPosition();
            if (nextBarPosition != null && !nextBarPosition.IsBusy)
            {
                StartMovingToBar(nextBarPosition);
            }
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
                StartConsumingOrder();
            }
        }

        public void UpdateWithStatusConsumingOrder(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartReviewingOrder();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusReviewingOrder(float deltaTime)
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

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            Handles.Label(transform.position + 1f * Vector3.up, Status.ToString(), centeredStyle);

            if (product.HasValue)
            {
                Handles.Label(transform.position + 1f * Vector3.down, product.Value.Name, centeredStyle);
            }
        }
    }
}