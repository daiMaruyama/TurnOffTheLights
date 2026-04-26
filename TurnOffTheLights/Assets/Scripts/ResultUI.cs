using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using GameJamCore;
using GameJamScene;

/// <summary>
/// ゲーム結果表示UI。
/// 電気代から評価ランクを計算して表示する。
/// リトライ・タイトル戻るボタンのコールバックも持つ。
/// </summary>
public class ResultUI : MonoBehaviour
{
	[Header("UI References")]
	[Tooltip("リザルトパネル全体（最初は非表示にする）")]
	[SerializeField] GameObject _panel;

	[Tooltip("電気代を表示するテキスト")]
	[SerializeField] TextMeshProUGUI _costText;

	[Tooltip("ランクを表示するテキスト")]
	[SerializeField] TextMeshProUGUI _rankText;

	[Header("ランク基準（電気代の上限値）")]
	[Tooltip("Sランク：この値以下")]
	[SerializeField] float _rankSThreshold = 5000f;

	[Tooltip("Aランク：この値以下")]
	[SerializeField] float _rankAThreshold = 20000f;

	[Tooltip("Bランク：この値以下、これを超えるとC")]
	[SerializeField] float _rankBThreshold = 50000f;

	[Header("シーン名")]
	[Tooltip("リトライ時に再ロードするシーン名")]
	[SerializeField] string _inGameSceneName = "InGame";

	[Tooltip("タイトル戻り時にロードするシーン名")]
	[SerializeField] string _titleSceneName = "Title";

	/// <summary>
	/// リザルトを非表示にする。GameManagerからゲーム開始時に呼ばれる。
	/// </summary>
	public void Hide()
	{
		if (_panel != null)
		{
			_panel.SetActive(false);
		}
	}

	/// <summary>
	/// リザルトを表示する。電気代に応じてランクも計算して表示する。
	/// </summary>
	public void Show(float totalCost)
	{
		if (_panel != null)
		{
			_panel.SetActive(true);
		}

		if (_costText != null)
		{
			_costText.text = $"電気代: {Mathf.FloorToInt(totalCost):N0}円";
		}

		if (_rankText != null)
		{
			_rankText.text = CalculateRank(totalCost);
		}
	}

	/// <summary>
	/// リトライボタンのOnClickに登録する。InGameシーンを再ロードする。
	/// </summary>
	public void OnRetryButtonClicked()
	{
		LoadSceneAsync(_inGameSceneName).Forget();
	}

	/// <summary>
	/// タイトル戻るボタンのOnClickに登録する。
	/// </summary>
	public void OnTitleButtonClicked()
	{
		LoadSceneAsync(_titleSceneName).Forget();
	}

	string CalculateRank(float cost)
	{
		if (cost <= _rankSThreshold) return "S";
		if (cost <= _rankAThreshold) return "A";
		if (cost <= _rankBThreshold) return "B";
		return "C";
	}

	async UniTaskVoid LoadSceneAsync(string sceneName)
	{
		await ServiceLocator.Get<ISceneService>().LoadAsync(sceneName);
	}
}
