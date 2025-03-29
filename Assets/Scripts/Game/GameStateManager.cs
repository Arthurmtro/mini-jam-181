using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BunnyCoffee
{

    [Serializable]
    public struct GameState
    {
        public int Money;
        public int NumEmployees;
        public string ApplianceLevelsString;
        public int[] ApplianceLevels => ApplianceLevelsString.Split(',').ToArray().Select(int.Parse).ToArray();

        public GameState(int money = 0, int numEmployees = 1, string applianceLevelsString = "0")
        {
            Money = money;
            NumEmployees = numEmployees;
            ApplianceLevelsString = applianceLevelsString;
        }
    }

    public class GameStateManager : MonoBehaviour
    {
        GameState gameState = new(0, 1, "0");
        public GameState GameState => gameState;

        const string SaveDataKey = "game.saveData.v2";

        void Start()
        {
            Load();
        }

        public void AddMoney(int moneyToAdd)
        {
            gameState.Money += moneyToAdd;
            Save();
        }

        public void AddEmployee()
        {
            gameState.NumEmployees++;
            Save();
        }

        public void AddAppliance()
        {
            gameState.ApplianceLevelsString += ",0";
            Save();
        }

        public void UpdateApplicationLevels(string levels)
        {
            gameState.ApplianceLevelsString = levels;
            Save();
        }

        void Reset()
        {
            PlayerPrefs.DeleteKey(SaveDataKey);
            gameState = new GameState(0, 1, "0");
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(SaveDataKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(SaveDataKey);
            if (json == null || json == "")
            {
                return;
            }
            gameState = JsonUtility.FromJson<GameState>(json);
        }

        void Save()
        {
            var json = JsonUtility.ToJson(gameState);

            PlayerPrefs.SetString(SaveDataKey, json);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            Handles.Label(transform.position, $"Money: {gameState.Money}\nEmployees: {gameState.NumEmployees}\nAppliances: {gameState.ApplianceLevelsString}", centeredStyle);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(GameStateManager))]
        public class GameManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                GameStateManager manager = (GameStateManager)target;

                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Reset (CAUTION!)"))
                    {
                        manager.Reset();
                    }
                }
            }
#endif
        }

    }
}
