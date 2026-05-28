using System.Collections.Generic;
using UnityEngine;

public class QUTMSControllerDrive : MonoBehaviour
{
    [Header("Control")]
    public bool enableManualDrive = true;
    public bool autoFindWheels = true;

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public KeyCode brakeKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.R;

    [Header("Vehicle")]
    public Rigidbody vehicleRigidbody;
    public Transform vehicleRoot;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Driving")]
    public float maxSteerAngle = 28f;
    public float maxMotorTorque = 320f;
    public float maxBrakeTorque = 1400f;
    public float reverseTorqueMultiplier = 0.45f;
    public float steeringSmoothing = 8f;
    public float throttleSmoothing = 6f;
    public float brakeSmoothing = 10f;

    [Header("Speed Limiting")]
    public bool limitTopSpeed = true;
    public float topSpeedKmh = 75f;

    [Header("Reset")]
    public float resetHeight = 0.75f;

    [Header("Debug")]
    public float steerInput;
    public float throttleInput;
    public float brakeInput;
    public float speedKmh;

    private float smoothedSteer;
    private float smoothedThrottle;
    private float smoothedBrake;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Reset()
    {
        vehicleRoot = transform;
        vehicleRigidbody = GetComponentInParent<Rigidbody>();

        if (vehicleRigidbody == null)
            vehicleRigidbody = GetComponent<Rigidbody>();

        FindWheels();
    }

    private void Awake()
    {
        if (vehicleRoot == null)
            vehicleRoot = transform;

        if (vehicleRigidbody == null)
            vehicleRigidbody = GetComponentInParent<Rigidbody>();

        if (vehicleRigidbody == null)
            vehicleRigidbody = GetComponent<Rigidbody>();

        if (autoFindWheels)
            FindWheels();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        if (!enableManualDrive)
            return;

        ReadInput();

        if (Input.GetKeyDown(resetKey))
            ResetVehicle();
    }

    private void FixedUpdate()
    {
        if (!enableManualDrive)
            return;

        if (vehicleRigidbody != null)
            speedKmh = vehicleRigidbody.linearVelocity.magnitude * 3.6f;

        smoothedSteer = Mathf.Lerp(smoothedSteer, steerInput, Time.fixedDeltaTime * steeringSmoothing);
        smoothedThrottle = Mathf.Lerp(smoothedThrottle, throttleInput, Time.fixedDeltaTime * throttleSmoothing);
        smoothedBrake = Mathf.Lerp(smoothedBrake, brakeInput, Time.fixedDeltaTime * brakeSmoothing);

        ApplyWheelControl();
    }

    private void ReadInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        try
        {
            horizontal = Input.GetAxisRaw(horizontalAxis);
            vertical = Input.GetAxisRaw(verticalAxis);
        }
        catch
        {
            horizontal = 0f;
            vertical = 0f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical += 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical -= 1f;

        steerInput = Mathf.Clamp(horizontal, -1f, 1f);

        if (vertical >= 0f)
        {
            throttleInput = Mathf.Clamp01(vertical);
            brakeInput = Input.GetKey(brakeKey) ? 1f : 0f;
        }
        else
        {
            throttleInput = Mathf.Clamp(vertical, -1f, 0f);
            brakeInput = Input.GetKey(brakeKey) ? 1f : 0f;
        }
    }

    private void ApplyWheelControl()
    {
        float steerAngle = smoothedSteer * maxSteerAngle;

        if (frontLeftWheel != null)
            frontLeftWheel.steerAngle = steerAngle;

        if (frontRightWheel != null)
            frontRightWheel.steerAngle = steerAngle;

        float motorTorque = 0f;

        if (!limitTopSpeed || speedKmh < topSpeedKmh || Mathf.Sign(smoothedThrottle) != Mathf.Sign(GetForwardSpeed()))
        {
            if (smoothedThrottle >= 0f)
                motorTorque = smoothedThrottle * maxMotorTorque;
            else
                motorTorque = smoothedThrottle * maxMotorTorque * reverseTorqueMultiplier;
        }

        float brakeTorque = smoothedBrake * maxBrakeTorque;

        ApplyDriveTorque(rearLeftWheel, motorTorque);
        ApplyDriveTorque(rearRightWheel, motorTorque);

        ApplyBrakeTorque(frontLeftWheel, brakeTorque);
        ApplyBrakeTorque(frontRightWheel, brakeTorque);
        ApplyBrakeTorque(rearLeftWheel, brakeTorque);
        ApplyBrakeTorque(rearRightWheel, brakeTorque);
    }

    private float GetForwardSpeed()
    {
        if (vehicleRigidbody == null)
            return 0f;

        return Vector3.Dot(vehicleRigidbody.linearVelocity, transform.forward);
    }

    private void ApplyDriveTorque(WheelCollider wheel, float torque)
    {
        if (wheel == null)
            return;

        wheel.motorTorque = torque;
    }

    private void ApplyBrakeTorque(WheelCollider wheel, float torque)
    {
        if (wheel == null)
            return;

        wheel.brakeTorque = torque;
    }

    private void FindWheels()
    {
        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>(true);

        if (wheels == null || wheels.Length == 0)
            return;

        List<WheelCollider> front = new List<WheelCollider>();
        List<WheelCollider> rear = new List<WheelCollider>();

        float averageLocalZ = 0f;

        foreach (WheelCollider wheel in wheels)
        {
            Vector3 local = transform.InverseTransformPoint(wheel.transform.position);
            averageLocalZ += local.z;
        }

        averageLocalZ /= wheels.Length;

        foreach (WheelCollider wheel in wheels)
        {
            Vector3 local = transform.InverseTransformPoint(wheel.transform.position);

            if (local.z >= averageLocalZ)
                front.Add(wheel);
            else
                rear.Add(wheel);
        }

        AssignLeftRight(front, out frontLeftWheel, out frontRightWheel);
        AssignLeftRight(rear, out rearLeftWheel, out rearRightWheel);
    }

    private void AssignLeftRight(List<WheelCollider> wheels, out WheelCollider left, out WheelCollider right)
    {
        left = null;
        right = null;

        if (wheels == null || wheels.Count == 0)
            return;

        if (wheels.Count == 1)
        {
            left = wheels[0];
            return;
        }

        wheels.Sort((a, b) =>
        {
            float ax = transform.InverseTransformPoint(a.transform.position).x;
            float bx = transform.InverseTransformPoint(b.transform.position).x;
            return ax.CompareTo(bx);
        });

        left = wheels[0];
        right = wheels[wheels.Count - 1];
    }

    public void ResetVehicle()
    {
        Transform target = vehicleRigidbody != null ? vehicleRigidbody.transform : transform;

        target.position = startPosition + Vector3.up * resetHeight;
        target.rotation = startRotation;

        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.linearVelocity = Vector3.zero;
            vehicleRigidbody.angularVelocity = Vector3.zero;
        }

        ClearWheelForces(frontLeftWheel);
        ClearWheelForces(frontRightWheel);
        ClearWheelForces(rearLeftWheel);
        ClearWheelForces(rearRightWheel);
    }

    private void ClearWheelForces(WheelCollider wheel)
    {
        if (wheel == null)
            return;

        wheel.motorTorque = 0f;
        wheel.brakeTorque = 0f;
        wheel.steerAngle = 0f;
    }
}