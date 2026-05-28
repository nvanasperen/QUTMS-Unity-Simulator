using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Ackermann;

[DisallowMultipleComponent]
public class QUTMSAckermannCommandSubscriber : MonoBehaviour
{
    [Header("References")]
    public QUTMSVehicleSimulationProfile vehicle;

    [Header("ROS Topic")]
    public string commandTopic = "/control/driving_command";
    public bool subscribeOnStart = true;

    [Header("Steering Input")]
    public float maxSteeringRad = 0.28f;
    public bool invertSteering = false;

    [Header("Throttle / Brake Input")]
    public float maxAccelerationMps2 = 5.0f;
    public float maxBrakeMps2 = 10.0f;

    [Tooltip("Used when Ackermann speed is provided but acceleration is small/zero.")]
    public float speedToAccelerationGain = 0.5f;

    [Tooltip("Ignore tiny requested speeds below this value.")]
    public float speedDeadzoneMps = 0.1f;

    [Header("Command Safety")]
    public bool zeroInputsWhenCommandTimesOut = true;
    public float commandTimeoutS = 0.5f;

    [Header("Live ROS Input Debug")]
    [ReadOnlyInspector] public bool hasReceivedCommand = false;
    [ReadOnlyInspector] public float lastCommandAgeS = 0.0f;
    [ReadOnlyInspector] public float receivedSteeringRad = 0.0f;
    [ReadOnlyInspector] public float receivedSpeedMps = 0.0f;
    [ReadOnlyInspector] public float receivedAccelerationMps2 = 0.0f;

    [Header("Live Vehicle Inputs")]
    [ReadOnlyInspector] public float outputSteeringInput = 0.0f;
    [ReadOnlyInspector] public float outputThrottleInput = 0.0f;
    [ReadOnlyInspector] public float outputBrakeInput = 0.0f;

    private float lastCommandTimeS = -999.0f;

    private void Reset()
    {
        vehicle = GetComponent<QUTMSVehicleSimulationProfile>();
    }

    private void Start()
    {
        if (vehicle == null)
            vehicle = GetComponent<QUTMSVehicleSimulationProfile>();

        if (subscribeOnStart)
        {
            ROSConnection.GetOrCreateInstance().Subscribe<AckermannDriveStampedMsg>(
                commandTopic,
                OnCommandReceived
            );
        }
    }

    private void Update()
    {
        if (!hasReceivedCommand)
            return;

        lastCommandAgeS = Time.time - lastCommandTimeS;

        if (zeroInputsWhenCommandTimesOut && lastCommandAgeS > commandTimeoutS)
            ApplyInputs(0.0f, 0.0f, 0.0f);
    }

    private void OnCommandReceived(AckermannDriveStampedMsg msg)
    {
        if (vehicle == null)
            return;

        hasReceivedCommand = true;
        lastCommandTimeS = Time.time;
        lastCommandAgeS = 0.0f;

        receivedSteeringRad = (float)msg.drive.steering_angle;
        receivedSpeedMps = (float)msg.drive.speed;
        receivedAccelerationMps2 = (float)msg.drive.acceleration;

        float steeringSign = invertSteering ? -1.0f : 1.0f;
        float steeringInput = steeringSign * Mathf.Clamp(
            receivedSteeringRad / Mathf.Max(0.001f, maxSteeringRad),
            -1.0f,
            1.0f
        );

        float throttleInput = 0.0f;
        float brakeInput = 0.0f;

        if (receivedAccelerationMps2 > 0.0f || receivedSpeedMps > speedDeadzoneMps)
        {
            float accelerationDemand = Mathf.Max(
                receivedAccelerationMps2,
                receivedSpeedMps * speedToAccelerationGain
            );

            throttleInput = Mathf.Clamp01(accelerationDemand / Mathf.Max(0.001f, maxAccelerationMps2));
            brakeInput = 0.0f;
        }
        else if (receivedAccelerationMps2 < 0.0f)
        {
            throttleInput = 0.0f;
            brakeInput = Mathf.Clamp01(-receivedAccelerationMps2 / Mathf.Max(0.001f, maxBrakeMps2));
        }

        ApplyInputs(steeringInput, throttleInput, brakeInput);
    }

    private void ApplyInputs(float steering, float throttle, float brake)
    {
        if (vehicle == null)
            return;

        outputSteeringInput = steering;
        outputThrottleInput = throttle;
        outputBrakeInput = brake;

        vehicle.steeringInput = steering;
        vehicle.throttleInput = throttle;
        vehicle.brakeInput = brake;
    }
}

public class ReadOnlyInspectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        using (new UnityEditor.EditorGUI.DisabledScope(true))
        {
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
#endif