using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KidSample : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _navMeshAgent;

    //í«Ç¢Ç©ÇØÇÈëŒè€
    [SerializeField]
    private Transform _player;

    void Update()
    {
        _navMeshAgent.SetDestination(_player.position);
    }
}
