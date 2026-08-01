using System.Collections;
using UnityEngine;
using GiantWorld.Core;

namespace GiantWorld.World
{
    /// <summary>
    /// Builds the entire kitchen open-world at runtime using scaled primitives.
    /// Coffee mug = mountain, books = buildings, kitchen floor = open world.
    /// </summary>
    public class WorldBuilder : MonoBehaviour
    {
        static Material floorMat, mugMat, bookMat, tableMat, wallMat, metalMat;

        public Transform WorldRoot { get; private set; }
        public Vector3 PlayerSpawn => new Vector3(-20f, 1f, -15f);

        public Bosses.CatBoss CatBoss { get; private set; }
        public Bosses.VacuumBoss VacuumBoss { get; private set; }
        public Bosses.WashingMachineBoss WashingBoss { get; private set; }
        public Bosses.FootstepsBoss FootstepsBoss { get; private set; }

        public void BuildAll(Transform player)
        {
            var routine = BuildAllWithBossesRoutine(player);
            while (routine.MoveNext()) { }
        }

        public IEnumerator BuildEnvironmentRoutine()
        {
            WorldRoot = new GameObject("KitchenWorld").transform;
            InitMaterials();

            BuildFloor();
            yield return null;

            BuildCoffeeMugMountain(new Vector3(25f, 0f, 20f));
            yield return null;

            BuildBookCity(new Vector3(-30f, 0f, 25f));
            yield return null;

            BuildTableLegs(new Vector3(0f, 0f, -40f));
            BuildSink(new Vector3(45f, 0f, -10f));
            yield return null;

            BuildStove(new Vector3(-45f, 0f, -5f));
            BuildCrumbHills();
            yield return null;

            BuildCollectibles();
            yield return null;
        }

        public void BuildBossArenasFor(Transform player)
        {
            if (player == null) return;
            BuildBossArenas(player);
        }

        public IEnumerator BuildAllWithBossesRoutine(Transform player)
        {
            yield return BuildEnvironmentRoutine();
            BuildBossArenasFor(player);
        }

        void InitMaterials()
        {
            floorMat = CreateMaterial(new Color(0.85f, 0.82f, 0.75f));
            mugMat = CreateMaterial(new Color(0.9f, 0.3f, 0.2f));
            bookMat = CreateMaterial(new Color(0.2f, 0.35f, 0.65f));
            tableMat = CreateMaterial(new Color(0.55f, 0.35f, 0.15f));
            wallMat = CreateMaterial(new Color(0.95f, 0.93f, 0.88f));
            metalMat = CreateMaterial(new Color(0.7f, 0.72f, 0.75f));
        }

        public static Material CreateMaterial(Color color) => MaterialCache.Get(color);

        void BuildFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "KitchenFloor";
            floor.transform.SetParent(WorldRoot);
            floor.transform.localScale = new Vector3(12f, 1f, 12f);
            floor.transform.position = Vector3.zero;
            floor.layer = LayerMask.NameToLayer("Ground");
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;
        }

        void BuildCoffeeMugMountain(Vector3 basePos)
        {
            var mugRoot = new GameObject("CoffeeMugMountain").transform;
            mugRoot.SetParent(WorldRoot);
            mugRoot.position = basePos;

            // Mug body — cylinder mountain
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "MugBody";
            body.transform.SetParent(mugRoot);
            body.transform.localScale = new Vector3(8f, 6f, 8f);
            body.transform.localPosition = new Vector3(0f, 6f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = mugMat;

            // Handle — torus-like arch
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "MugHandle";
            handle.transform.SetParent(mugRoot);
            handle.transform.localScale = new Vector3(1.5f, 5f, 3f);
            handle.transform.localPosition = new Vector3(5f, 5f, 0f);
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
            handle.GetComponent<Renderer>().sharedMaterial = mugMat;

            // Coffee surface at top — dark liquid hazard
            var coffee = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coffee.name = "HotCoffee";
            coffee.transform.SetParent(mugRoot);
            coffee.transform.localScale = new Vector3(7f, 0.1f, 7f);
            coffee.transform.localPosition = new Vector3(0f, 12f, 0f);
            coffee.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.25f, 0.12f, 0.05f));
            coffee.GetComponent<Collider>().isTrigger = true;
            coffee.AddComponent<HazardZone>();

