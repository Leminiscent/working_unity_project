using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.StateMachine
{
    public class StateMachine<T>
    {
        private T _owner;

        public State<T> CurrentState { get; private set; }
        public Stack<State<T>> StateStack { get; private set; }

        public StateMachine(T owner)
        {
            _owner = owner;

            StateStack = new Stack<State<T>>();
        }

        public void Execute()
        {
            CurrentState?.Execute();
        }

        public void Push(State<T> newState)
        {
            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(_owner);
        }

        public void Pop()
        {
            StateStack.Pop();
            CurrentState.Exit();
            CurrentState = StateStack.Peek();
        }

        public void ChangeState(State<T> newState)
        {
            if (CurrentState != null)
            {
                StateStack.Pop();
                CurrentState.Exit();
            }
            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(_owner);
        }

        public IEnumerator PushAndWait(State<T> newState)
        {
            State<T> prevState = CurrentState;

            Push(newState);
            yield return new WaitUntil(() => CurrentState == prevState);
        }

        public State<T> GetPrevState()
        {
            return StateStack.ElementAt(1);
        }
    }
}