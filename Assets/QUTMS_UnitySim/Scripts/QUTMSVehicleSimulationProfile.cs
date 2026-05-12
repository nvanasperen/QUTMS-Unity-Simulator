using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#endif

[ExecuteAlways]
public class QUTMSVehicleSimulationProfile : MonoBehaviour
{
    public enum SourceVehicleModel
    {
        DynamicBicycle,
        PointMass
    }

    public enum SourceCommandMode
    {
        Velocity,
        Acceleration
    }

    [Serializable]
    public class ChassisPresetValues
    {
        [InspectorName("Preset Name")]
        public string presetName = "QEV-3 Chassis";

        [InspectorName("Chassis Model Prefab")]
        public GameObject chassisModelPrefab;

        [InspectorName("Chassis Collider Box")]
        public GameObject chassisColliderBox;

        [InspectorName("Chassis Mass (kg)")]
        public float chassisMassKg = 235.0f;

        [InspectorName("Gravity (m/s²)")]
        public float gravityMps2 = 9.81f;

        [InspectorName("Yaw Inertia Izz (kg·m²)")]
        public float yawInertiaIzzKgM2 = 31.27f;

        [InspectorName("Front Weight Fraction")]
        [Range(0.0f, 1.0f)]
        public float frontWeightFraction = 0.45f;

        [InspectorName("Wheelbase From Chassis (m)")]
        public float chassisWheelbaseM = 1.580f;

        [InspectorName("Front Track From Chassis (m)")]
        public float chassisFrontTrackM = 1.400f;

        [InspectorName("Rear Track From Chassis (m)")]
        public float chassisRearTrackM = 1.400f;

        [InspectorName("Drag Coefficient")]
        public float dragCoefficient = 1.44475f;

        [InspectorName("Downforce Coefficient")]
        public float downforceCoefficient = 3.22455f;

        [InspectorName("Minimum Steering Angle (rad)")]
        public float minSteeringRad = -0.28f;

        [InspectorName("Maximum Steering Angle (rad)")]
        public float maxSteeringRad = 0.28f;

        [InspectorName("Steering Lock Time (s)")]
        public float steeringLockTimeS = 1.5f;

        [InspectorName("Steering Command Scale")]
        public float steeringCommandScale = 16.0f / 90.0f;

        [InspectorName("Steering Publish Scale")]
        public float steeringPublishScale = 90.0f / 16.0f;

        [InspectorName("Maximum Acceleration (m/s²)")]
        public float maxAccelerationMps2 = 5.0f;

        [InspectorName("Minimum Acceleration (m/s²)")]
        public float minAccelerationMps2 = -10.0f;

        [InspectorName("Maximum Velocity (m/s)")]
        public float maxVelocityMps = 30.0f;

        [InspectorName("Minimum Velocity (m/s)")]
        public float minVelocityMps = 0.0f;

        public void SetQev3Defaults()
        {
            presetName = "QEV-3 Chassis";
            chassisMassKg = 235.0f;
            gravityMps2 = 9.81f;
            yawInertiaIzzKgM2 = 31.27f;
            frontWeightFraction = 0.45f;
            chassisWheelbaseM = 1.580f;
            chassisFrontTrackM = 1.400f;
            chassisRearTrackM = 1.400f;
            dragCoefficient = 1.44475f;
            downforceCoefficient = 3.22455f;
            minSteeringRad = -0.28f;
            maxSteeringRad = 0.28f;
            steeringLockTimeS = 1.5f;
            steeringCommandScale = 16.0f / 90.0f;
            steeringPublishScale = 90.0f / 16.0f;
            maxAccelerationMps2 = 5.0f;
            minAccelerationMps2 = -10.0f;
            maxVelocityMps = 30.0f;
            minVelocityMps = 0.0f;
        }

        public void CopyFrom(ChassisPresetValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;
            chassisModelPrefab = other.chassisModelPrefab;
            chassisColliderBox = other.chassisColliderBox;
            chassisMassKg = other.chassisMassKg;
            gravityMps2 = other.gravityMps2;
            yawInertiaIzzKgM2 = other.yawInertiaIzzKgM2;
            frontWeightFraction = other.frontWeightFraction;
            chassisWheelbaseM = other.chassisWheelbaseM;
            chassisFrontTrackM = other.chassisFrontTrackM;
            chassisRearTrackM = other.chassisRearTrackM;
            dragCoefficient = other.dragCoefficient;
            downforceCoefficient = other.downforceCoefficient;
            minSteeringRad = other.minSteeringRad;
            maxSteeringRad = other.maxSteeringRad;
            steeringLockTimeS = other.steeringLockTimeS;
            steeringCommandScale = other.steeringCommandScale;
            steeringPublishScale = other.steeringPublishScale;
            maxAccelerationMps2 = other.maxAccelerationMps2;
            minAccelerationMps2 = other.minAccelerationMps2;
            maxVelocityMps = other.maxVelocityMps;
            minVelocityMps = other.minVelocityMps;
        }
    }

    [Serializable]
    public class WheelPackagePresetValues
    {
        [InspectorName("Preset Name")]
        public string presetName = "QUTMS Dry Wheel Package";

        [InspectorName("Front Left Wheel")]
        public GameObject frontLeftWheel;

        [InspectorName("Front Right Wheel")]
        public GameObject frontRightWheel;

        [InspectorName("Rear Left Wheel")]
        public GameObject rearLeftWheel;

        [InspectorName("Rear Right Wheel")]
        public GameObject rearRightWheel;

        [InspectorName("Wheel Assembly Mass (kg)")]
        public float wheelAssemblyMassKg = 8.0f;

        [InspectorName("Radius (m)")]
        public float radiusM = 0.2032f;

        [InspectorName("Width (m)")]
        public float widthM = 0.20f;

        [InspectorName("Tyre Coefficient")]
        public float tyreCoefficient = 1.0f;

        [InspectorName("Pacejka B")]
        public float pacejkaB = 12.56f;

        [InspectorName("Pacejka C")]
        public float pacejkaC = -1.38f;

        [InspectorName("Pacejka D")]
        public float pacejkaD = 1.60f;

        [InspectorName("Pacejka E")]
        public float pacejkaE = -0.58f;

        [InspectorName("Auto Scale Visual Mesh To Tyre Radius")]
        public bool autoScaleVisualMeshToTyreRadius = true;

        [InspectorName("Visual Mesh Native Radius (m)")]
        public float visualMeshNativeRadiusM = 0.2525f;

        [InspectorName("Visual Mesh Scale Multiplier")]
        public float visualMeshScaleMultiplier = 1.0f;

        [InspectorName("Scale Centre Offset With Visual Scale")]
        public bool scaleModelCentreOffsetWithVisualScale = true;

        [InspectorName("Model Centre Offset (m)")]
        public Vector3 modelCentreOffsetM = new Vector3(0.0f, 0.0493f, 0.0f);

        [InspectorName("Model Rotation Offset (deg)")]
        public Vector3 modelRotationOffsetDeg = Vector3.zero;

        public float GetVisualMeshScale()
        {
            float scale = Mathf.Max(0.001f, visualMeshScaleMultiplier);

            if (autoScaleVisualMeshToTyreRadius)
                scale *= Mathf.Max(0.001f, radiusM) / Mathf.Max(0.001f, visualMeshNativeRadiusM);

            return scale;
        }

        public Vector3 GetModelCentreOffsetForCurrentScale()
        {
            if (!scaleModelCentreOffsetWithVisualScale)
                return modelCentreOffsetM;

            return modelCentreOffsetM * GetVisualMeshScale();
        }

        public void NormalizeVisualSettings()
        {
            if (visualMeshNativeRadiusM <= 0.001f)
                visualMeshNativeRadiusM = 0.2525f;

            if (visualMeshScaleMultiplier <= 0.001f)
                visualMeshScaleMultiplier = 1.0f;
        }

        public void SetDryDefaults()
        {
            presetName = "QUTMS Dry Wheel Package";
            wheelAssemblyMassKg = 8.0f;

            radiusM = 0.2032f;
            widthM = 0.20f;

            tyreCoefficient = 1.0f;
            pacejkaB = 12.56f;
            pacejkaC = -1.38f;
            pacejkaD = 1.60f;
            pacejkaE = -0.58f;

            autoScaleVisualMeshToTyreRadius = true;
            visualMeshNativeRadiusM = 0.2525f;
            visualMeshScaleMultiplier = 1.0f;
            scaleModelCentreOffsetWithVisualScale = true;

            modelCentreOffsetM = new Vector3(0.0f, 0.0493f, 0.0f);
            modelRotationOffsetDeg = Vector3.zero;
        }

        public void SetWetDefaults()
        {
            presetName = "QUTMS Wet Wheel Package";
            wheelAssemblyMassKg = 8.0f;

            radiusM = 0.2525f;
            widthM = 0.20f;

            tyreCoefficient = 1.0f;
            pacejkaB = 15.07f;
            pacejkaC = -1.66f;
            pacejkaD = 1.31f;
            pacejkaE = -0.58f;

            autoScaleVisualMeshToTyreRadius = true;
            visualMeshNativeRadiusM = 0.2525f;
            visualMeshScaleMultiplier = 1.0f;
            scaleModelCentreOffsetWithVisualScale = true;

            modelCentreOffsetM = new Vector3(0.0f, 0.0493f, 0.0f);
            modelRotationOffsetDeg = Vector3.zero;
        }

