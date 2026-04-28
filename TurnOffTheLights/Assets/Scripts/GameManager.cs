using System.Collections;
using UnityEngine;

/// <summary>
/// InGameシーンの司令塔。
/// 開始カウントダウン、終了演出、リザルト表示を制御する。
/// </summary>
public class GameManager : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("ゲーム終了時に表示するリザルトUI")]
	[SerializeField] ResultUI _resultUI;

	GameHUD _gameHUD;
	bool _isEnding;

	void Start()
	{
		EnsureResultUI();
		EnsureGameHUD();

		if (_resultUI != null)
		{
			_resultUI.Hide();
		}

		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnGameEnd.AddListener(OnGameEnd);
			StartCoroutine(BeginGameRoutine());
		}
		else
		{
			Debug.LogError("ScoreManager.Instance が見つかりません。シーン上に配置されているか確認してください。");
		}
	}

	void EnsureResultUI()
	{
		if (_resultUI != null)
		{
			return;
		}

		_resultUI = FindAnyObjectByType<ResultUI>(FindObjectsInactive.Include);

		if (_resultUI == null)
		{
			_resultUI = ResultUI.CreateFallbackUI();
		}
	}

	void EnsureGameHUD()
	{
		if (_gameHUD != null)
		{
			return;
		}

		_gameHUD = FindAnyObjectByType<GameHUD>(FindObjectsInactive.Include);
	}

	IEnumerator BeginGameRoutine()
	{
		if (_gameHUD != null)
		{
			yield return _gameHUD.PlayIntroSequence();
		}

		ScoreManager.Instance.StartGame();
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
		if (_isEnding)
		{
			return;
		}

		_isEnding = true;
		StartCoroutine(EndGameRoutine(totalElectricityCost));
	}

	IEnumerator EndGameRoutine(float totalElectricityCost)
	{
		if (_gameHUD != null)
		{
			yield return _gameHUD.PlayFinishSequence();
		}

		if (_resultUI != null)
		{
			_resultUI.Show(totalElectricityCost);
		}
	}
}
