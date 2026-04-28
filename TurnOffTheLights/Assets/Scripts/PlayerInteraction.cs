using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// プレイヤーのインタラクション処理。
/// Raycastでライト点灯中のドアを検出し、Eキー長押しで閉める。
/// プレイヤーオブジェクトにアタッチして使用する。
/// Legacy Input Manager / new Input System の両方に対応。
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
	[Header("インタラクション設定")]
	[Tooltip("ドアに反応できる最大距離")]
	[SerializeField] float _interactionDistance = 3f;

	[Tooltip("検出範囲の半径（SphereCast用）。大きいほど判定が緩くなる")]
	[SerializeField] float _detectionRadius = 0.5f;

	[Tooltip("ドア判定用のレイヤーマスク")]
	[SerializeField] LayerMask _doorLayer;

	[Tooltip("長押しでドアを閉めるまでの時間（秒）")]
	[SerializeField] float _holdDuration = 1f;

	[Tooltip("Legacy Input Manager 用のキー")]
	[SerializeField] KeyCode _interactionKey = KeyCode.E;

	[Tooltip("プレイヤーのカメラ。未設定の場合はCamera.mainを使う")]
	[SerializeField] Camera _playerCamera;
	int _hoveredRoomIndex = -1;
	float _holdElapsed;
	bool _waitingForKeyRelease;

	void Awake()
	{
		if (_playerCamera == null)
		{
			_playerCamera = Camera.main;
		}
	}

	/// <summary>現在ターゲット中の部屋のインデックス。なければ-1</summary>
	public int HoveredRoomIndex => _hoveredRoomIndex;

	/// <summary>長押しの進行度（0〜1）。UIゲージのfillAmountに使う</summary>
	public float HoldProgress => _holdDuration > 0f ? Mathf.Clamp01(_holdElapsed / _holdDuration) : 0f;

	void Update()
	{
		UpdateHover();

		bool keyHeld = IsInteractionKeyHeld();

		if (!keyHeld)
		{
			_waitingForKeyRelease = false;
		}

		if (_hoveredRoomIndex >= 0 && keyHeld && !_waitingForKeyRelease)
		{
			_holdElapsed += Time.deltaTime;

			if (_holdElapsed >= _holdDuration)
			{
				RoomManager.Instance.TryCloseRoom(_hoveredRoomIndex);
				_holdElapsed = 0f;
				_hoveredRoomIndex = -1;
				_waitingForKeyRelease = true;
			}
		}
		else
		{
			_holdElapsed = 0f;
		}
	}

	/// <summary>
	/// カメラ正面のRayでドアを検出し、ライト点灯中の部屋なら hover 状態にする。
	/// </summary>
	void UpdateHover()
	{
		if (_playerCamera == null || RoomManager.Instance == null)
		{
			return;
		}

		Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

		if (Physics.SphereCast(ray, _detectionRadius, out RaycastHit hit, _interactionDistance, _doorLayer))
		{
			int roomIndex = FindHitRoomIndex(hit.transform);
			if (roomIndex >= 0)
			{
				var room = RoomManager.Instance.Rooms[roomIndex];

				if (room.IsOpen)
				{
					if (_hoveredRoomIndex != roomIndex)
					{
						_hoveredRoomIndex = roomIndex;
						_holdElapsed = 0f;
					}
					return;
				}
			}
		}

		if (_hoveredRoomIndex != -1)
		{
			_hoveredRoomIndex = -1;
			_holdElapsed = 0f;
		}
	}

	int FindHitRoomIndex(Transform hitTransform)
	{
		var rooms = RoomManager.Instance.Rooms;

		for (int i = 0; i < rooms.Count; i++)
		{
			Transform door = rooms[i].Door;
			if (door == null)
			{
				continue;
			}

			if (hitTransform == door || hitTransform.IsChildOf(door) || door.IsChildOf(hitTransform))
			{
				return i;
			}
		}

		return -1;
	}

	/// <summary>
	/// Legacy / new Input System の両方をチェックする。
	/// どちらかでキーが押下されていればtrueを返す。
	/// </summary>
	bool IsInteractionKeyHeld()
	{
#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
		{
			return true;
		}
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
		if (Input.GetKey(_interactionKey))
		{
			return true;
		}
#endif
		return false;
	}
}
