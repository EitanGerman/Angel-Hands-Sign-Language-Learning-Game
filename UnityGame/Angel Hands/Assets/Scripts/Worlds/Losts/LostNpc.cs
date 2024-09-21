using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.Animations.NPCs;

// non player character for Lost Level/World
namespace Assets.Scripts.Worlds.losts.lostNcp
{
    /*
    NPC in the lost level has to be positioned on the terrain
    NPC has detination object to walk to
    NPC can follow player   
    NPC can be interacted with by player
    */
    public class LostNpc : MonoBehaviour
{
        // npc id
        #region npc identity
        public string name;
        public int id;
        public GameObject destinationObject;

        #endregion

        // npc interaction with player
        #region status
        private GameObject player; //represent the actuall player in the game
        private bool haveReachedDestination = false;
        #region interaction
        private bool isFollowingPlayer = false;
        private bool wasInteractedByPlayer = false;
        private NavMeshAgent navMeshAgent; //AI Navigator
        private int interactionCount = 0;
        #endregion
        #endregion

        // identify palyer with Tag of Player
        private GameObject getPlayer()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            return player;
        }

        // set navMesh destination to be destinationObject
        public void walkToDestinationAlone()
        {
            if (destinationObject != null & navMeshAgent != null)
            {
                navMeshAgent.SetDestination(destinationObject.transform.position);
            }
            else
            {
                Debug.LogWarning("Destination object or NavMeshAgent is not set.");
            }
        }

        // return true if npc is close to player within 2.0 meters 
        public bool isCloseToPlayer()
        {
            GameObject player = getPlayer();
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                return distance <= 2.0f;
            }
            else
            {
                Debug.LogWarning("Player GameObject with tag 'Player' not found.");
                return false;
            }
        }

        // set navMesh destination to be Player
        public void followerPlayer()
        {
            GameObject player = getPlayer();
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(player.transform.position);
            }
            else
            {
                Debug.LogWarning("player or NavMeshAgent is not set.");
            }
        }

    }//class 
}//namespace

