using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 全部屋の状態を管理するマネージャー。
/// ドアの開閉アニメーションとライトのON/OFFを制御する。
/// シーン上の空GameObjectにアタッチして使用する。
/// </summary>
public class RoomManager : MonoBehaviour
{
	[Header("全部屋データ")]
	[SerializeField] List<RoomData> _rooms = new List<RoomData>();

	static RoomManager _instance;
	public static RoomManager Instance => _instance;

	/// <summary>登録されている部屋のリスト</summary>
	public List<RoomData> Rooms => _rooms;

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		InitializeRooms();
	}

	/// <summary>
	/// 全部屋をドア閉・ライト消灯の初期状態にする。
	/// </summary>
	void InitializeRooms()
	{
		for (int i = 0; i < _rooms.Count; i++)
		{
			var room = _rooms[i];
			room.IsOpen = false;
			room.IsAnimating = false;

			if (room.RoomLight != null)
			{
				room.RoomLight.enabled = false;
			}
			else
			{
				Debug.LogWarning($"Room {i + 1} のLight参照が未設定です。");
			}
		}
	}

	/// <summary>
	/// 指定した部屋のドアを閉めてライトを消す。プレイヤー操作用。
	/// </summary>
	public void CloseRoom(int roomIndex)
	{
		TryCloseRoom(roomIndex);
	}

	/// <summary>
	/// 指定した部屋のドアを開けてライトをつける。子供AI用。
	/// </summary>
	public void OpenRoom(int roomIndex)
	{
		TryOpenRoom(roomIndex);
	}

	/// <summary>
	/// ドアを閉める処理を試行する。成功時trueを返す。
	/// </summary>
	public bool TryCloseRoom(int roomIndex)
	{
		if (roomIndex < 0 || roomIndex >= _rooms.Count) return false;
		if (_rooms[roomIndex].IsAnimating) return false;
		if (!_rooms[roomIndex].IsOpen) return false;

		StartCoroutine(ToggleRoomCoroutine(roomIndex, false));
		return true;
	}

	/// <summary>
	/// ドアを開ける処理を試行する。成功時trueを返す。
	/// </summary>
	public bool TryOpenRoom(int roomIndex)
	{
		if (roomIndex < 0 || roomIndex >= _rooms.Count) return false;
		if (_rooms[roomIndex].IsAnimating) return false;
		if (_rooms[roomIndex].IsOpen) return false;

		StartCoroutine(ToggleRoomCoroutine(roomIndex, true));
		return true;
	}

	/// <summary>
	/// ランダムな閉まっている部屋のインデックスを取得する。子供AI用。
	/// 全部屋が開いている場合は-1を返す。
	/// </summary>
	public int GetRandomClosedRoomIndex()
	{
		List<int> closedRooms = new List<int>();

		for (int i = 0; i < _rooms.Count; i++)
		{
			bool isLightOn = _rooms[i].RoomLight != null && _rooms[i].RoomLight.enabled;
			bool isClosed = !_rooms[i].IsOpen || !isLightOn;

			if (isClosed && !_rooms[i].IsAnimating)
			{
				closedRooms.Add(i);
			}
		}

		if (closedRooms.Count == 0) return -1;

		return closedRooms[Random.Range(0, closedRooms.Count)];
	}

	/// <summary>
	/// ドアの開閉アニメーションとライト切り替えを行うコルーチン。
	/// </summary>
	IEnumerator ToggleRoomCoroutine(int roomIndex, bool open)
	{
		RoomData room = _rooms[roomIndex];
		room.IsAnimating = true;

		Vector3 originalRotation = room.Door.localRotation.eulerAngles;

		float startAngle = open ? 0f : -90f;
		float endAngle = open ? -90f : 0f;
		float elapsed = 0f;

		while (elapsed < room.AnimationDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / room.AnimationDuration;
			float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
			room.Door.localRotation = Quaternion.Euler(originalRotation.x, currentAngle, originalRotation.z);
			yield return null;
		}

		room.Door.localRotation = Quaternion.Euler(originalRotation.x, endAngle, originalRotation.z);

		if (room.RoomLight != null)
		{
			room.RoomLight.enabled = open;
		}
		else
		{
			Debug.LogWarning($"Room {roomIndex + 1} のLightが未設定のため、ライト切り替えできません。");
		}

		room.IsOpen = open;
		room.IsAnimating = false;
	}

	/// <summary>
	/// 現在開いている部屋の数を返す。スコア計算・ゲームオーバー判定用。
	/// </summary>
	public int GetOpenRoomCount()
	{
		int count = 0;
		foreach (var room in _rooms)
		{
			bool isLightOn = room.RoomLight != null && room.RoomLight.enabled;
			if (room.IsOpen && isLightOn)
			{
				count++;
			}
		}
		return count;
	}
}
