using Unity.Netcode;
using UnityEngine;

namespace AI
{
    /// <summary>
    /// Local-player ship controller.
    /// Input is only read on the owner client — movement is applied via ServerRpc
    /// so the server stays authoritative.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerShipController : NetworkBehaviour
    {
        [Header("Movement")]
        public float thrustForce = 20f;
        public float turnSpeed   = 6f;
        public float maxSpeed    = 15f;
        public float drag        = 1.5f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb            = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.linearDamping       = drag;
            _rb.constraints = RigidbodyConstraints.None;
        }

        private void Update()
        {
            if (!IsOwner) return;

            float pitch = -Input.GetAxis("Vertical");
            float yaw   =  Input.GetAxis("Horizontal");
            float roll  =  Input.GetKey(KeyCode.Q) ? -1f :
                           Input.GetKey(KeyCode.E) ?  1f : 0f;
            float thrust = Input.GetKey(KeyCode.Space) ? 1f : 0f;

            SendInputServerRpc(pitch, yaw, roll, thrust);
        }

        [ServerRpc]
        private void SendInputServerRpc(float pitch, float yaw, float roll, float thrust)
        {
            // Rotate
            transform.Rotate(pitch * turnSpeed * Time.fixedDeltaTime * 60f,
                             yaw   * turnSpeed * Time.fixedDeltaTime * 60f,
                             roll  * turnSpeed * Time.fixedDeltaTime * 60f);

            // Thrust
            if (thrust > 0 && _rb.linearVelocity.magnitude < maxSpeed)
                _rb.AddForce(transform.forward * thrustForce * thrust, ForceMode.Force);
        }
    }
}