        public void CopyFrom(WheelPackagePresetValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;

            frontLeftWheel = other.frontLeftWheel;
            frontRightWheel = other.frontRightWheel;
            rearLeftWheel = other.rearLeftWheel;
            rearRightWheel = other.rearRightWheel;

            wheelAssemblyMassKg = other.wheelAssemblyMassKg;
            radiusM = other.radiusM;
            widthM = other.widthM;

            tyreCoefficient = other.tyreCoefficient;
            pacejkaB = other.pacejkaB;
            pacejkaC = other.pacejkaC;
            pacejkaD = other.pacejkaD;
            pacejkaE = other.pacejkaE;

            autoScaleVisualMeshToTyreRadius = other.autoScaleVisualMeshToTyreRadius;
            visualMeshNativeRadiusM = other.visualMeshNativeRadiusM;
            visualMeshScaleMultiplier = other.visualMeshScaleMultiplier;
            scaleModelCentreOffsetWithVisualScale = other.scaleModelCentreOffsetWithVisualScale;

            modelCentreOffsetM = other.modelCentreOffsetM;
            modelRotationOffsetDeg = other.modelRotationOffsetDeg;

            NormalizeVisualSettings();
        }
    }

    [Serializable]
    public class SuspensionPresetValues
    {
        [InspectorName("Preset Name")]
        public string presetName = "QEV-3 Baseline Suspension";

        [InspectorName("Suspension Distance (m)")]
        public float suspensionDistanceM = 0.12f;

        [InspectorName("Spring Strength (N/m)")]
        public float springStrengthNPerM = 12000.0f;

        [InspectorName("Damper Strength (N·s/m)")]
        public float damperStrengthNsPerM = 1400.0f;

        [InspectorName("Target Position (0–1)")]
        [Range(0.0f, 1.0f)]
        public float targetPosition = 0.65f;

        [InspectorName("Wheel Damping Rate")]
        public float wheelDampingRate = 5.0f;

        [InspectorName("Force App Point Distance (m)")]
        public float forceAppPointDistanceM = 0.0f;

        [InspectorName("Anti-Roll Bar Stiffness (N/m)")]
        public float antiRollBarStiffness = 5000.0f;

        public void SetBaselineDefaults()
        {
            presetName = "QEV-3 Baseline Suspension";
            suspensionDistanceM = 0.12f;
            springStrengthNPerM = 12000.0f;
            damperStrengthNsPerM = 1400.0f;
            targetPosition = 0.65f;
            wheelDampingRate = 5.0f;
            forceAppPointDistanceM = 0.0f;
            antiRollBarStiffness = 5000.0f;
        }

        public void CopyFrom(SuspensionPresetValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;
            suspensionDistanceM = other.suspensionDistanceM;
            springStrengthNPerM = other.springStrengthNPerM;
            damperStrengthNsPerM = other.damperStrengthNsPerM;
            targetPosition = other.targetPosition;
            wheelDampingRate = other.wheelDampingRate;
            forceAppPointDistanceM = other.forceAppPointDistanceM;
            antiRollBarStiffness = other.antiRollBarStiffness;
        }
    }

    [Serializable]
    public class FullCarValues
    {
        public bool useDerivedTotalMass = true;
        public float totalMassKg = 267.0f;

        public bool useDerivedWheelAssemblyTotalMass = true;
        public float wheelAssemblyTotalMassKg = 32.0f;

        public bool useDerivedWheelRadius = true;
        public float wheelRadiusM = 0.2032f;

        public bool useDerivedWheelWidth = true;
        public float wheelWidthM = 0.20f;

        public bool useDerivedWheelbase = true;
        public float wheelbaseM = 1.580f;

        public bool useDerivedFrontTrack = true;
        public float frontTrackM = 1.400f;

        public bool useDerivedRearTrack = true;
        public float rearTrackM = 1.400f;

        public bool useDerivedYawInertiaIzz = true;
        public float yawInertiaIzzKgM2 = 31.27f;

        public bool useDerivedSteeringLimit = true;
        public float steeringLimitRad = 0.28f;

        public bool useDerivedFrontWeightFraction = true;
        public float frontWeightFraction = 0.45f;

        public bool useDerivedDragCoefficient = true;
        public float dragCoefficient = 1.44475f;

        public bool useDerivedDownforceCoefficient = true;
        public float downforceCoefficient = 3.22455f;

        public bool useDerivedMaxAcceleration = true;
        public float maxAccelerationMps2 = 5.0f;

        public bool useDerivedMinAcceleration = true;
        public float minAccelerationMps2 = -10.0f;

        public bool useDerivedMaxVelocity = true;
        public float maxVelocityMps = 30.0f;

        public bool useDerivedSuspensionDistance = true;
        public float suspensionDistanceM = 0.12f;

        public bool useDerivedSpringStrength = true;
        public float springStrengthNPerM = 12000.0f;

        public bool useDerivedDamperStrength = true;
        public float damperStrengthNsPerM = 1400.0f;

        public void RefreshFrom(ChassisPresetValues chassis, WheelPackagePresetValues wheels, SuspensionPresetValues suspension)
        {
            if (chassis == null || wheels == null || suspension == null)
                return;

            if (useDerivedWheelAssemblyTotalMass)
                wheelAssemblyTotalMassKg = wheels.wheelAssemblyMassKg * 4.0f;

            if (useDerivedTotalMass)
                totalMassKg = chassis.chassisMassKg + wheelAssemblyTotalMassKg;

            if (useDerivedWheelRadius)
                wheelRadiusM = wheels.radiusM;

            if (useDerivedWheelWidth)
                wheelWidthM = wheels.widthM;

            if (useDerivedWheelbase)
                wheelbaseM = chassis.chassisWheelbaseM;

            if (useDerivedFrontTrack)
                frontTrackM = chassis.chassisFrontTrackM;

            if (useDerivedRearTrack)
                rearTrackM = chassis.chassisRearTrackM;

            if (useDerivedYawInertiaIzz)
                yawInertiaIzzKgM2 = chassis.yawInertiaIzzKgM2;

            if (useDerivedSteeringLimit)
                steeringLimitRad = Mathf.Max(Mathf.Abs(chassis.minSteeringRad), Mathf.Abs(chassis.maxSteeringRad));

            if (useDerivedFrontWeightFraction)
                frontWeightFraction = chassis.frontWeightFraction;

            if (useDerivedDragCoefficient)
                dragCoefficient = chassis.dragCoefficient;

            if (useDerivedDownforceCoefficient)
                downforceCoefficient = chassis.downforceCoefficient;

            if (useDerivedMaxAcceleration)
                maxAccelerationMps2 = chassis.maxAccelerationMps2;

            if (useDerivedMinAcceleration)
                minAccelerationMps2 = chassis.minAccelerationMps2;

            if (useDerivedMaxVelocity)
                maxVelocityMps = chassis.maxVelocityMps;

            if (useDerivedSuspensionDistance)
                suspensionDistanceM = suspension.suspensionDistanceM;

            if (useDerivedSpringStrength)
                springStrengthNPerM = suspension.springStrengthNPerM;

            if (useDerivedDamperStrength)
                damperStrengthNsPerM = suspension.damperStrengthNsPerM;
        }
    }

    [Serializable]
    public class TrackConditionPresetValues
    {
        public string presetName = "Dry";
        public float groundFrictionMu = 100.0f;
        public float groundFrictionMu2 = 50.0f;
        public float objectNoisePercent = 0.0f;
        public float coneNoiseRadiusM = 0.0f;
        public float colourNoisePercent = 0.0f;

        public void SetDryDefaults()
        {
            presetName = "Dry";
            groundFrictionMu = 100.0f;
            groundFrictionMu2 = 50.0f;
            objectNoisePercent = 0.0f;
            coneNoiseRadiusM = 0.0f;
            colourNoisePercent = 0.0f;
        }

        public void SetWetDefaults()
        {
            presetName = "Wet";
            groundFrictionMu = 100.0f;
            groundFrictionMu2 = 50.0f;
            objectNoisePercent = 0.0f;
            coneNoiseRadiusM = 0.0f;
            colourNoisePercent = 0.0f;
        }

        public void CopyFrom(TrackConditionPresetValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;
            groundFrictionMu = other.groundFrictionMu;
            groundFrictionMu2 = other.groundFrictionMu2;
            objectNoisePercent = other.objectNoisePercent;
            coneNoiseRadiusM = other.coneNoiseRadiusM;
            colourNoisePercent = other.colourNoisePercent;
        }
    }

    [Serializable]
    public class SensorConditionValues
    {
        public string presetName = "Default Sensors";

        public float lidarUpdateRateHz = 10.0f;
        public float lidarMinViewDistanceM = 1.0f;
        public float lidarMaxViewDistanceM = 20.0f;
        public float lidarFovRad = 3.141593f;
        public float lidarRangeNoiseM = 0.01f;
        public float lidarBearingNoiseRad = 0.001f;

        public float cameraUpdateRateHz = 15.0f;
        public float cameraMinViewDistanceM = 0.5f;
        public float cameraMaxViewDistanceM = 10.0f;
        public float cameraFovRad = 1.91986f;
        public float cameraRangeNoiseM = 0.01f;
        public float cameraBearingNoiseRad = 0.01f;

        public float imuUpdateRateHz = 200.0f;
        public Vector3 imuOrientationNoise = new Vector3(0.001f, 0.0f, 0.001f);
        public Vector3 imuAngularVelocityNoise = new Vector3(0.0f, 0.0f, 0.1f);
        public Vector3 imuLinearAccelerationNoise = Vector3.zero;

        public float slamUpdateRateHz = 20.0f;
        public float slamXNoiseM = 0.1f;
        public float slamYNoiseM = 0.1f;

