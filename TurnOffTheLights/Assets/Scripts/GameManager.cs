using UnityEngine;

/// <summary>
/// InGameシーンの司令塔。
/// シーン開始時にScoreManagerを起動し、終了時にResultUIを表示する。
/// </summary>
public class GameManager : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("ゲーム終了時に表示するリザルトUI")]
	[SerializeField] ResultUI _resultUI;

	void Start()
	{
		if (_resultUI != null)
		{
			_resultUI.Hide();
		}

		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnGameEnd.AddListener(OnGameEnd);
			ScoreManager.Instance.StartGame();
		}
		else
		{
			Debug.LogError("ScoreManager.Instance が見つかりません。シーン上に配置されているか確認してください。");
		}
	}

	void OnDestroy()
	{
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnGameEnd.RemoveListener(OnGameEnd);
		}
	}

	void OnGameEnd(float totalElectricityCost)
	{
		if (_resultUI != null)
		{
			_resultUI.Show(totalElectricityCost);
		}
	}
}
