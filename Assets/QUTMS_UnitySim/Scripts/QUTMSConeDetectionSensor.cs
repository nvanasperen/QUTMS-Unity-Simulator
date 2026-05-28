using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class QUTMSConeDetectionSensor : MonoBehaviour
{
    public enum ConeType { Unknown, Blue, Yellow, Orange, BigOrange }

    [System.Serializable]
    public class ConeDetection
    {
        public ConeType type;
        public GameObject source;
        public Vector3 worldPosition;
        public Vector3 sensorLocalPosition;
        public float distanceM;
        public float bearingDeg;
    }

    [Header("Sensor")]
    public Transform sensorTransform;

    [Header("LiDAR Ground Truth")]
    public float lidarMaxRangeM = 25f;
    public float lidarMinRangeM = 0.25f;
    [Range(1f, 360f)] public float lidarHorizontalFovDeg = 360f;
    public bool lidarOnlyForward = false;

    [Header("Forward / Camera Ground Truth")]
    public float cameraMaxRangeM = 20f;
    public float cameraMinRangeM = 0.25f;
    [Range(1f, 180f)] public float cameraHorizontalFovDeg = 110f;
    public bool cameraOnlyForward = true;

    [Header("Cone Search")]
    public bool refreshConeCacheOnStart = true;
    public bool refreshConeCacheEachScan = false;
    public string[] coneNameTokens = { "blue", "yellow", "orange", "big_orange", "cone" };

    [Header("Runtime")]
    public float scanRateHz = 10f;
    public bool printDetections = true;
    public int maxPrintedDetections = 8;

    [Header("Debug Visualisation")]
    public bool drawDebug = true;
    public bool drawOnlyWhenSelected = true;
    public bool scanInEditModeForGizmos = true;

    [Header("Debug Visualisation - FOV")]
    public bool drawLidarFov = true;
    public bool drawCameraFov = false;
    public Color lidarFovColour = new Color(1.0f, 1.0f, 1.0f, 0.55f);
    public Color cameraFovColour = new Color(0.2f, 0.85f, 1.0f, 0.45f);

    [Header("Debug Visualisation - Detected Cones")]
    public bool drawDetectionLines = true;
    public bool drawLidarDetectionLines = true;
    public bool drawCameraDetectionLines = false;
    public int maxDrawnDetectionLines = 80;
    public float detectionMarkerRadiusM = 0.10f;
    public float detectionLineVerticalOffsetM = 0.05f;

    public Color blueConeLineColour = new Color(0.15f, 0.35f, 1.0f, 1.0f);
    public Color yellowConeLineColour = new Color(1.0f, 0.92f, 0.15f, 1.0f);
    public Color orangeConeLineColour = new Color(1.0f, 0.45f, 0.05f, 1.0f);
    public Color bigOrangeConeLineColour = new Color(1.0f, 0.25f, 0.0f, 1.0f);
    public Color unknownConeLineColour = Color.white;

    public readonly List<GameObject> coneCache = new List<GameObject>();
    public readonly List<ConeDetection> lidarDetections = new List<ConeDetection>();
    public readonly List<ConeDetection> cameraDetections = new List<ConeDetection>();

    private float scanTimer;
    private float lastEditorGizmoScanTime = -1000f;

    private void Reset()
    {
        sensorTransform = transform;
    }

    private void Start()
    {
        if (sensorTransform == null)
            sensorTransform = transform;

        if (refreshConeCacheOnStart)
            RefreshConeCache();
    }

    private void Update()
    {
        float interval = 1f / Mathf.Max(0.01f, scanRateHz);
        scanTimer += Time.deltaTime;

        if (scanTimer < interval)
            return;

        scanTimer = 0f;
        Scan(true);
    }

    [ContextMenu("Refresh Cone Cache")]
    public void RefreshConeCache()
    {
        coneCache.Clear();

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject obj = allObjects[i];

            if (obj == null)
                continue;

            if (LooksLikeConeRoot(obj))
                coneCache.Add(obj);
        }

        Debug.Log("[QUTMSConeDetectionSensor] cachedCones=" + coneCache.Count);
    }

    [ContextMenu("Scan Now")]
    public void Scan()
    {
        Scan(true);
    }

    public void Scan(bool allowPrinting)
    {
        if (sensorTransform == null)
            sensorTransform = transform;

        if (refreshConeCacheEachScan || coneCache.Count == 0)
            RefreshConeCache();

        lidarDetections.Clear();
        cameraDetections.Clear();

        for (int i = 0; i < coneCache.Count; i++)
        {
            GameObject cone = coneCache[i];

            if (cone == null)
                continue;

            TryAddDetection(
                cone,
                lidarDetections,
                lidarMinRangeM,
                lidarMaxRangeM,
                lidarHorizontalFovDeg,
                lidarOnlyForward
            );

            TryAddDetection(
                cone,
                cameraDetections,
                cameraMinRangeM,
                cameraMaxRangeM,
                cameraHorizontalFovDeg,
                cameraOnlyForward
            );
        }

        lidarDetections.Sort((a, b) => a.distanceM.CompareTo(b.distanceM));
        cameraDetections.Sort((a, b) => a.distanceM.CompareTo(b.distanceM));

        if (allowPrinting && printDetections)
            PrintDetections();
    }

    private void TryAddDetection(
        GameObject cone,
        List<ConeDetection> output,
        float minRange,
        float maxRange,
        float horizontalFovDeg,
        bool onlyForward
    )
    {
        Vector3 worldPosition = cone.transform.position;
        Vector3 local = sensorTransform.InverseTransformPoint(worldPosition);

        float distance = local.magnitude;

        if (distance < minRange || distance > maxRange)
            return;

        if (onlyForward && local.z < 0f)
            return;

        float bearingDeg = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        float halfFov = horizontalFovDeg * 0.5f;

        if (Mathf.Abs(bearingDeg) > halfFov)
            return;

        output.Add(new ConeDetection
        {
            type = GuessConeType(cone),
            source = cone,
            worldPosition = worldPosition,
            sensorLocalPosition = local,
            distanceM = distance,
            bearingDeg = bearingDeg
        });
    }

    private bool LooksLikeConeRoot(GameObject obj)
    {
        if (obj == null)
            return false;

        string n = obj.name.ToLowerInvariant();

        bool nameLooksRight = false;

        for (int i = 0; i < coneNameTokens.Length; i++)
        {
            string token = coneNameTokens[i];

            if (!string.IsNullOrWhiteSpace(token) && n.Contains(token.ToLowerInvariant()))
            {
                nameLooksRight = true;
                break;
            }
        }

        if (!nameLooksRight)
            return false;

        if (n.Contains("camera") || n.Contains("light") || n.Contains("mesh") || n.Contains("visual"))
            return false;

        // Important: reject prefab children inside a spawned cone.
        // The spawned cone root is the object named blue_cone_12 / yellow_cone_30 / etc.
        // Child meshes often also contain "cone", "blue", or "yellow" in their names.
        if (obj.transform.parent != null && LooksLikeConeName(obj.transform.parent.gameObject.name))
            return false;

        // Prefer generated cone roots from EUFSTrackSpawner.
        // This prevents random cone-looking mesh children/import pieces being counted.
        if (n.Contains("_cone_"))
            return true;

        return false;
    }

    private bool LooksLikeConeName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return false;

        string n = objectName.ToLowerInvariant();

        if (n.Contains("camera") || n.Contains("light") || n.Contains("mesh") || n.Contains("visual"))
            return false;

        for (int i = 0; i < coneNameTokens.Length; i++)
        {
            string token = coneNameTokens[i];

            if (!string.IsNullOrWhiteSpace(token) && n.Contains(token.ToLowerInvariant()))
                return true;
        }

        return false;
    }

    private ConeType GuessConeType(GameObject obj)
    {
        string n = obj.name.ToLowerInvariant();

        if (n.Contains("big_orange"))
            return ConeType.BigOrange;

        if (n.Contains("blue"))
            return ConeType.Blue;

        if (n.Contains("yellow"))
            return ConeType.Yellow;

        if (n.Contains("orange"))
            return ConeType.Orange;

        return ConeType.Unknown;
    }

    private void PrintDetections()
    {
        Debug.Log(
            "[QUTMSConeDetectionSensor]" +
            " cachedCones=" + coneCache.Count +
            " lidarDetections=" + lidarDetections.Count +
            " cameraDetections=" + cameraDetections.Count
        );
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug || drawOnlyWhenSelected)
            return;

        DrawDebugGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug)
            return;

        DrawDebugGizmos();
    }

    private void DrawDebugGizmos()
    {
        Transform origin = sensorTransform != null ? sensorTransform : transform;

        if (scanInEditModeForGizmos && !Application.isPlaying)
        {
            // Avoid spamming expensive scene searches every single repaint.
            if (Time.realtimeSinceStartup - lastEditorGizmoScanTime > 0.20f)
            {
                lastEditorGizmoScanTime = Time.realtimeSinceStartup;
                Scan(false);
            }
        }

        if (drawLidarFov)
        {
            Gizmos.color = lidarFovColour;
            DrawSensorFov(origin, lidarMaxRangeM, lidarHorizontalFovDeg);
        }

        if (drawCameraFov)
        {
            Gizmos.color = cameraFovColour;
            DrawSensorFov(origin, cameraMaxRangeM, cameraHorizontalFovDeg);
        }

        if (!drawDetectionLines)
            return;

        if (drawLidarDetectionLines)
            DrawDetectionLines(origin, lidarDetections);

        if (drawCameraDetectionLines)
            DrawDetectionLines(origin, cameraDetections);
    }

    private void DrawDetectionLines(Transform origin, List<ConeDetection> detections)
    {
        if (origin == null || detections == null)
            return;

        int maxLines = Mathf.Max(0, maxDrawnDetectionLines);
        int count = Mathf.Min(detections.Count, maxLines);

        Vector3 start = origin.position + Vector3.up * detectionLineVerticalOffsetM;

        for (int i = 0; i < count; i++)
        {
            ConeDetection detection = detections[i];

            if (detection == null || detection.source == null)
                continue;

            Gizmos.color = GetConeDebugColour(detection.type);

            Vector3 end = detection.worldPosition + Vector3.up * detectionLineVerticalOffsetM;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, Mathf.Max(0.001f, detectionMarkerRadiusM));
        }
    }

    private Color GetConeDebugColour(ConeType type)
    {
        switch (type)
        {
            case ConeType.Blue:
                return blueConeLineColour;

            case ConeType.Yellow:
                return yellowConeLineColour;

            case ConeType.Orange:
                return orangeConeLineColour;

            case ConeType.BigOrange:
                return bigOrangeConeLineColour;

            default:
                return unknownConeLineColour;
        }
    }

    private void DrawSensorFov(Transform origin, float range, float fovDeg)
    {
        if (origin == null)
            return;

        Gizmos.DrawWireSphere(origin.position, range);

        Vector3 leftDir = Quaternion.AngleAxis(-fovDeg * 0.5f, origin.up) * origin.forward;
        Vector3 rightDir = Quaternion.AngleAxis(fovDeg * 0.5f, origin.up) * origin.forward;

        Gizmos.DrawLine(origin.position, origin.position + leftDir.normalized * range);
        Gizmos.DrawLine(origin.position, origin.position + rightDir.normalized * range);
    }
}