        public void SetDefaults()
        {
            presetName = "Default Sensors";

            lidarUpdateRateHz = 10.0f;
            lidarMinViewDistanceM = 1.0f;
            lidarMaxViewDistanceM = 20.0f;
            lidarFovRad = 3.141593f;
            lidarRangeNoiseM = 0.01f;
            lidarBearingNoiseRad = 0.001f;

            cameraUpdateRateHz = 15.0f;
            cameraMinViewDistanceM = 0.5f;
            cameraMaxViewDistanceM = 10.0f;
            cameraFovRad = 1.91986f;
            cameraRangeNoiseM = 0.01f;
            cameraBearingNoiseRad = 0.01f;

            imuUpdateRateHz = 200.0f;
            imuOrientationNoise = new Vector3(0.001f, 0.0f, 0.001f);
            imuAngularVelocityNoise = new Vector3(0.0f, 0.0f, 0.1f);
            imuLinearAccelerationNoise = Vector3.zero;

            slamUpdateRateHz = 20.0f;
            slamXNoiseM = 0.1f;
            slamYNoiseM = 0.1f;
        }

        public void CopyFrom(SensorConditionValues other)
        {
            if (other == null)
                return;

            presetName = other.presetName;

            lidarUpdateRateHz = other.lidarUpdateRateHz;
            lidarMinViewDistanceM = other.lidarMinViewDistanceM;
            lidarMaxViewDistanceM = other.lidarMaxViewDistanceM;
            lidarFovRad = other.lidarFovRad;
            lidarRangeNoiseM = other.lidarRangeNoiseM;
            lidarBearingNoiseRad = other.lidarBearingNoiseRad;

            cameraUpdateRateHz = other.cameraUpdateRateHz;
            cameraMinViewDistanceM = other.cameraMinViewDistanceM;
            cameraMaxViewDistanceM = other.cameraMaxViewDistanceM;
            cameraFovRad = other.cameraFovRad;
            cameraRangeNoiseM = other.cameraRangeNoiseM;
            cameraBearingNoiseRad = other.cameraBearingNoiseRad;

            imuUpdateRateHz = other.imuUpdateRateHz;
            imuOrientationNoise = other.imuOrientationNoise;
            imuAngularVelocityNoise = other.imuAngularVelocityNoise;
            imuLinearAccelerationNoise = other.imuLinearAccelerationNoise;

            slamUpdateRateHz = other.slamUpdateRateHz;
            slamXNoiseM = other.slamXNoiseM;
            slamYNoiseM = other.slamYNoiseM;
        }
    }

    [Serializable]
    public class MotionNoiseValues
    {
        public Vector3 positionNoise = new Vector3(0.01f, 0.01f, 0.1f);
        public Vector3 orientationNoise = new Vector3(0.001f, 0.0f, 0.001f);
        public Vector3 linearVelocityNoise = new Vector3(0.1f, 0.1f, 0.0f);
        public Vector3 angularVelocityNoise = new Vector3(0.0f, 0.0f, 0.1f);
        public Vector3 linearAccelerationNoise = Vector3.zero;

        public void SetDefaults()
        {
            positionNoise = new Vector3(0.01f, 0.01f, 0.1f);
            orientationNoise = new Vector3(0.001f, 0.0f, 0.001f);
            linearVelocityNoise = new Vector3(0.1f, 0.1f, 0.0f);
            angularVelocityNoise = new Vector3(0.0f, 0.0f, 0.1f);
            linearAccelerationNoise = Vector3.zero;
        }

        public void CopyFrom(MotionNoiseValues other)
        {
            if (other == null)
                return;

            positionNoise = other.positionNoise;
            orientationNoise = other.orientationNoise;
            linearVelocityNoise = other.linearVelocityNoise;
            angularVelocityNoise = other.angularVelocityNoise;
            linearAccelerationNoise = other.linearAccelerationNoise;
        }
    }

    [Serializable]
    public class WheelSpeedNoiseValues
    {
        public float frontLeftWheelSpeedNoise = 0.5f;
        public float frontRightWheelSpeedNoise = 0.5f;
        public float rearLeftWheelSpeedNoise = 0.3f;
        public float rearRightWheelSpeedNoise = 0.3f;

        public void SetDefaults()
        {
            frontLeftWheelSpeedNoise = 0.5f;
            frontRightWheelSpeedNoise = 0.5f;
            rearLeftWheelSpeedNoise = 0.3f;
            rearRightWheelSpeedNoise = 0.3f;
        }

        public void CopyFrom(WheelSpeedNoiseValues other)
        {
            if (other == null)
                return;

            frontLeftWheelSpeedNoise = other.frontLeftWheelSpeedNoise;
            frontRightWheelSpeedNoise = other.frontRightWheelSpeedNoise;
            rearLeftWheelSpeedNoise = other.rearLeftWheelSpeedNoise;
            rearRightWheelSpeedNoise = other.rearRightWheelSpeedNoise;
        }
    }

    [Serializable]
    public class InterfaceRuntimeValues
    {
        public float vehicleUpdateRateHz = 1000.0f;
        public float vehiclePublishRateHz = 200.0f;
        public float publishRateHz = 100.0f;
        public float controlDelayS = 0.035f;
        public SourceVehicleModel sourceVehicleModel = SourceVehicleModel.DynamicBicycle;
        public SourceCommandMode sourceCommandMode = SourceCommandMode.Velocity;

        public void SetDefaults()
        {
            vehicleUpdateRateHz = 1000.0f;
            vehiclePublishRateHz = 200.0f;
            publishRateHz = 100.0f;
            controlDelayS = 0.035f;
            sourceVehicleModel = SourceVehicleModel.DynamicBicycle;
            sourceCommandMode = SourceCommandMode.Velocity;
        }
    }

    [Serializable]
    public class SceneBindingValues
    {
        public Transform physicsRoot;
        public Transform chassisVisualRoot;
        public Transform wheelColliderRigRoot;
        public Rigidbody targetRigidbody;

        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;

        public Transform frontLeftWheelVisual;
        public Transform frontRightWheelVisual;
        public Transform rearLeftWheelVisual;
        public Transform rearRightWheelVisual;

        public float wheelColliderLocalYOffsetM = 0.0f;
        public float wheelColliderLocalZOffsetM = 0.0f;

        public Transform sensorRoot;
        public Transform lidarTransform;
        public Transform imuTransform;
        public Camera forwardCamera;
    }

    [Header("Runtime Inputs")]
    [Range(-1f, 1f)] public float steeringInput = 0.0f;
    [Range(0f, 1f)] public float throttleInput = 0.0f;
    [Range(0f, 1f)] public float brakeInput = 0.0f;

    [Header("Car")]
    [InspectorName("Selected Chassis Preset")]
    public string selectedChassisPresetName = "QEV-3 Chassis";

    [InspectorName("Selected Wheel Package Preset")]
    public string selectedWheelPackagePresetName = "QUTMS Dry Wheel Package";

    [InspectorName("Selected Suspension Preset")]
    public string selectedSuspensionPresetName = "QEV-3 Baseline Suspension";

    [InspectorName("Derived Full-Car Values")]
    public FullCarValues car = new FullCarValues();

    [Header("Chassis")]
    public ChassisPresetValues chassis = new ChassisPresetValues();

    [Header("Wheels")]
    public WheelPackagePresetValues wheelPackage = new WheelPackagePresetValues();

    [Header("Suspension")]
    public SuspensionPresetValues suspension = new SuspensionPresetValues();

    [Header("Track Conditions")]
    public string selectedTrackConditionPresetName = "Dry";
    public TrackConditionPresetValues trackConditions = new TrackConditionPresetValues();

    [Header("Sensors")]
    public string selectedSensorPresetName = "Default Sensors";
    public SensorConditionValues sensorConditions = new SensorConditionValues();
    public MotionNoiseValues motionNoise = new MotionNoiseValues();
    public WheelSpeedNoiseValues wheelSpeedNoise = new WheelSpeedNoiseValues();

    [Header("Interface / Runtime")]
    public InterfaceRuntimeValues interfaceRuntime = new InterfaceRuntimeValues();

    [HideInInspector]
    public SceneBindingValues sceneBinding = new SceneBindingValues();

    private void Awake()
    {
        if (Application.isPlaying)
        {
            Time.fixedDeltaTime = 0.005f;
        }
    }

    public void RefreshDerivedCarValues()
    {
        wheelPackage.NormalizeVisualSettings();
        car.RefreshFrom(chassis, wheelPackage, suspension);
    }

    public void ResetSelectedChassisPreset()
    {
        if (selectedChassisPresetName == "QEV-3 Chassis")
            chassis.SetQev3Defaults();

        selectedChassisPresetName = chassis.presetName;
        RefreshDerivedCarValues();
    }

    public void ResetSelectedWheelPackagePreset()
    {
        if (selectedWheelPackagePresetName == "QUTMS Wet Wheel Package")
            wheelPackage.SetWetDefaults();
        else
            wheelPackage.SetDryDefaults();

        selectedWheelPackagePresetName = wheelPackage.presetName;
        RefreshDerivedCarValues();
    }

    public void ResetSelectedSuspensionPreset()
    {
        suspension.SetBaselineDefaults();
        selectedSuspensionPresetName = suspension.presetName;
        RefreshDerivedCarValues();
    }

    public void ResetSelectedTrackConditionPreset()
    {
        if (selectedTrackConditionPresetName == "Wet")
            trackConditions.SetWetDefaults();
        else
            trackConditions.SetDryDefaults();

        selectedTrackConditionPresetName = trackConditions.presetName;
    }

