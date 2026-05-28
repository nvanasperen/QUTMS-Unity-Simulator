using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_6000_0_OR_NEWER
using TrackPhysicsMaterial = UnityEngine.PhysicsMaterial;
#else
using TrackPhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

[ExecuteAlways]
public class EUFSTrackSpawner : MonoBehaviour
{
    [Serializable]
    public class TrackConditionValues
    {
        public string presetName = "Dry";

        [Header("Track Surface Physics")]
        public float groundFrictionMu = 100.0f;
        public float groundFrictionMu2 = 50.0f;
        public float groundBounciness = 0.0f;

        [Header("Cone Generation / Perception Noise")]
        [Range(0.0f, 100.0f)]
        public float objectNoisePercent = 0.0f;

        public float coneNoiseRadiusM = 0.0f;

        [Range(0.0f, 100.0f)]
        public float colourNoisePercent = 0.0f;

        public bool deterministicNoise = true;
        public int randomSeed = 12345;

        [Header("Track Surface Visual")]
        public Material surfaceMaterialOverride;
        public Texture2D surfaceTexture;
        public Color surfaceColour = new Color(0.16f, 0.16f, 0.16f, 1.0f);
        public Vector2 surfaceTextureTiling = new Vector2(40.0f, 40.0f);

        public void SetDryDefaults()
        {
            presetName = "Dry";

            groundFrictionMu = 100.0f;
            groundFrictionMu2 = 50.0f;
            groundBounciness = 0.0f;

            objectNoisePercent = 0.0f;
            coneNoiseRadiusM = 0.0f;
            colourNoisePercent = 0.0f;

            deterministicNoise = true;
            randomSeed = 12345;

            surfaceColour = new Color(0.16f, 0.16f, 0.16f, 1.0f);
            surfaceTextureTiling = new Vector2(40.0f, 40.0f);
        }

        public void SetWetDefaults()
        {
            presetName = "Wet";

            groundFrictionMu = 35.0f;
            groundFrictionMu2 = 18.0f;
            groundBounciness = 0.0f;

            objectNoisePercent = 0.0f;
            coneNoiseRadiusM = 0.0f;
            colourNoisePercent = 0.0f;

            deterministicNoise = true;
            randomSeed = 12345;

            surfaceColour = new Color(0.08f, 0.10f, 0.11f, 1.0f);
            surfaceTextureTiling = new Vector2(40.0f, 40.0f);
        }

        public void CopyFrom(TrackConditionValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;

            groundFrictionMu = other.groundFrictionMu;
            groundFrictionMu2 = other.groundFrictionMu2;
            groundBounciness = other.groundBounciness;

            objectNoisePercent = other.objectNoisePercent;
            coneNoiseRadiusM = other.coneNoiseRadiusM;
            colourNoisePercent = other.colourNoisePercent;

            deterministicNoise = other.deterministicNoise;
            randomSeed = other.randomSeed;

            surfaceMaterialOverride = other.surfaceMaterialOverride;
            surfaceTexture = other.surfaceTexture;
            surfaceColour = other.surfaceColour;
            surfaceTextureTiling = other.surfaceTextureTiling;
        }
    }

    [Header("Track CSV")]
    public TextAsset trackCsv;

    [Header("Cone Prefabs")]
    public GameObject blueConePrefab;
    public GameObject yellowConePrefab;
    public GameObject orangeConePrefab;
    public GameObject bigOrangeConePrefab;

    [Header("Cone Collision Layer")]
    [Tooltip("Layer assigned to every spawned cone and its children. Tyre hitboxes should be configured to collide with this layer only.")]
    public string spawnedConeLayerName = "Cone";
    public bool assignSpawnedConeLayerRecursively = true;
    public bool autoCreateConeLayerInEditor = true;

    [Tooltip("Optional helper. If the TyreHitbox layer exists, configure it to collide with Cone only.")]
    public bool autoConfigureTyreHitboxCollisionMatrix = true;
    public string tyreHitboxLayerName = "TyreHitbox";

    [Header("Spawner Behaviour")]
    public bool clearExistingBeforeSpawn = true;
    public bool spawnOnPlay = true;

