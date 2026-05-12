using UnityEngine;

[ExecuteAlways]
public class QUTMSSensorRig : MonoBehaviour
{
    [Header("Sensor Root")]
    public Transform sensorRoot;

    [Header("Velodyne / LiDAR - from processed.urdf")]
    public Vector3 velodyneBaseLocalPosition = new Vector3(0.0f, 0.85f, -0.3f);
    public Vector3 velodyneScanLocalOffset = new Vector3(0.0f, 0.0377f, 0.0f);
    public Vector3 velodyneLocalEuler = Vector3.zero;

    public bool createVelodynePlaceholder = true;
    public float velodynePlaceholderRadius = 0.06f;
    public float velodynePlaceholderHeight = 0.08f;

    [Header("Forward Camera")]
    public bool createForwardCamera = true;
    public Vector3 cameraLocalPosition = new Vector3(0.0f, 0.75f, 0.35f);
    public Vector3 cameraLocalEuler = new Vector3(0.0f, 0.0f, 0.0f);
    public float cameraFov = 70.0f;
    public float cameraNearClip = 0.05f;
    public float cameraFarClip = 200.0f;

    [Header("Runtime References")]
    public Transform velodyneBase;
    public Transform velodyneScan;
    public Camera forwardCamera;

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
            sensorRoot = GetOrCreateChild(transform, "SensorRig");

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

        forwardCamera.fieldOfView = cameraFov;
        forwardCamera.nearClipPlane = cameraNearClip;
        forwardCamera.farClipPlane = cameraFarClip;
        forwardCamera.enabled = false; // sensor camera, not player view
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(childName);
        go.transform.SetParent(parent, false);
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