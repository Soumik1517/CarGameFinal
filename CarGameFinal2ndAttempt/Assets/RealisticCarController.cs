using UnityEngine;

public class RealisticCarController : MonoBehaviour
{
    [Header("Wheels")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;

    [Header("Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 3000f;
    public float downforce = 100f;

    [Header("Boost & Drift")]
    public ParticleSystem boosterParticles;
    public ParticleSystem DriftParticle;

    [Header("Engine Sounds")]
    public AudioSource startUpSound;
    public AudioSource engineLowOn;
    public AudioSource engineLowOff;
    public AudioSource engineHighOn;
    public AudioSource engineHighOff;

    [Header("Effects Sounds")]
    public AudioSource brakeSound;
    public AudioSource driftSound;

    private float horizontalInput;
    private float verticalInput;
    private float currentMotorTorque;
    private float currentGrip = 1f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.9f, 0);
        SetupFriction(frontLeft);
        SetupFriction(frontRight);
        SetupFriction(rearLeft);
        SetupFriction(rearRight);

        if (boosterParticles != null) boosterParticles.Stop();
        if (startUpSound != null) startUpSound.Play();

        if (engineLowOn != null) engineLowOn.Play();
        if (engineLowOff != null) engineLowOff.Play();
        if (engineHighOn != null) engineHighOn.Play();
        if (engineHighOff != null) engineHighOff.Play();

        if (brakeSound != null) brakeSound.Stop();
        if (driftSound != null) driftSound.Stop();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    void ApplySteering()
    {
        float speed = rb.velocity.magnitude * 3.6f; // Convert m/s to km/h
    float steerLimit = maxSteerAngle;

    // Reduce steering angle as speed increases (prevents flipping)
    if (speed > 50f)
        steerLimit = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.4f, (speed - 10f) / 250f);

    float baseSteer = steerLimit * horizontalInput;

    // Countersteer behavior during drift
    Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
    float slipAngle = Mathf.Atan2(localVel.x, Mathf.Abs(localVel.z)) * Mathf.Rad2Deg;

    float counterSteer = 0f;
    if (rb.velocity.magnitude > 10f && Mathf.Abs(slipAngle) > 5f)
        counterSteer = -slipAngle * 0.05f;

    float finalSteer = baseSteer + counterSteer;
    frontLeft.steerAngle = finalSteer;
    frontRight.steerAngle = finalSteer;
    }

    void ApplyMotor()
    {
        float speed = rb.velocity.magnitude;
        float torqueMultiplier = Mathf.Clamp01(1f - (speed / 100f));
        float targetTorque = verticalInput * maxMotorTorque * torqueMultiplier;
        currentMotorTorque = Mathf.Lerp(currentMotorTorque, targetTorque, Time.deltaTime * 5f);

        frontLeft.motorTorque = currentMotorTorque;
        frontRight.motorTorque = currentMotorTorque;
        rearLeft.motorTorque = currentMotorTorque;
        rearRight.motorTorque = currentMotorTorque;

        if (Input.GetKey(KeyCode.W))
        {
            if (boosterParticles != null && !boosterParticles.isPlaying)
                boosterParticles.Play();
        }
        else
        {
            if (boosterParticles != null && boosterParticles.isPlaying)
                boosterParticles.Stop();
        }

        UpdateEngineSound();
    }

    void UpdateEngineSound()
    {
        float speed = rb.velocity.magnitude;
        bool accelerating = verticalInput > 0.1f;

        float lowVol = Mathf.Clamp01(1f - speed / 50f);
        float highVol = Mathf.Clamp01(speed / 40f);
        float pitch = Mathf.Lerp(0.8f, 2.0f, speed / 100f);

        if (engineLowOn != null) engineLowOn.volume = accelerating ? lowVol : 0;
        if (engineHighOn != null) engineHighOn.volume = accelerating ? highVol : 0;
        if (engineLowOff != null) engineLowOff.volume = !accelerating ? lowVol : 0;
        if (engineHighOff != null) engineHighOff.volume = !accelerating ? highVol : 0;

        if (engineLowOn != null) engineLowOn.pitch = pitch;
        if (engineLowOff != null) engineLowOff.pitch = pitch;
        if (engineHighOn != null) engineHighOn.pitch = pitch;
        if (engineHighOff != null) engineHighOff.pitch = pitch;
    }

    void ApplyBrakes()
    {
        bool isHandbrake = Input.GetKey(KeyCode.Space);

        if (isHandbrake)
        {
            float deceleration = brakeForce * 0.25f * Time.fixedDeltaTime;
            Vector3 brakeForceVector = -rb.velocity.normalized * deceleration;
            rb.AddForce(brakeForceVector, ForceMode.Acceleration);

            rearLeft.brakeTorque = brakeForce * 0.4f;
            rearRight.brakeTorque = brakeForce * 0.4f;
            frontLeft.brakeTorque = 0f;
            frontRight.brakeTorque = 0f;
            currentGrip = Mathf.Lerp(currentGrip, 0.5f, Time.deltaTime * 5f);

            if (brakeSound != null && !brakeSound.isPlaying) brakeSound.Play();
        }
        else
        {
            rearLeft.brakeTorque = 0f;
            rearRight.brakeTorque = 0f;
            frontLeft.brakeTorque = 0f;
            frontRight.brakeTorque = 0f;
            currentGrip = Mathf.Lerp(currentGrip, 1f, Time.deltaTime * 3f);

            if (brakeSound != null && brakeSound.isPlaying) brakeSound.Stop();
        }

        ApplyDriftGrip(currentGrip);
    }

    void ApplyDriftGrip(float gripMultiplier)
    {
        SetWheelGrip(rearLeft, 1.2f * gripMultiplier, 1.5f * gripMultiplier);
        SetWheelGrip(rearRight, 1.2f * gripMultiplier, 1.5f * gripMultiplier);
    }

    void SetWheelGrip(WheelCollider wc, float forwardStiff, float sidewaysStiff)
    {
        WheelFrictionCurve forward = wc.forwardFriction;
        forward.stiffness = forwardStiff;
        wc.forwardFriction = forward;

        WheelFrictionCurve sideways = wc.sidewaysFriction;
        sideways.stiffness = sidewaysStiff;
        wc.sidewaysFriction = sideways;
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * rb.velocity.magnitude * downforce);
    }