    public void ResetSelectedSensorPreset()
    {
        sensorConditions.SetDefaults();
        motionNoise.SetDefaults();
        wheelSpeedNoise.SetDefaults();
        selectedSensorPresetName = sensorConditions.presetName;
    }

    public void GenerateOrUpdateFullCar()
    {
        RefreshDerivedCarValues();

        Transform physicsRoot = EnsureNamedChild(transform, "QUTMS Full Car");
        physicsRoot.localPosition = Vector3.zero;
        physicsRoot.localRotation = Quaternion.identity;
        physicsRoot.localScale = Vector3.one;
        sceneBinding.physicsRoot = physicsRoot;

        Rigidbody rigidbody = EnsureRigidbody(physicsRoot);
        ConfigureRigidbody(rigidbody);
        sceneBinding.targetRigidbody = rigidbody;

        BuildChassisVisual(physicsRoot);
        BuildChassisCollider(physicsRoot);
        BuildWheelColliders(physicsRoot);
        BuildWheelVisuals(physicsRoot);
        SyncWheelVisualsToColliders();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
#endif
    }

    private void BuildChassisVisual(Transform physicsRoot)
    {
        Transform visualRoot = EnsureNamedChild(physicsRoot, "Chassis Visual");
        visualRoot.localPosition = new Vector3(0.0f, wheelPackage.radiusM, 0.0f);
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;
        sceneBinding.chassisVisualRoot = visualRoot;

        ClearChildren(visualRoot);

        if (chassis.chassisModelPrefab == null)
            return;

        GameObject instance = InstantiateProfileObject(chassis.chassisModelPrefab, visualRoot, "Chassis Model");
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        RemovePhysicsFromVisualObject(instance);
    }

    private void BuildChassisCollider(Transform physicsRoot)
    {
        Transform colliderRoot = EnsureNamedChild(physicsRoot, "Chassis Collider");
        colliderRoot.localPosition = Vector3.zero;
        colliderRoot.localRotation = Quaternion.identity;
        colliderRoot.localScale = Vector3.one;

        ClearChildren(colliderRoot);

        if (chassis.chassisColliderBox != null)
        {
            GameObject instance = InstantiateProfileObject(chassis.chassisColliderBox, colliderRoot, "Chassis Collider Box");
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return;
        }

        GameObject generated = new GameObject("Generated Chassis Collider Box");
        generated.transform.SetParent(colliderRoot, false);
        generated.transform.localPosition = Vector3.zero;
        generated.transform.localRotation = Quaternion.identity;
        generated.transform.localScale = Vector3.one;

        BoxCollider box = generated.AddComponent<BoxCollider>();

        float radius = Mathf.Max(0.01f, wheelPackage.radiusM);
        float wheelbase = Mathf.Max(0.01f, car.wheelbaseM);
        float widestTrack = Mathf.Max(car.frontTrackM, car.rearTrackM);

        float width = Mathf.Max(0.30f, widestTrack * 0.65f);
        float height = Mathf.Max(0.25f, radius * 1.25f);
        float length = Mathf.Max(0.50f, wheelbase * 1.10f);

        box.size = new Vector3(width, height, length);
        box.center = new Vector3(0.0f, radius + height * 0.5f + 0.02f, 0.0f);
    }

    private void BuildWheelColliders(Transform physicsRoot)
    {
        Transform wheelColliderRoot = EnsureNamedChild(physicsRoot, "Wheel Colliders");
        wheelColliderRoot.localPosition = Vector3.zero;
        wheelColliderRoot.localRotation = Quaternion.identity;
        wheelColliderRoot.localScale = Vector3.one;
        sceneBinding.wheelColliderRigRoot = wheelColliderRoot;

        float radius = Mathf.Max(0.01f, wheelPackage.radiusM);
        float halfWheelbase = Mathf.Max(0.01f, car.wheelbaseM) * 0.5f;
        float halfFrontTrack = Mathf.Max(0.01f, car.frontTrackM) * 0.5f;
        float halfRearTrack = Mathf.Max(0.01f, car.rearTrackM) * 0.5f;

        float y = radius + sceneBinding.wheelColliderLocalYOffsetM;
        float frontZ = halfWheelbase + sceneBinding.wheelColliderLocalZOffsetM;
        float rearZ = -halfWheelbase + sceneBinding.wheelColliderLocalZOffsetM;

        sceneBinding.frontLeftWheelCollider = ConfigureWheelCollider(
            wheelColliderRoot,
            "Front Left Wheel Collider",
            new Vector3(-halfFrontTrack, y, frontZ)
        );

        sceneBinding.frontRightWheelCollider = ConfigureWheelCollider(
            wheelColliderRoot,
            "Front Right Wheel Collider",
            new Vector3(halfFrontTrack, y, frontZ)
        );

        sceneBinding.rearLeftWheelCollider = ConfigureWheelCollider(
            wheelColliderRoot,
            "Rear Left Wheel Collider",
            new Vector3(-halfRearTrack, y, rearZ)
        );

        sceneBinding.rearRightWheelCollider = ConfigureWheelCollider(
            wheelColliderRoot,
            "Rear Right Wheel Collider",
            new Vector3(halfRearTrack, y, rearZ)
        );
    }

    private WheelCollider ConfigureWheelCollider(Transform parent, string name, Vector3 localPosition)
    {
        Transform wheelTransform = EnsureNamedChild(parent, name);
        wheelTransform.localPosition = localPosition;
        wheelTransform.localRotation = Quaternion.identity;
        wheelTransform.localScale = Vector3.one;

        WheelCollider collider = wheelTransform.GetComponent<WheelCollider>();

        if (collider == null)
            collider = wheelTransform.gameObject.AddComponent<WheelCollider>();

        collider.mass = Mathf.Max(0.01f, wheelPackage.wheelAssemblyMassKg);
        collider.radius = Mathf.Max(0.01f, wheelPackage.radiusM);
        collider.suspensionDistance = Mathf.Max(0.0f, suspension.suspensionDistanceM);
        collider.wheelDampingRate = Mathf.Max(0.0f, suspension.wheelDampingRate);
        collider.forceAppPointDistance = suspension.forceAppPointDistanceM;

        JointSpring spring = collider.suspensionSpring;
        spring.spring = Mathf.Max(0.0f, suspension.springStrengthNPerM);
        spring.damper = Mathf.Max(0.0f, suspension.damperStrengthNsPerM);
        spring.targetPosition = Mathf.Clamp01(suspension.targetPosition);
        collider.suspensionSpring = spring;

        collider.forwardFriction = BuildWheelFrictionCurve(true);
        collider.sidewaysFriction = BuildWheelFrictionCurve(false);

        return collider;
    }

    private WheelFrictionCurve BuildWheelFrictionCurve(bool forward)
    {
        float tyreCoefficient = Mathf.Max(0.01f, wheelPackage.tyreCoefficient);
        float pacejkaPeak = Mathf.Max(0.10f, Mathf.Abs(wheelPackage.pacejkaD));

        float trackGripScale = GetTrackGripScale(forward);
        float effectiveGrip = tyreCoefficient * trackGripScale;

        WheelFrictionCurve curve = new WheelFrictionCurve();

        if (forward)
        {
            curve.extremumSlip = 0.20f;
            curve.extremumValue = pacejkaPeak * effectiveGrip;
            curve.asymptoteSlip = 0.65f;
            curve.asymptoteValue = pacejkaPeak * effectiveGrip * 0.75f;
            curve.stiffness = effectiveGrip * 2.5f;
        }
        else
        {
            curve.extremumSlip = 0.15f;
            curve.extremumValue = pacejkaPeak * effectiveGrip;
            curve.asymptoteSlip = 0.50f;
            curve.asymptoteValue = pacejkaPeak * effectiveGrip * 0.70f;
            curve.stiffness = effectiveGrip * 2.5f;
        }

        return curve;
    }
    private float GetTrackGripScale(bool forward)
    {
        if (trackConditions == null)
            return 1.0f;

        if (forward)
            return Mathf.Clamp(trackConditions.groundFrictionMu / 100.0f, 0.01f, 5.0f);

        return Mathf.Clamp(trackConditions.groundFrictionMu2 / 50.0f, 0.01f, 5.0f);
    }

    private void BuildWheelVisuals(Transform physicsRoot)
    {
        Transform visualRoot = EnsureNamedChild(physicsRoot, "Wheel Visuals");
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        sceneBinding.frontLeftWheelVisual = ConfigureWheelVisual(
            visualRoot,
            "Front Left Wheel Visual",
            sceneBinding.frontLeftWheelCollider,
            wheelPackage.frontLeftWheel
        );

        sceneBinding.frontRightWheelVisual = ConfigureWheelVisual(
            visualRoot,
            "Front Right Wheel Visual",
            sceneBinding.frontRightWheelCollider,
            wheelPackage.frontRightWheel
        );

        sceneBinding.rearLeftWheelVisual = ConfigureWheelVisual(
            visualRoot,
            "Rear Left Wheel Visual",
            sceneBinding.rearLeftWheelCollider,
            wheelPackage.rearLeftWheel
        );

        sceneBinding.rearRightWheelVisual = ConfigureWheelVisual(
            visualRoot,
            "Rear Right Wheel Visual",
            sceneBinding.rearRightWheelCollider,
            wheelPackage.rearRightWheel
        );
    }

