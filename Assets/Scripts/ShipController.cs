using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class ShipController : NetworkBehaviour
{
    [Header("Thrust")]
    public float thrust = 40f;
    public float maxSpeed = 60f;
    public float minSpeed = 10f;
    public float boostMultiplier = 1.8f;

    [Header("Turning")]
    public float pitchSpeed = 70f;
    public float yawSpeed = 50f;
    public float rollSpeed = 110f;
    public float bankAmount = 25f;

    [Header("Feel")]
    public float turnSmoothing = 4f;

    private Rigidbody rb;
    private float currentPitch, currentYaw, currentRoll;

    private float _pitchInput, _yawInput, _rollInput;
    private bool _boosting, _braking;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 4f;
    }

    public override void OnNetworkSpawn()
    {
        var cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.gameObject.SetActive(IsOwner);

        var listener = GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = IsOwner;

        if (!IsOwner)
        {
            rb.isKinematic = true;
            // Let NetworkTransform move the rigidbody for non-owners
            var nt = GetComponent<NetworkTransform>();
            if (nt != null) nt.enabled = true;
        }
    }

    // Replace FixedUpdate with these two:
    private void Update()
    {
        if (!IsOwner) return;
        _pitchInput = Input.GetAxis("Vertical");
        _yawInput = Input.GetAxis("Horizontal");
        _rollInput = 0f;
        if (Input.GetKey(KeyCode.Q)) _rollInput += 1f;
        if (Input.GetKey(KeyCode.E)) _rollInput -= 1f;
        _boosting = Input.GetKey(KeyCode.LeftShift);
        _braking = Input.GetKey(KeyCode.LeftControl);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        currentPitch = Mathf.Lerp(currentPitch, _pitchInput, Time.fixedDeltaTime * turnSmoothing);
        currentYaw = Mathf.Lerp(currentYaw, _yawInput, Time.fixedDeltaTime * turnSmoothing);
        currentRoll = Mathf.Lerp(currentRoll, _rollInput, Time.fixedDeltaTime * turnSmoothing);

        float autoBank = -currentYaw * bankAmount;
        Quaternion deltaRot = Quaternion.Euler(
            -currentPitch * pitchSpeed * Time.fixedDeltaTime,
             currentYaw * yawSpeed * Time.fixedDeltaTime,
            (currentRoll * rollSpeed + autoBank) * Time.fixedDeltaTime
        );
        rb.MoveRotation(rb.rotation * deltaRot);

        float targetSpeed = _boosting ? maxSpeed * boostMultiplier
                          : _braking ? minSpeed
                          : Mathf.Lerp(minSpeed, maxSpeed, 0.7f);

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity,
            transform.forward * targetSpeed, Time.fixedDeltaTime * 2f);
    }
}