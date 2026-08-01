using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !UNITY_WEBGL
using UnityEngine.InputSystem.UI;
#endif

namespace GiantWorld.Core
{
    /// <summary>
    /// Bootstraps the entire game at runtime — world, player, camera, UI, lighting.
    /// Attach to an empty GameObject in the Main scene and press Play.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            try
            {
                EnsureGameManager();
                EnsureEventSystem();
                var player = CreatePlayer();
                var camera = CreateCamera(player.transform);
                var canvas = CreateCanvas();
                var world = CreateWorld(player.transform);
                var ui = SetupUI(canvas, player);
                SetupLighting();
                SetupBossTracking(world, ui);

                Debug.Log("[Giant World] Kitchen loaded. You are insect-sized. Survive 4 bosses!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Giant World] Bootstrap failed: " + ex);
                CreateFallbackCamera();
            }
        }

        void Update()
        {
            Bosses.CameraShake.UpdateShake();
        }

        void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            // StandaloneInputModule is reliable on WebGL/itch.io
            es.AddComponent<StandaloneInputModule>();
        }

        void CreateFallbackCamera()
        {
            if (Camera.main != null) return;
            var camGo = new GameObject("Fallback Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.2f, 0.35f);
        }

        GameObject CreatePlayer()
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) go.layer = playerLayer;

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            go.AddComponent<Player.PlayerHealth>();
            go.AddComponent<Player.PlayerController>();
            go.AddComponent<Player.PlayerCombat>();

            // Visual — insect body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "BodyVisual";
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.GetComponent<Renderer>().material = World.WorldBuilder.CreateMaterial(new Color(0.2f, 0.8f, 0.3f));
            Destroy(body.GetComponent<Collider>());

            var antennaL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antennaL.transform.SetParent(go.transform);
            antennaL.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
            antennaL.transform.localPosition = new Vector3(-0.15f, 1.6f, 0.1f);
            antennaL.transform.localRotation = Quaternion.Euler(20f, 0f, -15f);
            antennaL.GetComponent<Renderer>().material = World.WorldBuilder.CreateMaterial(Color.black);
            Destroy(antennaL.GetComponent<Collider>());

            var antennaR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antennaR.transform.SetParent(go.transform);
            antennaR.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
            antennaR.transform.localPosition = new Vector3(0.15f, 1.6f, 0.1f);
            antennaR.transform.localRotation = Quaternion.Euler(20f, 0f, 15f);
            antennaR.GetComponent<Renderer>().material = World.WorldBuilder.CreateMaterial(Color.black);
            Destroy(antennaR.GetComponent<Collider>());

            return go;
        }

        Camera CreateCamera(Transform player)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.65f, 0.85f);

            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<Player.FollowCamera>();
            follow.SetTarget(player);
            Bosses.CameraShake.RegisterCamera(camGo.transform);

            return cam;
        }

        Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvas;
        }

        World.WorldBuilder CreateWorld(Transform player)
        {
            var wbGo = new GameObject("WorldBuilder");
            var wb = wbGo.AddComponent<World.WorldBuilder>();
            wb.BuildAll(player);
            player.position = wb.PlayerSpawn;
            return wb;
        }

        UI.UIManager SetupUI(Canvas canvas, GameObject player)
        {
            var uiGo = new GameObject("UIManager");
            var ui = uiGo.AddComponent<UI.UIManager>();
            var health = player.GetComponent<Player.PlayerHealth>();
            ui.BindPlayer(health);
            ui.BuildUI(canvas, health);
            return ui;
        }

        void SetupLighting()
        {
            var existing = FindObjectOfType<Light>();
            if (existing != null && existing.type == LightType.Directional) return;

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.shadows = LightShadows.None; // Soft shadows can fail on some WebGL GPUs
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.65f, 0.65f, 0.7f);
        }

        void SetupBossTracking(World.WorldBuilder world, UI.UIManager ui)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnBossStarted += _ =>
            {
                Bosses.BossBase active = gm.CurrentBoss switch
                {
                    BossType.Cat => world.CatBoss,
                    BossType.Vacuum => world.VacuumBoss,
                    BossType.WashingMachine => world.WashingBoss,
                    BossType.Footsteps => world.FootstepsBoss,
                    _ => null
                };
                if (active != null) ui.TrackBoss(active);
            };
        }
    }
}
