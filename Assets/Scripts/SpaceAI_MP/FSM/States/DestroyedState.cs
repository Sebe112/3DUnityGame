using UnityEngine;

namespace AI.FSM.States
{
    public class DestroyedState : IState
    {
        private readonly EnemyAI _ai;
        public DestroyedState(EnemyAI ai) => _ai = ai;

        public void OnEnter()
        {
            _ai.rb.linearVelocity  = Vector3.zero;
            _ai.rb.angularVelocity = Vector3.zero;
            // Spawn networked explosion VFX here
            UnityEngine.Object.Destroy(_ai.gameObject, 2f);
        }

        public void OnUpdate()      { }
        public void OnFixedUpdate() { }
        public void OnExit()        { }
    }
}
