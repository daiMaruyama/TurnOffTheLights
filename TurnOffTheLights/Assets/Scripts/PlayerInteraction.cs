using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public LayerMask doorLayer;

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

    void TryCloseDoor()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, doorLayer))
        {
            // どの部屋のドアか判定
            for (int i = 0; i < RoomManager.Instance.rooms.Count; i++)
            {
                if (hit.transform == RoomManager.Instance.rooms[i]._door)
                {
                    // ドアを閉めて電気を消す
                    RoomManager.Instance.CloseRoom(i);
                    break;
                }
            }
        }
    }
}