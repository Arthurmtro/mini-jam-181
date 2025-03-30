using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BunnyCoffee
{
    public class GameManager : MonoBehaviour
    {
        const int MaxEmployees = 2;
        const int MaxCustomers = 20;
        // seconds after the controllers are updated - to avoid too many irrelevant updates
        const float processEvery = 0.1f;
        const float baseTimeToCustomer = 7.5f;

        [Header("State")]
        [SerializeField] bool IsActive = false;

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
        [SerializeField] EmployeeController[] employeeControllers;
        [SerializeField] Transform tablesContainer;
        TableController[] tableControllers;
        [SerializeField] UIController ui;

        [SerializeField] Transform appliancesContainer;
        ApplianceController[] appliances;
        int NextApplianceIndex => appliances != null ? Array.FindIndex(appliances, appliance => !appliance.IsActive) : -1;
        ApplianceController NextAppliance => appliances != null ? Array.Find(appliances, appliance => !appliance.IsActive) : null;
        bool CanAddAppliance => NextApplianceIndex >= 0 && NextApplianceIndex < appliances.Length;
        ApplianceController NextApplianceToLevelUp => appliances != null ? appliances.Where(appliance => appliance.CanLevelUp).OrderBy(appliance => appliance.NextLevelPrice).FirstOrDefault() : null;
        bool CanLevelUpAnyAppliance => appliances != null ? appliances.Any(appliance => appliance.CanLevelUp) : false;
        int TotalApplianceLevel => appliances != null ? appliances.Where(appliance => appliance.IsActive).Sum(appliance => appliance.Level) : 0;

        readonly EmployeeController[] employees = new EmployeeController[MaxEmployees];
        int NumActiveEmployees => employees != null ? employees.Where(employee => employee.IsActive).Sum(employee => 1) : 1;
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

        float NextTimeToCustomer => baseTimeToCustomer / NumActiveEmployees;
        float timeToCustomer = baseTimeToCustomer;
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
                GameObject newEmployee = Instantiate(employeeControllers[i % employeeControllers.Length].gameObject, employeesContainer);
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
            ui.SetShowBackdrop(false);
            ui.SetShowHeader(true);
            UpdateUI();
            IsActive = true;
        }

        void Update()
        {
            if (!IsActive)
            {
                return;
            }

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
                    timeToCustomer = NextTimeToCustomer;
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
                timeToCustomer = NextTimeToCustomer;
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
            var freeTables = tableControllers.Where(table => !table.IsBusy).ToArray();
            if (freeTables.Length == 0)
            {
                return null;
            }

            return freeTables[UnityEngine.Random.Range(0, freeTables.Length)];
        }

        public ApplianceController FindFreeAppliance(string productId)
        {
            return Array.Find(appliances, appliance => appliance.CanPrepare(productId));
        }

        // Change in game state

        public void CompleteProduct(Product product)
        {
            gameState.AddMoney(product.Price);
            UpdateUI();
        }

        public void HireEmployee()
        {
            EmployeeController next = NextEmployee;
            if (next == null || gameState.GameState.NumEmployees >= employees.Length || gameState.GameState.Money < NextEmployee.Price)
            {
                return;
            }

            AddEmployee();
            gameState.AddEmployee(next.Price);
            UpdateUI();
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
            UpdateUI();
        }

        public void LevelUpAppliance()
        {
            ApplianceController nextApplianceToLevelUp = NextApplianceToLevelUp;
            if (nextApplianceToLevelUp == null || !CanLevelUpAnyAppliance || nextApplianceToLevelUp.Price > gameState.GameState.Money)
            {
                return;
            }

            int price = nextApplianceToLevelUp.Price;
            nextApplianceToLevelUp.LevelUp();
            string levels = string.Join(",", appliances.Where(appliance => appliance.IsActive).Select(appliance => appliance.Level.ToString()));

            Debug.Log(levels);

            gameState.UpdateApplianceLevels(levels, price);
            UpdateUI();
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
            UpdateUI();
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
            UpdateUI();
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
            int gridSize = 5;
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
                for (int i = 0; i < type.Levels.Length && i <= appliance.Level; i++)
                {
                    ApplianceTypeLevel level = type.Levels[i];

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

        void UpdateUI()
        {
            ui.SetMoney(gameState.GameState.Money);

            bool isEmployeeEnabled = CanAddEmployee && NextEmployee != null && NextEmployee.Price <= gameState.GameState.Money;
            string employeePrice = NextEmployee != null ? NextEmployee.Price.ToString() : "-";
            ui.UpdateEmployeeButton(gameState.GameState.NumEmployees, employeePrice, !isEmployeeEnabled);

            bool isApplianceEnabled = CanAddAppliance && NextAppliance != null && NextAppliance.Price <= gameState.GameState.Money;
            string appliancePrice = NextAppliance != null ? NextAppliance.Price.ToString() : "-";
            ui.UpdateBuyApplianceButton(gameState.GameState.ApplianceLevels.Length, appliancePrice, !isApplianceEnabled);

            bool isApplianceUpgradeEnabled = CanLevelUpAnyAppliance && NextApplianceToLevelUp != null && NextApplianceToLevelUp.Price <= gameState.GameState.Money;
            string applianceUpgradePrice = NextApplianceToLevelUp != null ? NextApplianceToLevelUp.Price.ToString() : "-";
            ui.UpdateUpgradeApplianceButton(TotalApplianceLevel, applianceUpgradePrice, !isApplianceUpgradeEnabled);

            bool isDecorationEnabled = CanAddDecoration && NextDecoration != null && NextDecoration.Price <= gameState.GameState.Money;
            string decorationPrice = NextDecoration != null ? NextDecoration.Price.ToString() : "-";
            ui.UpdateDecorationButton(gameState.GameState.NumDecorations, decorationPrice, !isDecorationEnabled);
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