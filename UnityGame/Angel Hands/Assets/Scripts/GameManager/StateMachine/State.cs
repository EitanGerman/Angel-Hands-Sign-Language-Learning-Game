using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameManager.StateMachine.Enums;

namespace Assets.Scripts.GameManager.StateMachine
{
    public class State <T>
    {

        #region Constructors
        public State(T Name, List<T> PossibleStates)
        {
            name = Name;
            possibleStates = PossibleStates;
        }
        #endregion Constructors

        #region PublicFields

        #endregion PublicFields


        #region PrivateFields
        private T name;
        private List<T> possibleStates;
        #endregion PrivateFields


        #region Getters
        public T GetNextState(int nextState)
        {
            return possibleStates[nextState];
        }

        #endregion Getters
    }
}
