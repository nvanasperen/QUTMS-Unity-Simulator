using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class QUTMSRocketLeagueCamera : MonoBehaviour
{
    [Header("References")]
    public QUTMSVehicleSimulationProfile profile;
    public Camera targetCamera;
    public Transform explicitTarget;

    [Header("Chase Camera")]
    public float chaseDistance = 5.8f;
    public float chaseHeight = 2.2f;
    public float lookAtHeight = 0.75f;
    public float lookAheadSeconds = 0.18f;

    public float positionSharpness = 9.0f;
    public float rotationSharpness = 12.0f;

    [Header("Rocket League Style Swivel")]
    public float rightStickYawSpeedDeg = 155.0f;
    public float rightStickPitchSpeedDeg = 70.0f;
    public float maxManualYawDeg = 115.0f;
    public float minManualPitchDeg = -18.0f;
    public float maxManualPitchDeg = 38.0f;
    public float manualReturnSharpness = 4.5f;
    public bool returnCameraWhenStickReleased = true;

    [Header("Track View / Triangle")]
    public bool trackViewEnabled = false;
    public float trackViewDistance = 16.0f;
    public float trackViewHeight = 14.0f;
    public float trackViewLookAtHeight = 0.3f;
    public float trackViewPitchDeg = 62.0f;
    public float trackViewPositionSharpness = 5.0f;
    public float trackViewRotationSharpness = 7.0f;

    [Header("Input")]
    public float rightStickDeadzone = 0.08f;
    public bool keyboardFallback = true;

    [Header("Debug")]
    public bool drawDebug = false;

    private Rigidbody targetRigidbody;
    private Transform target;

    private float manualYawDeg;
    private float manualPitchDeg;

    private bool previousKeyboardTrackToggle;

    private void Reset()
    {
        profile = GetComponent<QUTMSVehicleSimulationProfile>();

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Awake()
    {
        ResolveReferences(true);
    }

    private void LateUpdate()
    {
        ResolveReferences(false);

        if (target == null)
            return;

        ReadCameraInput();

        if (trackViewEnabled)
            UpdateTrackView();
        else
            UpdateChaseView();
    }

    private void ResolveReferences(bool forceCameraSearch)
    {
        if (profile == null)
            profile = GetComponent<QUTMSVehicleSimulationProfile>();

        if ((targetCamera == null && forceCameraSearch) || targetCamera == null)
            targetCamera = Camera.main;

        if (explicitTarget != null)
        {
            target = explicitTarget;
            targetRigidbody = explicitTarget.GetComponent<Rigidbody>();
            return;
        }

        if (profile != null && profile.sceneBinding != null && profile.sceneBinding.targetRigidbody != null)
        {
            targetRigidbody = profile.sceneBinding.targetRigidbody;
            target = targetRigidbody.transform;
            return;
        }

        if (profile != null)
        {
            Transform fullCar = profile.transform.Find("QUTMS Full Car");

            if (fullCar != null)
            {
                target = fullCar;
                targetRigidbody = fullCar.GetComponent<Rigidbody>();
            }
        }
    }

    private void ReadCameraInput()
    {
        Vector2 rightStick = Vector2.zero;
        bool trianglePressed = false;

#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;

        if (pad != null)
        {
            rightStick = pad.rightStick.ReadValue();

            if (rightStick.magnitude < rightStickDeadzone)
                rightStick = Vector2.zero;

            trianglePressed = pad.buttonNorth.wasPressedThisFrame;
        }
#endif

        if (keyboardFallback)
        {
            if (Input.GetKey(KeyCode.J))
                rightStick.x -= 1.0f;

            if (Input.GetKey(KeyCode.L))
                rightStick.x += 1.0f;

            if (Input.GetKey(KeyCode.I))
                rightStick.y += 1.0f;

            if (Input.GetKey(KeyCode.K))
                rightStick.y -= 1.0f;

            bool keyboardTrackToggle = Input.GetKey(KeyCode.T);

            if (keyboardTrackToggle && !previousKeyboardTrackToggle)
                trianglePressed = true;

            previousKeyboardTrackToggle = keyboardTrackToggle;
        }

        if (trianglePressed)
            trackViewEnabled = !trackViewEnabled;

        if (rightStick != Vector2.zero)
        {
            manualYawDeg += rightStick.x * rightStickYawSpeedDeg * Time.deltaTime;
            manualPitchDeg -= rightStick.y * rightStickPitchSpeedDeg * Time.deltaTime;

            manualYawDeg = Mathf.Clamp(manualYawDeg, -maxManualYawDeg, maxManualYawDeg);
            manualPitchDeg = Mathf.Clamp(manualPitchDeg, minManualPitchDeg, maxManualPitchDeg);
        }
        else if (returnCameraWhenStickReleased)
        {
            float t = 1.0f - Mathf.Exp(-manualReturnSharpness * Time.deltaTime);
            manualYawDeg = Mathf.Lerp(manualYawDeg, 0.0f, t);
            manualPitchDeg = Mathf.Lerp(manualPitchDeg, 0.0f, t);
        }
    }

    private void UpdateChaseView()
    {
        Transform camTransform = GetCameraTransform();

        if (camTransform == null)
            return;

        Vector3 targetPosition = target.position;
        Vector3 targetForward = GetFlatForward();

        Quaternion yawRotation = Quaternion.AngleAxis(manualYawDeg, Vector3.up);
        Vector3 cameraForward = yawRotation * targetForward;

        Vector3 velocityLookAhead = GetVelocity() * lookAheadSeconds;
        Vector3 lookPoint = targetPosition + Vector3.up * lookAtHeight + velocityLookAhead;

        float pitchHeightOffset = Mathf.Sin(manualPitchDeg * Mathf.Deg2Rad) * chaseDistance;
        float adjustedHeight = chaseHeight + pitchHeightOffset;

        Vector3 desiredPosition =
            targetPosition
            - cameraForward * chaseDistance
            + Vector3.up * adjustedHeight;

        Quaternion desiredRotation = Quaternion.LookRotation(
            (lookPoint - desiredPosition).normalized,
            Vector3.up
        );

        SmoothCamera(camTransform, desiredPosition, desiredRotation, positionSharpness, rotationSharpness);
    }

    private void UpdateTrackView()
    {
        Transform camTransform = GetCameraTransform();

        if (camTransform == null)
            return;

        Vector3 targetPosition = target.position;
        Vector3 targetForward = GetFlatForward();

        Quaternion yawRotation = Quaternion.AngleAxis(manualYawDeg, Vector3.up);
        Vector3 cameraForward = yawRotation * targetForward;

        Vector3 lookPoint = targetPosition + Vector3.up * trackViewLookAtHeight;

        Vector3 desiredPosition =
            targetPosition
            - cameraForward * trackViewDistance
            + Vector3.up * trackViewHeight;

        Quaternion desiredRotation = Quaternion.LookRotation(
            (lookPoint - desiredPosition).normalized,
            Vector3.up
        );

        SmoothCamera(
            camTransform,
            desiredPosition,
            desiredRotation,
            trackViewPositionSharpness,
            trackViewRotationSharpness
        );
    }

    private void SmoothCamera(
        Transform camTransform,
        Vector3 desiredPosition,
        Quaternion desiredRotation,
        float posSharpness,
        float rotSharpness
    )
    {
        float posT = 1.0f - Mathf.Exp(-Mathf.Max(0.01f, posSharpness) * Time.deltaTime);
        float rotT = 1.0f - Mathf.Exp(-Mathf.Max(0.01f, rotSharpness) * Time.deltaTime);

        camTransform.position = Vector3.Lerp(camTransform.position, desiredPosition, posT);
        camTransform.rotation = Quaternion.Slerp(camTransform.rotation, desiredRotation, rotT);
    }

    private Transform GetCameraTransform()
    {
        if (targetCamera != null)
            return targetCamera.transform;

        return transform;
    }

    private Vector3 GetFlatForward()
    {
        Vector3 forward = target.forward;
        forward.y = 0.0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return forward.normalized;
    }

    private Vector3 GetVelocity()
    {
        if (targetRigidbody == null)
            return Vector3.zero;

#if UNITY_6000_0_OR_NEWER
        return targetRigidbody.linearVelocity;
#else
        return targetRigidbody.velocity;
#endif
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug || target == null)
            return;

        Gizmos.DrawWireSphere(target.position + Vector3.up * lookAtHeight, 0.15f);
    }
}