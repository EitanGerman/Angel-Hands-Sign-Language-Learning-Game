using System;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.GameManager.StateMachine
{
    public class StateMachine<T> : MonoBehaviour
    {
        #region PublicFields
        public static StateMachine<T> stateMachine;

        #endregion PublicFields
        //Dictionary<>


        #region PrivateFields
        private List<State<T>> states;
        #endregion PrivateFields
        // Start is called before the first frame update
        void Start()
        {
            LoadStateMachineFromConfig();
        }

        public static StateMachine<T> Instance
        {
            get
            {
                if (stateMachine == null)
                    stateMachine = new StateMachine<T>();
                return stateMachine;
            }
        }

        private StateMachine()
        {
            stateMachine = new StateMachine<T>();
            LoadStateMachineFromConfig();
        }


        private void LoadStateMachineFromConfig()
        {

            throw new NotImplementedException();
        }


        public void NextState()
        {

            throw new NotImplementedException();
        }

    }
}
