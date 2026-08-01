using System.Collections.Generic;
using UnityEngine;

namespace GiantWorld.Core
{
    /// <summary>
    /// Loads a pre-built material from Resources so WebGL includes the shader in the build.
    /// Reuses materials by color to avoid WebGL heap spikes.
    /// </summary>
    public static class MaterialCache
    {
        static Material baseMaterial;
        static readonly Dictionary<Color32, Material> byColor = new Dictionary<Color32, Material>();

        public static Material Get(Color color)
        {
            var key = (Color32)color;
            if (byColor.TryGetValue(key, out var cached))
                return cached;

            if (baseMaterial == null)
            {
                baseMaterial = Resources.Load<Material>("Materials/BaseUnlit");
                if (baseMaterial == null)
                {
                    Debug.LogWarning("[Giant World] BaseUnlit material missing — using runtime fallback.");
                    var shader = Shader.Find("Unlit/Color");
                    if (shader == null) shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
                    if (shader == null) shader = Shader.Find("Standard");
                    baseMaterial = new Material(shader);
                }
            }

            var mat = new Material(baseMaterial);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            byColor[key] = mat;
            return mat;
        }
    }
}
