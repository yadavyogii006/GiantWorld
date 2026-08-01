using UnityEngine;

namespace GiantWorld.Core
{
    /// <summary>
    /// Loads a pre-built material from Resources so WebGL includes the shader in the build.
    /// </summary>
    public static class MaterialCache
    {
        static Material baseMaterial;

        public static Material Get(Color color)
        {
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
            return mat;
        }
    }
}
