using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.NPCs.WanderAround { 

    public class NPCWander : MonoBehaviour
    {
        public float wanderRadius = 10f;
        public float wanderTimer = 5f;
    
        private NavMeshAgent agent;
        private Animator animator;
        private float timer;
        private bool isWalking;
    
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            timer = wanderTimer;
            isWalking = false;
    
        }
    
        void Update()
        {
            timer += Time.deltaTime;
    
            if (timer >= wanderTimer)
            {
                // RandomNavSphere is a helper function that returns a random position to walk to
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                // Check if the new position is on the NavMesh
                if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                timer = 0;
            }
    
            // Update the IsWalking boolean based on the agent's velocity
            bool currentlyWalking = agent.velocity.sqrMagnitude > 0.1f;
            if (currentlyWalking != isWalking)
            {
                isWalking = currentlyWalking;
                animator.SetBool("IsWalking", isWalking);
                //Debug.Log(isWalking ? "NPC started walking." : "NPC stopped walking.");
            }
    
            // Adjust the NPC's Y position to ensure it is correctly placed on the ground
            Vector3 position = transform.position;
            position.y = Terrain.activeTerrain.SampleHeight(transform.position);
            transform.position = position;
        }
    
        // Helper function to get a random position within a sphere of radius dist
        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection += origin;
    
            NavMeshHit navHit;
            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
    
            return navHit.position;
        }
    
    }

}