using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Ackermann;

public class QUTMSAckermannCommandSubscriber : MonoBehaviour
{
    public QUTMSVehicleSimulationProfile vehicle;

    public string commandTopic = "/control/driving_command";

    public float maxSteeringRad = 0.28f;
    public float maxAccelerationMps2 = 5.0f;
    public float maxBrakeMps2 = 10.0f;

    private void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<AckermannDriveStampedMsg>(
            commandTopic,
            OnCommandReceived
        );
    }

    private void OnCommandReceived(AckermannDriveStampedMsg msg)
    {
        if (vehicle == null)
            return;

        float steeringRad = msg.drive.steering_angle;
        float acceleration = msg.drive.acceleration;
        float speed = msg.drive.speed;

        vehicle.steeringInput = Mathf.Clamp(steeringRad / maxSteeringRad, -1.0f, 1.0f);

        if (acceleration > 0.0f || speed > 0.1f)
        {
            vehicle.throttleInput = Mathf.Clamp01(Mathf.Max(acceleration, speed * 0.5f) / maxAccelerationMps2);
            vehicle.brakeInput = 0.0f;
        }
        else if (acceleration < 0.0f)
        {
            vehicle.throttleInput = 0.0f;
            vehicle.brakeInput = Mathf.Clamp01(-acceleration / maxBrakeMps2);
        }
        else
        {
            vehicle.throttleInput = 0.0f;
            vehicle.brakeInput = 0.0f;
        }
    }
}