using UnityEngine;

namespace AI.FSM.States
{
    public class PursuitState : IState
    {
        private readonly EnemyAI _ai;
        private Transform _target;

        public PursuitState(EnemyAI ai) => _ai = ai;

        public void OnEnter()  { }
        public void OnUpdate() => _target = _ai.GetNearestPlayer();

        public void OnFixedUpdate()
        {
            if (_target != null)
                _ai.ThrustToward(_target.position);
        }

        public void OnExit() { }
    }
}
