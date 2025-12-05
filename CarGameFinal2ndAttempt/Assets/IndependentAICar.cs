using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class IndependentAICar : MonoBehaviour
{
    [Header("Track Path")]
    public List<Transform> waypoints;
    public float waypointThreshold = 5f;

    [Header("AI Settings")]
    public float maxSpeed = 50f;
    public float maxSteerAngle = 30f;
    public float acceleration = 3f; // lower = slower acceleration
    public int difficulty = 2; // 1-easy, 2-medium, 3-hard

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 2f;
    public float minDistanceMoved = 2f;
    public float respawnCooldown = 1.5f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;

    private Rigidbody rb;
    private int currentWaypoint = 0;
    private float currentSpeed = 0f;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float respawnTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.7f, 0);

        lastPosition = transform.position;

        // Set max speed based on difficulty
        switch (difficulty)
        {
            case 1: maxSpeed = 600f; break;
            case 2: maxSpeed = 800f; break;
            case 3: maxSpeed = 1400f; break;
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 toTarget = target.position - transform.position;
        Vector3 localTarget = transform.InverseTransformPoint(target.position);

        // --- Steering ---
        float steerInput = Mathf.Clamp(localTarget.x / localTarget.magnitude, -1f, 1f);
        float steerAngle = steerInput * maxSteerAngle;
        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;

        // --- Throttle / Speed ---
        float angleToTarget = Vector3.Angle(transform.forward, toTarget);
        float slowdownFactor = Mathf.Pow(Mathf.Clamp01(1f - (angleToTarget / 90f)), 7f); // sharper slowdown
        float targetSpeed = maxSpeed * slowdownFactor;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);

        frontLeft.motorTorque = currentSpeed;
        frontRight.motorTorque = currentSpeed;
        rearLeft.motorTorque = currentSpeed;
        rearRight.motorTorque = currentSpeed;

        // --- Check waypoint reached ---
        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }

        // --- Face waypoint direction ---
        Vector3 lookDir = (target.position - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2f); // smooth rotation
        }

        // --- Update wheel positions ---
        UpdateWheelPoses();

        // --- Stuck detection ---
        stuckTimer += Time.fixedDeltaTime;

        if (respawnTimer > 0f)
        {
            respawnTimer -= Time.fixedDeltaTime;
            stuckTimer = 0f; // ignore stuck check during cooldown
        }
        else if (stuckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < minDistanceMoved)
            {
                RespawnAtNextWaypoint();
                respawnTimer = respawnCooldown;
            }

            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    void RespawnAtNextWaypoint()
    {
        Transform target = waypoints[currentWaypoint];
        transform.position = target.position + Vector3.up * 1f;
        transform.rotation = target.rotation;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
    }

    void UpdateWheelPoses()
    {
        UpdateWheelPose(frontLeft, frontLeftT);
        UpdateWheelPose(frontRight, frontRightT);
        UpdateWheelPose(rearLeft, rearLeftT);
        UpdateWheelPose(rearRight, rearRightT);
    }

    void UpdateWheelPose(WheelCollider col, Transform t)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion quat);
        t.position = pos;
        t.rotation = quat;
    }
}
