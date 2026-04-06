using UnityEngine;

/// <summary>
/// 部屋ごとのデータを保持するクラス。
/// RoomManagerのリストに登録してInspectorから設定する。
/// </summary>
[System.Serializable]
public class RoomData
{
	[Tooltip("ドアのTransform")]
	[SerializeField] Transform _door;

	[Tooltip("部屋のライト")]
	[SerializeField] Light _roomLight;

	[Tooltip("ドアの回転アニメーション時間（秒）")]
	[SerializeField] float _animationDuration = 0.5f;

	bool _isOpen;
	bool _isAnimating;

	public Transform Door => _door;
	public Light RoomLight => _roomLight;
	public float AnimationDuration => _animationDuration;

	public bool IsOpen
	{
		get => _isOpen;
		set => _isOpen = value;
	}

	public bool IsAnimating
	{
		get => _isAnimating;
		set => _isAnimating = value;
	}
}
