using AI.FSM;
using AI.FSM.States;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    /// <summary>
    /// Server-authoritative enemy AI.
    /// Only runs FSM and physics on the server — position synced to clients via
    /// NetworkTransform (add that component alongside this one).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class EnemyAI : NetworkBehaviour
    {
        [Header("Patrol")]
        public Transform[] waypoints;

        [Header("Detection")]
        public float detectionRange = 30f;
        public float loseRange      = 45f;

        [Header("Combat")]
        public float weaponRange  = 10f;
        public float fireCooldown = 1.5f;
        public float weaponDamage = 10f;

        [Header("Movement")]
        public float thrustForce = 15f;
        public float turnSpeed   = 5f;
        public float maxSpeed    = 12f;
        public float drag        = 1.5f;

        [Header("Debug")]
        public bool showGizmos = true;

        [HideInInspector] public Rigidbody rb;

        public string CurrentStateName { get; private set; }

        private StateMachine _fsm;

        private void Awake()
        {
            rb            = GetComponent<Rigidbody>();
            rb.linearDamping       = drag;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        public override void OnNetworkSpawn()
        {
            // Only the server runs the AI
            if (!IsServer) return;
            BuildFSM();
        }

        private void BuildFSM()
        {
            _fsm = new StateMachine();

            var patrol    = new PatrolState(this, waypoints);
            var pursuit   = new PursuitState(this);
            var combat    = new CombatState(this, fireCooldown, weaponDamage);
            var destroyed = new DestroyedState(this);

            _fsm.AddTransition(patrol,  pursuit,   () => DistToNearestPlayer() <= detectionRange);
            _fsm.AddTransition(pursuit, patrol,    () => DistToNearestPlayer() > loseRange);
            _fsm.AddTransition(pursuit, combat,    () => DistToNearestPlayer() <= weaponRange);
            _fsm.AddTransition(combat,  pursuit,   () => DistToNearestPlayer() > weaponRange);
            _fsm.AddAnyTransition(destroyed,       () => GetComponent<ShipHealth>()?.IsDead ?? false);

            _fsm.SetState(patrol);
        }

        private void Update()
        {
            if (!IsServer) return;
            CurrentStateName = _fsm.CurrentState?.GetType().Name ?? "None";
            _fsm.Tick();
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;
            _fsm.FixedTick();
        }

        // ── Movement helpers ────────────────────────────────────────────

        public void ThrustToward(Vector3 target)
        {
            var dir = (target - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                var targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
            }
            if (rb.linearVelocity.magnitude < maxSpeed)
                rb.AddForce(transform.forward * thrustForce, ForceMode.Force);
        }

        public void Brake() =>
            rb.AddForce(-rb.linearVelocity * drag, ForceMode.Force);

        // ── Player targeting ────────────────────────────────────────────

        /// <summary>Returns the nearest connected player's Transform, or null.</summary>
        public Transform GetNearestPlayer()
        {
            Transform nearest = null;
            float bestDist = float.MaxValue;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObj = client.PlayerObject;
                if (playerObj == null) continue;
                float d = Vector3.Distance(transform.position, playerObj.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest  = playerObj.transform;
                }
            }
            return nearest;
        }

        public float DistToNearestPlayer()
        {
            var t = GetNearestPlayer();
            return t != null ? Vector3.Distance(transform.position, t.position) : float.MaxValue;
        }

        // ── Gizmos ───────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = new Color(1, 0.5f, 0);
            Gizmos.DrawWireSphere(transform.position, loseRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, weaponRange);
        }
    }
}
