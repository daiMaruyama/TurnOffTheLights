using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 子供AIの行動を制御する。
/// NavMeshで閉まっている部屋へ移動し、ドアを開けてライトをつける。
/// 子供キャラクターにアタッチして使用する。
/// </summary>
public class KidAI : MonoBehaviour
{
	[Header("行動設定")]
	[Tooltip("ドア前での待機時間（秒）")]
	[SerializeField] float _actionInterval = 5f;

	[Tooltip("到着判定の距離")]
	[SerializeField] float _arrivalThreshold = 1.0f;

	[Tooltip("ドア手前で停止する距離")]
	[SerializeField] float _doorStopOffset = 0.9f;

	NavMeshAgent _agent;
	Animator _animator;
	Rigidbody _rb;
	float _timer;
	int _targetRoomIndex = -1;
	bool _isWaitingAtDoor;
	bool _isMovingToNextRoom;
	bool _hasStartedPatrol;

	void Start()
	{
		_agent = GetComponent<NavMeshAgent>();
		_animator = GetComponent<Animator>();
		_rb = GetComponent<Rigidbody>();
		_timer = 0;

		if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
		{
			transform.position = hit.position;
		}

		if (_rb != null)
		{
			_rb.isKinematic = true;
			_rb.detectCollisions = true;
		}

		if (_agent != null)
		{
			_agent.isStopped = true;
		}
	}

	void Update()
	{
		bool gameActive = ScoreManager.Instance != null && ScoreManager.Instance.IsGameActive;

		if (!gameActive)
		{
			StopMovement();
			UpdateAnimation(false);
			return;
		}

		if (!_hasStartedPatrol)
		{
			_hasStartedPatrol = true;
			Invoke(nameof(PickRandomRoom), 0.5f);
		}

		bool isMoving = _agent != null && _agent.velocity.magnitude > 0.05f && !_isWaitingAtDoor;
		UpdateAnimation(isMoving);

		if (_agent == null || !_agent.isOnNavMesh || !_agent.enabled) return;
		if (_isMovingToNextRoom) return;

		if (_agent.isStopped && !_isWaitingAtDoor)
		{
			_agent.isStopped = false;
		}

		_timer += Time.deltaTime;

		if (_targetRoomIndex != -1 && !_isWaitingAtDoor)
		{
			Vector3 doorPos = RoomManager.Instance.Rooms[_targetRoomIndex].Door.position;
			float distanceToDoor = (doorPos - transform.position).magnitude;

			if (distanceToDoor <= _arrivalThreshold + _doorStopOffset)
			{
				_isWaitingAtDoor = true;
				_agent.isStopped = true;
				_timer = 0;
				Debug.Log($"子供が部屋{_targetRoomIndex + 1}のドア前に到着");
			}
		}

		if (_isWaitingAtDoor && _timer >= _actionInterval)
		{
			TurnOnLight();
			_isWaitingAtDoor = false;
			_isMovingToNextRoom = true;
			_agent.isStopped = false;
			_timer = 0;
			Invoke(nameof(PickRandomRoom), 1f);
		}
	}

	void UpdateAnimation(bool isMoving)
	{
		if (_animator != null)
		{
			_animator.SetBool("IsMoving", isMoving);
		}
	}

	void StopMovement()
	{
		if (_agent != null && _agent.enabled)
		{
			_agent.isStopped = true;
			_agent.ResetPath();
		}

		_timer = 0f;
		_isWaitingAtDoor = false;
		_isMovingToNextRoom = false;
		_targetRoomIndex = -1;

		if (_rb != null)
		{
			_rb.linearVelocity = Vector3.zero;
			_rb.angularVelocity = Vector3.zero;
		}
	}

	/// <summary>
	/// ランダムな閉まっている部屋を選び、ドア前へ移動を開始する。
	/// </summary>
	void PickRandomRoom()
	{
		if (!_agent.isOnNavMesh || !_agent.enabled) return;

		_isMovingToNextRoom = false;
		_targetRoomIndex = RoomManager.Instance.GetRandomClosedRoomIndex();

		if (_targetRoomIndex != -1)
		{
			Transform door = RoomManager.Instance.Rooms[_targetRoomIndex].Door;
			Vector3 direction = (door.position - transform.position).normalized;
			Vector3 stopPos = door.position - direction * _doorStopOffset;

			_agent.SetDestination(stopPos);
			Debug.Log($"子供が部屋{_targetRoomIndex + 1}のドアへ向かいます");
		}
		else
		{
			Vector3 randomPos = RandomNavSphere(transform.position, 10f);
			_agent.SetDestination(randomPos);
			Debug.Log("閉まっている部屋がないので徘徊します");
		}
	}

	/// <summary>
	/// 目標の部屋のドアを開けてライトをつける。
	/// </summary>
	void TurnOnLight()
	{
		if (_targetRoomIndex == -1) return;

		bool opened = RoomManager.Instance.TryOpenRoom(_targetRoomIndex);

		if (opened)
		{
			Debug.Log($"子供が部屋{_targetRoomIndex + 1}の電気をつけました");
		}
		else
		{
			Debug.LogWarning($"子供が部屋{_targetRoomIndex + 1}の電気をつけられませんでした");
		}
	}

	/// <summary>
	/// 指定範囲内のNavMesh上のランダムな位置を返す。徘徊用。
	/// </summary>
	Vector3 RandomNavSphere(Vector3 origin, float distance)
	{
		Vector3 randomDirection = Random.insideUnitSphere * distance + origin;
		NavMesh.SamplePosition(randomDirection, out var navHit, distance, NavMesh.AllAreas);
		return navHit.position;
	}
}
