using UnityEngine;

namespace GiantWorld.Core
{
    /// <summary>
    /// Fallback camera helpers — scene already contains GameBootstrap + Main Camera.
    /// </summary>
    public static class AutoStart
    {
        public static void EnsureFallbackCameraPublic() => EnsureFallbackCamera();

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
            cam.allowHDR = false;
            cam.allowMSAA = false;
            camGo.AddComponent<AudioListener>();
        }
    }
}