    [Header("Spawn Root")]
    public string spawnedConesRootName = "Spawned Cones";
    public Transform spawnedConesRoot;

    [Header("CSV Conversion")]
    public float csvPositionScale = 1.0f;
    public bool useLocalPosition = true;

    [Header("Cone Height Offsets")]
    public float blueConeYOffset = 0.0f;
    public float yellowConeYOffset = 0.0f;
    public float orangeConeYOffset = 0.0f;
    public float bigOrangeConeYOffset = -0.15f;

    [Header("Track Surface")]
    [HideInInspector]
    public string selectedTrackConditionPresetName = "Dry";

    public TrackConditionValues conditions = new TrackConditionValues();

    public bool generateOrControlPlane = true;
    public bool createPlaneAsSceneSibling = true;
    public GameObject trackPlane;
    public string trackPlaneName = "Plane";
    public Vector3 trackPlaneWorldPosition = new Vector3(0.0f, -0.15f, 0.0f);
    public Vector3 trackPlaneWorldRotationDeg = Vector3.zero;
    public Vector3 trackPlaneLocalScale = new Vector3(100.0f, 1.0f, 100.0f);

    [Header("Track Surface Visual Override")]
    [Tooltip("Optional material to apply directly to the generated plane. If assigned, it overrides the texture/colour fields below.")]
    public Material trackSurfaceMaterialOverride;

    [Tooltip("Drag your asphalt texture here. This is applied to the generated track plane material.")]
    public Texture2D trackSurfaceTexture;

    public Color trackSurfaceColour = new Color(0.16f, 0.16f, 0.16f, 1.0f);
    public Vector2 trackSurfaceTextureTiling = new Vector2(40.0f, 40.0f);

    [Header("Cone Physics")]
    public bool makeConesSmackable = true;
    public float coneMassKg = 0.35f;
    public float coneLinearDamping = 0.05f;
    public float coneAngularDamping = 0.02f;
    public bool addConvexMeshCollidersIfMissing = true;

    [Header("Squishy Cone Debug Physics")]
    public bool useSquishyConePhysics = true;
    public bool useSimpleSquishyConeCollider = true;
    public float squishyConeMassKg = 2.0f;
    public float squishyConeLinearDamping = 4.0f;
    public float squishyConeAngularDamping = 8.0f;
    public float squishyConeMaxLinearSpeedMps = 8.0f;
    public float squishyConeMaxAngularSpeedRadps = 10.0f;
    public float squishyConeMaxDepenetrationVelocity = 1.5f;
    public Vector3 squishyConeColliderCenter = new Vector3(0.0f, 0.18f, 0.0f);
    public float squishyConeColliderRadius = 0.16f;
    public float squishyConeColliderHeight = 0.42f;

    [Header("Cone Collider Per-Type Offsets")]
    [Tooltip("Use this for wrapped blue/yellow prefabs where the visual child is rotation-corrected/offset inside a clean parent.")]
    public bool useWrappedBlueYellowColliderOffset = true;

    [Tooltip("Added to Squishy Cone Collider Center only for blue/yellow cones. This changes the collider, not the spawned cone position.")]
    public Vector3 wrappedBlueYellowColliderCenterOffset = new Vector3(0.0f, -0.16f, 0.0f);

    public Vector3 orangeConeColliderCenterOffset = Vector3.zero;
    public Vector3 bigOrangeConeColliderCenterOffset = Vector3.zero;

    private Material generatedTrackPlaneMaterial;
    private TrackPhysicsMaterial generatedTrackPhysicsMaterial;

    public void ResetSelectedTrackConditionPreset()
    {
        EnsureConditions();

        if (selectedTrackConditionPresetName == "Wet")
            conditions.SetWetDefaults();
        else
            conditions.SetDryDefaults();

        selectedTrackConditionPresetName = conditions.presetName;
        ApplyTrackPlanePhysics();
        ApplyTrackPlaneVisuals();
    }

