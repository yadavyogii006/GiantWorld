using UnityEngine;

namespace GiantWorld.Core
{
    /// <summary>
    /// On-screen boot status for WebGL / itch.io (IMGUI — minimal heap use at startup).
    /// </summary>
    public class WebGLDebugUI : MonoBehaviour
    {
        public static string Status = "Starting Giant World...";
        static WebGLDebugUI instance;

        public static void EnsureCreated()
        {
            if (instance != null) return;
            var go = new GameObject("WebGLDebugUI");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<WebGLDebugUI>();
        }

        void OnGUI()
        {
            var rect = new Rect(10, 10, Screen.width - 20, 36);
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(16, 14, Screen.width - 32, 30), Status);
        }

        public static void Hide()
        {
            if (instance != null)
                instance.enabled = false;
        }
    }
}
