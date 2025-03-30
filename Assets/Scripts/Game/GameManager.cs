using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BunnyCoffee
{
    public class GameManager : MonoBehaviour
    {
        const int MaxEmployees = 4;
        const int MaxCustomers = 40;
        // seconds after the controllers are updated - to avoid too many irrelevant updates
        const float processEvery = 0.1f;

        [Header("Resources")]
        [SerializeField] ResourceManager resources;
        [SerializeField] GameStateManager gameState;
        Dictionary<string, Product> products = new();

        [Header("Positions")]
        [SerializeField] Transform employeesIdlePositionsContainer;
        [SerializeField] Transform employeeInactivePosition;
        [SerializeField] Transform customerInactivePosition;
        [SerializeField] Transform queuePositionsContainer;
        [SerializeField] Transform barPositionsContainer;
        Transform employeesContainer;
        EmployeeIdlePosition[] employeeIdlePositions;
        Transform customersContainer;
        QueuePosition[] queuePositions;
        BarPosition[] barPositions;

        [Header("Controllers")]
        [SerializeField] EmployeeController employeeController;
        [SerializeField] Transform tablesContainer;
        TableController[] tableControllers;

        [SerializeField] Transform appliancesContainer;
        ApplianceController[] appliances;
        int NextApplianceIndex => appliances != null ? Array.FindIndex(appliances, appliance => !appliance.IsActive) : -1;
        ApplianceController NextAppliance => appliances != null ? Array.Find(appliances, appliance => !appliance.IsActive) : null;
        bool CanAddAppliance => NextApplianceIndex >= 0 && NextApplianceIndex < appliances.Length;

        readonly EmployeeController[] employees = new EmployeeController[MaxEmployees];
        int NextEmployeeIndex => employees != null ? Array.FindIndex(employees, employee => !employee.IsActive) : -1;
        EmployeeController NextEmployee => employees != null ? Array.Find(employees, employee => !employee.IsActive) : null;
        bool CanAddEmployee => NextEmployeeIndex >= 0 && NextEmployeeIndex < employees.Length;

        [SerializeField] Transform decorationsContainer;
        DecorationController[] decorations;
        int NextDecorationIndex => Array.FindIndex(decorations, decoration => !decoration.IsActive);
        DecorationController NextDecoration => Array.Find(decorations, decoration => !decoration.IsActive);
        bool CanAddDecoration => NextDecorationIndex >= 0 && NextDecorationIndex < decorations.Length;

        readonly CustomerController[] customers = new CustomerController[MaxCustomers];
        int lastCustomerIndex = 0;

        float timeToCustomer = 5f;
        float accumulatedDelta = 0;

        void Start()
        {
            // Init positions
            employeeIdlePositions = employeesIdlePositionsContainer.GetComponentsInChildren<EmployeeIdlePosition>().ToArray();
            queuePositions = queuePositionsContainer.GetComponentsInChildren<QueuePosition>().ToArray();
            for (int i = 0; i < queuePositions.Length; i++)
            {
                queuePositions[i].Index = i;
            }

            barPositions = barPositionsContainer.GetComponentsInChildren<BarPosition>().ToArray();
            tableControllers = tablesContainer.GetComponentsInChildren<TableController>().ToArray();
            appliances = appliancesContainer.GetComponentsInChildren<ApplianceController>().ToArray();
            decorations = decorationsContainer.GetComponentsInChildren<DecorationController>().ToArray();

            customersContainer = new GameObject("Customers").transform;
            customersContainer.SetParent(transform);
            employeesContainer = new GameObject("Employees").transform;
            employeesContainer.SetParent(transform);

            for (int i = 0; i < MaxEmployees; i++)
            {
                GameObject newEmployee = Instantiate(employeeController.gameObject, employeesContainer);
                newEmployee.name = $"[{i}] Employee";
                EmployeeIdlePosition employeeIdlePosition = employeeIdlePositions[i % employeeIdlePositions.Length];
                newEmployee.transform.position = GetInactivePosition(employeeInactivePosition.position, i);

                employees[i] = newEmployee.GetComponent<EmployeeController>();
            }

            for (int i = 0; i < MaxCustomers; i++)
            {
                CustomerType randomCustomerType = resources.CustomerTypes.AtIndex(UnityEngine.Random.Range(0, resources.CustomerTypes.Count));
                GameObject newCustomer = Instantiate(randomCustomerType.Controller.gameObject, customersContainer);
                newCustomer.name = $"[{i}] Customer (Type={randomCustomerType.Name})";
                newCustomer.transform.position = GetInactivePosition(customerInactivePosition.position, i);

                customers[i] = newCustomer.GetComponent<CustomerController>();
            }

            // restore saved state
            foreach (var applianceLevel in gameState.GameState.ApplianceLevels)
            {
                AddAppliance(applianceLevel);
            }
            for (int i = 0; i < gameState.GameState.NumEmployees; i++)
            {
                AddEmployee();
            }
            for (int i = 0; i < gameState.GameState.NumDecorations; i++)
            {
                AddDecoration();
            }

            CalculateAllProducts();
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
                AddCustomer();
            }

            accumulatedDelta = 0;
        }

        void AddAppliance(int level = 0)
        {
            int nextIndex = NextApplianceIndex;
            if (!CanAddAppliance || level < 0)
            {
                return;
            }

            appliances[nextIndex].Activate(level);
            CalculateAllProducts();
        }

        void AddEmployee()
        {
            int nextIndex = NextEmployeeIndex;
            if (!CanAddEmployee)
            {
                return;
            }

            employees[nextIndex].Activate(this);
        }

        void AddCustomer()
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
                    nextCustomer.ActivateToBar(barPosition);
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
                nextCustomer.ActivateToQueue(queuePosition);
                timeToCustomer = 2f;
            }
        }

        void AddDecoration()
        {
            int nextIndex = NextDecorationIndex;
            if (nextIndex < 0 || nextIndex >= decorations.Length)
            {
                return;
            }

            decorations[nextIndex].Activate();
        }

        void UpdateAppliance(int index, float timeDelta)
        {
            if (!appliances[index].IsActive)
            {
                return;
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

        public EmployeeIdlePosition FindEmployeeIdlePosition()
        {
            return Array.Find(employeeIdlePositions, position => !position.IsBusy);
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

        public BarPosition FindFreeBarPosition()
        {
            return Array.Find(barPositions, position => !position.IsBusy);
        }


        public Product? GetRandomProduct()
        {
            if (products.Count == 0)
            {
                return null;
            }

            return products.Values.ElementAt(UnityEngine.Random.Range(0, products.Count));
        }

        public TableController FindFreeTable()
        {
            return Array.Find(tableControllers, table => !table.IsBusy);
        }

        public ApplianceController FindFreeAppliance(string productId)
        {
            return Array.Find(appliances, appliance => appliance.CanPrepare(productId));
        }

        // Change in game state

        public void CompleteProduct(Product product)
        {
            gameState.AddMoney(product.Price);
        }

        public void HireEmployee()
        {
            EmployeeController next = NextEmployee;
            if (next == null || gameState.GameState.NumEmployees >= employees.Length)
            {
                return;
            }

            AddEmployee();
            gameState.AddEmployee(next.Price);
        }

        public void BuyAppliance()
        {
            ApplianceController next = NextAppliance;
            Debug.Log(gameState.GameState.ApplianceLevels.Length);
            Debug.Log(appliances.Length);
            if (next == null || gameState.GameState.ApplianceLevels.Length >= appliances.Length || gameState.GameState.Money < next.Price)
            {
                return;
            }

            AddAppliance(0);
            gameState.AddAppliance(next.Price);
        }

        public void LevelUpAppliance(int index)
        {
            if (index < 0 || index >= appliances.Length)
            {
                return;
            }

            int price = appliances[index].Price;
            string levels = string.Join(",", appliances.Select(appliance => appliance.Level.ToString()));
            appliances[index].LevelUp();
            gameState.UpdateApplianceLevels(levels, price);
        }

        public void BuyDecoration()
        {
            DecorationController next = NextDecoration;
            if (next == null || gameState.GameState.NumDecorations >= decorations.Length)
            {
                return;
            }

            AddDecoration();
            gameState.AddDecoration(next.Price);
        }

        //

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

        Vector3 GetInactivePosition(Vector3 basePosition, int index)
        {
            int gridSize = 10;
            int distance = 5;
            int x = index / gridSize;
            int y = index % gridSize;

            return new Vector3(basePosition.x + x * distance, basePosition.y + y * distance, basePosition.z);
        }

        void CalculateAllProducts()
        {
            products = new Dictionary<string, Product>();
            foreach (var appliance in appliances)
            {
                if (!appliance.IsActive)
                {
                    continue;
                }

                ApplianceType type = resources.ApplianceTypes.ById(appliance.TypeId);
                foreach (ApplianceTypeLevel level in type.Levels)
                {
                    foreach (ApplianceTypeProduct levelProduct in level.Products)
                    {
                        Product product = resources.ProductTypes.ById(levelProduct.ProductId);

                        if (!products.ContainsKey(product.Id))
                        {
                            products.Add(product.Id, product);
                        }
                    }
                }
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GameManager))]
        public class GameManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                GameManager manager = (GameManager)target;

                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    if (manager.CanAddEmployee && manager.NextEmployee != null)
                        if (GUILayout.Button($"Hire Employee (${manager.NextEmployee.Price})"))
                        {
                            manager.HireEmployee();
                        }

                    if (manager.CanAddAppliance && manager.NextAppliance != null)
                    {
                        if (GUILayout.Button($"Buy Appliance (${manager.NextAppliance.Price})"))
                        {
                            manager.BuyAppliance();
                        }
                    }

                    if (manager.appliances != null)
                    {
                        for (int i = 0; i < manager.appliances.Length; i++)
                        {
                            var appliance = manager.appliances[i];

                            if (appliance.CanLevelUp)
                            {
                                if (GUILayout.Button($"{appliance.name} [{i}] - Level UP (${appliance.NextLevelPrice})"))
                                {
                                    manager.LevelUpAppliance(i);
                                }

                            }
                        }
                    }

                    if (manager.CanAddDecoration && manager.NextDecoration != null)
                    {
                        if (GUILayout.Button($"Buy Decoration (${manager.NextDecoration.Price})"))
                        {
                            manager.BuyDecoration();
                        }
                    }

                }
#endif
            }
        }
    }
}