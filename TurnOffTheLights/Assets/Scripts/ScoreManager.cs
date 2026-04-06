using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 電気代スコアを管理するマネージャー。
/// ON状態の部屋数 × 時間 × レートで電気代を加算し続ける。
/// </summary>
public class ScoreManager : MonoBehaviour
{
	[Header("スコア設定")]
	[Tooltip("1部屋あたり1秒ごとに加算される電気代（円）")]
	[SerializeField] float _electricityRatePerSecond = 10f;

	[Header("時間設定")]
	[Tooltip("ゲームの制限時間（秒）")]
	[SerializeField] float _gameDuration = 120f;

	float _totalElectricityCost;
	float _remainingTime;
	bool _isGameActive;

	/// <summary>現在の累計電気代（円）</summary>
	public float TotalElectricityCost => _totalElectricityCost;

	/// <summary>残り時間（秒）</summary>
	public float RemainingTime => _remainingTime;

	/// <summary>ゲーム進行中かどうか</summary>
	public bool IsGameActive => _isGameActive;

	/// <summary>ゲーム終了時に発火するイベント（最終電気代を渡す）</summary>
	public UnityEvent<float> OnGameEnd = new UnityEvent<float>();

	/// <summary>スコア更新時に発火するイベント（電気代, 残り時間）</summary>
	public UnityEvent<float, float> OnScoreUpdated = new UnityEvent<float, float>();

	static ScoreManager _instance;
	public static ScoreManager Instance => _instance;

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}
		_instance = this;
	}

	/// <summary>
	/// ゲーム開始時に呼び出す。スコアとタイマーを初期化する。
	/// </summary>
	public void StartGame()
	{
		_totalElectricityCost = 0f;
		_remainingTime = _gameDuration;
		_isGameActive = true;
	}

	void Update()
	{
		if (!_isGameActive)
		{
			return;
		}

		_remainingTime -= Time.deltaTime;

		if (_remainingTime <= 0f)
		{
			_remainingTime = 0f;
			EndGame();
			return;
		}

		int openRoomCount = RoomManager.Instance.GetOpenRoomCount();
		float costThisFrame = openRoomCount * _electricityRatePerSecond * Time.deltaTime;
		_totalElectricityCost += costThisFrame;

		OnScoreUpdated.Invoke(_totalElectricityCost, _remainingTime);
	}

	void EndGame()
	{
		_isGameActive = false;
		OnGameEnd.Invoke(_totalElectricityCost);
	}
}
