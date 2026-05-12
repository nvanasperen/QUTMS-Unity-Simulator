
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class QUTMSGeneratedPrefabBuilder
{
    private const string ManifestPath = "Assets/EUFS/Generated/vehicle_manifest.json";
    private const string OutputRoot = "Assets/EUFS/GeneratedPrefabs";
    private const string VehicleOut = OutputRoot + "/Vehicle";
    private const string MaterialOut = OutputRoot + "/Materials";

    [System.Serializable]
    public class VehicleManifest
    {
        public string name;
        public VehicleNode[] nodes;
        public PhysicsManifest physics;
    }

    [System.Serializable]
    public class VehicleNode
    {
        public string name;
        public string parent;
        public string kind;
        public string mesh;
        public string primitive;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public string materialHint;
    }

    [System.Serializable]
    public class PhysicsManifest
    {
        public float rigidbodyMass;
        public WheelColliderManifest[] wheelColliders;
        public BodyColliderManifest[] bodyColliders;
    }

    [System.Serializable]
    public class WheelColliderManifest
    {
        public string name;
        public string link;
        public float[] position;
        public float[] rotation;
        public float radius;
        public float width;
        public float mass;
        public float suspensionDistance;
        public float forceAppPointDistance;
        public bool steer;
        public bool drive;
    }

    [System.Serializable]
    public class BodyColliderManifest
    {
        public string name;
        public string link;
        public float[] position;
        public float[] rotation;
        public float[] size;
        public string source;
    }

    [MenuItem("Tools/QUTMS/Build Vehicle From Generated Manifest")]
    public static void BuildVehicle()
    {
        EnsureFolder(OutputRoot);
        EnsureFolder(VehicleOut);
        EnsureFolder(MaterialOut);

        TextAsset manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(ManifestPath);
        if (manifestAsset == null)
        {
            Debug.LogError("Missing manifest: " + ManifestPath);
            return;
        }

        VehicleManifest manifest = JsonUtility.FromJson<VehicleManifest>(manifestAsset.text);
        if (manifest == null || manifest.nodes == null)
        {
            Debug.LogError("Could not parse vehicle manifest.");
            return;
        }

        GameObject prefabRoot = new GameObject(string.IsNullOrEmpty(manifest.name) ? "QEVVehicle" : manifest.name);
        Dictionary<string, Transform> map = new Dictionary<string, Transform>();

        foreach (VehicleNode node in manifest.nodes)
        {
            if (!string.IsNullOrEmpty(node.parent)) continue;
            GameObject go = CreateNode(node);
            go.transform.SetParent(prefabRoot.transform, false);
            ApplyTransform(go.transform, node);
            map[node.name] = go.transform;
        }

        bool progress = true;
        int safety = 0;

        while (progress && safety < 1000)
        {
            progress = false;
            safety++;
            foreach (VehicleNode node in manifest.nodes)
            {
                if (map.ContainsKey(node.name)) continue;
                if (string.IsNullOrEmpty(node.parent)) continue;
                if (!map.ContainsKey(node.parent)) continue;

                GameObject go = CreateNode(node);
                go.transform.SetParent(map[node.parent], false);
                ApplyTransform(go.transform, node);
                map[node.name] = go.transform;
                progress = true;
            }
        }

        foreach (VehicleNode node in manifest.nodes)
        {
            if (!map.ContainsKey(node.name))
                Debug.LogWarning("Unbuilt node, missing parent chain: " + node.name + " parent=" + node.parent);
        }

        BuildPhysics(prefabRoot, manifest);

        string path = VehicleOut + "/" + prefabRoot.name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        Object.DestroyImmediate(prefabRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Built vehicle prefab with physics: " + path);
    }

    private static void BuildPhysics(GameObject prefabRoot, VehicleManifest manifest)
    {
        if (manifest.physics == null)
        {
            Debug.LogWarning("No physics manifest found. Visual prefab only.");
            return;
        }

        Rigidbody rb = prefabRoot.GetComponent<Rigidbody>();
        if (rb == null) rb = prefabRoot.AddComponent<Rigidbody>();

        rb.mass = manifest.physics.rigidbodyMass > 0 ? manifest.physics.rigidbodyMass : 250f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        GameObject physicsRoot = new GameObject("Physics_Generated_From_URDF");
        physicsRoot.transform.SetParent(prefabRoot.transform, false);
        physicsRoot.transform.localPosition = Vector3.zero;
        physicsRoot.transform.localRotation = Quaternion.identity;
        physicsRoot.transform.localScale = Vector3.one;

        if (manifest.physics.bodyColliders != null)
        {
            foreach (BodyColliderManifest body in manifest.physics.bodyColliders)
            {
                GameObject go = new GameObject(string.IsNullOrEmpty(body.name) ? "BoxCollider" : body.name);
                go.transform.SetParent(physicsRoot.transform, false);
                go.transform.localPosition = ToVector3(body.position, Vector3.zero);
                go.transform.localRotation = ToQuaternion(body.rotation);
                go.transform.localScale = Vector3.one;

                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = Vector3.zero;
                box.size = ToVector3(body.size, Vector3.one);

                Debug.Log("Added body collider: " + go.name + " source=" + body.source);
            }
        }

        if (manifest.physics.wheelColliders != null)
        {
            foreach (WheelColliderManifest wheel in manifest.physics.wheelColliders)
            {
                GameObject go = new GameObject(string.IsNullOrEmpty(wheel.name) ? "WheelCollider" : wheel.name);
                go.transform.SetParent(physicsRoot.transform, false);
                go.transform.localPosition = ToVector3(wheel.position, Vector3.zero);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                WheelCollider wc = go.AddComponent<WheelCollider>();
                wc.radius = wheel.radius > 0 ? wheel.radius : 0.23f;
                wc.mass = wheel.mass > 0 ? wheel.mass : 20f;
                wc.suspensionDistance = wheel.suspensionDistance > 0 ? wheel.suspensionDistance : 0.12f;
                wc.forceAppPointDistance = wheel.forceAppPointDistance;

                JointSpring spring = wc.suspensionSpring;
                spring.spring = 25000f;
                spring.damper = 3500f;
                spring.targetPosition = 0.5f;
                wc.suspensionSpring = spring;

                WheelFrictionCurve forward = wc.forwardFriction;
                forward.stiffness = 1.0f;
                wc.forwardFriction = forward;

                WheelFrictionCurve sideways = wc.sidewaysFriction;
                sideways.stiffness = 1.0f;
                wc.sidewaysFriction = sideways;

                WheelRole role = go.AddComponent<WheelRole>();
                role.linkName = wheel.link;
                role.canSteer = wheel.steer;
                role.canDrive = wheel.drive;
                role.width = wheel.width;

                Debug.Log("Added wheel collider: " + go.name + " link=" + wheel.link + " radius=" + wc.radius);
            }
        }
    }

    private static GameObject CreateNode(VehicleNode node)
    {
        GameObject go;

        if (node.kind == "mesh" && !string.IsNullOrEmpty(node.mesh))
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(node.mesh);

            if (source == null)
            {
                Debug.LogWarning("Missing mesh asset: " + node.mesh);
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = node.name + "_MISSING_MESH";
                go.transform.localScale = Vector3.one * 0.1f;
            }
            else
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(source);
                go.name = node.name;
            }

            StripImportedSceneJunk(go);
            ApplyConvertedMaterials(go, node.materialHint);
        }
        else if (node.kind == "cylinder")
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = node.name;
            ApplyConvertedMaterials(go, node.materialHint);
        }
        else
        {
            go = new GameObject(node.name);
        }

        return go;
    }

    private static void ApplyTransform(Transform t, VehicleNode node)
    {
        t.localPosition = ToVector3(node.position, Vector3.zero);
        t.localRotation = ToQuaternion(node.rotation);
        t.localScale = ToVector3(node.scale, Vector3.one);
    }

    private static Vector3 ToVector3(float[] v, Vector3 fallback)
    {
        if (v == null || v.Length < 3) return fallback;
        return new Vector3(v[0], v[1], v[2]);
    }

    private static Quaternion ToQuaternion(float[] q)
    {
        if (q == null || q.Length < 4) return Quaternion.identity;
        return new Quaternion(q[0], q[1], q[2], q[3]);
    }

    private static void StripImportedSceneJunk(GameObject root)
    {
        Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras) Object.DestroyImmediate(cam.gameObject);

        Light[] lights = root.GetComponentsInChildren<Light>(true);
        foreach (Light light in lights) Object.DestroyImmediate(light.gameObject);
    }

    private static void ApplyConvertedMaterials(GameObject obj, string materialHint)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] source = renderer.sharedMaterials;

            if (source == null || source.Length == 0)
            {
                renderer.sharedMaterial = MaterialFromHint(materialHint);
                continue;
            }

            Material[] converted = new Material[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) converted[i] = MaterialFromHint(materialHint);
                else converted[i] = ConvertMaterial(source[i], materialHint);
            }

            renderer.sharedMaterials = converted;
        }
    }

    private static Material MaterialFromHint(string hint)
    {
        string h = string.IsNullOrEmpty(hint) ? "" : hint.ToLowerInvariant();

        if (h.Contains("black"))
            return CreateOrGetMaterial("URDF_Black", new Color(0.02f, 0.02f, 0.02f), 0f, 0.45f);

        if (h.Contains("invisible"))
            return CreateOrGetMaterial("URDF_InvisibleFallback", new Color(0.1f, 0.1f, 0.1f, 0.25f), 0f, 0.2f);

        return CreateOrGetMaterial("VehicleDefault", new Color(0.35f, 0.35f, 0.35f), 0f, 0.35f);
    }

    private static Material ConvertMaterial(Material source, string hint)
    {
        Color c = Color.white;

        if (source.HasProperty("_BaseColor")) c = source.GetColor("_BaseColor");
        else if (source.HasProperty("_Color")) c = source.GetColor("_Color");

        bool magenta = c.r > 0.8f && c.g < 0.2f && c.b > 0.8f;
        if (magenta) return MaterialFromHint(hint);

        return CreateOrGetMaterial(source.name + "_URP", c, 0f, 0.35f);
    }

    private static Material CreateOrGetMaterial(string name, Color color, float metallic, float smoothness)
    {
        string safe = Sanitize(name);
        string path = MaterialOut + "/" + safe + ".mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit.");
            return null;
        }

        Material mat = new Material(shader);
        mat.name = safe;

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static string Sanitize(string raw)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) raw = raw.Replace(c, '_');
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

public class WheelRole : MonoBehaviour
{
    public string linkName;
    public bool canSteer;
    public bool canDrive;
    public float width;
}
