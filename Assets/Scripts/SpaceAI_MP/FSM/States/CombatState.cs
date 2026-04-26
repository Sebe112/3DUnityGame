using UnityEngine;

namespace AI.FSM.States
{
    public class CombatState : IState
    {
        private readonly EnemyAI _ai;
        private readonly float _cooldown;
        private readonly float _damage;
        private float _lastFireTime;
        private Transform _target;

        public CombatState(EnemyAI ai, float cooldown, float damage)
        {
            _ai       = ai;
            _cooldown = cooldown;
            _damage   = damage;
        }

        public void OnEnter() => _lastFireTime = -_cooldown;

        public void OnUpdate()
        {
            _target = _ai.GetNearestPlayer();

            if (Time.time - _lastFireTime >= _cooldown)
            {
                Fire();
                _lastFireTime = Time.time;
            }
        }

        public void OnFixedUpdate()
        {
            if (_target == null) return;
            float dist = Vector3.Distance(_ai.transform.position, _target.position);
            if (dist > _ai.weaponRange * 0.6f)
                _ai.ThrustToward(_target.position);
            else
                _ai.Brake();
        }

        public void OnExit() { }

        private void Fire()
        {
            if (_target == null) return;
            var health = _target.GetComponent<PlayerShipHealth>();
            if (health != null)
                health.TakeDamageServerRpc(_damage);
            Debug.Log($"{_ai.name} fired at {_target.name}");
            // Spawn networked projectile here
        }
    }
}
