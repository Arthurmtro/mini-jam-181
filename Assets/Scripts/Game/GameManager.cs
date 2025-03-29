using System;
using System.Linq;
using UnityEngine;

namespace BunnyCoffee
{
    public class GameManager : MonoBehaviour
    {
        const int MaxAppliances = 4;
        const int MaxEmployees = 10;
        const int MaxCustomers = 40;
        // seconds after the controllers are updated - to avoid too many irrelevant updates
        const float processEvery = 0.1f;

        [Header("Resources")]
        [SerializeField] ApplianceTypeCollection applianceTypes;
        [SerializeField] ProductCollection productTypes;
        [SerializeField] EmployeeController employeeController;
        [SerializeField] CustomerTypeCollection customerTypes;

        [Header("Positions")]
        [SerializeField] Transform employeeInactivePosition;
        [SerializeField] Transform customerInactivePosition;
        [SerializeField] Transform queuePositionsContainer;
        [SerializeField] Transform barPositionsContainer;
        public Vector3 CustomerLeavePosition => customerInactivePosition.position;
        Transform employeesContainer;
        Transform customersContainer;
        QueuePosition[] queuePositions;
        BarPosition[] barPositions;

        [Header("Controllers")]
        [SerializeField] Transform tableControllersContainer;
        TableController[] tableControllers;

        [SerializeField] Transform applianceControllersContainer;
        ApplianceController[] appliances;

        // readonly Appliance[] appliances = new Appliance[MaxAppliances];
        int nextApplianceIndex => Array.FindIndex(appliances, appliance => !appliance.IsActive);
        readonly EmployeeController[] employees = new EmployeeController[MaxEmployees];
        int nextEmployeeIndex => Array.FindIndex(employees, employee => !employee.IsActive);

        readonly CustomerController[] customers = new CustomerController[MaxCustomers];
        int lastCustomerIndex = 0;

        float timeToCustomer = 2f;
        float accumulatedDelta = 0;

        void Start()
        {
            // Init positions
            queuePositions = queuePositionsContainer.GetComponentsInChildren<QueuePosition>().ToArray();
            for (int i = 0; i < queuePositions.Length; i++)
            {
                queuePositions[i].Index = i;
            }

            barPositions = barPositionsContainer.GetComponentsInChildren<BarPosition>().ToArray();
            tableControllers = tableControllersContainer.GetComponentsInChildren<TableController>().ToArray();
            appliances = applianceControllersContainer.GetComponentsInChildren<ApplianceController>().ToArray();

            customersContainer = new GameObject("Customers").transform;
            customersContainer.SetParent(transform);
            employeesContainer = new GameObject("Employees").transform;
            employeesContainer.SetParent(transform);

            for (int i = 0; i < MaxEmployees; i++)
            {
                GameObject newEmployee = Instantiate(employeeController.gameObject, employeesContainer);
                newEmployee.name = $"[{i}] Employee";
                newEmployee.transform.position = employeeInactivePosition.position;

                employees[i] = newEmployee.GetComponent<EmployeeController>();
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                CustomerType randomCustomerType = customerTypes.AtIndex(UnityEngine.Random.Range(0, customerTypes.Count));
                GameObject newCustomer = Instantiate(randomCustomerType.Controller.gameObject, customersContainer);
                newCustomer.name = $"[{i}] Customer (Type={randomCustomerType.Name})";
                newCustomer.transform.position = GetCustomerInactivePosition(i);

                customers[i] = newCustomer.GetComponent<CustomerController>();
            }

            // AddAppliance(applianceTypes.AtIndex(0), 0);
            AddEmployee();
            AddEmployee();
            AddEmployee();
            AddEmployee();
            AddEmployee();
            // AddCustomer(productTypes.AtIndex(0).Id);
        }