    public void SpawnTrack()
    {
        PrepareConeCollisionLayers();

        if (generateOrControlPlane)
            GenerateOrUpdateTrackPlane();

        if (trackCsv == null)
        {
            Debug.LogError("No track CSV assigned.");
            return;
        }

        if (clearExistingBeforeSpawn)
            ClearSpawnedTrack();

        System.Random random = conditions.deterministicNoise
            ? new System.Random(conditions.randomSeed)
            : new System.Random();

        string[] lines = trackCsv.text.Split(
            new[] { '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');

            if (parts.Length < 4)
                continue;

            string tag = parts[0].Trim();

            float x = ParseFloat(parts[1]) * csvPositionScale;
            float z = ParseFloat(parts[2]) * csvPositionScale;
            float direction = ParseFloat(parts[3]);

            GameObject prefab = null;
            float yOffset = 0.0f;

            if (tag == "blue")
            {
                prefab = PickConePrefabWithColourNoise(blueConePrefab, random);
                yOffset = blueConeYOffset;
            }
            else if (tag == "yellow")
            {
                prefab = PickConePrefabWithColourNoise(yellowConePrefab, random);
                yOffset = yellowConeYOffset;
            }
            else if (tag == "orange")
            {
                prefab = PickConePrefabWithColourNoise(orangeConePrefab, random);
                yOffset = orangeConeYOffset;
            }
            else if (tag == "big_orange")
            {
                prefab = bigOrangeConePrefab;
                yOffset = bigOrangeConeYOffset;
            }
            else if (tag == "car_start")
            {
                continue;
            }

            if (prefab == null)
            {
                Debug.LogWarning("No prefab assigned for tag: " + tag);
                continue;
            }

            if (ShouldDropObject(random))
                continue;

            Vector3 localPosition = new Vector3(x, yOffset, z);
            localPosition += GetConeNoiseOffset(random);

            Quaternion localRotation = Quaternion.Euler(0.0f, -direction * Mathf.Rad2Deg, 0.0f);

            GameObject cone = CreateCone(prefab);
            cone.name = tag + "_cone_" + i;
            ApplySpawnedConeLayer(cone);

            if (useLocalPosition)
            {
                cone.transform.localPosition = localPosition;
                cone.transform.localRotation = localRotation;
            }
            else
            {
                cone.transform.SetPositionAndRotation(
                    transform.TransformPoint(localPosition),
                    transform.rotation * localRotation
                );
            }

            if (makeConesSmackable)
                ConfigureConePhysics(cone, tag);
        }
    }

    public void GenerateOrUpdateTrackPlane()
    {
        if (trackPlane == null)
        {
            Transform parent = GetTrackPlaneParent();
            Transform existing = parent != null ? parent.Find(trackPlaneName) : null;

            if (existing != null)
            {
                trackPlane = existing.gameObject;
            }
            else
            {
                trackPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                trackPlane.name = trackPlaneName;

                if (parent != null)
                    trackPlane.transform.SetParent(parent, true);
            }
        }

        trackPlane.transform.position = trackPlaneWorldPosition;
        trackPlane.transform.rotation = Quaternion.Euler(trackPlaneWorldRotationDeg);
        trackPlane.transform.localScale = trackPlaneLocalScale;

        ApplyTrackPlanePhysics();
        ApplyTrackPlaneVisuals();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(trackPlane);
#endif
    }

    public void ApplyTrackPlanePhysics()
    {
        if (trackPlane == null)
            return;

        Collider collider = trackPlane.GetComponent<Collider>();

        if (collider == null)
            collider = trackPlane.AddComponent<MeshCollider>();

        collider.sharedMaterial = GetOrCreateTrackPhysicsMaterial();

#if UNITY_EDITOR
        EditorUtility.SetDirty(trackPlane);
#endif
    }

    public void ApplyTrackPlaneVisuals()
    {
        if (trackPlane == null)
            return;

        Renderer renderer = trackPlane.GetComponent<Renderer>();

        if (renderer == null)
            return;

        Material overrideMaterial = trackSurfaceMaterialOverride != null
            ? trackSurfaceMaterialOverride
            : conditions.surfaceMaterialOverride;

        if (overrideMaterial != null)
        {
            renderer.sharedMaterial = overrideMaterial;
            return;
        }

        renderer.sharedMaterial = GetOrCreateTrackPlaneMaterial();

#if UNITY_EDITOR
        EditorUtility.SetDirty(trackPlane);
#endif
    }

