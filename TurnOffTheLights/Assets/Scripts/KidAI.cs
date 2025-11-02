using UnityEngine;
using UnityEngine.AI;

public class KidAI : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float actionInterval = 5f; // 電気をつける間隔

    float _timer;
    int _targetRoomIndex = -1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _timer = actionInterval;

        // NavMesh上の最も近い位置にワープ
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        // 初期化を遅らせる
        Invoke("PickRandomRoom", 0.5f);
    }

    void Update()
    {
        // NavMeshAgentが有効か、NavMesh上にいるかチェック
        if (!agent.isOnNavMesh || !agent.enabled) return;

        _timer += Time.deltaTime;

        // 目的地に到着したか
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (_timer >= actionInterval)
            {
                TurnOnLight();
                _timer = 0;
                PickRandomRoom();
            }
        }
    }

    void PickRandomRoom()
    {
        // NavMeshが準備できてるか確認
        if (!agent.isOnNavMesh || !agent.enabled) return;

        _targetRoomIndex = RoomManager.Instance.GetRandomClosedRoomIndex();

        if (_targetRoomIndex != -1)
        {
            // 部屋のドアに向かう
            Vector3 doorPos = RoomManager.Instance.rooms[_targetRoomIndex]._door.position;
            agent.SetDestination(doorPos);
        }
        else
        {
            // 閉まってる部屋がない場合は適当に徘徊
            Vector3 randomPos = RandomNavSphere(transform.position, 10f);
            agent.SetDestination(randomPos);
        }
    }

    void TurnOnLight()
    {
        if (_targetRoomIndex != -1)
        {
            RoomManager.Instance.OpenRoom(_targetRoomIndex);
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

        return navHit.position;
    }
}