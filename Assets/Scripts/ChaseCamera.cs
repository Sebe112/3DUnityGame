using Unity.Netcode;
using UnityEngine;

public class ChaseCamera : NetworkBehaviour
{
    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 5f, -15f);
    public float positionSmoothing = 500f;
    public float rotationSmoothing = 500f;

    [Header("Feel")]
    public float lookAheadDistance = 1000f;
    public float speedFovBoost = 8f;
    public float baseFov = 60f;

    private Camera cam;
    private Rigidbody targetRb;
    private Transform target;

    public override void OnNetworkSpawn()
    {
        // Each camera finds its own parent ship — no manual target needed
        target = transform.parent;
        targetRb = target?.GetComponent<Rigidbody>();
        cam = GetComponent<Camera>();

        // Only activate for the owner
        gameObject.SetActive(IsOwner);

        var listener = GetComponent<AudioListener>();
        if (listener != null) listener.enabled = IsOwner;
    }

    private void LateUpdate()
    {
        if (!IsOwner || target == null) return;

        Vector3 desiredPos = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos,
            Time.deltaTime * positionSmoothing);

        Vector3 lookPoint = target.position + target.forward * lookAheadDistance;
        Quaternion desiredRot = Quaternion.LookRotation(
            lookPoint - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot,
            Time.deltaTime * rotationSmoothing);

        if (targetRb != null && cam != null)
        {
            float speedRatio = targetRb.linearVelocity.magnitude / 80f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                baseFov + speedFovBoost * Mathf.Clamp01(speedRatio),
                Time.deltaTime * 3f);
        }
    }
}