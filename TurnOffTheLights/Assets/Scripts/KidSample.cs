using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KidSample : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent _navMeshAgent;

    //í«Ç¢Ç©ÇØÇÈëŒè€
    [SerializeField]
    Transform _player;

    void Update()
    {
        _navMeshAgent.SetDestination(_player.position);
    }
}
