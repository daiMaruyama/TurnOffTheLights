using UnityEngine;

[System.Serializable]
public class RoomData
{
    [Tooltip("Door Position")] public Transform _door;
    [Tooltip("Light")] public Light _roomLight;
    [Tooltip("Door Rotate Time(s)")] public float _animationDuration = 0.5f;

    [HideInInspector]
    public bool _isOpen = false;         // 現在の状態
    [HideInInspector]
    public bool _isAnimating = false;    // アニメーション中か
}