            // Summit platform
            var summit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            summit.name = "Summit";
            summit.transform.SetParent(mugRoot);
            summit.transform.localScale = new Vector3(3f, 0.3f, 3f);
            summit.transform.localPosition = new Vector3(0f, 12.3f, 0f);
            summit.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.6f, 0.55f, 0.5f));

            // Label sign
            CreateSign(mugRoot, new Vector3(0f, 14f, 0f), "MUG PEAK", "A coffee mug. A mountain.");
        }

        void BuildBookCity(Vector3 basePos)
        {
            var cityRoot = new GameObject("BookCity").transform;
            cityRoot.SetParent(WorldRoot);
            cityRoot.position = basePos;

            Color[] colors =
            {
                new Color(0.8f, 0.2f, 0.2f),
                new Color(0.2f, 0.6f, 0.3f),
                new Color(0.3f, 0.3f, 0.7f),
                new Color(0.7f, 0.6f, 0.2f),
                new Color(0.5f, 0.2f, 0.6f)
            };

            for (int i = 0; i < 12; i++)
            {
                float x = (i % 4) * 6f - 9f;
                float z = (i / 4) * 7f;
                float h = Random.Range(3f, 10f);

                var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                book.name = $"BookBuilding_{i}";
                book.transform.SetParent(cityRoot);
                book.transform.localScale = new Vector3(4f, h, 5f);
                book.transform.localPosition = new Vector3(x, h * 0.5f, z);
                book.GetComponent<Renderer>().sharedMaterial = CreateMaterial(colors[i % colors.Length]);
            }

            CreateSign(cityRoot, new Vector3(0f, 12f, -5f), "BOOK CITY", "Each book is a skyscraper.");
        }

        void BuildTableLegs(Vector3 basePos)
        {
            var tableRoot = new GameObject("TableLegs").transform;
            tableRoot.SetParent(WorldRoot);
            tableRoot.position = basePos;

            Vector3[] legPositions = { new Vector3(-15f, 0f, -10f), new Vector3(15f, 0f, -10f), new Vector3(-15f, 0f, 10f), new Vector3(15f, 0f, 10f) };
            foreach (var lp in legPositions)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "TableLeg";
                leg.transform.SetParent(tableRoot);
                leg.transform.localScale = new Vector3(3f, 15f, 3f);
                leg.transform.localPosition = lp + new Vector3(0f, 15f, 0f);
                leg.GetComponent<Renderer>().sharedMaterial = tableMat;
            }

            // Table underside — ceiling of the world section
            var underside = GameObject.CreatePrimitive(PrimitiveType.Cube);
            underside.name = "TableUnderside";
            underside.transform.SetParent(tableRoot);
            underside.transform.localScale = new Vector3(40f, 1f, 30f);
            underside.transform.localPosition = new Vector3(0f, 30f, 0f);
            underside.GetComponent<Renderer>().sharedMaterial = tableMat;
        }

        void BuildSink(Vector3 pos)
        {
            var sinkRoot = new GameObject("SinkBasin").transform;
            sinkRoot.SetParent(WorldRoot);
            sinkRoot.position = pos;

            var basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basin.transform.SetParent(sinkRoot);
            basin.transform.localScale = new Vector3(12f, 3f, 8f);
            basin.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            basin.GetComponent<Renderer>().sharedMaterial = metalMat;

            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "WaterHazard";
            water.transform.SetParent(sinkRoot);
            water.transform.localScale = new Vector3(10f, 0.5f, 6f);
            water.transform.localPosition = new Vector3(0f, 1f, 0f);
            water.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.3f, 0.6f, 0.9f, 0.7f));
            water.GetComponent<Collider>().isTrigger = true;
            water.AddComponent<HazardZone>();

            CreateSign(sinkRoot, new Vector3(0f, 5f, 0f), "THE SINK", "A lake in miniature.");
        }

        void BuildStove(Vector3 pos)
        {
            var stoveRoot = new GameObject("Stove").transform;
            stoveRoot.SetParent(WorldRoot);
            stoveRoot.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(stoveRoot);
            body.transform.localScale = new Vector3(10f, 4f, 8f);
            body.transform.localPosition = new Vector3(0f, 2f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = metalMat;

            for (int i = 0; i < 4; i++)
            {
                var burner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                burner.name = $"Burner_{i}";
                burner.transform.SetParent(stoveRoot);
                burner.transform.localScale = new Vector3(2f, 0.2f, 2f);
                burner.transform.localPosition = new Vector3((i % 2) * 4f - 2f, 4.1f, (i / 2) * 4f - 2f);
                burner.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.3f, 0.1f));
                burner.GetComponent<Collider>().isTrigger = true;
                burner.AddComponent<HazardZone>();
            }

            CreateSign(stoveRoot, new Vector3(0f, 6f, 0f), "STOVE VOLCANO", "Burners are lava pools.");
        }

        void BuildCrumbHills()
        {
            for (int i = 0; i < 8; i++)
            {
                var crumb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crumb.name = $"Crumb_{i}";
                crumb.transform.SetParent(WorldRoot);
                float scale = Random.Range(0.8f, 2.5f);
                crumb.transform.localScale = Vector3.one * scale;
                crumb.transform.position = new Vector3(Random.Range(-50f, 50f), scale * 0.4f, Random.Range(-50f, 50f));
                crumb.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.75f, 0.55f, 0.25f));
            }
        }

        void BuildCollectibles()
        {
            Vector3[] positions =
            {
                new Vector3(-10f, 1f, 0f), new Vector3(10f, 1f, 5f), new Vector3(0f, 1f, 15f),
                new Vector3(-25f, 1f, -20f), new Vector3(30f, 1f, -15f), new Vector3(-5f, 1f, 30f)
            };

            foreach (var p in positions)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.name = "SugarCrystal";
                c.transform.SetParent(WorldRoot);
                c.transform.position = p;
                c.transform.localScale = Vector3.one * 0.6f;
                c.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(1f, 0.95f, 0.7f));
                var col = c.GetComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 1.5f;
                c.AddComponent<Collectible>();
            }
        }

        void BuildBossArenas(Transform player)
        {
            CatBoss = BuildCatBoss(new Vector3(0f, 0f, 0f), player);
            VacuumBoss = BuildVacuumBoss(new Vector3(-35f, 0f, -25f), player);
            WashingBoss = BuildWashingBoss(new Vector3(40f, 0f, -30f), player);
            FootstepsBoss = BuildFootstepsBoss(new Vector3(0f, 0f, 45f), player);
        }

        Bosses.CatBoss BuildCatBoss(Vector3 pos, Transform player)
        {
            var root = new GameObject("Boss_Cat").transform;
            root.SetParent(WorldRoot);
            root.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CatBody";
            body.transform.SetParent(root);
            body.transform.localScale = new Vector3(6f, 3f, 10f);
            body.transform.localPosition = new Vector3(0f, 2f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.55f, 0.15f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "CatHead";
            head.transform.SetParent(root);
            head.transform.localScale = Vector3.one * 3.5f;
            head.transform.localPosition = new Vector3(0f, 3f, 5f);
            head.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.9f, 0.55f, 0.15f));

            var paw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            paw.name = "CatPaw";
            paw.transform.SetParent(root);
            paw.transform.localScale = Vector3.one * 2f;
            paw.transform.localPosition = new Vector3(2f, 1f, 4f);
            paw.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.95f, 0.6f, 0.2f));

            var boss = root.gameObject.AddComponent<Bosses.CatBoss>();
            boss.Initialize(player);
            boss.SetupVisuals(body.transform, paw.transform);

            var weak = head.AddComponent<Bosses.BossWeakPoint>();
            weak.Bind(boss);

            CreateBossTrigger(root.position, new Vector3(30f, 5f, 30f), boss, "Cat Territory");
            return boss;
        }

        Bosses.VacuumBoss BuildVacuumBoss(Vector3 pos, Transform player)
        {
            var root = new GameObject("Boss_Vacuum").transform;
            root.SetParent(WorldRoot);
            root.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(root);
            body.transform.localScale = new Vector3(4f, 2f, 4f);
            body.transform.localPosition = new Vector3(0f, 2f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = metalMat;

            var nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nozzle.name = "Nozzle";
            nozzle.transform.SetParent(root);
            nozzle.transform.localScale = new Vector3(2f, 1f, 2f);
            nozzle.transform.localPosition = new Vector3(0f, 1f, 4f);
            nozzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            nozzle.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.2f, 0.2f, 0.2f));

            var hose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hose.transform.SetParent(root);
            hose.transform.localScale = new Vector3(1f, 3f, 1f);
            hose.transform.localPosition = new Vector3(-3f, 3f, -2f);
            hose.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.15f, 0.15f, 0.15f));

            var particles = CreateDustParticles(root);

            var boss = root.gameObject.AddComponent<Bosses.VacuumBoss>();
            boss.Initialize(player);
            boss.SetupVisuals(nozzle.transform, particles);
            boss.SetPatrolPoints(new[]
            {
                pos + new Vector3(-10f, 0f, 0f),
                pos + new Vector3(10f, 0f, 0f),
                pos + new Vector3(10f, 0f, 15f),
                pos + new Vector3(-10f, 0f, 15f)
            });

            nozzle.AddComponent<Bosses.BossWeakPoint>().Bind(boss);
            CreateBossTrigger(pos, new Vector3(25f, 5f, 20f), boss, "Vacuum Zone");
            return boss;
        }

        ParticleSystem CreateDustParticles(Transform parent)
        {
            var go = new GameObject("DustParticles");
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(0f, 1f, 3f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.3f;
            main.startSpeed = 5f;
            main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.6f);
            main.maxParticles = 30;
            var emission = ps.emission;
            emission.rateOverTime = 15f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            ps.Stop();
            return ps;
        }

        Bosses.WashingMachineBoss BuildWashingBoss(Vector3 pos, Transform player)
        {
            var root = new GameObject("Boss_WashingMachine").transform;
            root.SetParent(WorldRoot);
            root.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(root);
            body.transform.localScale = new Vector3(8f, 8f, 6f);
            body.transform.localPosition = new Vector3(0f, 4f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.95f, 0.95f, 0.98f));

            var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drum.name = "Drum";
            drum.transform.SetParent(root);
            drum.transform.localScale = new Vector3(4f, 0.5f, 4f);
            drum.transform.localPosition = new Vector3(0f, 5f, 0f);
            drum.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            drum.GetComponent<Renderer>().sharedMaterial = metalMat;
            Destroy(drum.GetComponent<Collider>());

            var door = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            door.name = "Door";
            door.transform.SetParent(root);
            door.transform.localScale = new Vector3(3.5f, 0.3f, 3.5f);
            door.transform.localPosition = new Vector3(0f, 4f, 3.1f);
            door.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            door.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.7f, 0.8f, 0.9f, 0.5f));
            Destroy(door.GetComponent<Collider>());

            var splash = new GameObject("SplashOrigin").transform;
            splash.SetParent(root);
            splash.localPosition = new Vector3(0f, 6f, 2f);

            var boss = root.gameObject.AddComponent<Bosses.WashingMachineBoss>();
            boss.Initialize(player);
            boss.SetupVisuals(drum.transform, door.transform, splash);
            drum.AddComponent<Bosses.BossWeakPoint>().Bind(boss);

            CreateBossTrigger(pos, new Vector3(20f, 6f, 18f), boss, "Laundry Lair");
            return boss;
        }

        Bosses.FootstepsBoss BuildFootstepsBoss(Vector3 pos, Transform player)
        {
            var root = new GameObject("Boss_Footsteps").transform;
            root.SetParent(WorldRoot);
            root.position = pos;

            var leftFoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftFoot.name = "LeftFoot";
            leftFoot.transform.SetParent(root);
            leftFoot.transform.localScale = new Vector3(6f, 2f, 12f);
            leftFoot.transform.localPosition = new Vector3(-8f, 20f, 0f);
            leftFoot.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.3f, 0.25f, 0.2f));
            Destroy(leftFoot.GetComponent<Collider>());

            var rightFoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightFoot.name = "RightFoot";
            rightFoot.transform.SetParent(root);
            rightFoot.transform.localScale = new Vector3(6f, 2f, 12f);
            rightFoot.transform.localPosition = new Vector3(8f, 20f, 0f);
            rightFoot.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.3f, 0.25f, 0.2f));
            Destroy(rightFoot.GetComponent<Collider>());

            var boss = root.gameObject.AddComponent<Bosses.FootstepsBoss>();
            boss.Initialize(player);
            boss.SetupVisuals(leftFoot.transform, rightFoot.transform);

            var weak = leftFoot.AddComponent<Bosses.BossWeakPoint>();
            weak.Bind(boss);

            CreateBossTrigger(pos, new Vector3(40f, 5f, 35f), boss, "Human Approaches...");
            return boss;
        }

        void CreateBossTrigger(Vector3 center, Vector3 size, Bosses.BossBase boss, string label)
        {
            var trigger = new GameObject($"Trigger_{boss.BossName}");
            trigger.transform.SetParent(WorldRoot);
            trigger.transform.position = center;
            var box = trigger.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var bt = trigger.AddComponent<BossTrigger>();
            bt.Bind(boss);

            CreateSign(trigger.transform, new Vector3(0f, size.y + 2f, 0f), label.ToUpper(), "Boss Arena");
        }

        void CreateSign(Transform parent, Vector3 localPos, string title, string subtitle)
        {
            var sign = new GameObject($"Sign_{title}");
            sign.transform.SetParent(parent);
            sign.transform.localPosition = localPos;

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(sign.transform);
            pole.transform.localScale = new Vector3(0.2f, 2f, 0.2f);
            pole.transform.localPosition = Vector3.zero;
            pole.GetComponent<Renderer>().sharedMaterial = tableMat;
            Destroy(pole.GetComponent<Collider>());

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.transform.SetParent(sign.transform);
            board.transform.localScale = new Vector3(4f, 1.5f, 0.2f);
            board.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            board.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.95f, 0.9f, 0.7f));
            Destroy(board.GetComponent<Collider>());
        }
    }
}
