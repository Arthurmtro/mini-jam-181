using UnityEngine;
using UnityEditor;

namespace BunnyCoffee
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRandomColor : MonoBehaviour
    {
        [SerializeField] Color[] colors = new Color[0];

        SpriteRenderer sprite;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            SetColor();
        }

        public void SetColor()
        {
            if (colors.Length > 0)
            {
                Color randomColor = colors[Random.Range(0, colors.Length)];
                sprite.color = randomColor;
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(SpriteRandomColor))]
        public class SpriteRandomColorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                SpriteRandomColor script = (SpriteRandomColor)target;

                if (GUILayout.Button("Set Random Color"))
                {
                    script.Awake();
                    script.SetColor();
                }
            }
        }
#endif
    }
}