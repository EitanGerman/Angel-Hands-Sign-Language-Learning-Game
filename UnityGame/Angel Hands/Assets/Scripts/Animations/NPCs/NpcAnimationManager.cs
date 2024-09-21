using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Animations.NPCs
{
    public class NpcAnimationManager : MonoBehaviour
    {
        private NavMeshAgent navMeshAgent;

        private Animator animator; // Reference to the Animator component

        public void startWalking()
        { 
            animator.SetBool("isWalking", true);
        }
        public void stopWalking()
        {
            animator.SetBool("isWalking", false);
        }
    }
}