    private Transform ConfigureWheelVisual(Transform parent, string name, WheelCollider collider, GameObject source)
    {
        Transform slot = EnsureNamedChild(parent, name);

        if (collider != null)
        {
            slot.position = collider.transform.position;
            slot.rotation = collider.transform.rotation;
        }
        else
        {
            slot.localPosition = Vector3.zero;
            slot.localRotation = Quaternion.identity;
        }

        slot.localScale = Vector3.one;

        ClearChildren(slot);

        if (source != null)
        {
            GameObject instance = InstantiateProfileObject(source, slot, name + " Mesh");

            float visualMeshScale = wheelPackage.GetVisualMeshScale();

            instance.transform.localPosition = wheelPackage.GetModelCentreOffsetForCurrentScale();
            instance.transform.localRotation = Quaternion.Euler(wheelPackage.modelRotationOffsetDeg);
            instance.transform.localScale = Vector3.one * visualMeshScale;

            RemovePhysicsFromVisualObject(instance);
        }

        return slot;
    }

    private void ConfigureRigidbody(Rigidbody rb)
    {
        if (rb == null)
            return;

        rb.mass = Mathf.Max(0.01f, car.totalMassKg);
        rb.useGravity = chassis.gravityMps2 > 0.0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.0f;
        rb.angularDamping = 0.05f;
#else
        rb.drag = 0.0f;
        rb.angularDrag = 0.05f;
#endif

        float clampedFrontWeight = Mathf.Clamp01(car.frontWeightFraction);
        float centreOfMassZ = (clampedFrontWeight - 0.5f) * Mathf.Max(0.01f, car.wheelbaseM);
        rb.centerOfMass = new Vector3(0.0f, wheelPackage.radiusM, centreOfMassZ);

        float yawInertia = Mathf.Max(0.01f, car.yawInertiaIzzKgM2);
        rb.inertiaTensor = new Vector3(
            Mathf.Max(0.01f, yawInertia * 0.70f),
            yawInertia,
            Mathf.Max(0.01f, yawInertia * 0.90f)
        );
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying || sceneBinding.targetRigidbody == null)
            return;

        ApplyDownforce();
        ApplyAntiRollBars();
        ApplyDriveCommands();
    }

    private void ApplyDownforce()
    {
        float speed = sceneBinding.targetRigidbody.linearVelocity.magnitude;
        float downforce = car.downforceCoefficient * speed * speed;

        sceneBinding.targetRigidbody.AddForceAtPosition(
            -transform.up * downforce,
            sceneBinding.targetRigidbody.worldCenterOfMass
        );
    }

    private void ApplyAntiRollBars()
    {
        if (sceneBinding.frontLeftWheelCollider == null || sceneBinding.frontRightWheelCollider == null ||
            sceneBinding.rearLeftWheelCollider == null || sceneBinding.rearRightWheelCollider == null)
            return;

        ApplyAxleAntiRoll(sceneBinding.frontLeftWheelCollider, sceneBinding.frontRightWheelCollider);
        ApplyAxleAntiRoll(sceneBinding.rearLeftWheelCollider, sceneBinding.rearRightWheelCollider);
    }

    private void ApplyAxleAntiRoll(WheelCollider left, WheelCollider right)
    {
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = left.GetGroundHit(out WheelHit hitL);
        if (groundedL)
            travelL = (-left.transform.InverseTransformPoint(hitL.point).y - left.radius) / left.suspensionDistance;

        bool groundedR = right.GetGroundHit(out WheelHit hitR);
        if (groundedR)
            travelR = (-right.transform.InverseTransformPoint(hitR.point).y - right.radius) / right.suspensionDistance;

        float antiRollForce = (travelL - travelR) * suspension.antiRollBarStiffness;

        if (groundedL)
            sceneBinding.targetRigidbody.AddForceAtPosition(left.transform.up * -antiRollForce, left.transform.position);

        if (groundedR)
            sceneBinding.targetRigidbody.AddForceAtPosition(right.transform.up * antiRollForce, right.transform.position);
    }

    private void ApplyDriveCommands()
    {
        float steerAngleDeg = steeringInput * (car.steeringLimitRad * Mathf.Rad2Deg);

        sceneBinding.frontLeftWheelCollider.steerAngle = steerAngleDeg;
        sceneBinding.frontRightWheelCollider.steerAngle = steerAngleDeg;

        float maxTotalDriveForce = car.totalMassKg * car.maxAccelerationMps2;
        float maxWheelTorque = (maxTotalDriveForce / 4.0f) * car.wheelRadiusM;
        float appliedMotorTorque = throttleInput * maxWheelTorque;

        sceneBinding.frontLeftWheelCollider.motorTorque = appliedMotorTorque;
        sceneBinding.frontRightWheelCollider.motorTorque = appliedMotorTorque;
        sceneBinding.rearLeftWheelCollider.motorTorque = appliedMotorTorque;
        sceneBinding.rearRightWheelCollider.motorTorque = appliedMotorTorque;

        float maxTotalBrakeForce = car.totalMassKg * Mathf.Abs(car.minAccelerationMps2);
        float maxBrakeTorque = (maxTotalBrakeForce / 4.0f) * car.wheelRadiusM;
        float appliedBrakeTorque = brakeInput * maxBrakeTorque;

        sceneBinding.frontLeftWheelCollider.brakeTorque = appliedBrakeTorque;
        sceneBinding.frontRightWheelCollider.brakeTorque = appliedBrakeTorque;
        sceneBinding.rearLeftWheelCollider.brakeTorque = appliedBrakeTorque;
        sceneBinding.rearRightWheelCollider.brakeTorque = appliedBrakeTorque;
    }

    private void LateUpdate()
    {
        SyncWheelVisualsToColliders();
    }

    private void SyncWheelVisualsToColliders()
    {
        SyncWheelVisual(sceneBinding.frontLeftWheelCollider, sceneBinding.frontLeftWheelVisual);
        SyncWheelVisual(sceneBinding.frontRightWheelCollider, sceneBinding.frontRightWheelVisual);
        SyncWheelVisual(sceneBinding.rearLeftWheelCollider, sceneBinding.rearLeftWheelVisual);
        SyncWheelVisual(sceneBinding.rearRightWheelCollider, sceneBinding.rearRightWheelVisual);
    }

    private void SyncWheelVisual(WheelCollider collider, Transform visual)
    {
        if (collider == null || visual == null)
            return;

        if (!Application.isPlaying)
        {
            visual.SetPositionAndRotation(collider.transform.position, collider.transform.rotation);
            return;
        }

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        visual.SetPositionAndRotation(position, rotation);
    }

    private Rigidbody EnsureRigidbody(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (rb == null)
            rb = target.gameObject.AddComponent<Rigidbody>();

        return rb;
    }

    private static Transform EnsureNamedChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);

        if (existing != null)
            return existing;

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        return child.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyObjectSafe(parent.GetChild(i).gameObject);
    }

    private static void DestroyObjectSafe(UnityEngine.Object obj)
    {
        if (obj == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(obj);
        else
            Destroy(obj);
#else
        Destroy(obj);
#endif
    }

    private static GameObject InstantiateProfileObject(GameObject source, Transform parent, string instanceName)
    {
        GameObject instance = null;

#if UNITY_EDITOR
        if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(source))
        {
            UnityEngine.Object prefabObject = PrefabUtility.InstantiatePrefab(source, parent);
            instance = prefabObject as GameObject;
        }
#endif

        if (instance == null)
            instance = Instantiate(source, parent);

        instance.name = instanceName;
        return instance;
    }

    private static void RemovePhysicsFromVisualObject(GameObject visualObject)
    {
        if (visualObject == null)
            return;

        Collider[] colliders = visualObject.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        Rigidbody[] rigidbodies = visualObject.GetComponentsInChildren<Rigidbody>(true);

        for (int i = 0; i < rigidbodies.Length; i++)
            DestroyObjectSafe(rigidbodies[i]);
    }

    private void OnValidate()
    {
        RefreshDerivedCarValues();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(QUTMSVehicleSimulationProfile))]
public class QUTMSVehicleSimulationProfileEditor : Editor
{
    private const string ChassisPresetFolder = "Assets/QUTMS/Presets/Chassis";
    private const string WheelPackagePresetFolder = "Assets/QUTMS/Presets/WheelPackages";
    private const string SuspensionPresetFolder = "Assets/QUTMS/Presets/Suspension";
    private const string TrackConditionPresetFolder = "Assets/QUTMS/Presets/TrackConditions";
    private const string SensorPresetFolder = "Assets/QUTMS/Presets/Sensors";

    private bool showDerivedCarValues = true;
    private bool showChassis = true;
    private bool showWheelPackage = true;
    private bool showSuspension = true;
    private bool showTrackConditions = false;
    private bool showSensorConditions = false;
    private bool showMotionNoise = false;
    private bool showWheelSpeedNoise = false;
    private bool showInterfaceRuntime = false;

    public override void OnInspectorGUI()
    {
        QUTMSVehicleSimulationProfile values = (QUTMSVehicleSimulationProfile)target;

        serializedObject.Update();

        DrawCarSection(values);
        DrawChassisSection(values);
        DrawWheelPackageSection(values);
        DrawSuspensionSection(values);
        DrawTrackConditionsSection(values);
        DrawSensorsSection(values);
        DrawInterfaceRuntimeSection();

        if (serializedObject.ApplyModifiedProperties())
        {
            values.RefreshDerivedCarValues();
            EditorUtility.SetDirty(values);
        }
    }

    private void DrawCarSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(4.0f);
        EditorGUILayout.LabelField("Car", EditorStyles.boldLabel);

        DrawChassisSelector(values);
        DrawWheelPackageSelector(values);
        DrawSuspensionSelector(values);

        EditorGUILayout.Space(8.0f);

