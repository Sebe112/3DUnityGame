using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Thrust")]
    public float thrust = 40f;
    public float maxSpeed = 60f;
    public float minSpeed = 10f;
    public float boostMultiplier = 1.8f;

    [Header("Turning (degrees per second)")]
    public float pitchSpeed = 70f;
    public float yawSpeed = 50f;
    public float rollSpeed = 110f;
    public float bankAmount = 25f; 

    [Header("Feel")]
    public float turnSmoothing = 4f;

    Rigidbody rb;
    float currentPitch, currentYaw, currentRoll;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;       // if on older Unity, use rb.drag
        rb.angularDamping = 4f;      // if on older Unity, use rb.angularDrag
    }

    void FixedUpdate()
    {
        float pitchInput = Input.GetAxis("Vertical");
        float yawInput = Input.GetAxis("Horizontal");
        float rollInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rollInput += 1f;
        if (Input.GetKey(KeyCode.E)) rollInput -= 1f;

        bool boosting = Input.GetKey(KeyCode.LeftShift);
        bool braking  = Input.GetKey(KeyCode.LeftControl);

        currentPitch = Mathf.Lerp(currentPitch, pitchInput, Time.fixedDeltaTime * turnSmoothing);
        currentYaw   = Mathf.Lerp(currentYaw,   yawInput,   Time.fixedDeltaTime * turnSmoothing);
        currentRoll  = Mathf.Lerp(currentRoll,  rollInput,  Time.fixedDeltaTime * turnSmoothing);

        float autoBank = -currentYaw * bankAmount;
        Quaternion deltaRot = Quaternion.Euler(
            -currentPitch * pitchSpeed * Time.fixedDeltaTime,
             currentYaw   * yawSpeed   * Time.fixedDeltaTime,
            (currentRoll * rollSpeed + autoBank) * Time.fixedDeltaTime
        );
        rb.MoveRotation(rb.rotation * deltaRot);

        float targetSpeed = boosting ? maxSpeed * boostMultiplier
                          : braking  ? minSpeed
                          : Mathf.Lerp(minSpeed, maxSpeed, 0.7f); // cruise speed

        Vector3 desiredVelocity = transform.forward * targetSpeed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.fixedDeltaTime * 2f);
    }
}
