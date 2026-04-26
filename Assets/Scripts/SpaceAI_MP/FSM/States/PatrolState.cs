using UnityEngine;

namespace AI.FSM.States
{
    public class PatrolState : IState
    {
        private readonly EnemyAI _ai;
        private readonly Transform[] _waypoints;
        private int _index;

        public PatrolState(EnemyAI ai, Transform[] waypoints)
        {
            _ai        = ai;
            _waypoints = waypoints;
        }

        public void OnEnter() => AdvanceWaypoint();
        public void OnUpdate() { }

        public void OnFixedUpdate()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            _ai.ThrustToward(_waypoints[_index].position);
            if (Vector3.Distance(_ai.transform.position, _waypoints[_index].position) < 2f)
                AdvanceWaypoint();
        }

        public void OnExit() { }

        private void AdvanceWaypoint()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            _index = (_index + 1) % _waypoints.Length;
        }
    }
}