        if (GUILayout.Button("Generate / Update Full Car", GUILayout.Height(30.0f)))
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(values, "Generate / Update Full Car");
            values.GenerateOrUpdateFullCar();
            EditorUtility.SetDirty(values);
        }

        EditorGUILayout.Space(6.0f);
        DrawDerivedFullCarValues();
    }

    private void DrawChassisSelector(QUTMSVehicleSimulationProfile values)
    {
        string[] names = GetPresetNames(ChassisPresetFolder, new[] { "QEV-3 Chassis" });
        int currentIndex = IndexOf(names, values.selectedChassisPresetName);
        int nextIndex = EditorGUILayout.Popup("Chassis Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(values, "Change Chassis Preset");
            values.selectedChassisPresetName = names[nextIndex];

            if (values.selectedChassisPresetName == "QEV-3 Chassis")
                values.chassis.SetQev3Defaults();
            else
                LoadChassisPreset(values, values.selectedChassisPresetName);

            values.RefreshDerivedCarValues();
            EditorUtility.SetDirty(values);
        }
    }

    private void DrawWheelPackageSelector(QUTMSVehicleSimulationProfile values)
    {
        string[] names = GetPresetNames(WheelPackagePresetFolder, new[] { "QUTMS Dry Wheel Package", "QUTMS Wet Wheel Package" });
        int currentIndex = IndexOf(names, values.selectedWheelPackagePresetName);
        int nextIndex = EditorGUILayout.Popup("Wheel Package Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(values, "Change Wheel Package Preset");
            values.selectedWheelPackagePresetName = names[nextIndex];

            if (values.selectedWheelPackagePresetName == "QUTMS Wet Wheel Package")
                values.wheelPackage.SetWetDefaults();
            else if (values.selectedWheelPackagePresetName == "QUTMS Dry Wheel Package")
                values.wheelPackage.SetDryDefaults();
            else
                LoadWheelPackagePreset(values, values.selectedWheelPackagePresetName);

            values.RefreshDerivedCarValues();
            EditorUtility.SetDirty(values);
        }
    }

    private void DrawSuspensionSelector(QUTMSVehicleSimulationProfile values)
    {
        string[] names = GetPresetNames(SuspensionPresetFolder, new[] { "QEV-3 Baseline Suspension" });
        int currentIndex = IndexOf(names, values.selectedSuspensionPresetName);
        int nextIndex = EditorGUILayout.Popup("Suspension Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(values, "Change Suspension Preset");
            values.selectedSuspensionPresetName = names[nextIndex];

            if (values.selectedSuspensionPresetName == "QEV-3 Baseline Suspension")
                values.suspension.SetBaselineDefaults();
            else
                LoadSuspensionPreset(values, values.selectedSuspensionPresetName);

            values.RefreshDerivedCarValues();
            EditorUtility.SetDirty(values);
        }
    }

    private void DrawDerivedFullCarValues()
    {
        showDerivedCarValues = EditorGUILayout.Foldout(showDerivedCarValues, "Derived Full-Car Values", true);

        if (!showDerivedCarValues)
            return;

        EditorGUI.indentLevel++;

        DrawDerivedFloat("useDerivedTotalMass", "totalMassKg", "Total Mass (kg)");
        DrawDerivedFloat("useDerivedWheelAssemblyTotalMass", "wheelAssemblyTotalMassKg", "Wheel Assembly Total Mass (kg)");
        DrawDerivedFloat("useDerivedWheelRadius", "wheelRadiusM", "Wheel Radius (m)");
        DrawDerivedFloat("useDerivedWheelWidth", "wheelWidthM", "Wheel Width (m)");
        DrawDerivedFloat("useDerivedWheelbase", "wheelbaseM", "Wheelbase (m)");
        DrawDerivedFloat("useDerivedFrontTrack", "frontTrackM", "Front Track (m)");
        DrawDerivedFloat("useDerivedRearTrack", "rearTrackM", "Rear Track (m)");
        DrawDerivedFloat("useDerivedYawInertiaIzz", "yawInertiaIzzKgM2", "Yaw Inertia Izz (kg·m²)");
        DrawDerivedFloat("useDerivedSteeringLimit", "steeringLimitRad", "Steering Limit (rad)");
        DrawDerivedFloat("useDerivedFrontWeightFraction", "frontWeightFraction", "Front Weight Fraction");
        DrawDerivedFloat("useDerivedDragCoefficient", "dragCoefficient", "Drag Coefficient");
        DrawDerivedFloat("useDerivedDownforceCoefficient", "downforceCoefficient", "Downforce Coefficient");
        DrawDerivedFloat("useDerivedMaxAcceleration", "maxAccelerationMps2", "Maximum Acceleration (m/s²)");
        DrawDerivedFloat("useDerivedMinAcceleration", "minAccelerationMps2", "Minimum Acceleration (m/s²)");
        DrawDerivedFloat("useDerivedMaxVelocity", "maxVelocityMps", "Maximum Velocity (m/s)");
        DrawDerivedFloat("useDerivedSuspensionDistance", "suspensionDistanceM", "Suspension Distance (m)");
        DrawDerivedFloat("useDerivedSpringStrength", "springStrengthNPerM", "Spring Strength (N/m)");
        DrawDerivedFloat("useDerivedDamperStrength", "damperStrengthNsPerM", "Damper Strength (N·s/m)");

        EditorGUI.indentLevel--;
    }

    private void DrawDerivedFloat(string derivedBoolName, string valueName, string label)
    {
        SerializedProperty root = serializedObject.FindProperty("car");

        if (root == null)
            return;

        SerializedProperty useDerived = root.FindPropertyRelative(derivedBoolName);
        SerializedProperty value = root.FindPropertyRelative(valueName);

        if (useDerived == null || value == null)
            return;

        GUILayout.BeginHorizontal();

        useDerived.boolValue = EditorGUILayout.ToggleLeft("Derived", useDerived.boolValue, GUILayout.Width(78.0f));

        using (new EditorGUI.DisabledScope(useDerived.boolValue))
        {
            EditorGUILayout.PropertyField(value, new GUIContent(label));
        }

        GUILayout.EndHorizontal();
    }

    private void DrawChassisSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Chassis", EditorStyles.boldLabel);

        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("chassis"), "Chassis Values", ref showChassis);

        if (GUILayout.Button("Save As New Chassis Preset"))
            QUTMSChassisPresetSaveWindow.Open(values);
    }

    private void DrawWheelPackageSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Wheel Package", EditorStyles.boldLabel);

        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("wheelPackage"), "Wheel Package Values", ref showWheelPackage);

        if (GUILayout.Button("Save As New Wheel Package Preset"))
            QUTMSWheelPackagePresetSaveWindow.Open(values);
    }

    private void DrawSuspensionSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Suspension", EditorStyles.boldLabel);

        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("suspension"), "Suspension Values", ref showSuspension);

        if (GUILayout.Button("Save As New Suspension Preset"))
            QUTMSSuspensionPresetSaveWindow.Open(values);
    }

    private void DrawTrackConditionsSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Track Conditions", EditorStyles.boldLabel);

        string[] names = GetPresetNames(TrackConditionPresetFolder, new[] { "Dry", "Wet" });
        int currentIndex = IndexOf(names, values.selectedTrackConditionPresetName);
        int nextIndex = EditorGUILayout.Popup("Track Condition Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(values, "Change Track Condition Preset");
            values.selectedTrackConditionPresetName = names[nextIndex];

            if (values.selectedTrackConditionPresetName == "Wet")
                values.trackConditions.SetWetDefaults();
            else if (values.selectedTrackConditionPresetName == "Dry")
                values.trackConditions.SetDryDefaults();
            else
                LoadTrackConditionPreset(values, values.selectedTrackConditionPresetName);

            EditorUtility.SetDirty(values);
        }

        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("trackConditions"), "Conditions", ref showTrackConditions);

        if (GUILayout.Button("Save As New Track Condition Preset"))
            QUTMSTrackConditionPresetSaveWindow.Open(values);
    }

    private void DrawSensorsSection(QUTMSVehicleSimulationProfile values)
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Sensors", EditorStyles.boldLabel);

        string[] names = GetPresetNames(SensorPresetFolder, new[] { "Default Sensors" });
        int currentIndex = IndexOf(names, values.selectedSensorPresetName);
        int nextIndex = EditorGUILayout.Popup("Sensor Preset", currentIndex, names);

        if (nextIndex != currentIndex)
        {
            Undo.RecordObject(values, "Change Sensor Preset");
            values.selectedSensorPresetName = names[nextIndex];

            if (values.selectedSensorPresetName == "Default Sensors")
                values.ResetSelectedSensorPreset();
            else
                LoadSensorPreset(values, values.selectedSensorPresetName);

            EditorUtility.SetDirty(values);
        }

        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("sensorConditions"), "Sensor Conditions", ref showSensorConditions);
        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("motionNoise"), "Motion Noise", ref showMotionNoise);
        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("wheelSpeedNoise"), "Wheel Speed Noise", ref showWheelSpeedNoise);

        if (GUILayout.Button("Save As New Sensor Preset"))
            QUTMSSensorPresetSaveWindow.Open(values);
    }

    private void DrawInterfaceRuntimeSection()
    {
        EditorGUILayout.Space(12.0f);
        EditorGUILayout.LabelField("Interface / Runtime", EditorStyles.boldLabel);
        DrawChildFieldsWithoutRootLabel(serializedObject.FindProperty("interfaceRuntime"), "Runtime Values", ref showInterfaceRuntime);
    }

    private void DrawChildFieldsWithoutRootLabel(SerializedProperty property, string foldoutName, ref bool show)
    {
        if (property == null)
            return;

        show = EditorGUILayout.Foldout(show, foldoutName, true);

        if (!show)
            return;

        EditorGUI.indentLevel++;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }

        EditorGUI.indentLevel--;
    }

    private static string[] GetPresetNames(string folder, string[] builtIns)
    {
        List<string> names = new List<string>();

        for (int i = 0; i < builtIns.Length; i++)
        {
            if (!names.Contains(builtIns[i]))
                names.Add(builtIns[i]);
        }

        if (AssetDatabase.IsValidFolder(folder))
        {
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string name = Path.GetFileNameWithoutExtension(path);

                if (!string.IsNullOrWhiteSpace(name) && !names.Contains(name))
                    names.Add(name);
            }
        }

        return names.ToArray();
    }

    private static int IndexOf(string[] names, string selected)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == selected)
                return i;
        }

        return 0;
    }

    private static string PresetPath(string folder, string presetName)
    {
        return folder + "/" + SanitizeFileName(presetName) + ".json";
    }

    private static string SanitizeFileName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Unnamed Preset";

        string safe = raw;

        foreach (char c in Path.GetInvalidFileNameChars())
            safe = safe.Replace(c, '_');

        safe = safe.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
        return safe;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');

        if (parts.Length == 0)
            return;

        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static string GetAssetPath(GameObject asset)
    {
        if (asset == null)
            return "";

        return AssetDatabase.GetAssetPath(asset);
    }

    private static GameObject LoadGameObjectAtPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void LoadChassisPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        string path = PresetPath(ChassisPresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        ChassisPresetDisk loaded = JsonUtility.FromJson<ChassisPresetDisk>(json);

        if (loaded == null)
            return;

        values.chassis.presetName = loaded.presetName;
        values.chassis.chassisModelPrefab = LoadGameObjectAtPath(loaded.chassisModelPrefabPath);
        values.chassis.chassisColliderBox = LoadGameObjectAtPath(loaded.chassisColliderBoxPath);
        values.chassis.chassisMassKg = loaded.chassisMassKg;
        values.chassis.gravityMps2 = loaded.gravityMps2;
        values.chassis.yawInertiaIzzKgM2 = loaded.yawInertiaIzzKgM2;
        values.chassis.frontWeightFraction = loaded.frontWeightFraction;
        values.chassis.chassisWheelbaseM = loaded.chassisWheelbaseM;
        values.chassis.chassisFrontTrackM = loaded.chassisFrontTrackM;
        values.chassis.chassisRearTrackM = loaded.chassisRearTrackM;
        values.chassis.dragCoefficient = loaded.dragCoefficient;
        values.chassis.downforceCoefficient = loaded.downforceCoefficient;
        values.chassis.minSteeringRad = loaded.minSteeringRad;
        values.chassis.maxSteeringRad = loaded.maxSteeringRad;
        values.chassis.steeringLockTimeS = loaded.steeringLockTimeS;
        values.chassis.steeringCommandScale = loaded.steeringCommandScale;
        values.chassis.steeringPublishScale = loaded.steeringPublishScale;
        values.chassis.maxAccelerationMps2 = loaded.maxAccelerationMps2;
        values.chassis.minAccelerationMps2 = loaded.minAccelerationMps2;
        values.chassis.maxVelocityMps = loaded.maxVelocityMps;
        values.chassis.minVelocityMps = loaded.minVelocityMps;

        values.selectedChassisPresetName = values.chassis.presetName;
    }

    public static void SaveChassisPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        if (values == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(ChassisPresetFolder);

        ChassisPresetDisk save = new ChassisPresetDisk
        {
            presetName = presetName,
            chassisModelPrefabPath = GetAssetPath(values.chassis.chassisModelPrefab),
            chassisColliderBoxPath = GetAssetPath(values.chassis.chassisColliderBox),
            chassisMassKg = values.chassis.chassisMassKg,
            gravityMps2 = values.chassis.gravityMps2,
            yawInertiaIzzKgM2 = values.chassis.yawInertiaIzzKgM2,
            frontWeightFraction = values.chassis.frontWeightFraction,
            chassisWheelbaseM = values.chassis.chassisWheelbaseM,
            chassisFrontTrackM = values.chassis.chassisFrontTrackM,
            chassisRearTrackM = values.chassis.chassisRearTrackM,
            dragCoefficient = values.chassis.dragCoefficient,
            downforceCoefficient = values.chassis.downforceCoefficient,
            minSteeringRad = values.chassis.minSteeringRad,
            maxSteeringRad = values.chassis.maxSteeringRad,
            steeringLockTimeS = values.chassis.steeringLockTimeS,
            steeringCommandScale = values.chassis.steeringCommandScale,
            steeringPublishScale = values.chassis.steeringPublishScale,
            maxAccelerationMps2 = values.chassis.maxAccelerationMps2,
            minAccelerationMps2 = values.chassis.minAccelerationMps2,
            maxVelocityMps = values.chassis.maxVelocityMps,
            minVelocityMps = values.chassis.minVelocityMps
        };

        File.WriteAllText(PresetPath(ChassisPresetFolder, presetName), JsonUtility.ToJson(save, true));

        values.selectedChassisPresetName = presetName;
        values.chassis.presetName = presetName;
        values.RefreshDerivedCarValues();

        EditorUtility.SetDirty(values);
        AssetDatabase.Refresh();
    }

    private static void LoadWheelPackagePreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        string path = PresetPath(WheelPackagePresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        WheelPackagePresetDisk loaded = JsonUtility.FromJson<WheelPackagePresetDisk>(json);

        if (loaded == null)
            return;

        values.wheelPackage.presetName = loaded.presetName;
        values.wheelPackage.frontLeftWheel = LoadGameObjectAtPath(loaded.frontLeftWheelPath);
        values.wheelPackage.frontRightWheel = LoadGameObjectAtPath(loaded.frontRightWheelPath);
        values.wheelPackage.rearLeftWheel = LoadGameObjectAtPath(loaded.rearLeftWheelPath);
        values.wheelPackage.rearRightWheel = LoadGameObjectAtPath(loaded.rearRightWheelPath);

        values.wheelPackage.wheelAssemblyMassKg = loaded.wheelAssemblyMassKg;
        values.wheelPackage.radiusM = loaded.radiusM;
        values.wheelPackage.widthM = loaded.widthM;

        values.wheelPackage.tyreCoefficient = loaded.tyreCoefficient;
        values.wheelPackage.pacejkaB = loaded.pacejkaB;
        values.wheelPackage.pacejkaC = loaded.pacejkaC;
        values.wheelPackage.pacejkaD = loaded.pacejkaD;
        values.wheelPackage.pacejkaE = loaded.pacejkaE;

        values.wheelPackage.autoScaleVisualMeshToTyreRadius = loaded.autoScaleVisualMeshToTyreRadius;
        values.wheelPackage.visualMeshNativeRadiusM = loaded.visualMeshNativeRadiusM;
        values.wheelPackage.visualMeshScaleMultiplier = loaded.visualMeshScaleMultiplier;
        values.wheelPackage.scaleModelCentreOffsetWithVisualScale = loaded.scaleModelCentreOffsetWithVisualScale;

        values.wheelPackage.modelCentreOffsetM = loaded.modelCentreOffsetM;
        values.wheelPackage.modelRotationOffsetDeg = loaded.modelRotationOffsetDeg;

        values.wheelPackage.NormalizeVisualSettings();

        values.selectedWheelPackagePresetName = values.wheelPackage.presetName;
    }

    public static void SaveWheelPackagePreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        if (values == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(WheelPackagePresetFolder);

        values.wheelPackage.NormalizeVisualSettings();

        WheelPackagePresetDisk save = new WheelPackagePresetDisk
        {
            presetName = presetName,

            frontLeftWheelPath = GetAssetPath(values.wheelPackage.frontLeftWheel),
            frontRightWheelPath = GetAssetPath(values.wheelPackage.frontRightWheel),
            rearLeftWheelPath = GetAssetPath(values.wheelPackage.rearLeftWheel),
            rearRightWheelPath = GetAssetPath(values.wheelPackage.rearRightWheel),

            wheelAssemblyMassKg = values.wheelPackage.wheelAssemblyMassKg,
            radiusM = values.wheelPackage.radiusM,
            widthM = values.wheelPackage.widthM,

            tyreCoefficient = values.wheelPackage.tyreCoefficient,
            pacejkaB = values.wheelPackage.pacejkaB,
            pacejkaC = values.wheelPackage.pacejkaC,
            pacejkaD = values.wheelPackage.pacejkaD,
            pacejkaE = values.wheelPackage.pacejkaE,

            autoScaleVisualMeshToTyreRadius = values.wheelPackage.autoScaleVisualMeshToTyreRadius,
            visualMeshNativeRadiusM = values.wheelPackage.visualMeshNativeRadiusM,
            visualMeshScaleMultiplier = values.wheelPackage.visualMeshScaleMultiplier,
            scaleModelCentreOffsetWithVisualScale = values.wheelPackage.scaleModelCentreOffsetWithVisualScale,

            modelCentreOffsetM = values.wheelPackage.modelCentreOffsetM,
            modelRotationOffsetDeg = values.wheelPackage.modelRotationOffsetDeg
        };

        File.WriteAllText(PresetPath(WheelPackagePresetFolder, presetName), JsonUtility.ToJson(save, true));

        values.selectedWheelPackagePresetName = presetName;
        values.wheelPackage.presetName = presetName;
        values.RefreshDerivedCarValues();

        EditorUtility.SetDirty(values);
        AssetDatabase.Refresh();
    }

    private static void LoadSuspensionPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        string path = PresetPath(SuspensionPresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        QUTMSVehicleSimulationProfile.SuspensionPresetValues loaded =
            JsonUtility.FromJson<QUTMSVehicleSimulationProfile.SuspensionPresetValues>(json);

        if (loaded == null)
            return;

        values.suspension.CopyFrom(loaded);
        values.selectedSuspensionPresetName = values.suspension.presetName;
    }

    public static void SaveSuspensionPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        if (values == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(SuspensionPresetFolder);

        QUTMSVehicleSimulationProfile.SuspensionPresetValues save =
            new QUTMSVehicleSimulationProfile.SuspensionPresetValues();

        save.CopyFrom(values.suspension);
        save.presetName = presetName;

        File.WriteAllText(PresetPath(SuspensionPresetFolder, presetName), JsonUtility.ToJson(save, true));

        values.selectedSuspensionPresetName = presetName;
        values.suspension.presetName = presetName;
        values.RefreshDerivedCarValues();

        EditorUtility.SetDirty(values);
        AssetDatabase.Refresh();
    }

    private static void LoadTrackConditionPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        string path = PresetPath(TrackConditionPresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        QUTMSVehicleSimulationProfile.TrackConditionPresetValues loaded =
            JsonUtility.FromJson<QUTMSVehicleSimulationProfile.TrackConditionPresetValues>(json);

        if (loaded == null)
            return;

        values.trackConditions.CopyFrom(loaded);
        values.selectedTrackConditionPresetName = values.trackConditions.presetName;
    }

    public static void SaveTrackConditionPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        if (values == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(TrackConditionPresetFolder);

        QUTMSVehicleSimulationProfile.TrackConditionPresetValues save =
            new QUTMSVehicleSimulationProfile.TrackConditionPresetValues();

        save.CopyFrom(values.trackConditions);
        save.presetName = presetName;

        File.WriteAllText(PresetPath(TrackConditionPresetFolder, presetName), JsonUtility.ToJson(save, true));

        values.selectedTrackConditionPresetName = presetName;
        values.trackConditions.presetName = presetName;

        EditorUtility.SetDirty(values);
        AssetDatabase.Refresh();
    }

    private static void LoadSensorPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        string path = PresetPath(SensorPresetFolder, presetName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        SensorPresetDisk loaded = JsonUtility.FromJson<SensorPresetDisk>(json);

        if (loaded == null)
            return;

        values.selectedSensorPresetName = presetName;
        values.sensorConditions.CopyFrom(loaded.sensorConditions);
        values.motionNoise.CopyFrom(loaded.motionNoise);
        values.wheelSpeedNoise.CopyFrom(loaded.wheelSpeedNoise);
        values.sensorConditions.presetName = presetName;
    }

    public static void SaveSensorPreset(QUTMSVehicleSimulationProfile values, string presetName)
    {
        if (values == null || string.IsNullOrWhiteSpace(presetName))
            return;

        EnsureFolder(SensorPresetFolder);

        SensorPresetDisk save = new SensorPresetDisk
        {
            presetName = presetName,
            sensorConditions = new QUTMSVehicleSimulationProfile.SensorConditionValues(),
            motionNoise = new QUTMSVehicleSimulationProfile.MotionNoiseValues(),
            wheelSpeedNoise = new QUTMSVehicleSimulationProfile.WheelSpeedNoiseValues()
        };

        save.sensorConditions.CopyFrom(values.sensorConditions);
        save.sensorConditions.presetName = presetName;
        save.motionNoise.CopyFrom(values.motionNoise);
        save.wheelSpeedNoise.CopyFrom(values.wheelSpeedNoise);

        File.WriteAllText(PresetPath(SensorPresetFolder, presetName), JsonUtility.ToJson(save, true));

        values.selectedSensorPresetName = presetName;
        values.sensorConditions.presetName = presetName;

        EditorUtility.SetDirty(values);
        AssetDatabase.Refresh();
    }

    [Serializable]
    private class ChassisPresetDisk
    {
        public string presetName;
        public string chassisModelPrefabPath;
        public string chassisColliderBoxPath;

        public float chassisMassKg;
        public float gravityMps2;
        public float yawInertiaIzzKgM2;
        public float frontWeightFraction;
        public float chassisWheelbaseM;
        public float chassisFrontTrackM;
        public float chassisRearTrackM;
        public float dragCoefficient;
        public float downforceCoefficient;
        public float minSteeringRad;
        public float maxSteeringRad;
        public float steeringLockTimeS;
        public float steeringCommandScale;
        public float steeringPublishScale;
        public float maxAccelerationMps2;
        public float minAccelerationMps2;
        public float maxVelocityMps;
        public float minVelocityMps;
    }

    [Serializable]
    private class WheelPackagePresetDisk
    {
        public string presetName;

        public string frontLeftWheelPath;
        public string frontRightWheelPath;
        public string rearLeftWheelPath;
        public string rearRightWheelPath;

        public float wheelAssemblyMassKg;
        public float radiusM;
        public float widthM;

        public float tyreCoefficient;
        public float pacejkaB;
        public float pacejkaC;
        public float pacejkaD;
        public float pacejkaE;

        public bool autoScaleVisualMeshToTyreRadius;
        public float visualMeshNativeRadiusM;
        public float visualMeshScaleMultiplier;
        public bool scaleModelCentreOffsetWithVisualScale;

        public Vector3 modelCentreOffsetM;
        public Vector3 modelRotationOffsetDeg;
    }

    [Serializable]
    private class SensorPresetDisk
    {
        public string presetName;
        public QUTMSVehicleSimulationProfile.SensorConditionValues sensorConditions;
        public QUTMSVehicleSimulationProfile.MotionNoiseValues motionNoise;
        public QUTMSVehicleSimulationProfile.WheelSpeedNoiseValues wheelSpeedNoise;
    }
}

