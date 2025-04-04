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

            UnityEditor.Handles.Label(position, text, style);
        }
    }
}