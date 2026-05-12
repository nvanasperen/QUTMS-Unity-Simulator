using UnityEditor;
using UnityEngine;

public static class EUFSPrefabGenerator
{
    private const string OutputRoot = "Assets/EUFS/GeneratedPrefabs";
    private const string TrackPrefabOut = OutputRoot + "/Tracks";
    private const string VehiclePrefabOut = OutputRoot + "/Vehicle";
    private const string MaterialOut = OutputRoot + "/Materials";

    private const string TrackMeshRoot = "Assets/EUFS/Tracks/meshes";
    private const string RacecarMeshRoot = "Assets/EUFS/Racecar/meshes";

    [MenuItem("Tools/EUFS/Generate Clean Prefabs")]
    public static void GenerateCleanPrefabs()
    {
        EnsureFolder(OutputRoot);
        EnsureFolder(TrackPrefabOut);
        EnsureFolder(VehiclePrefabOut);
        EnsureFolder(MaterialOut);

        GenerateConePrefabs();
        GenerateVehiclePrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("EUFS clean prefab generation complete.");
    }

    private static void GenerateConePrefabs()
    {
        Material reflective = CreateOrGetMaterial("ReflectiveWhite", new Color(0.9f, 0.9f, 0.85f), 0f, 0.65f, true);
        Material blue = CreateOrGetMaterial("ConeBlue", new Color(0.0f, 0.16f, 1.0f), 0f, 0.25f, false);
        Material yellow = CreateOrGetMaterial("ConeYellow", new Color(1.0f, 0.82f, 0.0f), 0f, 0.25f, false);
        Material orange = CreateOrGetMaterial("ConeOrange", new Color(1.0f, 0.32f, 0.0f), 0f, 0.25f, false);

        CreateConeWrapper("BlueCone", TrackMeshRoot + "/cone_blue.dae", TrackPrefabOut + "/BlueCone.prefab", blue, reflective);
        CreateConeWrapper("YellowCone", TrackMeshRoot + "/cone_yellow.dae", TrackPrefabOut + "/YellowCone.prefab", yellow, reflective);
        CreateConeWrapper("OrangeCone", TrackMeshRoot + "/cone.dae", TrackPrefabOut + "/OrangeCone.prefab", orange, reflective);
        CreateConeWrapper("BigOrangeCone", TrackMeshRoot + "/cone_big.dae", TrackPrefabOut + "/BigOrangeCone.prefab", orange, reflective);
    }

    private static void GenerateVehiclePrefab()
    {
        Material black = CreateOrGetMaterial("URDF_Black", new Color(0.02f, 0.02f, 0.02f), 0f, 0.45f, false);

        GameObject root = new GameObject("QEVVehicle");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        GameObject baseLink = CreateChild(root.transform, "base_link", Vector3.zero, Quaternion.identity);

        // URDF: base_link -> chassis origin xyz="0 0 0.2032"
        GameObject chassis = CreateChild(
            baseLink.transform,
            "chassis",
            RosToUnityPosition(new Vector3(0f, 0f, 0.2032f)),
            Quaternion.identity
        );

        // URDF chassis visual: lando.dae, visual origin xyz/rpy = 0
        GameObject lando = InstantiateModelSmart("lando", chassis.transform, "lando_visual");
        lando.transform.localPosition = Vector3.zero;
        lando.transform.localRotation = Quaternion.identity;
        lando.transform.localScale = Vector3.one;
        StripImportedSceneJunk(lando);
        ApplyVehicleMaterials(lando);

        // URDF wheel geometry: cylinder length="0.2" radius="0.2032"
        AddUrdfCylinderWheel(chassis.transform, "left_front_wheel", 0.79f, 0.70f, black);
        AddUrdfCylinderWheel(chassis.transform, "right_front_wheel", 0.79f, -0.70f, black);
        AddUrdfCylinderWheel(chassis.transform, "left_rear_wheel", -0.79f, 0.70f, black);
        AddUrdfCylinderWheel(chassis.transform, "right_rear_wheel", -0.79f, -0.70f, black);

        // URDF: chassis -> velodyne_base_link origin xyz="-0.3 0.0 0.85"
        GameObject velodyneBase = CreateChild(
            chassis.transform,
            "velodyne_base_link",
            RosToUnityPosition(new Vector3(-0.3f, 0f, 0.85f)),
            Quaternion.identity
        );

        GameObject vlpBase1 = InstantiateModelSmart("VLP16_base_1", velodyneBase.transform, "VLP16_base_1_visual");
        GameObject vlpBase2 = InstantiateModelSmart("VLP16_base_2", velodyneBase.transform, "VLP16_base_2_visual");
        StripImportedSceneJunk(vlpBase1);
        StripImportedSceneJunk(vlpBase2);
        ApplyVehicleMaterials(vlpBase1);
        ApplyVehicleMaterials(vlpBase2);

        // URDF: velodyne_base_link -> velodyne origin xyz="0 0 0.0377"
        GameObject velodyne = CreateChild(
            velodyneBase.transform,
            "velodyne",
            RosToUnityPosition(new Vector3(0f, 0f, 0.0377f)),
            Quaternion.identity
        );

        GameObject vlpScan = InstantiateModelSmart("VLP16_scan", velodyne.transform, "VLP16_scan_visual");

        // URDF velodyne visual origin: xyz="0 0 -0.0377"
        vlpScan.transform.localPosition = RosToUnityPosition(new Vector3(0f, 0f, -0.0377f));
        vlpScan.transform.localRotation = Quaternion.identity;
        vlpScan.transform.localScale = Vector3.one;
        StripImportedSceneJunk(vlpScan);
        ApplyVehicleMaterials(vlpScan);

        string prefabPath = VehiclePrefabOut + "/QEVVehicle.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        Debug.Log("Generated vehicle prefab: " + prefabPath);
    }

