using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.CommonTypes;
using Assets.Scripts.Worlds.losts.lostNcp;
using Unity.VisualScripting;
using Assets.Scripts.Utills.Communication;
using System.Diagnostics.Tracing;


// non player character for Lost Level/World
namespace Assets.Scripts.Worlds.player{
    /*
    Player in the lost level has to be positioned on the terrain
    Player recive input 
    Player can interact with LostNpc and help them to reach destination
    Player can move around using AI Navigator
    */

    public class Player : MonoBehaviour
    {
        #region player words

        #region learned
        public Dictionary<string, Word> learned_words = new Dictionary<string, Word>(); // save data of all words learned and statics of each word
        #endregion

        #endregion

        #region player interactions
        public Dictionary<string,LostNpc> npcs_history = new Dictionary<string,LostNpc>(); // save data of all NPCs interacted with
        public GameObject current_npc; // current NPC player is interacting with
        public NavMeshAgent navMeshAgent; //AI Navigator
        #endregion

        #region input 
        public CommunicationType communicationType;
        public string input_source;   
        CommunicationManager communicationManager = new CommunicationManager();
        #endregion

        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            communicationManager.initiate(communicationType, input_source);
        }

        void Update() 
        { 
        Debug.Log("Player input: " + communicationManager.getOutput());
        }

        void OnDestroy()
        {
            communicationManager.destroy();
        }

        public void followNPC()
        {
            if (navMeshAgent != null && current_npc != null)
            {
                navMeshAgent.SetDestination(current_npc.transform.position);
            }
            else
            {
                Debug.LogWarning("player or NavMeshAgent is not set.");
            }
        }

    }

}