using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance; // シングルトン

    [Header("All Rooms")]
    public List<RoomData> rooms = new List<RoomData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // プレイヤー用：ドアを閉めて電気を消す
    public void CloseRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count) return;
        if (!rooms[roomIndex]._isOpen) return;

        StartCoroutine(ToggleRoomCoroutine(roomIndex, false));
    }

    // 子供AI用：ドアを開けて電気をつける
    public void OpenRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count) return;
        if (rooms[roomIndex]._isOpen) return;

        StartCoroutine(ToggleRoomCoroutine(roomIndex, true));
    }

    // ランダムな閉まってる部屋のインデックスを取得（子供AI用）
    public int GetRandomClosedRoomIndex()
    {
        List<int> closedRooms = new List<int>();

        for (int i = 0; i < rooms.Count; i++)
        {
            if (!rooms[i]._isOpen && !rooms[i]._isAnimating)
            {
                closedRooms.Add(i);
            }
        }

        if (closedRooms.Count == 0) return -1;

        return closedRooms[Random.Range(0, closedRooms.Count)];
    }

    // ドアのアニメーション
    IEnumerator ToggleRoomCoroutine(int roomIndex, bool open)
    {
        RoomData room = rooms[roomIndex];
        room._isAnimating = true;

        float startAngle = open ? 0f : -90f;
        float endAngle = open ? -90f : 0f;

        float elapsed = 0f;

        while (elapsed < room._animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / room._animationDuration;

            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            room._door.localRotation = Quaternion.Euler(0, currentAngle, 0);

            yield return null;
        }

        room._door.localRotation = Quaternion.Euler(0, endAngle, 0);
        room._roomLight.enabled = open;
        room._isOpen = open;
        room._isAnimating = false;
    }

    // 現在開いてる部屋数（ゲームオーバー判定用）
    public int GetOpenRoomCount()
    {
        int count = 0;
        foreach (var room in rooms)
        {
            if (room._isOpen) count++;
        }
        return count;
    }
}