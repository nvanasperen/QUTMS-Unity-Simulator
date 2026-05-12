using System;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class EUFSTrackSpawner : MonoBehaviour
{
    public TextAsset trackCsv;

    public GameObject blueConePrefab;
    public GameObject yellowConePrefab;
    public GameObject orangeConePrefab;
    public GameObject bigOrangeConePrefab;

    public bool clearExistingBeforeSpawn = true;
    public bool spawnOnPlay = true;

    [Header("CSV Conversion")]
    public float csvPositionScale = 0.01f;
    public bool useLocalPosition = true;

    [Header("Cone Height Offsets")]
    public float blueConeYOffset = 0.25f;
    public float yellowConeYOffset = 0.25f;
    public float orangeConeYOffset = 0.25f;
    public float bigOrangeConeYOffset = 0.0f;

    public void SpawnTrack()
    {
        if (trackCsv == null)
        {
            Debug.LogError("No track CSV assigned.");
            return;
        }

        if (clearExistingBeforeSpawn)
        {
            ClearSpawnedTrack();
        }

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
            float yOffset = 0f;

            if (tag == "blue")
            {
                prefab = blueConePrefab;
                yOffset = blueConeYOffset;
            }
            else if (tag == "yellow")
            {
                prefab = yellowConePrefab;
                yOffset = yellowConeYOffset;
            }
            else if (tag == "orange")
            {
                prefab = orangeConePrefab;
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

            Vector3 localPosition = new Vector3(x, yOffset, z);
            Quaternion localRotation = Quaternion.Euler(0f, -direction * Mathf.Rad2Deg, 0f);

            GameObject cone = CreateCone(prefab);
            cone.name = tag + "_cone_" + i;

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
        }
    }

    public void ClearSpawnedTrack()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private GameObject CreateCone(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject cone = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
            return cone;
        }
#endif

        return Instantiate(prefab, transform);
    }

    private float ParseFloat(string value)
    {
        return float.Parse(value.Trim(), CultureInfo.InvariantCulture);
    }

    private void Start()
    {
        if (Application.isPlaying && spawnOnPlay)
        {
            SpawnTrack();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EUFSTrackSpawner))]
public class EUFSTrackSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EUFSTrackSpawner spawner = (EUFSTrackSpawner)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn / Refresh Track"))
        {
            spawner.SpawnTrack();
            EditorUtility.SetDirty(spawner.gameObject);
        }

        if (GUILayout.Button("Clear Spawned Track"))
        {
            spawner.ClearSpawnedTrack();
            EditorUtility.SetDirty(spawner.gameObject);
        }
    }
}
#endif