using UnityEngine;

namespace BunnyCoffee
{
    public static class GizmosUtils
    {
        public static void DrawCenteredText(Vector3 position, string text)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(position, text, style);
#endif
        }
    }
}