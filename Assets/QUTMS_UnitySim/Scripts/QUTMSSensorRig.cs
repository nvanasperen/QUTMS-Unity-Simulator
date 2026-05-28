using UnityEngine;

[ExecuteAlways]
public class QUTMSSensorRig : MonoBehaviour
{
    [Header("Sensor Root")]
    public Transform sensorRoot;

    [Header("Velodyne / LiDAR")]
    public Vector3 velodyneBaseLocalPosition = new Vector3(0.0f, 0.85f, -0.30f);
    public Vector3 velodyneScanLocalOffset = new Vector3(0.0f, 0.0377f, 0.0f);
    public Vector3 velodyneLocalEuler = Vector3.zero;

    public bool createVelodynePlaceholder = true;
    public float velodynePlaceholderRadius = 0.06f;
    public float velodynePlaceholderHeight = 0.08f;

    [Header("Forward Camera")]
    public bool createForwardCamera = true;
    public Vector3 cameraLocalPosition = new Vector3(0.0f, 0.95f, -0.20f);
    public Vector3 cameraLocalEuler = new Vector3(8.0f, 0.0f, 0.0f);
    public float cameraFov = 70.0f;
    public float cameraNearClip = 0.05f;
    public float cameraFarClip = 200.0f;
    public bool forceLandscapeAspect = true;
    public float cameraAspectWidth = 16.0f;
    public float cameraAspectHeight = 9.0f;

    [Header("Runtime References")]
    public Transform velodyneBase;
    public Transform velodyneScan;
    public Camera forwardCamera;

    [Header("Debug Gizmos")]
    public bool drawLidarDebug = true;
    public float lidarDebugRangeM = 20.0f;
    [Range(1.0f, 360.0f)]
    public float lidarDebugHorizontalFovDeg = 180.0f;

    [Header("Apply")]
    public bool autoApplyInEditor = true;

    private void Reset()
    {
        ApplySensorRig();
    }

    private void OnValidate()
    {
        if (autoApplyInEditor)
            ApplySensorRig();
    }

    private void Awake()
    {
        ApplySensorRig();
    }

    [ContextMenu("Apply Sensor Rig")]
    public void ApplySensorRig()
    {
        if (sensorRoot == null)
            sensorRoot = transform;

        sensorRoot.localScale = Vector3.one;

        ApplyVelodyne();
        ApplyForwardCamera();
    }

    private void ApplyVelodyne()
    {
        if (!createVelodynePlaceholder)
            return;

        velodyneBase = GetOrCreateChild(sensorRoot, "Velodyne_VLP16_Base");
        velodyneBase.localPosition = velodyneBaseLocalPosition;
        velodyneBase.localRotation = Quaternion.Euler(velodyneLocalEuler);
        velodyneBase.localScale = Vector3.one;

        velodyneScan = GetOrCreateChild(velodyneBase, "Velodyne_VLP16_Scan");
        velodyneScan.localPosition = velodyneScanLocalOffset;
        velodyneScan.localRotation = Quaternion.identity;
        velodyneScan.localScale = Vector3.one;

        GameObject body = GetOrCreatePrimitiveChild(
            velodyneBase,
            "Velodyne_Visual",
            PrimitiveType.Cylinder
        );

        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.identity;

        // Unity cylinder height is along local Y and default height is 2 units.
        body.transform.localScale = new Vector3(
            velodynePlaceholderRadius * 2.0f,
            velodynePlaceholderHeight * 0.5f,
            velodynePlaceholderRadius * 2.0f
        );
    }

    private void ApplyForwardCamera()
    {
        if (!createForwardCamera)
            return;

        Transform cameraTransform = GetOrCreateChild(sensorRoot, "Forward_Camera");
        cameraTransform.localPosition = cameraLocalPosition;
        cameraTransform.localRotation = Quaternion.Euler(cameraLocalEuler);
        cameraTransform.localScale = Vector3.one;

        forwardCamera = cameraTransform.GetComponent<Camera>();
        if (forwardCamera == null)
            forwardCamera = cameraTransform.gameObject.AddComponent<Camera>();

        forwardCamera.fieldOfView = Mathf.Clamp(cameraFov, 1.0f, 179.0f);
        forwardCamera.nearClipPlane = Mathf.Max(0.001f, cameraNearClip);
        forwardCamera.farClipPlane = Mathf.Max(forwardCamera.nearClipPlane + 0.001f, cameraFarClip);
        forwardCamera.enabled = false; // sensor camera, not player view

        if (forceLandscapeAspect)
            forwardCamera.aspect = Mathf.Max(0.001f, cameraAspectWidth) / Mathf.Max(0.001f, cameraAspectHeight);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawLidarDebug || velodyneScan == null)
            return;

        float range = Mathf.Max(0.01f, lidarDebugRangeM);
        float halfFov = Mathf.Clamp(lidarDebugHorizontalFovDeg, 1.0f, 360.0f) * 0.5f;

        Vector3 origin = velodyneScan.position;
        Vector3 forward = velodyneScan.forward;
        Vector3 left = Quaternion.AngleAxis(-halfFov, velodyneScan.up) * forward;
        Vector3 right = Quaternion.AngleAxis(halfFov, velodyneScan.up) * forward;

        Gizmos.DrawLine(origin, origin + left.normalized * range);
        Gizmos.DrawLine(origin, origin + right.normalized * range);
        Gizmos.DrawWireSphere(origin, 0.08f);
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static GameObject GetOrCreatePrimitiveChild(
        Transform parent,
        string childName,
        PrimitiveType primitiveType
    )
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
            return existing.gameObject;

        GameObject go = GameObject.CreatePrimitive(primitiveType);
        go.name = childName;
        go.transform.SetParent(parent, false);

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
                Destroy(collider);
            else
                DestroyImmediate(collider);
        }

        return go;
    }
}
