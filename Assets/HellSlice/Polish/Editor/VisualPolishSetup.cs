using System.IO;
using HellSlice.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HellSlice.Polish.EditorTools
{
    /// <summary>
    /// Adds the visual layer to the authored scenes without regenerating them: hell sky, emissive lava,
    /// particle flames, sparse ash/embers/smoke, restrained post-processing and character models.
    /// Re-running replaces only what this tool added ("Visual Polish (added)" root and PolishSkin components).
    /// </summary>
    public static class VisualPolishSetup
    {
        const string Root = "Assets/HellSlice/Polish";
        const string Mat = Root + "/Materials";
        const string PlayerModel = Root + "/Characters/BusinessMan/BusinessMan.fbx";
        const string AddedRootName = "Visual Polish (added)";

        [MenuItem("Tools/A Day Through Hell/Apply Visual Polish")]
        public static void ApplyMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplyAll();
            EditorSceneManager.OpenScene(SliceBuild.First);
        }

        /// <summary>Regenerates the shared prefabs/profile from code (only needed after editing this tool).</summary>
        [MenuItem("Tools/A Day Through Hell/Rebuild Visual Polish Assets")]
        public static void RebuildAssetsMenu() { rebuild = true; try { ApplyAll(); } finally { rebuild = false; } }
        static bool rebuild;

        /// <summary>Batch-mode entry: -executeMethod HellSlice.Polish.EditorTools.VisualPolishSetup.ApplyAll</summary>
        public static void ApplyAll()
        {
            var mats = EnsureMaterials();
            var profile = EnsureProfile();
            var prefabs = EnsurePrefabs(mats, profile);
            // Every stage shares the hell sky/background (approved for shared use across levels).
            foreach (var scene in new[] { SliceBuild.First, SliceBuild.Possession, SliceBuild.FeedingFlame, SliceBuild.Landing })
                Apply(scene, mats, prefabs);
            AssetDatabase.SaveAssets();
            Debug.Log("[VisualPolish] Applied to FirstPlayable, WeightOfPossession, FeedingFlame and NextLanding.");
        }

        /// <summary>Batch-mode entry: apply then build Windows player.</summary>
        public static void ApplyAndBuild() { ApplyAll(); SliceBuild.BuildWindows(); }

        struct Mats { public Material Sky, Lava, Reveal, Flame, FlameCore, Ember, Ash, Smoke, Wisp; }

        static Mats EnsureMaterials()
        {
            Directory.CreateDirectory(Mat);
            var m = new Mats
            {
                Sky = MakeMat("HellSky", "HellSlice/Hell Sky"),
                Lava = MakeMat("Lava", "HellSlice/Lava"),
                Reveal = MakeMat("ShadowReveal", "HellSlice/Shadow Reveal"),
                Flame = Glow("Flame", new Color(1.9f, .55f, .12f, .8f), 1, 1),
                FlameCore = Glow("FlameCore", new Color(2.4f, 1.3f, .45f, .75f), 1, 1),
                Ember = Glow("Ember", new Color(4f, 1.4f, .35f, 1), 0, 1),
                Ash = Glow("Ash", new Color(.32f, .29f, .28f, 1), 0, 10),
                Smoke = Glow("Smoke", new Color(.07f, .045f, .04f, 1), 2, 10),
                Wisp = Glow("Wisp", new Color(.55f, .62f, .85f, 1), 2, 1),
            };
            m.Reveal.SetFloat("_Reveal", 0);
            EditorUtility.SetDirty(m.Reveal);
            return m;
        }
        static Material MakeMat(string name, string shader)
        {
            string path = $"{Mat}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            var s = Shader.Find(shader);
            if (s == null) throw new System.InvalidOperationException("Shader not found/compiled: " + shader);
            if (existing != null) { existing.shader = s; return existing; }
            var mat = new Material(s) { name = name };
            AssetDatabase.CreateAsset(mat, path); return mat;
        }
        static Material Glow(string name, Color tint, float shape, float dstBlend)
        {
            var m = MakeMat(name, "HellSlice/Glow Particle");
            m.SetColor("_Tint", tint); m.SetFloat("_Shape", shape); m.SetFloat("_DstBlend", dstBlend);
            m.SetFloat("_Softness", shape > 1.5f ? 1.4f : 2f);
            m.renderQueue = 3000; EditorUtility.SetDirty(m); return m;
        }

        static VolumeProfile EnsureProfile()
        {
            // Re-use the asset (stable GUID across runs and machines); only its components are rebuilt.
            string path = Root + "/HellVolumeProfile.asset";
            var p = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (p != null && !rebuild) return p; // existing asset is the source of truth; no churn on re-apply
            if (p == null) { p = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(p, path); }
            foreach (var old in p.components.ToArray()) { AssetDatabase.RemoveObjectFromAsset(old); Object.DestroyImmediate(old, true); }
            p.components.Clear();
            var tone = p.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
            var bloom = p.Add<Bloom>(true);
            bloom.threshold.Override(.95f); bloom.intensity.Override(.9f); bloom.scatter.Override(.68f);
            bloom.tint.Override(new Color(1f, .62f, .4f)); bloom.highQualityFiltering.Override(false);
            var vig = p.Add<Vignette>(true); vig.intensity.Override(.34f); vig.smoothness.Override(.45f); vig.color.Override(new Color(.08f, 0, 0));
            var ca = p.Add<ColorAdjustments>(true);
            ca.postExposure.Override(.25f); ca.contrast.Override(16f); ca.saturation.Override(6f); ca.colorFilter.Override(new Color(1f, .93f, .88f));
            foreach (var c in p.components) { c.name = c.GetType().Name; AssetDatabase.AddObjectToAsset(c, p); }
            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        struct Prefabs { public GameObject Flame, Atmosphere, Post; }
        const string PrefabDir = Root + "/Prefabs";

        /// <summary>Reusable assets: drop FlameEruption under any flame hazard, HellAtmosphere and HellPostProcessing into any scene.</summary>
        static Prefabs EnsurePrefabs(Mats m, VolumeProfile profile)
        {
            Directory.CreateDirectory(PrefabDir);
            GameObject Load(string n) => AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{n}.prefab");
            if (!rebuild && Load("FlameEruption") != null && Load("HellAtmosphere") != null && Load("HellPostProcessing") != null)
                return new Prefabs { Flame = Load("FlameEruption"), Atmosphere = Load("HellAtmosphere"), Post = Load("HellPostProcessing") };
            var flame = new GameObject("FlameEruption");
            // Five nozzle jets across the lane: a flamethrower / eruption column, not a campfire.
            for (int j = -2; j <= 2; j++)
            {
                var nozzle = new GameObject("Nozzle " + (j + 3)); nozzle.transform.SetParent(flame.transform, false);
                nozzle.transform.localPosition = new Vector3(j * .82f, 0, 0);
                Jet(nozzle.transform, m.Flame, 24, new Vector2(.55f, .8f), new Vector2(.35f, .6f), new Vector2(11, 15), 18, new Color(1, .62f, .25f));
                Jet(nozzle.transform, m.FlameCore, 14, new Vector2(.18f, .3f), new Vector2(.2f, .32f), new Vector2(9, 12), 8, new Color(1, .9f, .65f));
            }
            Sparks(flame.transform, m.Ember);
            var smoke = new GameObject("Eruption smoke"); smoke.transform.SetParent(flame.transform, false); smoke.transform.localPosition = Vector3.up * 5.5f;
            EruptionSmoke(smoke.transform, m.Smoke);
            var light = new GameObject("Flame light").AddComponent<Light>(); light.transform.SetParent(flame.transform, false);
            light.transform.localPosition = Vector3.up * 2.2f; light.type = LightType.Point; light.color = new Color(1, .45f, .12f);
            light.range = 11; light.intensity = 7; light.shadows = LightShadows.None;
            light.gameObject.AddComponent<PolishLightFlicker>();

            var atmosphere = new GameObject("HellAtmosphere");
            var defaultPlay = new Bounds(new Vector3(0, 0, 20), new Vector3(14, 4, 54));
            var defaultLava = new Bounds(new Vector3(0, -2, 22), new Vector3(70, .1f, 85));
            Ash(atmosphere.transform, defaultPlay, m.Ash);
            Embers(atmosphere.transform, defaultLava, m.Ember);
            Smoke(atmosphere.transform, defaultLava, m.Smoke);
            Wisps(atmosphere.transform, defaultPlay, m.Wisp);

            var post = new GameObject("HellPostProcessing");
            var volume = post.AddComponent<Volume>(); volume.isGlobal = true; volume.sharedProfile = profile; volume.priority = 10;

            return new Prefabs { Flame = SavePrefab(flame), Atmosphere = SavePrefab(atmosphere), Post = SavePrefab(post) };
        }
        static GameObject SavePrefab(GameObject go)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{go.name}.prefab"); // overwrite keeps the GUID
            Object.DestroyImmediate(go);
            return prefab;
        }
        /// <summary>Scene-specific placement of the shared atmosphere prefab (stored as instance overrides).</summary>
        static void FitAtmosphere(Transform root, Bounds play, Bounds lava, bool hasLava)
        {
            void Box(string child, Vector3 position, Vector3 scale)
            {
                var t = root.Find(child); if (t == null) return;
                t.position = position; var sh = t.GetComponent<ParticleSystem>().shape; sh.scale = scale;
            }
            Box("Drifting ash", new Vector3(play.center.x, play.max.y + 5, play.center.z), new Vector3(play.size.x + 8, 1, play.size.z + 8));
            Box("Lava embers", new Vector3(lava.center.x, lava.max.y + .1f, lava.center.z), new Vector3(lava.size.x, .2f, lava.size.z));
            Box("Smoke plumes", new Vector3(lava.center.x, lava.max.y + .5f, lava.center.z), new Vector3(lava.size.x, .2f, lava.size.z));
            var smoke = root.Find("Smoke plumes"); if (smoke != null) smoke.gameObject.SetActive(hasLava);
            var wisps = root.Find("Distant wisps");
            if (wisps != null)
            {
                wisps.position = new Vector3(play.center.x, play.min.y + 1, play.center.z);
                var sh = wisps.GetComponent<ParticleSystem>().shape; sh.radius = Mathf.Max(play.extents.x, play.extents.z) + 18;
            }
        }

        static void Apply(string scenePath, Mats m, Prefabs prefabs)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects()) if (go.name == AddedRootName) Object.DestroyImmediate(go);
            foreach (var skin in Object.FindObjectsByType<PolishSkin>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(skin);
            var added = new GameObject(AddedRootName);
            var session = Object.FindFirstObjectByType<SliceSession>();
            bool landing = session != null && session.landing;

            // Sky, fog, ambient: oppressive warm haze with a cool fill so bodies and safe ground stay readable.
            RenderSettings.skybox = m.Sky;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.13f, .045f, .03f); RenderSettings.fogDensity = landing ? .016f : .02f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            // Cool sky fill keeps the route and bodies readable; warm equator/ground echo the lava bounce.
            RenderSettings.ambientSkyColor = new Color(.62f, .64f, .80f);
            RenderSettings.ambientEquatorColor = new Color(.66f, .34f, .22f);
            RenderSettings.ambientGroundColor = new Color(.26f, .11f, .07f);
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (light.type == LightType.Directional) { light.color = new Color(1f, .68f, .5f); light.intensity = 1.9f; light.shadows = LightShadows.Soft; }
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                cam.clearFlags = CameraClearFlags.Skybox; cam.farClipPlane = Mathf.Max(cam.farClipPlane, 250);
                var data = cam.GetUniversalAdditionalCameraData(); data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                EditorUtility.SetDirty(cam); EditorUtility.SetDirty(data);
            }
            PrefabUtility.InstantiatePrefab(prefabs.Post, added.transform);

            // Lava: every renderer under a lethal lava volume and any scenery lava plane.
            Bounds? lavaBounds = null;
            foreach (var lava in Object.FindObjectsByType<SliceLava>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (var r in lava.GetComponentsInChildren<Renderer>(true)) { Retexture(r, m.Lava); lavaBounds = Grow(lavaBounds, r.bounds); }
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (r.name.ToLowerInvariant().Contains("lava")) { Retexture(r, m.Lava); lavaBounds = Grow(lavaBounds, r.bounds); }

            // Flames: particle fire inside each hazard's existing Flames object, so timing/toggling is unchanged.
            foreach (var hazard in Object.FindObjectsByType<SliceHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (hazard.Flames == null) continue;
                foreach (var r in hazard.Flames.GetComponentsInChildren<MeshRenderer>(true)) { r.enabled = false; EditorUtility.SetDirty(r); }
                foreach (var name in new[] { "Flame VFX", "FlameEruption" }) { var old = hazard.Flames.Find(name); if (old != null) Object.DestroyImmediate(old.gameObject); }
                var vfx = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Flame, hazard.Flames);
                vfx.transform.position = new Vector3(hazard.transform.position.x, hazard.transform.position.y + .05f, hazard.transform.position.z);
                EditorUtility.SetDirty(hazard.Flames.gameObject);
            }

            // Atmosphere: sparse and cheap (Intel Xe budget).
            var atmosphere = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Atmosphere, added.transform);
            Bounds play = PlayBounds();
            Bounds lavaArea = lavaBounds ?? new Bounds(play.center + Vector3.down * 3, new Vector3(play.size.x + 30, 1, play.size.z + 20));
            FitAtmosphere(atmosphere.transform, play, lavaArea, lavaBounds.HasValue);

            // Characters.
            var playerModel = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModel);
            foreach (var p in Object.FindObjectsByType<SlicePlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var skin = p.gameObject.AddComponent<PolishSkin>();
                skin.Role = PolishSkin.Kind.Player; skin.Model = playerModel; skin.RevealTemplate = m.Reveal; skin.TargetHeight = 1.8f;
                EditorUtility.SetDirty(p.gameObject);
            }
            // Enemies are left alone: the Stage 1 Cyclops is authored by the developer through SliceEnemy.Appearance
            // (Cyclops-Visual.prefab), and other stages keep their own guardians.

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static Bounds? Grow(Bounds? b, Bounds add) { if (b == null) return add; var v = b.Value; v.Encapsulate(add); return v; }
        static void Retexture(Renderer r, Material mat)
        {
            var slots = r.sharedMaterials; for (int i = 0; i < slots.Length; i++) slots[i] = mat;
            r.sharedMaterials = slots; r.shadowCastingMode = ShadowCastingMode.Off; EditorUtility.SetDirty(r);
        }
        static Bounds PlayBounds()
        {
            Bounds? b = null;
            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (!c.isTrigger && c.bounds.size.magnitude < 200) b = Grow(b, c.bounds);
            return b ?? new Bounds(new Vector3(0, 0, 20), new Vector3(14, 4, 54));
        }

        // ---------- particle builders ----------
        static ParticleSystem Particles(Transform parent, string name, Material mat, int max)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main; main.maxParticles = max; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = true; main.loop = true;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }
        static Gradient Fade(params (Color c, float t)[] keys)
        {
            var g = new Gradient();
            var ck = new GradientColorKey[keys.Length]; var ak = new GradientAlphaKey[keys.Length];
            for (int i = 0; i < keys.Length; i++) { ck[i] = new GradientColorKey(keys[i].c, keys[i].t); ak[i] = new GradientAlphaKey(keys[i].c.a, keys[i].t); }
            g.SetKeys(ck, ak); return g;
        }
        static void Velocity(ParticleSystem ps, Vector2 x, Vector2 y, Vector2 z)
        {
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
            v.x = new ParticleSystem.MinMaxCurve(x.x, x.y); v.y = new ParticleSystem.MinMaxCurve(y.x, y.y); v.z = new ParticleSystem.MinMaxCurve(z.x, z.y);
        }
        static void Jet(Transform parent, Material mat, float rate, Vector2 life, Vector2 size, Vector2 speed, int burst, Color start)
        {
            var ps = Particles(parent, "Jet " + mat.name, mat, 120);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y); main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startColor = start; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.gravityModifier = -.35f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var em = ps.emission; em.rateOverTime = rate; em.SetBursts(new[] { new ParticleSystem.Burst(0, (short)burst) });
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 7; sh.radius = .12f; sh.rotation = new Vector3(-90, 0, 0);
            var col = ps.colorOverLifetime; col.enabled = true;
            col.color = Fade((new Color(1, 1, .9f, 0), 0), (new Color(1, .85f, .45f, 1), .06f), (new Color(1, .42f, .08f, .95f), .4f), (new Color(.7f, .1f, .02f, .5f), .75f), (new Color(.15f, .02f, .01f, 0), 1));
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .5f), new Keyframe(.4f, 1.1f), new Keyframe(1, 1.7f)));
            var lim = ps.limitVelocityOverLifetime; lim.enabled = true; lim.limit = 7; lim.dampen = .08f; // fast blast, then billowing
            var noise = ps.noise; noise.enabled = true; noise.strength = .7f; noise.frequency = 1.4f; noise.scrollSpeed = 2.5f;
            var r = ps.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = .11f; r.lengthScale = 2.2f;
        }
        static void Sparks(Transform parent, Material mat)
        {
            var ps = Particles(parent, "Eruption sparks", mat, 160);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(.7f, 1.5f); main.startSpeed = new ParticleSystem.MinMaxCurve(9, 17);
            main.startSize = new ParticleSystem.MinMaxCurve(.04f, .09f); main.gravityModifier = 1.4f; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .8f, .4f), new Color(1, .45f, .1f));
            var em = ps.emission; em.rateOverTime = 55; em.SetBursts(new[] { new ParticleSystem.Burst(0, 45) });
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 22; sh.radius = 1.6f; sh.radiusThickness = 1; sh.rotation = new Vector3(-90, 0, 0);
            var r = ps.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = .06f; r.lengthScale = 2;
        }
        static void EruptionSmoke(Transform parent, Material mat)
        {
            var ps = Particles(parent, "Smoke column", mat, 40);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4); main.startSpeed = new ParticleSystem.MinMaxCurve(1, 2.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(2, 3.5f); main.startColor = new Color(1, 1, 1, .7f); main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var em = ps.emission; em.rateOverTime = 9;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(4, .5f, .8f);
            var col = ps.colorOverLifetime; col.enabled = true; col.color = Fade((new Color(1, 1, 1, 0), 0), (new Color(1, 1, 1, .6f), .2f), (new Color(1, 1, 1, 0), 1));
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(1, 1.8f)));
        }
        static void Ash(Transform parent, Bounds play, Material mat)
        {
            var ps = Particles(parent, "Drifting ash", mat, 220);
            ps.transform.position = new Vector3(play.center.x, play.max.y + 5, play.center.z);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(9, 14); main.startSpeed = 0;
            main.startSize = new ParticleSystem.MinMaxCurve(.03f, .07f); main.startColor = new Color(1, 1, 1, .75f); main.prewarm = true;
            var em = ps.emission; em.rateOverTime = 16;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(play.size.x + 8, 1, play.size.z + 8);
            Velocity(ps, new Vector2(-.3f, .3f), new Vector2(-.7f, -.35f), new Vector2(-.2f, .4f));
            var noise = ps.noise; noise.enabled = true; noise.strength = .35f; noise.frequency = .4f;
        }
        static void Embers(Transform parent, Bounds lava, Material mat)
        {
            var ps = Particles(parent, "Lava embers", mat, 180);
            ps.transform.position = new Vector3(lava.center.x, lava.max.y + .1f, lava.center.z);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(3, 6); main.startSpeed = 0;
            main.startSize = new ParticleSystem.MinMaxCurve(.04f, .11f); main.prewarm = true;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .5f, .12f), new Color(1, .8f, .35f));
            var em = ps.emission; em.rateOverTime = 28;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(lava.size.x, .2f, lava.size.z);
            Velocity(ps, new Vector2(-.3f, .3f), new Vector2(.6f, 1.6f), new Vector2(-.3f, .3f));
            var col = ps.colorOverLifetime; col.enabled = true; col.color = Fade((new Color(1, 1, 1, 0), 0), (new Color(1, 1, 1, 1), .1f), (new Color(1, .6f, .4f, .8f), .7f), (new Color(1, .3f, .1f, 0), 1));
            var noise = ps.noise; noise.enabled = true; noise.strength = .8f; noise.frequency = .6f; noise.scrollSpeed = .4f;
        }
        static void Smoke(Transform parent, Bounds lava, Material mat)
        {
            var ps = Particles(parent, "Smoke plumes", mat, 40);
            ps.transform.position = new Vector3(lava.center.x, lava.max.y + .5f, lava.center.z);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(7, 11); main.startSpeed = 0; main.prewarm = true;
            main.startSize = new ParticleSystem.MinMaxCurve(4, 8); main.startColor = new Color(1, 1, 1, .5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var em = ps.emission; em.rateOverTime = 3.5f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(lava.size.x, .2f, lava.size.z);
            Velocity(ps, new Vector2(-.2f, .2f), new Vector2(.5f, 1f), new Vector2(-.2f, .2f));
            var col = ps.colorOverLifetime; col.enabled = true; col.color = Fade((new Color(1, 1, 1, 0), 0), (new Color(1, 1, 1, .55f), .25f), (new Color(1, 1, 1, 0), 1));
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .5f), new Keyframe(1, 1.6f)));
        }
        static void Wisps(Transform parent, Bounds play, Material mat)
        {
            // Pale restless wisps far beyond the walls: atmosphere only, never on the playable route.
            var ps = Particles(parent, "Distant wisps", mat, 60);
            ps.transform.position = new Vector3(play.center.x, play.min.y + 1, play.center.z);
            var main = ps.main; main.startLifetime = new ParticleSystem.MinMaxCurve(6, 10); main.startSpeed = 0; main.prewarm = true;
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.6f); main.startColor = new Color(1, 1, 1, .35f);
            var em = ps.emission; em.rateOverTime = 6;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Donut;
            sh.radius = Mathf.Max(play.extents.x, play.extents.z) + 18; sh.donutRadius = 6; sh.rotation = new Vector3(90, 0, 0);
            Velocity(ps, new Vector2(-.3f, .3f), new Vector2(.4f, 1f), new Vector2(-.3f, .3f));
            var col = ps.colorOverLifetime; col.enabled = true; col.color = Fade((new Color(1, 1, 1, 0), 0), (new Color(1, 1, 1, .8f), .3f), (new Color(1, 1, 1, 0), 1));
            var noise = ps.noise; noise.enabled = true; noise.strength = 1.2f; noise.frequency = .3f; noise.scrollSpeed = .3f;
            var r = ps.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = .6f; r.lengthScale = 1.2f;
        }
    }
}