    public void ClearSpawnedTrack()
    {
        Transform root = GetSpawnedConesRoot(false);

        if (root != null)
        {
            ClearChildren(root);

            if (root.childCount == 0 && root.name == spawnedConesRootName)
            {
                if (Application.isPlaying)
                    Destroy(root.gameObject);
                else
                    DestroyImmediate(root.gameObject);

                spawnedConesRoot = null;
            }
        }

        // Legacy cleanup: older versions spawned cones directly under this spawner.
        // This removes old duplicated cones without touching the track plane.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (trackPlane != null && child.gameObject == trackPlane)
                continue;

            if (child.name == spawnedConesRootName)
                continue;

            if (LooksLikeSpawnedCone(child.gameObject))
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
#endif
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private bool LooksLikeSpawnedCone(GameObject obj)
    {
        if (obj == null)
            return false;

        string n = obj.name.ToLowerInvariant();

        return
            n.Contains("_cone_") ||
            n.StartsWith("blue_cone") ||
            n.StartsWith("yellow_cone") ||
            n.StartsWith("orange_cone") ||
            n.StartsWith("big_orange_cone");
    }

    private Transform GetSpawnedConesRoot(bool createIfMissing)
    {
        if (spawnedConesRoot != null)
            return spawnedConesRoot;

        Transform existing = transform.Find(spawnedConesRootName);

        if (existing != null)
        {
            spawnedConesRoot = existing;
            return spawnedConesRoot;
        }

        if (!createIfMissing)
            return null;

        GameObject root = new GameObject(spawnedConesRootName);
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        spawnedConesRoot = root.transform;
        return spawnedConesRoot;
    }

    private void EnsureConditions()
    {
        if (conditions == null)
            conditions = new TrackConditionValues();
    }

    private Transform GetTrackPlaneParent()
    {
        if (createPlaneAsSceneSibling && transform.parent != null)
            return transform.parent;

        return transform;
    }

    private TrackPhysicsMaterial GetOrCreateTrackPhysicsMaterial()
    {
        EnsureConditions();

        if (generatedTrackPhysicsMaterial == null)
        {
            generatedTrackPhysicsMaterial = new TrackPhysicsMaterial();
            generatedTrackPhysicsMaterial.name = "Generated Track Physics Material";
        }

        generatedTrackPhysicsMaterial.staticFriction = Mathf.Max(0.0f, conditions.groundFrictionMu);
        generatedTrackPhysicsMaterial.dynamicFriction = Mathf.Max(0.0f, conditions.groundFrictionMu2);
        generatedTrackPhysicsMaterial.bounciness = Mathf.Max(0.0f, conditions.groundBounciness);

        return generatedTrackPhysicsMaterial;
    }

    private Material GetOrCreateTrackPlaneMaterial()
    {
        EnsureConditions();

        if (generatedTrackPlaneMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            generatedTrackPlaneMaterial = new Material(shader);
            generatedTrackPlaneMaterial.name = "Generated Track Plane Material";
        }

        Color chosenColour = trackSurfaceColour;
        Texture2D chosenTexture = trackSurfaceTexture != null ? trackSurfaceTexture : conditions.surfaceTexture;
        Vector2 chosenTiling = trackSurfaceTextureTiling;

        if (chosenTiling.x <= 0.001f)
            chosenTiling.x = 1.0f;

        if (chosenTiling.y <= 0.001f)
            chosenTiling.y = 1.0f;

        if (generatedTrackPlaneMaterial.HasProperty("_BaseColor"))
            generatedTrackPlaneMaterial.SetColor("_BaseColor", chosenColour);

        if (generatedTrackPlaneMaterial.HasProperty("_Color"))
            generatedTrackPlaneMaterial.SetColor("_Color", chosenColour);

        if (generatedTrackPlaneMaterial.HasProperty("_BaseMap"))
        {
            generatedTrackPlaneMaterial.SetTexture("_BaseMap", chosenTexture);
            generatedTrackPlaneMaterial.SetTextureScale("_BaseMap", chosenTiling);
        }

        if (generatedTrackPlaneMaterial.HasProperty("_MainTex"))
        {
            generatedTrackPlaneMaterial.SetTexture("_MainTex", chosenTexture);
            generatedTrackPlaneMaterial.SetTextureScale("_MainTex", chosenTiling);
        }

        return generatedTrackPlaneMaterial;
    }

    private bool ShouldDropObject(System.Random random)
    {
        float probability = Mathf.Clamp01(conditions.objectNoisePercent * 0.01f);
        return random.NextDouble() < probability;
    }

    private Vector3 GetConeNoiseOffset(System.Random random)
    {
        float radius = Mathf.Max(0.0f, conditions.coneNoiseRadiusM);

        if (radius <= 0.0f)
            return Vector3.zero;

        double angle = random.NextDouble() * Math.PI * 2.0;
        double distance = Math.Sqrt(random.NextDouble()) * radius;

        return new Vector3(
            (float)(Math.Cos(angle) * distance),
            0.0f,
            (float)(Math.Sin(angle) * distance)
        );
    }

    private GameObject PickConePrefabWithColourNoise(GameObject intendedPrefab, System.Random random)
    {
        float probability = Mathf.Clamp01(conditions.colourNoisePercent * 0.01f);

        if (random.NextDouble() >= probability)
            return intendedPrefab;

        List<GameObject> alternatives = new List<GameObject>();

        AddAlternative(alternatives, blueConePrefab, intendedPrefab);
        AddAlternative(alternatives, yellowConePrefab, intendedPrefab);
        AddAlternative(alternatives, orangeConePrefab, intendedPrefab);

        if (alternatives.Count == 0)
            return intendedPrefab;

        return alternatives[random.Next(0, alternatives.Count)];
    }

    private static void AddAlternative(List<GameObject> alternatives, GameObject candidate, GameObject intended)
    {
        if (candidate == null)
            return;

        if (candidate == intended)
            return;

        if (!alternatives.Contains(candidate))
            alternatives.Add(candidate);
    }

    private void ApplySpawnedConeLayer(GameObject cone)
    {
        if (cone == null || string.IsNullOrWhiteSpace(spawnedConeLayerName))
            return;

        int layer = LayerMask.NameToLayer(spawnedConeLayerName);

        if (layer < 0)
            return;

        if (assignSpawnedConeLayerRecursively)
        {
            Transform[] children = cone.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                children[i].gameObject.layer = layer;
        }
        else
        {
            cone.layer = layer;
        }
    }

    private void PrepareConeCollisionLayers()
    {
#if UNITY_EDITOR
        if (autoCreateConeLayerInEditor)
            EnsureProjectLayerExists(spawnedConeLayerName);
#endif

        if (!autoConfigureTyreHitboxCollisionMatrix)
            return;

        int tyreLayer = LayerMask.NameToLayer(tyreHitboxLayerName);
        int coneLayer = LayerMask.NameToLayer(spawnedConeLayerName);

        if (tyreLayer < 0 || coneLayer < 0)
            return;

        for (int i = 0; i < 32; i++)
            Physics.IgnoreLayerCollision(tyreLayer, i, i != coneLayer);

        Physics.IgnoreLayerCollision(tyreLayer, coneLayer, false);
    }

#if UNITY_EDITOR
    private static void EnsureProjectLayerExists(string layerName)
    {
        if (string.IsNullOrWhiteSpace(layerName))
            return;

        if (LayerMask.NameToLayer(layerName) >= 0)
            return;

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);

            if (string.IsNullOrWhiteSpace(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                return;
            }
        }

        Debug.LogWarning("[EUFSTrackSpawner] No free user layer slot available for '" + layerName + "'.");
    }
#endif

