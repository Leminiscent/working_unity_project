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
            if (CurrentState != null)
            {
                CurrentState.Execute();
            }
        }

        public void Push(State<T> newState)
        {
            if (newState == null)
            {
                Debug.LogWarning("Push() called with null state.");
                return;
            }

            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(_owner);
        }

        public void Pop()
        {
            if (StateStack.Count == 0)
            {
                Debug.LogWarning("Pop() called on empty stack.");
                return;
            }

            _ = StateStack.Pop();
            CurrentState.Exit();
            CurrentState = StateStack.Peek();
        }

        public void ChangeState(State<T> newState)
        {
            if (newState == null)
            {
                Debug.LogWarning("ChangeState() called with null state.");
                return;
            }

            if (CurrentState != null)
            {
                _ = StateStack.Pop();
                CurrentState.Exit();
            }
            StateStack.Push(newState);
            CurrentState = newState;
            CurrentState.Enter(_owner);
        }

        public IEnumerator PushAndWait(State<T> newState)
        {
            if (newState == null)
            {
                Debug.LogWarning("PushAndWait() called with null state.");
                yield break;
            }

            State<T> prevState = CurrentState;
            Push(newState);
            yield return new WaitUntil(() => CurrentState == prevState);
        }

        public State<T> GetPrevState()
        {
            if (StateStack.Count < 2)
            {
                Debug.LogWarning("GetPrevState() called with empty or single state stack.");
                return null;
            }

            return StateStack.ElementAt(1);
        }
    }
}