public class QUTMSChassisPresetSaveWindow : EditorWindow
{
    private QUTMSVehicleSimulationProfile targetValues;
    private string presetName = "";

    public static void Open(QUTMSVehicleSimulationProfile values)
    {
        QUTMSChassisPresetSaveWindow window = CreateInstance<QUTMSChassisPresetSaveWindow>();
        window.titleContent = new GUIContent("Save Chassis Preset");
        window.targetValues = values;
        window.presetName = values != null && values.chassis != null ? values.chassis.presetName : "New Chassis Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Chassis Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetValues != null)
            {
                Undo.RecordObject(targetValues, "Save Chassis Preset");
                QUTMSVehicleSimulationProfileEditor.SaveChassisPreset(targetValues, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}

public class QUTMSWheelPackagePresetSaveWindow : EditorWindow
{
    private QUTMSVehicleSimulationProfile targetValues;
    private string presetName = "";

    public static void Open(QUTMSVehicleSimulationProfile values)
    {
        QUTMSWheelPackagePresetSaveWindow window = CreateInstance<QUTMSWheelPackagePresetSaveWindow>();
        window.titleContent = new GUIContent("Save Wheel Package Preset");
        window.targetValues = values;
        window.presetName = values != null && values.wheelPackage != null ? values.wheelPackage.presetName : "New Wheel Package Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Wheel Package Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetValues != null)
            {
                Undo.RecordObject(targetValues, "Save Wheel Package Preset");
                QUTMSVehicleSimulationProfileEditor.SaveWheelPackagePreset(targetValues, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}

public class QUTMSSuspensionPresetSaveWindow : EditorWindow
{
    private QUTMSVehicleSimulationProfile targetValues;
    private string presetName = "";

    public static void Open(QUTMSVehicleSimulationProfile values)
    {
        QUTMSSuspensionPresetSaveWindow window = CreateInstance<QUTMSSuspensionPresetSaveWindow>();
        window.titleContent = new GUIContent("Save Suspension Preset");
        window.targetValues = values;
        window.presetName = values != null && values.suspension != null ? values.suspension.presetName : "New Suspension Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Suspension Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetValues != null)
            {
                Undo.RecordObject(targetValues, "Save Suspension Preset");
                QUTMSVehicleSimulationProfileEditor.SaveSuspensionPreset(targetValues, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}

public class QUTMSTrackConditionPresetSaveWindow : EditorWindow
{
    private QUTMSVehicleSimulationProfile targetValues;
    private string presetName = "";

    public static void Open(QUTMSVehicleSimulationProfile values)
    {
        QUTMSTrackConditionPresetSaveWindow window = CreateInstance<QUTMSTrackConditionPresetSaveWindow>();
        window.titleContent = new GUIContent("Save Track Condition Preset");
        window.targetValues = values;
        window.presetName = values != null && values.trackConditions != null ? values.trackConditions.presetName : "New Track Condition Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Track Condition Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetValues != null)
            {
                Undo.RecordObject(targetValues, "Save Track Condition Preset");
                QUTMSVehicleSimulationProfileEditor.SaveTrackConditionPreset(targetValues, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}

public class QUTMSSensorPresetSaveWindow : EditorWindow
{
    private QUTMSVehicleSimulationProfile targetValues;
    private string presetName = "";

    public static void Open(QUTMSVehicleSimulationProfile values)
    {
        QUTMSSensorPresetSaveWindow window = CreateInstance<QUTMSSensorPresetSaveWindow>();
        window.titleContent = new GUIContent("Save Sensor Preset");
        window.targetValues = values;
        window.presetName = values != null && values.sensorConditions != null ? values.sensorConditions.presetName : "New Sensor Preset";
        window.minSize = new Vector2(420.0f, 110.0f);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save As New Sensor Preset", EditorStyles.boldLabel);
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save As New"))
        {
            if (targetValues != null)
            {
                Undo.RecordObject(targetValues, "Save Sensor Preset");
                QUTMSVehicleSimulationProfileEditor.SaveSensorPreset(targetValues, presetName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}
#endif