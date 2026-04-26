using System;
using System.Collections.Generic;

namespace AI.FSM
{
    public class StateMachine
    {
        private IState _currentState;
        private readonly Dictionary<Type, List<Transition>> _transitions = new();
        private List<Transition> _currentTransitions = new();
        private readonly List<Transition> _anyTransitions = new();
        private static readonly List<Transition> EmptyTransitions = new(0);

        public IState CurrentState => _currentState;

        public void Tick()
        {
            var transition = GetTriggeredTransition();
            if (transition != null) SetState(transition.To);
            _currentState?.OnUpdate();
        }

        public void FixedTick() => _currentState?.OnFixedUpdate();

        public void SetState(IState state)
        {
            if (state == _currentState) return;
            _currentState?.OnExit();
            _currentState = state;
            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            _currentTransitions ??= EmptyTransitions;
            _currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var list))
            {
                list = new List<Transition>();
                _transitions[from.GetType()] = list;
            }
            list.Add(new Transition(to, condition));
        }

        public void AddAnyTransition(IState to, Func<bool> condition) =>
            _anyTransitions.Add(new Transition(to, condition));

        private Transition GetTriggeredTransition()
        {
            foreach (var t in _anyTransitions)
                if (t.Condition()) return t;
            foreach (var t in _currentTransitions)
                if (t.Condition()) return t;
            return null;
        }

        private class Transition
        {
            public readonly IState To;
            public readonly Func<bool> Condition;
            public Transition(IState to, Func<bool> condition) { To = to; Condition = condition; }
        }
    }
}