    private void ConfigureConePhysics(GameObject cone, string tag)
    {
        if (cone == null)
            return;

        EnsureConeColliders(cone, tag);

        Rigidbody rb = cone.GetComponent<Rigidbody>();

        if (rb == null)
            rb = cone.AddComponent<Rigidbody>();

        float mass = useSquishyConePhysics ? squishyConeMassKg : coneMassKg;
        float linearDamping = useSquishyConePhysics ? squishyConeLinearDamping : coneLinearDamping;
        float angularDamping = useSquishyConePhysics ? squishyConeAngularDamping : coneAngularDamping;

        rb.mass = Mathf.Max(0.01f, mass);
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (useSquishyConePhysics)
        {
            rb.maxAngularVelocity = Mathf.Max(0.01f, squishyConeMaxAngularSpeedRadps);
            rb.maxDepenetrationVelocity = Mathf.Max(0.01f, squishyConeMaxDepenetrationVelocity);

            QUTMSConePhysicsLimiter limiter = cone.GetComponent<QUTMSConePhysicsLimiter>();
            if (limiter == null)
                limiter = cone.AddComponent<QUTMSConePhysicsLimiter>();

            limiter.maxLinearSpeedMps = Mathf.Max(0.01f, squishyConeMaxLinearSpeedMps);
            limiter.maxAngularSpeedRadps = Mathf.Max(0.01f, squishyConeMaxAngularSpeedRadps);
        }

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = Mathf.Max(0.0f, linearDamping);
        rb.angularDamping = Mathf.Max(0.0f, angularDamping);
#else
        rb.drag = Mathf.Max(0.0f, linearDamping);
        rb.angularDrag = Mathf.Max(0.0f, angularDamping);
#endif
    }

