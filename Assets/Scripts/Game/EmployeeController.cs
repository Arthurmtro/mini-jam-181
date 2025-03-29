using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BunnyCoffee
{
    public enum EmployeeStatus
    {
        Idle,
        Waiting,
        MovingToCustomer,
        AskingCustomer,
        WaitingForAppliance,
        MovingToAppliance,
        Preparing,
        MovingToCustomerToDeliver,
        Delivering,
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

        CustomerController customer;
        ApplianceController appliance;

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
        }

        // State init

        public void StartIdle()
        {
            Status = EmployeeStatus.Idle;
            customer = null;
        }

        public void StartWaiting()
        {
            Status = EmployeeStatus.Waiting;
        }

        public void StartMovingToCustomer()
        {
            Status = EmployeeStatus.MovingToCustomer;
            MoveToTarget(customer.BarPosition.Employee);
        }

        public void StartAskingCustomer()
        {
            Status = EmployeeStatus.AskingCustomer;
            RemainingTime = TimeToAskCustomer;

            customer.StartExplainingOrder();
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

            Status = EmployeeStatus.MovingToAppliance;
            MoveToTarget(appliance.EmployeePosition.position);
        }

        public void StartPreparing()
        {
            Status = EmployeeStatus.Preparing;
            RemainingTime = 3f;
            appliance.StartPreparing("");
        }

        public void StartMovingToCustomerToDeliver()
        {
            Status = EmployeeStatus.MovingToCustomerToDeliver;
            MoveToTarget(customer.BarPosition.Employee);
            appliance.Free();
        }

        public void StartDelivering()
        {
            Status = EmployeeStatus.Delivering;
            RemainingTime = TimeToDeliver;
            customer.StartReceivingOrder();
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
                case EmployeeStatus.Idle:
                    UpdateWithStatusIdle(game);
                    break;
                case EmployeeStatus.Waiting:
                    UpdateWithStatusWaiting(deltaTime);
                    break;
                case EmployeeStatus.MovingToCustomer:
                    UpdateWithStatusMovingToCustomer(deltaTime);
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
            }
        }

        public void UpdateWithStatusIdle(GameManager game)
        {
            CustomerController customerWaiting = game.FindCustomerWaiting();
            if (customerWaiting != null)
            {
                customer = customerWaiting;
                customer.Attend();
                StartMovingToCustomer();
            }
        }

        public void UpdateWithStatusWaiting(float deltaTime)
        {
        }

        public void UpdateWithStatusMovingToCustomer(float deltaTime)
        {
            if (HasReachedDestination)
            {
                StartAskingCustomer();
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
            appliance = game.FindFreeAppliance();
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
                StartIdle();
                return;
            }

            UpdateTimer(deltaTime);
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
        }
    }
}