    private static void AddUrdfCylinderWheel(Transform chassis, string linkName, float rosX, float rosY, Material wheelMaterial)
    {
        GameObject wheelLink = CreateChild(
            chassis,
            linkName,
            RosToUnityPosition(new Vector3(rosX, rosY, 0f)),
            Quaternion.identity
        );

        GameObject wheelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheelVisual.name = linkName + "_cylinder_visual";
        wheelVisual.transform.SetParent(wheelLink.transform, false);

        // URDF cylinder length is along its local axis. Unity cylinder height is local Y.
        // Wheel axle in ROS is Y; ROS Y maps to Unity -X, so align cylinder height to Unity X.
        wheelVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Unity cylinder default: radius 0.5, height 2.
        // URDF wheel: radius 0.2032, length 0.2.
        wheelVisual.transform.localScale = new Vector3(0.4064f, 0.1f, 0.4064f);

        Renderer renderer = wheelVisual.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = wheelMaterial;
    }

    private static void CreateConeWrapper(string prefabName, string modelPath, string prefabPath, Material bodyMaterial, Material stripMaterial)
    {
        GameObject root = new GameObject(prefabName);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        GameObject visual = InstantiateModel(modelPath, root.transform, prefabName + "_visual");
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        StripImportedSceneJunk(visual);
        ApplyConeMaterials(visual, bodyMaterial, stripMaterial);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static GameObject InstantiateModelSmart(string baseName, Transform parent, string objectName)
    {
        string[] possiblePaths =
        {
            RacecarMeshRoot + "/" + baseName + ".dae",
            RacecarMeshRoot + "/" + baseName + ".stl",
            RacecarMeshRoot + "/" + baseName + ".fbx",
            RacecarMeshRoot + "/" + baseName + ".obj"
        };

        foreach (string path in possiblePaths)
        {
            GameObject found = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (found != null)
                return InstantiateModel(path, parent, objectName);
        }

        Debug.LogWarning("Missing racecar model: " + baseName + " under " + RacecarMeshRoot);

        GameObject missing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        missing.name = objectName + "_MISSING_MODEL";
        missing.transform.SetParent(parent, false);
        missing.transform.localPosition = Vector3.zero;
        missing.transform.localRotation = Quaternion.identity;
        missing.transform.localScale = Vector3.one * 0.15f;

        return missing;
    }

    private static GameObject InstantiateModel(string path, Transform parent, string name)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (source == null)
        {
            Debug.LogWarning("Missing model at path: " + path);

            GameObject missing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            missing.name = name + "_MISSING_MODEL";
            missing.transform.SetParent(parent, false);
            missing.transform.localPosition = Vector3.zero;
            missing.transform.localRotation = Quaternion.identity;
            missing.transform.localScale = Vector3.one * 0.15f;

            return missing;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        StripImportedSceneJunk(instance);

        return instance;
    }

    private static void StripImportedSceneJunk(GameObject root)
    {
        Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
            Object.DestroyImmediate(cam.gameObject);

        Light[] lights = root.GetComponentsInChildren<Light>(true);
        foreach (Light light in lights)
            Object.DestroyImmediate(light.gameObject);
    }

    private static GameObject CreateChild(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.name = name;
        child.transform.localPosition = localPosition;
        child.transform.localRotation = localRotation;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static Vector3 RosToUnityPosition(Vector3 ros)
    {
        // ROS/Gazebo: X forward, Y left, Z up
        // Unity:      X right,   Y up,   Z forward
        return new Vector3(-ros.y, ros.z, ros.x);
    }

    private static void ApplyConeMaterials(GameObject obj, Material bodyMaterial, Material stripMaterial)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;

            if (mats == null || mats.Length == 0)
                continue;

            for (int i = 0; i < mats.Length; i++)
                mats[i] = i == 1 ? stripMaterial : bodyMaterial;

            renderer.sharedMaterials = mats;
        }
    }

    private static void ApplyVehicleMaterials(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] sourceMats = renderer.sharedMaterials;

            if (sourceMats == null || sourceMats.Length == 0)
                continue;

            Material[] fixedMats = new Material[sourceMats.Length];

            for (int i = 0; i < sourceMats.Length; i++)
            {
                Material source = sourceMats[i];

                if (source == null)
                {
                    fixedMats[i] = CreateOrGetMaterial("VehicleDefault", Color.gray, 0f, 0.35f, false);
                    continue;
                }

                Color color = ExtractUsefulColor(source);
                fixedMats[i] = CreateOrGetMaterial(source.name + "_URP", color, 0f, 0.35f, false);
            }

            renderer.sharedMaterials = fixedMats;
        }
    }

    private static Color ExtractUsefulColor(Material mat)
    {
        Color c = Color.white;

        if (mat.HasProperty("_BaseColor"))
            c = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color"))
            c = mat.GetColor("_Color");

        bool looksMagenta = c.r > 0.8f && c.g < 0.2f && c.b > 0.8f;

        if (looksMagenta)
            return new Color(0.15f, 0.15f, 0.15f);

        return c;
    }

    private static Material CreateOrGetMaterial(string name, Color baseColor, float metallic, float smoothness, bool emission)
    {
        string safeName = SanitizeFileName(name);
        string path = MaterialOut + "/" + safeName + ".mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit shader.");
            return null;
        }

        Material mat = new Material(shader);
        mat.name = safeName;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", baseColor);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        if (emission)
        {
            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", baseColor * 1.5f);
        }

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static string SanitizeFileName(string raw)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            raw = raw.Replace(c, '_');

        return raw.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}