    private void EnsureConeColliders(GameObject cone, string tag)
    {
        if (useSquishyConePhysics && useSimpleSquishyConeCollider)
        {
            ConfigureSimpleSquishyConeCollider(cone, tag);
            return;
        }

        Collider[] existingColliders = cone.GetComponentsInChildren<Collider>(true);

        if (existingColliders.Length > 0)
        {
            for (int i = 0; i < existingColliders.Length; i++)
                existingColliders[i].enabled = true;

            return;
        }

        if (!addConvexMeshCollidersIfMissing)
            return;

        MeshFilter[] meshFilters = cone.GetComponentsInChildren<MeshFilter>(true);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];

            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();

            if (meshCollider == null)
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = true;
            meshCollider.enabled = true;
        }
    }

    private void ConfigureSimpleSquishyConeCollider(GameObject cone, string tag)
    {
        if (cone == null)
            return;

        Collider[] existingColliders = cone.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < existingColliders.Length; i++)
            DestroyObjectSafe(existingColliders[i]);

        CapsuleCollider capsule = cone.GetComponent<CapsuleCollider>();
        if (capsule == null)
            capsule = cone.AddComponent<CapsuleCollider>();

        capsule.direction = 1;
        capsule.center = GetSquishyConeColliderCenter(tag);
        capsule.radius = Mathf.Max(0.001f, squishyConeColliderRadius);
        capsule.height = Mathf.Max(capsule.radius * 2.0f, squishyConeColliderHeight);
        capsule.isTrigger = false;

        capsule.sharedMaterial = GetOrCreateSquishyConePhysicsMaterial();
    }

    private Vector3 GetSquishyConeColliderCenter(string tag)
    {
        Vector3 center = squishyConeColliderCenter;

        if (string.Equals(tag, "blue", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tag, "yellow", StringComparison.OrdinalIgnoreCase))
        {
            if (useWrappedBlueYellowColliderOffset)
                center += wrappedBlueYellowColliderCenterOffset;
        }
        else if (string.Equals(tag, "orange", StringComparison.OrdinalIgnoreCase))
        {
            center += orangeConeColliderCenterOffset;
        }
        else if (string.Equals(tag, "big_orange", StringComparison.OrdinalIgnoreCase))
        {
            center += bigOrangeConeColliderCenterOffset;
        }

        return center;
    }

    private TrackPhysicsMaterial GetOrCreateSquishyConePhysicsMaterial()
    {
        TrackPhysicsMaterial material = new TrackPhysicsMaterial();
        material.name = "Generated Squishy Cone Physics Material";
        material.staticFriction = 0.9f;
        material.dynamicFriction = 0.8f;
        material.bounciness = 0.0f;
        return material;
    }

    private static void DestroyObjectSafe(UnityEngine.Object obj)
    {
        if (obj == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(obj);
        else
            Destroy(obj);
#else
        Destroy(obj);
#endif
    }

    private GameObject CreateCone(GameObject prefab)
    {
        Transform parent = GetSpawnedConesRoot(true);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject cone = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            return cone;
        }
#endif

        return Instantiate(prefab, parent);
    }

    private float ParseFloat(string value)
    {
        return float.Parse(value.Trim(), CultureInfo.InvariantCulture);
    }

    private void Start()
    {
        if (Application.isPlaying && spawnOnPlay)
            SpawnTrack();
    }

    private void OnValidate()
    {
        EnsureConditions();

        if (csvPositionScale <= 0.0f)
            csvPositionScale = 1.0f;

        if (trackSurfaceTextureTiling.x <= 0.001f)
            trackSurfaceTextureTiling.x = 1.0f;

        if (trackSurfaceTextureTiling.y <= 0.001f)
            trackSurfaceTextureTiling.y = 1.0f;

        if (squishyConeColliderRadius <= 0.001f)
            squishyConeColliderRadius = 0.16f;

        if (squishyConeColliderHeight <= squishyConeColliderRadius * 2.0f)
            squishyConeColliderHeight = squishyConeColliderRadius * 2.0f;

#if UNITY_EDITOR
        if (autoCreateConeLayerInEditor)
            EnsureProjectLayerExists(spawnedConeLayerName);
#endif
    }
}

