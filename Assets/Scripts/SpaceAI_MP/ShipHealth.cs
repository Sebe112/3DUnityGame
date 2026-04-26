using Unity.Netcode;
using UnityEngine;

namespace AI
{
    /// <summary>Server-authoritative health for enemy ships.</summary>
    public class ShipHealth : NetworkBehaviour
    {
        [SerializeField] private float maxShields = 50f;
        [SerializeField] private float maxHull    = 100f;

        // NetworkVariables auto-sync to all clients
        public NetworkVariable<float> Shields = new(50f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> Hull    = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool IsDead => Hull.Value <= 0f;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            Shields.Value = maxShields;
            Hull.Value    = maxHull;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float amount)
        {
            if (IsDead) return;
            float shieldDmg = Mathf.Min(Shields.Value, amount);
            Shields.Value  -= shieldDmg;
            Hull.Value      = Mathf.Max(0, Hull.Value - (amount - shieldDmg));
        }
    }
}
