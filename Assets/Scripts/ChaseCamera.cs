using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 5f, -15f);
    public float positionSmoothing = 500f;
    public float rotationSmoothing = 500f;

    [Header("Feel")]
    public float lookAheadDistance = 1000f;
    public float speedFovBoost = 8f;
    public float baseFov = 60f;

    Camera cam;
    Rigidbody targetRb;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * positionSmoothing);

        Vector3 lookPoint = target.position + target.forward * lookAheadDistance;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationSmoothing);

        if (targetRb != null && cam != null)
        {
            float speedRatio = targetRb.linearVelocity.magnitude / 80f;
            float targetFov = baseFov + speedFovBoost * Mathf.Clamp01(speedRatio);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 3f);
        }
    }
}