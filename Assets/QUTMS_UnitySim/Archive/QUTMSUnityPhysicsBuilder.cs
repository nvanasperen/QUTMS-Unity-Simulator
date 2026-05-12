using UnityEditor;
using UnityEngine;

public static class QUTMSUnityPhysicsBuilder
{
    private const string PhysicsRootName = "Physics_Generated_From_Unity";

    private const float WheelMass = 12f;
    private const float WheelDampingRate = 0.8f;
    private const float SuspensionDistance = 0.08f;
    private const float SpringStrength = 30000f;
    private const float DamperStrength = 9000f;
    private const float SuspensionTargetPosition = 0.48f;
    private const float ForceAppPointDistance = 0.05f;

    private const float ForwardGrip = 1.35f;
    private const float SideGrip = 1.75f;

    private const float WheelRadiusMultiplier = 1.0f;

    // Your tuned value. This aligns the WheelCollider circles with the visible tyre meshes.
    private const float WheelPositionYOffset = 0.06123456789f;

    private static readonly Vector3 ChassisBoxCenter = new Vector3(0f, 0.35f, 0f);
    private static readonly Vector3 ChassisBoxSize = new Vector3(1.35f, 0.55f, 2.2f);

    [MenuItem("Tools/QUTMS/Rebuild Unity Physics From Wheel Meshes")]
    public static void RebuildPhysics()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
            root = GameObject.Find("qev-3d");

        if (root == null)
            root = GameObject.Find("QEVVehicle");

        if (root == null)
        {
            Debug.LogError("Select the generated vehicle root first. Could not find qev-3d or QEVVehicle.");
            return;
        }

        Transform oldPhysics = root.transform.Find(PhysicsRootName);
        if (oldPhysics != null)
            Object.DestroyImmediate(oldPhysics.gameObject);

        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null)
            rb = root.AddComponent<Rigidbody>();

        rb.mass = 240f;
        rb.centerOfMass = new Vector3(0f, -0.55f, 0.05f);
        rb.linearDamping = 0.01f;
        rb.angularDamping = 2.6f;
        rb.maxAngularVelocity = 6.5f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        GameObject physicsRoot = new GameObject(PhysicsRootName);
        physicsRoot.transform.SetParent(root.transform, false);
        physicsRoot.transform.localPosition = Vector3.zero;
        physicsRoot.transform.localRotation = Quaternion.identity;
        physicsRoot.transform.localScale = Vector3.one;

        CreateChassisBox(physicsRoot.transform);

        CreateWheelColliderFromMesh(root.transform, physicsRoot.transform, "left_front_wheel", "WheelCollider_FL");
        CreateWheelColliderFromMesh(root.transform, physicsRoot.transform, "right_front_wheel", "WheelCollider_FR");
        CreateWheelColliderFromMesh(root.transform, physicsRoot.transform, "left_rear_wheel", "WheelCollider_RL");
        CreateWheelColliderFromMesh(root.transform, physicsRoot.transform, "right_rear_wheel", "WheelCollider_RR");

        EditorUtility.SetDirty(root);
        AssetDatabase.SaveAssets();

        Debug.Log("Rebuilt clean Unity physics. No QUTMSWheelRole components added.");
    }

    private static void CreateChassisBox(Transform physicsRoot)
    {
        GameObject boxObj = new GameObject("Chassis_BoxCollider");
        boxObj.transform.SetParent(physicsRoot, false);
        boxObj.transform.localPosition = ChassisBoxCenter;
        boxObj.transform.localRotation = Quaternion.identity;
        boxObj.transform.localScale = Vector3.one;

        BoxCollider box = boxObj.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = ChassisBoxSize;
    }

    private static void CreateWheelColliderFromMesh(
        Transform vehicleRoot,
        Transform physicsRoot,
        string wheelLinkName,
        string colliderName
    )
    {
        Transform wheelLink = FindDeepChild(vehicleRoot, wheelLinkName);

        if (wheelLink == null)
        {
            Debug.LogWarning("Missing wheel link: " + wheelLinkName);
            return;
        }

        Bounds wheelBounds;
        bool hasBounds = TryGetRendererBounds(wheelLink, out wheelBounds);

        if (!hasBounds)
        {
            Debug.LogWarning("No renderer bounds found under wheel link: " + wheelLinkName);
            return;
        }

        Vector3 colliderWorldPosition = wheelBounds.center + new Vector3(0f, WheelPositionYOffset, 0f);

        float radius = Mathf.Max(wheelBounds.extents.y, wheelBounds.extents.x) * WheelRadiusMultiplier;

        GameObject wcObj = new GameObject(colliderName);
        wcObj.transform.SetParent(physicsRoot, false);
        wcObj.transform.position = colliderWorldPosition;
        wcObj.transform.rotation = Quaternion.identity;
        wcObj.transform.localScale = Vector3.one;

        WheelCollider wc = wcObj.AddComponent<WheelCollider>();
        wc.mass = WheelMass;
        wc.radius = radius;
        wc.wheelDampingRate = WheelDampingRate;
        wc.suspensionDistance = SuspensionDistance;
        wc.forceAppPointDistance = ForceAppPointDistance;
        wc.center = Vector3.zero;
        wc.ConfigureVehicleSubsteps(5f, 6, 8);

        JointSpring spring = wc.suspensionSpring;
        spring.spring = SpringStrength;
        spring.damper = DamperStrength;
        spring.targetPosition = SuspensionTargetPosition;
        wc.suspensionSpring = spring;

        ApplyFriction(wc, ForwardGrip, SideGrip);

        Debug.Log(
            colliderName
            + " built from "
            + wheelLinkName
            + " | bounds center="
            + wheelBounds.center
            + " | bounds size="
            + wheelBounds.size
            + " | radius="
            + radius
        );
    }

    private static void ApplyFriction(WheelCollider wheel, float forwardStiffness, float sidewaysStiffness)
    {
        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.stiffness = forwardStiffness;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = sidewaysStiffness;
        wheel.sidewaysFriction = sideways;
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        bool found = false;
        bounds = new Bounds(root.position, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return found;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }
}