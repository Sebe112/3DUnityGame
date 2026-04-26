using Unity.Netcode;
using UnityEngine;

namespace AI
{
    /// <summary>Server-authoritative health for player ships.</summary>
    public class PlayerShipHealth : NetworkBehaviour
    {
        [SerializeField] private float maxShields = 75f;
        [SerializeField] private float maxHull    = 150f;

        public NetworkVariable<float> Shields = new(75f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> Hull    = new(150f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
            if (IsDead) Debug.Log($"{gameObject.name} destroyed!");
        }

        public void RepairShields(float amount)
        {
            if (!IsServer) return;
            Shields.Value = Mathf.Min(maxShields, Shields.Value + amount);
        }
    }
}
