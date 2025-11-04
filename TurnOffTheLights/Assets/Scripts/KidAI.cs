using UnityEngine;
using UnityEngine.AI;

public class KidAI : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float actionInterval = 5f;
    [SerializeField] float arrivalThreshold = 1.0f;
    [SerializeField] float doorStopOffset = 0.9f; // ドアより手前で止まる距離
    float _timer;
    int _targetRoomIndex = -1;
    bool _isWaitingAtDoor = false;
    bool _isMovingToNextRoom = false;
    Animator animator;
    Rigidbody rb;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        _timer = 0;

        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = true;
        }

        Invoke(nameof(PickRandomRoom), 0.5f);
    }

    void Update()
    {
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.05f && !_isWaitingAtDoor;
            animator.SetBool("IsMoving", isMoving);
        }

        if (!agent.isOnNavMesh || !agent.enabled) return;
        if (_isMovingToNextRoom) return;

        _timer += Time.deltaTime;

        if (_targetRoomIndex != -1 && !_isWaitingAtDoor)
        {
            Vector3 doorPos = RoomManager.Instance.rooms[_targetRoomIndex]._door.position;
            Vector3 toDoor = doorPos - transform.position;
            float distanceToDoor = toDoor.magnitude;

            // 距離がしきい値以内なら停止
            if (distanceToDoor <= arrivalThreshold + doorStopOffset)
            {
                _isWaitingAtDoor = true;
                agent.isStopped = true;
                _timer = 0;
                Debug.Log($"子供が部屋{_targetRoomIndex + 1}のドア前に到着（少し手前）");
            }
        }

        if (_isWaitingAtDoor && _timer >= actionInterval)
        {
            TurnOnLight();
            _isWaitingAtDoor = false;
            _isMovingToNextRoom = true;
            agent.isStopped = false;
            _timer = 0;
            Invoke(nameof(PickRandomRoom), 1f);
        }
    }

    void PickRandomRoom()
    {
        if (!agent.isOnNavMesh || !agent.enabled) return;

        _isMovingToNextRoom = false;
        _targetRoomIndex = RoomManager.Instance.GetRandomClosedRoomIndex();

        if (_targetRoomIndex != -1)
        {
            Transform door = RoomManager.Instance.rooms[_targetRoomIndex]._door;

            // ドアの前で止まる座標を計算
            Vector3 direction = (door.position - transform.position).normalized;
            Vector3 stopPos = door.position - direction * doorStopOffset;

            agent.SetDestination(stopPos);
            Debug.Log($"子供が部屋{_targetRoomIndex + 1}のドアへ向かいます（手前で止まる）");
        }
        else
        {
            Vector3 randomPos = RandomNavSphere(transform.position, 10f);
            agent.SetDestination(randomPos);
            Debug.Log("閉まっている部屋がないので徘徊します");
        }
    }

    void TurnOnLight()
    {
        if (_targetRoomIndex != -1)
        {
            RoomManager.Instance.OpenRoom(_targetRoomIndex);
            Debug.Log($"子供が部屋{_targetRoomIndex + 1}の電気をつけました");
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance + origin;
        NavMesh.SamplePosition(randomDirection, out var navHit, distance, NavMesh.AllAreas);
        return navHit.position;
    }
}
