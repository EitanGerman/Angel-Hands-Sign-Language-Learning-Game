using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.Worlds.losts.lostNcp;

// non player character for Lost Level/World

namespace Assets.Scripts.Worlds.Losts
{
    /*
    NPC in the lost level has to be positioned on the terrain
    NPC has detination object to walk to
    NPC can follow player   
    NPC can be interacted with by player
    */
    public class LostsManager : MonoBehaviour
    {
        // npcs id
        #region world identity
        public string world_name="Losts";


        #endregion

        #region npcs
        public List<LostNpc> npcs = new List<LostNpc>();
        #endregion

    }//class 
}//namespace