        void Update()
        {
            accumulatedDelta += Time.deltaTime;
            if (accumulatedDelta < processEvery)
            {
                return;
            }

            for (int i = 0; i < appliances.Length; i++)
            {
                UpdateAppliance(i, accumulatedDelta);
            }

            for (int i = 0; i < MaxEmployees; i++)
            {
                UpdateEmployee(i, accumulatedDelta);
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                UpdateCustomer(i, accumulatedDelta);
            }

            timeToCustomer = Mathf.Max(0, timeToCustomer - accumulatedDelta);
            if (timeToCustomer == 0)
            {
                AddCustomer(productTypes.AtIndex(0).Id);
            }

            accumulatedDelta = 0;
        }

        void AddEmployee()
        {
            int nextIndex = nextEmployeeIndex;
            if (nextIndex >= employees.Length)
            {
                return;
            }

            employees[nextIndex].Activate();
        }

        void AddCustomer(string productId)
        {
            CustomerController nextCustomer = FindNextCustomer();
            if (nextCustomer == null)
            {
                return;
            }

            if (IsQueueEmpty())
            {
                BarPosition barPosition = FindFreeBarPosition();
                if (barPosition != null)
                {
                    nextCustomer.ActivateToBar(barPosition, productId);
                    timeToCustomer = 2f;
                    return;
                }
            }

            if (!IsQueueReady())
            {
                return;
            }

            QueuePosition queuePosition = FindFreeQueuePosition();
            if (queuePosition != null)
            {
                nextCustomer.ActivateToQueue(queuePosition, productId);
                timeToCustomer = 2f;
            }
        }

        void UpdateAppliance(int index, float timeDelta)
        {
            if (!appliances[index].IsActive)
            {
                // return;
            }

            appliances[index].UpdateController(timeDelta);
        }

        void UpdateEmployee(int index, float timeDelta)
        {
            if (!employees[index].IsActive)
            {
                return;
            }

            employees[index].UpdateController(timeDelta, this);
        }

        void UpdateCustomer(int index, float timeDelta)
        {
            if (!customers[index].IsActive)
            {
                return;
            }

            customers[index].UpdateController(timeDelta, this);
        }

        public CustomerController FindCustomerWaiting()
        {
            return Array.Find(customers, customer => customer.Status == CustomerStatus.WaitingEmployee && !customer.IsAttended);
        }

        public BarPosition FindFreeBarPosition()
        {
            return Array.Find(barPositions, position => !position.IsBusy);
        }

        public QueuePosition FindFreeQueuePosition()
        {
            return Array.Find(queuePositions, position => !position.IsBusy);
        }

        public QueuePosition FindNextQueuePosition(QueuePosition position)
        {
            if (position == null || position.Index == 0)
            {
                return null;
            }

            return queuePositions[position.Index - 1];
        }

        public bool IsQueueEmpty()
        {
            return Array.TrueForAll(queuePositions, position => !position.IsBusy);
        }

        public bool IsQueueReady()
        {
            bool foundFree = false;
            for (int i = 0; i < queuePositions.Length; i++)
            {
                if (!queuePositions[i].IsBusy)
                {
                    foundFree = true;
                }
                else if (foundFree)
                {
                    return false;
                }
            }

            return true;
        }

        public TableController FindFreeTable()
        {
            return Array.Find(tableControllers, table => !table.IsBusy);
        }

        public ApplianceController FindFreeAppliance()
        {
            return Array.Find(appliances, appliance => appliance.IsFree);
        }

        CustomerController FindNextCustomer()
        {
            for (int i = 0; i < customers.Length; i++)
            {
                int index = (lastCustomerIndex + i) % customers.Length;
                if (!customers[index].IsActive)
                {
                    lastCustomerIndex = (index + 1) % customers.Length;
                    return customers[index];
                }

            }

            return null;
        }

        Vector3 GetCustomerInactivePosition(int index)
        {
            int gridSize = 10;
            int distance = 5;
            int x = index / gridSize;
            int y = index % gridSize;

            return new Vector3(customerInactivePosition.position.x + x * distance, customerInactivePosition.position.y + y * distance, customerInactivePosition.position.z);
        }
    }

}