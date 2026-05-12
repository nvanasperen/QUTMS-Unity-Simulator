using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class QUTMSControllerDrive : MonoBehaviour
{
    [Header("References")]
    public QUTMSVehicleSimulationProfile profile;

    [Header("Drive")]
    public bool rearWheelDrive = true;
    public bool frontWheelDrive = false;

    public float maxMotorTorque = 420.0f;
    public float maxBrakeTorque = 900.0f;
    public float handbrakeTorque = 1600.0f;
    public float engineBrakingTorque = 30.0f;
    public bool isReverseGear = false;

    [Header("Steering")]
    public bool useProfileSteeringLimit = true;
    public float maxSteerAngleDeg = 16.0f;
    public float steerResponse = 8.0f;
    public float steerReturnResponse = 12.0f;
    public float speedSteerReduction = 0.55f;
    public float speedForFullReductionMps = 22.0f;

    [Header("Input")]
    public bool invertThrottleBrake = false;
    public float deadzone = 0.05f;
    public bool keyboardFallback = true;

    [Header("Debug")]
    public bool printDebug = false;
    public float debugPrintInterval = 0.25f;

    private float steerInput;
    private float throttleInput;
    private float brakeInput;
    private bool handbrakeInput;

    private float currentSteerDeg;
    private float debugTimer;

    private Rigidbody rb;

    private void Reset()
    {
        profile = GetComponent<QUTMSVehicleSimulationProfile>();
    }

    private void Awake()
    {
        if (profile == null)
            profile = GetComponent<QUTMSVehicleSimulationProfile>();

        CacheRigidbody();
    }

    private void Update()
    {
        if (profile == null)
            return;

        ReadInputs();
    }

    private void FixedUpdate()
    {
        if (profile == null)
            return;

        CacheRigidbody();
        ApplyVehicleControls();

        if (printDebug)
            PrintDebug();
    }

    private void CacheRigidbody()
    {
        if (profile == null)
            return;

        if (profile.sceneBinding != null && profile.sceneBinding.targetRigidbody != null)
        {
            rb = profile.sceneBinding.targetRigidbody;
            return;
        }

        Transform fullCar = transform.Find("QUTMS Full Car");
        if (fullCar != null)
            rb = fullCar.GetComponent<Rigidbody>();
    }

    private void ReadInputs()
    {
        steerInput = 0.0f;
        throttleInput = 0.0f;
        brakeInput = 0.0f;
        handbrakeInput = false;

#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;

        if (pad != null)
        {
            steerInput = ApplyDeadzone(pad.leftStick.x.ReadValue());

            float rightTrigger = pad.rightTrigger.ReadValue();
            float leftTrigger = pad.leftTrigger.ReadValue();

            if (!invertThrottleBrake)
            {
                throttleInput = rightTrigger;
                brakeInput = leftTrigger;
            }
            else
            {
                throttleInput = leftTrigger;
                brakeInput = rightTrigger;
            }

            handbrakeInput = pad.leftShoulder.isPressed || pad.buttonWest.isPressed;

            if (pad.buttonNorth.wasPressedThisFrame)
            {
                isReverseGear = !isReverseGear;
            }

            return;
        }
#endif

        if (!keyboardFallback)
            return;

        float keyboardSteer = 0.0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            keyboardSteer -= 1.0f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            keyboardSteer += 1.0f;

        steerInput = keyboardSteer;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            throttleInput = 1.0f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            brakeInput = 1.0f;

        if (Input.GetKeyDown(KeyCode.R))
            isReverseGear = !isReverseGear;

        handbrakeInput = Input.GetKey(KeyCode.Space);
    }

    private float ApplyDeadzone(float value)
    {
        if (Mathf.Abs(value) < deadzone)
            return 0.0f;

        return Mathf.Clamp(value, -1.0f, 1.0f);
    }

    private void ApplyVehicleControls()
    {
        WheelCollider fl = profile.sceneBinding.frontLeftWheelCollider;
        WheelCollider fr = profile.sceneBinding.frontRightWheelCollider;
        WheelCollider rl = profile.sceneBinding.rearLeftWheelCollider;
        WheelCollider rr = profile.sceneBinding.rearRightWheelCollider;

        if (fl == null || fr == null || rl == null || rr == null)
            return;

        float steeringLimit = GetSteeringLimitDeg();
        float speed = rb != null ? rb.linearVelocity.magnitude : 0.0f;

        float speedReductionT = Mathf.Clamp01(speed / Mathf.Max(0.01f, speedForFullReductionMps));
        float steerMultiplier = Mathf.Lerp(1.0f, speedSteerReduction, speedReductionT);

        float targetSteerDeg = steerInput * steeringLimit * steerMultiplier;

        float response = Mathf.Abs(steerInput) > 0.01f ? steerResponse : steerReturnResponse;
        currentSteerDeg = Mathf.Lerp(currentSteerDeg, targetSteerDeg, response * Time.fixedDeltaTime);

        fl.steerAngle = currentSteerDeg;
        fr.steerAngle = currentSteerDeg;

        float motorTorque = 0.0f;
        float brakeTorque = 0.0f;

        if (throttleInput > 0.0f)
        {
            motorTorque = isReverseGear ? -throttleInput * maxMotorTorque : throttleInput * maxMotorTorque;
        }

        if (brakeInput > 0.0f)
        {
            brakeTorque = brakeInput * maxBrakeTorque;
        }

        if (throttleInput < 0.05f && brakeInput < 0.05f)
        {
            brakeTorque = engineBrakingTorque;
        }

        if (handbrakeInput)
        {
            brakeTorque = Mathf.Max(brakeTorque, handbrakeTorque);
        }

        ApplyDriveTorque(fl, fr, rl, rr, motorTorque);
        ApplyBrakeTorque(fl, fr, rl, rr, brakeTorque);
    }

    private float GetSteeringLimitDeg()
    {
        if (useProfileSteeringLimit && profile != null && profile.car != null)
            return Mathf.Rad2Deg * Mathf.Max(0.01f, profile.car.steeringLimitRad);

        return maxSteerAngleDeg;
    }

    private void ApplyDriveTorque(
        WheelCollider fl,
        WheelCollider fr,
        WheelCollider rl,
        WheelCollider rr,
        float motorTorque
    )
    {
        fl.motorTorque = frontWheelDrive ? motorTorque : 0.0f;
        fr.motorTorque = frontWheelDrive ? motorTorque : 0.0f;
        rl.motorTorque = rearWheelDrive ? motorTorque : 0.0f;
        rr.motorTorque = rearWheelDrive ? motorTorque : 0.0f;
    }

    private void ApplyBrakeTorque(
        WheelCollider fl,
        WheelCollider fr,
        WheelCollider rl,
        WheelCollider rr,
        float brakeTorque
    )
    {
        fl.brakeTorque = brakeTorque;
        fr.brakeTorque = brakeTorque;
        rl.brakeTorque = brakeTorque;
        rr.brakeTorque = brakeTorque;
    }

    private void PrintDebug()
    {
        debugTimer += Time.fixedDeltaTime;

        if (debugTimer < debugPrintInterval)
            return;

        debugTimer = 0.0f;

        string gearMode = isReverseGear ? "R" : "D";

        Debug.Log(
            "[QUTMSControllerDrive]" +
            " gear=" + gearMode +
            " throttle=" + throttleInput.ToString("F2") +
            " brake=" + brakeInput.ToString("F2") +
            " steerInput=" + steerInput.ToString("F2") +
            " steerDeg=" + currentSteerDeg.ToString("F2") +
            " speed=" + (rb != null ? rb.linearVelocity.magnitude.ToString("F2") : "NULL")
        );
    }
}