    void SetupFriction(WheelCollider wc)
    {
        var forward = wc.forwardFriction;
        forward.stiffness = 9f;
        wc.forwardFriction = forward;

        var sideways = wc.sidewaysFriction;
        sideways.stiffness = 9f;
        wc.sidewaysFriction = sideways;
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

    void UpdateFrictionForDrift()
    {
        float speed = rb.velocity.magnitude;
        float steerInput = Mathf.Abs(horizontalInput);
        float driftFactor = Mathf.Clamp01(steerInput * (speed / 30f));
        float driftThreshold = 0.03f;

        float baseForward = 1.1f;
        float baseSideways = 1.4f;

        float newSideways = Mathf.Lerp(baseSideways, 0.9f, driftFactor);
        float newForward = Mathf.Lerp(baseForward, 0.9f, driftFactor * 0.5f);

        ApplyDriftFriction(frontLeft, newForward, newSideways);
        ApplyDriftFriction(frontRight, newForward, newSideways);
        ApplyDriftFriction(rearLeft, newForward, newSideways);
        ApplyDriftFriction(rearRight, newForward, newSideways);

        if (driftFactor > driftThreshold && 
    Input.GetKey(KeyCode.Space) && 
    (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            ApplyDriftFriction(frontLeft, newForward * 1.0f, newSideways * 8f);
            ApplyDriftFriction(frontRight, newForward * 1.0f, newSideways * 8f);

            if (DriftParticle != null && !DriftParticle.isPlaying)
                DriftParticle.Play();
            if (driftSound != null && !driftSound.isPlaying)
                driftSound.Play();
        }
        else
        {
            if (DriftParticle != null && DriftParticle.isPlaying)
                DriftParticle.Stop();
            if (driftSound != null && driftSound.isPlaying)
                driftSound.Stop();
        }
    }

    void ApplyDriftFriction(WheelCollider wc, float forwardStiff, float sidewaysStiff)
    {
        WheelFrictionCurve forward = wc.forwardFriction;
        forward.stiffness = forwardStiff;
        wc.forwardFriction = forward;

        WheelFrictionCurve sideways = wc.sidewaysFriction;
        sideways.stiffness = sidewaysStiff;
        wc.sidewaysFriction = sideways;
    }

    void AntiRollBar(WheelCollider wheelL, WheelCollider wheelR)
{
    WheelHit hit;
    float travelL = 1.0f;
    float travelR = 1.0f;

    bool groundedL = wheelL.GetGroundHit(out hit);
    if (groundedL) travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;

    bool groundedR = wheelR.GetGroundHit(out hit);
    if (groundedR) travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

    float antiRollForce = (travelL - travelR) * 5000f;

    if (groundedL)
        rb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
    if (groundedR)
        rb.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
}

    void FixedUpdate()
    {
        GetInput();
        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
        ApplyDownforce();
        UpdateFrictionForDrift();
        UpdateWheelPoses();
        AntiRollBar(frontLeft, frontRight);
        AntiRollBar(rearLeft, rearRight);
    }
}
