using UnityEngine;

namespace GiantWorld.Core
{
    /// <summary>
    /// Ensures a camera exists and game bootstraps even if scene script reference breaks in WebGL builds.
    /// </summary>
    public static class AutoStart
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnGameLoad()
        {
            EnsureFallbackCamera();

            if (Object.FindObjectOfType<GameBootstrap>() != null)
                return;

            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        static void EnsureFallbackCamera()
        {
            if (Camera.main != null) return;

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.62f, 0.92f);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            camGo.AddComponent<AudioListener>();
        }

        public static void EnsureFallbackCameraPublic() => EnsureFallbackCamera();
    }
}
