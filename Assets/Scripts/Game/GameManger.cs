using System;
using System.Threading;
using System.Timers;
using UnityEngine;
using UnityEngine.Profiling;

namespace BunnyCoffee
{
    public class NewMonoBehaviourScript : MonoBehaviour
    {
        const int MaxAppliances = 4;
        const int MaxEmployees = 1;
        const int MaxCustomers = 10;

        [SerializeField] private ApplianceTypeCollection applianceTypes;
        [SerializeField] private ProductCollection productTypes;
        [SerializeField] private CustomerTypeCollection customerTypes;

        readonly Appliance[] appliances = new Appliance[MaxAppliances];
        int nextApplianceIndex => Array.FindIndex(appliances, appliance => !appliance.IsActive);
        readonly Employee[] employees = new Employee[MaxEmployees];
        int nextEmployeeIndex => Array.FindIndex(employees, employee => !employee.IsActive);
        readonly Customer[] customers = new Customer[MaxCustomers];
        int nextCustomerIndex => Array.FindIndex(customers, customer => !customer.IsActive);

        void Start()
        {
            for (int i = 0; i < MaxAppliances; i++)
            {
                appliances[i] = new Appliance();
            }

            for (int i = 0; i < MaxEmployees; i++)
            {
                employees[i] = new Employee();
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                customers[i] = new Customer();
            }

            AddAppliance(applianceTypes.AtIndex(0), 0);
            AddEmployee();
            AddCustomer(productTypes.AtIndex(0).Id);
        }

        void Update()
        {
            for (int i = 0; i < MaxAppliances; i++)
            {
                appliances[i].UpdateTimer(Time.deltaTime);
            }

            for (int i = 0; i < MaxEmployees; i++)
            {
                employees[i].UpdateTimer(Time.deltaTime);
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                UpdateCustomer(i);
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
            int nextIndex = nextCustomerIndex;
            if (nextIndex >= customers.Length)
            {
                return;
            }

            customers[nextIndex].Activate(productId);
        }

        void UpdateCustomer(int index)
        {
            if (!customers[index].IsActive)
            {
                return;
            }

            Debug.Log($"Status: {customers[index].Status}");
            switch (customers[index].Status)
            {
                case CustomerStatus.InQueue:
                    customers[index].OnMovingToBar();
                    break;
                case CustomerStatus.MovingToBar:
                    customers[index].OnThinkingOrder();
                    break;
                case CustomerStatus.WaitingEmployee:
                    customers[index].OnExplainingOrder();
                    break;
                case CustomerStatus.WaitingOrder:
                    customers[index].OnMovingToTable();
                    break;
                case CustomerStatus.MovingToTable:
                    customers[index].OnEnjoyingOrder();
                    break;
                case CustomerStatus.Leaving:
                    customers[index].Deactivate();
                    break;
            }

            customers[index].UpdateTimer(Time.deltaTime);

        }
    }
}