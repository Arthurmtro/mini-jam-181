using System;
using System.Linq;
using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

namespace BunnyCoffee
{
    public class GameManager : MonoBehaviour
    {
        const int MaxAppliances = 4;
        const int MaxEmployees = 1;
        const int MaxCustomers = 10;

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
        Vector3[] queuePositions;
        BarPosition[] barPositions;

        [Header("Controllers")]
        [SerializeField] Transform tableControllersContainer;
        TableController[] tableControllers;

        [SerializeField] Transform applianceControllersContainer;
        ApplianceController[] applianceControllers;

        readonly Appliance[] appliances = new Appliance[MaxAppliances];
        int nextApplianceIndex => Array.FindIndex(appliances, appliance => !appliance.IsActive);
        readonly EmployeeController[] employees = new EmployeeController[MaxEmployees];
        int nextEmployeeIndex => Array.FindIndex(employees, employee => !employee.IsActive);

        readonly CustomerController[] customers = new CustomerController[MaxCustomers];
        int lastCustomerIndex = 0;

        float timeToCustomer = 2f;

        void Start()
        {
            // Init positions
            queuePositions = barPositionsContainer.GetComponentsInChildren<Transform>().Where(t => t != queuePositionsContainer).Select(t => t.position).ToArray();
            barPositions = barPositionsContainer.GetComponentsInChildren<BarPosition>().ToArray();
            tableControllers = tableControllersContainer.GetComponentsInChildren<TableController>().ToArray();
            applianceControllers = applianceControllersContainer.GetComponentsInChildren<ApplianceController>().ToArray();

            customersContainer = new GameObject("Customers").transform;
            customersContainer.SetParent(transform);
            employeesContainer = new GameObject("Customers").transform;
            employeesContainer.SetParent(transform);

            for (int i = 0; i < MaxAppliances; i++)
            {
                appliances[i] = new Appliance();
            }

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

            AddAppliance(applianceTypes.AtIndex(0), 0);
            AddEmployee();
            // AddCustomer(productTypes.AtIndex(0).Id);
        }

        void Update()
        {
            for (int i = 0; i < MaxEmployees; i++)
            {
                UpdateEmployee(i);
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                UpdateCustomer(i);
            }

            timeToCustomer = Mathf.Max(0, timeToCustomer - Time.deltaTime);
            if (timeToCustomer == 0)
            {
                AddCustomer(productTypes.AtIndex(0).Id);
            }
        }

        void AddAppliance(ApplianceType type, int level)
        {
            int nextIndex = nextApplianceIndex;
            if (nextIndex >= appliances.Length)
            {
                return;
            }

            appliances[nextIndex].Activate(type, level);
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

            BarPosition barPosition = FindFreeBarPosition();
            if (barPosition == null)
            {
                return;
            }

            nextCustomer.ActivateToBar(barPosition, productId);
            timeToCustomer = 2f;
        }

        void UpdateEmployee(int index)
        {
            if (!employees[index].IsActive)
            {
                return;
            }

            employees[index].UpdateController(Time.deltaTime, this);
        }

        void UpdateCustomer(int index)
        {
            if (!customers[index].IsActive)
            {
                return;
            }

            customers[index].UpdateController(Time.deltaTime, this);
        }

        public CustomerController FindCustomerWaiting()
        {
            return Array.Find(customers, customer => customer.Status == CustomerStatus.WaitingEmployee && !customer.IsAttended);
        }

        public BarPosition FindFreeBarPosition()
        {
            return Array.Find(barPositions, position => !position.IsBusy);
        }

        public TableController FindFreeTable()
        {
            return Array.Find(tableControllers, table => !table.IsBusy);
        }

        public ApplianceController FindFreeAppliance()
        {
            return Array.Find(applianceControllers, appliance => appliance.IsFree);
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