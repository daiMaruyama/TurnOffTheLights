using UnityEngine;

/// <summary>
/// プレイヤーのインタラクション処理。
/// Raycastでドアを検出し、Eキーでドアを閉めてライトを消す。
/// プレイヤーオブジェクトにアタッチして使用する。
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
	[Header("インタラクション設定")]
	[Tooltip("ドアに反応できる最大距離")]
	[SerializeField] float _interactionDistance = 3f;

	[Tooltip("ドア判定用のレイヤーマスク")]
	[SerializeField] LayerMask _doorLayer;

	Camera _playerCamera;

	void Start()
	{
		_playerCamera = Camera.main;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.E))
		{
			TryCloseDoor();
		}
	}

	/// <summary>
	/// カメラ正面にRayを飛ばし、ドアに当たったら対応する部屋を閉める。
	/// </summary>
	void TryCloseDoor()
	{
		Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

		if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _doorLayer))
		{
			var rooms = RoomManager.Instance.Rooms;

			for (int i = 0; i < rooms.Count; i++)
			{
				if (hit.transform == rooms[i].Door)
				{
					RoomManager.Instance.CloseRoom(i);
					break;
				}
			}
		}
	}
}
