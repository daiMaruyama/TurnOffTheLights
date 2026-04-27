using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム中のHUD表示を管理する。
/// 電気代と残り時間をリアルタイムでテキスト更新する。
/// Canvas配下のUIオブジェクトにアタッチして使用する。
/// </summary>
public class GameHUD : MonoBehaviour
{
	[Header("UI References")]
	[Tooltip("電気代を表示するテキスト")]
	[SerializeField] Text _electricityCostText;

	[Tooltip("残り時間を表示するテキスト")]
	[SerializeField] Text _remainingTimeText;

	void OnEnable()
	{
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnScoreUpdated.AddListener(UpdateHUD);
		}
	}

	void OnDisable()
	{
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnScoreUpdated.RemoveListener(UpdateHUD);
		}
	}

	/// <summary>
	/// ScoreManagerからのイベントで毎フレーム呼ばれ、HUD表示を更新する。
	/// </summary>
	void UpdateHUD(float electricityCost, float remainingTime)
	{
		if (_electricityCostText != null)
		{
			_electricityCostText.text = $"電気代: {Mathf.FloorToInt(electricityCost)}円";
		}

		if (_remainingTimeText != null)
		{
			int minutes = Mathf.FloorToInt(remainingTime / 60f);
			int seconds = Mathf.FloorToInt(remainingTime % 60f);
			_remainingTimeText.text = $"残り {minutes:00}:{seconds:00}";
		}
	}
}
