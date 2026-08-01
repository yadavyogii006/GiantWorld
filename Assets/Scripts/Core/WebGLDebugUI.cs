using UnityEngine;
using UnityEngine.UI;

namespace GiantWorld.Core
{
    /// <summary>
    /// On-screen boot status — visible on itch.io even when 3D rendering fails.
    /// Uses uGUI (not IMGUI) for reliable WebGL display.
    /// </summary>
    public class WebGLDebugUI : MonoBehaviour
    {
        public static string Status = "Starting Giant World...";
        static WebGLDebugUI instance;
        static Text statusText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (instance != null) return;
            var go = new GameObject("WebGLDebugUI");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<WebGLDebugUI>();
            instance.BuildOverlay();
        }

        void BuildOverlay()
        {
            var canvasGo = new GameObject("DebugCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var textGo = new GameObject("StatusText");
            textGo.transform.SetParent(canvasGo.transform, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -8f);
            rect.sizeDelta = new Vector2(-20f, 80f);

            statusText = textGo.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 18;
            statusText.alignment = TextAnchor.UpperLeft;
            statusText.color = Color.white;
            statusText.text = Status;

            var bgGo = new GameObject("StatusBg");
            bgGo.transform.SetParent(textGo.transform, false);
            bgGo.transform.SetAsFirstSibling();
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(-6f, -4f);
            bgRect.offsetMax = new Vector2(6f, 4f);
            bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        }

        void Update()
        {
            if (statusText != null)
                statusText.text = Status;
        }

        public static void Hide()
        {
            if (instance != null)
                instance.gameObject.SetActive(false);
        }
    }
}
