using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BunnyCoffee
{
    public enum EmployeeStatus
    {
        Idle,
        MovingToCustomer,
        AskingCustomer,
        WaitingForAppliance,
        MovingToAppliance,
        Preparing,
        MovingToCustomerToDeliver,
        Delivering,
        WaitingIdlePosition,
        MovingToIdle,
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EmployeeController : MonoBehaviour
    {
        const float TimeToAskCustomer = 1;
        const float TimeToDeliver = 1;

        public bool IsActive { get; private set; }
        public EmployeeStatus Status { get; private set; }
        public bool NeedsTimerUpdate => Status == EmployeeStatus.AskingCustomer || Status == EmployeeStatus.Delivering;
        public float RemainingTime { get; private set; }
        public bool TimeHasFinished => RemainingTime == 0;

        EmployeeIdlePosition idlePosition;
        CustomerController customer;
        ApplianceController appliance;
        Product? product;

        NavMeshAgent agent;
        public bool HasReachedDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Reset()
        {
            Status = EmployeeStatus.Idle;
            RemainingTime = 0;
        }

        public void Deactivate()
        {
            Reset();
            IsActive = false;
        }

        public void Activate()
        {
            Reset();
            IsActive = true;
            StartWaitingIdlePosition();
        }

        // State init

        public void StartIdle()
        {
            Status = EmployeeStatus.Idle;
            customer = null;
        }

        public void StartMovingToCustomer()
        {
            Status = EmployeeStatus.MovingToCustomer;
            MoveToTarget(customer.BarPosition.EmployeePosition);
            if (idlePosition != null)
            {
                idlePosition.Free();
                idlePosition = null;
            }
        }

        public void StartAskingCustomer(GameManager game)
        {
            Status = EmployeeStatus.AskingCustomer;
            RemainingTime = TimeToAskCustomer;
            product = game.GetRandomProduct();

            if (product.HasValue)
            {
                customer.StartExplainingOrder(product.Value);
            }
        }

        public void StarWaitingForAppliance()
        {
            Status = EmployeeStatus.WaitingForAppliance;
        }

        public void StartMovingToAppliance()
        {
            if (appliance == null)
            {
                return;
            }

            appliance.Reserve();
            Status = EmployeeStatus.MovingToAppliance;
            MoveToTarget(appliance.EmployeePosition.position);
        }

        public void StartPreparing()
        {
            Status = EmployeeStatus.Preparing;

            if (product.HasValue)
            {

                RemainingTime = appliance.StartPreparing(product.Value.Id);
            }
        }

        public void StartMovingToCustomerToDeliver()
        {
            if (appliance != null)
            {
                appliance.Free();
                appliance = null;
            }

            Status = EmployeeStatus.MovingToCustomerToDeliver;
            MoveToTarget(customer.BarPosition.EmployeePosition);
        }

        public void StartDelivering()
        {
            Status = EmployeeStatus.Delivering;
            RemainingTime = TimeToDeliver;
            customer.StartReceivingOrder();
        }

        public void StartWaitingIdlePosition()
        {
            product = null;
            Status = EmployeeStatus.WaitingIdlePosition;
        }

        public void StartMovingToIdle(EmployeeIdlePosition position)
        {
            Status = EmployeeStatus.MovingToIdle;
            idlePosition = position;
            idlePosition.Reserve();
            MoveToTarget(idlePosition.EmployeePosition);
        }

        // update by status

        public void UpdateController(float deltaTime, GameManager game)
        {
            switch (Status)
            {
                case EmployeeStatus.Idle:
                    UpdateWithStatusIdle(game);
                    break;
                case EmployeeStatus.MovingToCustomer:
                    UpdateWithStatusMovingToCustomer(game);
                    break;
                case EmployeeStatus.AskingCustomer:
                    UpdateWithStatusAskingCustomer(deltaTime);
                    break;
                case EmployeeStatus.WaitingForAppliance:
                    UpdateWithStatusWaitingForAppliance(game);
                    break;
                case EmployeeStatus.MovingToAppliance:
                    UpdateWithStatusMovingToAppliance(deltaTime);
                    break;
                case EmployeeStatus.Preparing:
                    UpdateWithStatusPreparing(deltaTime);
                    break;
                case EmployeeStatus.MovingToCustomerToDeliver:
                    UpdateWithStatusMovingToCustomerToDeliver();
                    break;
                case EmployeeStatus.Delivering:
                    UpdateWithStatusDelivering(deltaTime);
                    break;
                case EmployeeStatus.WaitingIdlePosition:
                    UpdateWithStatusWaitingIdlePosition(game);
                    break;
                case EmployeeStatus.MovingToIdle:
                    UpdateWithStatusMovingToIdle(game);
                    break;
            }
        }

        public void UpdateWithStatusIdle(GameManager game)
        {
            FindCustomerToAttend(game);
        }

        public void UpdateWithStatusMovingToCustomer(GameManager game)
        {
            if (HasReachedDestination)
            {
                StartAskingCustomer(game);
            }
        }

        public void UpdateWithStatusAskingCustomer(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StarWaitingForAppliance();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusWaitingForAppliance(GameManager game)
        {
            if (product == null)
            {
                return;
            }

            appliance = game.FindFreeAppliance(product.Value.Id);
            if (appliance != null)
            {
                StartMovingToAppliance();
            }
        }

        public void UpdateWithStatusMovingToAppliance(float deltaTime)
        {
            if (HasReachedDestination)
            {
                StartPreparing();
            }
        }

        public void UpdateWithStatusPreparing(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartMovingToCustomerToDeliver();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusMovingToCustomerToDeliver()
        {
            if (HasReachedDestination)
            {
                StartDelivering();
            }
        }

        public void UpdateWithStatusDelivering(float deltaTime)
        {
            if (RemainingTime == 0)
            {
                StartWaitingIdlePosition();
                return;
            }

            UpdateTimer(deltaTime);
        }

        public void UpdateWithStatusWaitingIdlePosition(GameManager game)
        {
            EmployeeIdlePosition position = game.FindEmployeeIdlePosition();
            if (position != null)
            {
                StartMovingToIdle(position);
            }
        }

        public void UpdateWithStatusMovingToIdle(GameManager game)
        {
            if (FindCustomerToAttend(game))
            {
                return;
            }

            if (HasReachedDestination)
            {
                StartIdle();
            }
        }

        bool FindCustomerToAttend(GameManager game)
        {
            CustomerController customerWaiting = game.FindCustomerWaiting();
            if (customerWaiting != null)
            {
                customer = customerWaiting;
                customer.Attend();
                StartMovingToCustomer();
                return true;
            }

            return false;
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
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Handles.Label(transform.position + 1f * Vector3.up + 1f * Vector3.left, Status.ToString());

            if (product.HasValue)
            {
                Handles.Label(transform.position + 1f * Vector3.down, product.Value.Name);
            }
        }
    }
}