using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CharacterWalker : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

    }

    void Update()
    {
        agent.destination = target.position;
    }
}