public class QUTMSConePhysicsLimiter : MonoBehaviour
{
    public float maxLinearSpeedMps = 8.0f;
    public float maxAngularSpeedRadps = 10.0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
            return;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
#else
        Vector3 velocity = rb.velocity;
#endif

        float maxLinear = Mathf.Max(0.01f, maxLinearSpeedMps);
        if (velocity.sqrMagnitude > maxLinear * maxLinear)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = velocity.normalized * maxLinear;
#else
            rb.velocity = velocity.normalized * maxLinear;
#endif
        }

        float maxAngular = Mathf.Max(0.01f, maxAngularSpeedRadps);
        if (rb.angularVelocity.sqrMagnitude > maxAngular * maxAngular)
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngular;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EUFSTrackSpawner))]
public class EUFSTrackSpawnerEditor : Editor
{
    private const string TrackConditionPresetFolder = "Assets/QUTMS_UnitySim/Track/presets";

    public override void OnInspectorGUI()
    {
        EUFSTrackSpawner spawner = (EUFSTrackSpawner)target;

        serializedObject.Update();

        DrawTrackConditionPresetSelector(spawner);

        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Selected Track Condition Preset"))
        {
            Undo.RecordObject(spawner, "Apply Track Condition Preset");
            ApplyPreset(spawner, spawner.selectedTrackConditionPresetName);
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawner.gameObject);
        }

