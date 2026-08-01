using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GiantWorld.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        static GameBootstrap active;

        void Awake()
        {
            if (active != null && active != this)
            {
                Destroy(gameObject);
                return;
            }
            active = this;

            WebGLDebugUI.EnsureCreated();
            WebGLDebugUI.Status = "Booting Giant World...";
            AutoStart.EnsureFallbackCameraPublic();
            ForceWebGLCameraSettings();
        }

        void Start()
        {
            if (active != this) return;
            StartCoroutine(BootRoutine());
        }

        static void ForceWebGLCameraSettings()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.62f, 0.92f);
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
        }

        IEnumerator BootRoutine()
        {
            yield return null;

            EnsureGameManager();
            EnsureEventSystem();
            SetupLighting();

            if (!TrySetupCore(out World.WorldBuilder world, out GameObject player))
                yield break;

            yield return null;

            yield return world.BuildLandmarksRoutine();

            WebGLDebugUI.Status = "Loading bosses...";
            yield return world.BuildBossArenasRoutine(player.transform);

            WebGLDebugUI.Status = "Kitchen loaded! WASD move, Shift sprint, Space jump.";
            Debug.Log("[Giant World] Kitchen loaded.");
            Invoke(nameof(HideDebugOverlay), 10f);
        }

        bool TrySetupCore(out World.WorldBuilder world, out GameObject player)
        {
            world = null;
            player = null;

            try
            {
                var canvas = CreateCanvas();
                var wbGo = new GameObject("WorldBuilder");
                world = wbGo.AddComponent<World.WorldBuilder>();

                WebGLDebugUI.Status = "Creating floor...";
                world.BeginWorld();

                WebGLDebugUI.Status = "Spawning player...";
                player = CreatePlayer();
                PlacePlayer(player.transform, world.PlayerSpawn);
                SetupCamera(player.transform);

                var ui = SetupUI(canvas, player);
                SetupBossTracking(world, ui);

                WebGLDebugUI.Status = "READY — click game, then WASD to move!";
                return true;
            }
            catch (System.Exception ex)
            {
                FailBoot(ex);
                return false;
            }
        }

        static void PlacePlayer(Transform player, Vector3 spawn)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = spawn;
            if (cc != null) cc.enabled = true;
            player.GetComponent<Player.PlayerController>()?.ResetMotion();
        }

        void FailBoot(System.Exception ex)
        {
            WebGLDebugUI.Status = "Error: " + ex.Message;
            Debug.LogError("[Giant World] Bootstrap failed: " + ex);
            AutoStart.EnsureFallbackCameraPublic();
            ForceWebGLCameraSettings();
        }

        void HideDebugOverlay() => WebGLDebugUI.Hide();

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

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "BodyVisual";
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = MaterialCache.Get(new Color(0.1f, 0.95f, 0.2f));
            Destroy(body.GetComponent<Collider>());

            return go;
        }

        Camera SetupCamera(Transform player)
        {
            Camera cam = Camera.main;
            GameObject camGo;

            if (cam == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            else
            {
                camGo = cam.gameObject;
            }

            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.65f, 0.85f);
            cam.allowHDR = false;
            cam.allowMSAA = false;

            if (camGo.GetComponent<Player.FollowCamera>() == null)
                camGo.AddComponent<Player.FollowCamera>();

            var follow = camGo.GetComponent<Player.FollowCamera>();
            follow.SetTarget(player);
            follow.SnapToTarget();

            Bosses.CameraShake.RegisterCamera(camGo.transform);
            return cam;
        }

        Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvas;
        }

        UI.UIManager SetupUI(Canvas canvas, GameObject player)
        {
            try
            {
                var uiGo = new GameObject("UIManager");
                var ui = uiGo.AddComponent<UI.UIManager>();
                var health = player.GetComponent<Player.PlayerHealth>();
                ui.BindPlayer(health);
                ui.BuildUI(canvas, health);
                return ui;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Giant World] UI setup skipped: " + ex.Message);
                return null;
            }
        }

        void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        void SetupLighting()
        {
            foreach (var light in FindObjectsOfType<Light>())
                light.shadows = LightShadows.None;

            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Sun");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.shadows = LightShadows.None;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.75f, 0.75f, 0.78f);
        }

        void SetupBossTracking(World.WorldBuilder world, UI.UIManager ui)
        {
            if (ui == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnBossStarted += _ =>
            {
                Bosses.BossBase activeBoss = gm.CurrentBoss switch
                {
                    BossType.Cat => world.CatBoss,
                    BossType.Vacuum => world.VacuumBoss,
                    BossType.WashingMachine => world.WashingBoss,
                    BossType.Footsteps => world.FootstepsBoss,
                    _ => null
                };
                if (activeBoss != null) ui.TrackBoss(activeBoss);
            };
        }
    }
}