        if (GUILayout.Button("Save As New Track Condition Preset"))
        {
            EUFSTrackConditionPresetSaveWindow.Open(spawner);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate / Refresh Track Plane"))
        {
            Undo.RecordObject(spawner, "Generate Track Plane");
            spawner.GenerateOrUpdateTrackPlane();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        if (GUILayout.Button("Apply Track Plane Physics"))
        {
            Undo.RecordObject(spawner, "Apply Track Plane Physics");
            spawner.ApplyTrackPlanePhysics();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        if (GUILayout.Button("Apply Track Plane Visuals"))
        {
            Undo.RecordObject(spawner, "Apply Track Plane Visuals");
            spawner.ApplyTrackPlaneVisuals();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn / Refresh Track"))
        {
            Undo.RecordObject(spawner, "Spawn Track");
            spawner.SpawnTrack();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        if (GUILayout.Button("Clear Spawned Track"))
        {
            Undo.RecordObject(spawner, "Clear Spawned Track");
            spawner.ClearSpawnedTrack();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Set Wrapped Blue/Yellow Collider Offset (-0.16 Y)"))
        {
            Undo.RecordObject(spawner, "Set Wrapped Blue/Yellow Collider Offset");
            spawner.blueConeYOffset = 0.0f;
            spawner.yellowConeYOffset = 0.0f;
            spawner.useWrappedBlueYellowColliderOffset = true;
            spawner.wrappedBlueYellowColliderCenterOffset = new Vector3(0.0f, -0.16f, 0.0f);
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawner.gameObject);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTrackConditionPresetSelector(EUFSTrackSpawner spawner)
    {
        string[] names = GetPresetNames(TrackConditionPresetFolder, new[] { "Dry", "Wet" });

        int currentIndex = IndexOf(names, spawner.selectedTrackConditionPresetName);
        int nextIndex = EditorGUILayout.Popup("Track Condition Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(spawner, "Change Track Condition Preset");

            spawner.selectedTrackConditionPresetName = names[nextIndex];
            ApplyPreset(spawner, spawner.selectedTrackConditionPresetName);

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawner.gameObject);
        }
    }

    private static void ApplyPreset(EUFSTrackSpawner spawner, string presetName)
    {
        if (spawner == null)
            return;

        if (presetName == "Wet")
        {
            spawner.conditions.SetWetDefaults();
        }
        else if (presetName == "Dry")
        {
            spawner.conditions.SetDryDefaults();
        }
        else
        {
            LoadTrackConditionPreset(spawner, presetName);
        }

        spawner.selectedTrackConditionPresetName = spawner.conditions.presetName;
        spawner.ApplyTrackPlanePhysics();
        spawner.ApplyTrackPlaneVisuals();
    }

    private static string[] GetPresetNames(string folder, string[] builtIns)
    {
        List<string> names = new List<string>();

        for (int i = 0; i < builtIns.Length; i++)
        {
            if (!names.Contains(builtIns[i]))
                names.Add(builtIns[i]);
        }

        if (AssetDatabase.IsValidFolder(folder))
        {
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string name = Path.GetFileNameWithoutExtension(path);

                if (!string.IsNullOrWhiteSpace(name) && !names.Contains(name))
                    names.Add(name);
            }
        }

        return names.ToArray();
    }

    private static int IndexOf(string[] names, string selected)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == selected)
                return i;
        }

        return 0;
    }

    private static string PresetPath(string folder, string presetName)
    {
        return folder + "/" + SanitizeFileName(presetName) + ".json";
    }

    private static string SanitizeFileName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Unnamed Preset";

        string safe = raw;

        foreach (char c in Path.GetInvalidFileNameChars())
            safe = safe.Replace(c, '_');

        safe = safe.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
        return safe;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');

        if (parts.Length == 0)
            return;

        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static void LoadTrackConditionPreset(EUFSTrackSpawner spawner, string presetName)
    {
        string path = PresetPath(TrackConditionPresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        EUFSTrackSpawner.TrackConditionValues loaded =
            JsonUtility.FromJson<EUFSTrackSpawner.TrackConditionValues>(json);

        if (loaded == null)
            return;

        spawner.conditions.CopyFrom(loaded);
        spawner.selectedTrackConditionPresetName = spawner.conditions.presetName;
    }

    public static void SaveTrackConditionPreset(EUFSTrackSpawner spawner, string presetName)
    {
        if (spawner == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(TrackConditionPresetFolder);

        EUFSTrackSpawner.TrackConditionValues save =
            new EUFSTrackSpawner.TrackConditionValues();

        save.CopyFrom(spawner.conditions);
        save.presetName = presetName;

        File.WriteAllText(PresetPath(TrackConditionPresetFolder, presetName), JsonUtility.ToJson(save, true));

        spawner.selectedTrackConditionPresetName = presetName;
        spawner.conditions.presetName = presetName;

        EditorUtility.SetDirty(spawner);
        AssetDatabase.Refresh();
    }
}

public class EUFSTrackConditionPresetSaveWindow : EditorWindow
{
    private EUFSTrackSpawner targetSpawner;
    private string presetName = "";

    public static void Open(EUFSTrackSpawner spawner)
    {
        EUFSTrackConditionPresetSaveWindow window = CreateInstance<EUFSTrackConditionPresetSaveWindow>();
        window.titleContent = new GUIContent("Save Track Condition Preset");
        window.targetSpawner = spawner;
        window.presetName = spawner != null && spawner.conditions != null ? spawner.conditions.presetName : "New Track Condition Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Track Condition Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetSpawner != null)
            {
                Undo.RecordObject(targetSpawner, "Save Track Condition Preset");
                EUFSTrackSpawnerEditor.SaveTrackConditionPreset(targetSpawner, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}
#endif