using Cysharp.Threading.Tasks;
using GameJamCore;
using GameJamScene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲーム結果表示UI。
/// 電気代から評価ランクを計算して表示する。
/// 必要な参照がない場合は実行時に簡易UIを自動生成する。
/// </summary>
public class ResultUI : MonoBehaviour
{
	[Header("UI References")]
	[Tooltip("リザルトパネル全体（最初は非表示にする）")]
	[SerializeField] GameObject _panel;

	[Tooltip("電気代を表示するテキスト")]
	[SerializeField] Text _costText;

	[Tooltip("ランクを表示するテキスト")]
	[SerializeField] Text _rankText;

	[Tooltip("ひとこと評価を表示するテキスト")]
	[SerializeField] Text _commentText;

	[Header("ランク基準（電気代の上限値）")]
	[Tooltip("Sランク：この値以下")]
	[SerializeField] float _rankSThreshold = 3000f;

	[Tooltip("Aランク：この値以下")]
	[SerializeField] float _rankAThreshold = 10000f;

	[Tooltip("Bランク：この値以下、これを超えるとC")]
	[SerializeField] float _rankBThreshold = 30000f;

	[Header("シーン名")]
	[Tooltip("リトライ時に再ロードするシーン名")]
	[SerializeField] string _inGameSceneName = "InGame";

	[Tooltip("タイトル戻り時にロードするシーン名")]
	[SerializeField] string _titleSceneName = "Title";

	public static ResultUI CreateFallbackUI()
	{
		var root = new GameObject("RuntimeResultUI");
		return root.AddComponent<ResultUI>();
	}

	void Awake()
	{
		EnsureUI();
	}

	public void Hide()
	{
		EnsureUI();

		if (_panel != null)
		{
			_panel.SetActive(false);
		}
	}

	public void Show(float totalCost)
	{
		EnsureUI();

		if (_panel != null)
		{
			_panel.SetActive(true);
		}

		if (_costText != null)
		{
			_costText.text = $"TOTAL COST\n{Mathf.FloorToInt(totalCost):N0} 円";
		}

		string rank = CalculateRank(totalCost);

		if (_rankText != null)
		{
			_rankText.text = $"RANK  {rank}";
			_rankText.color = GetRankColor(rank);
		}

		if (_commentText != null)
		{
			_commentText.text = GetComment(rank);
		}
	}

	public void OnRetryButtonClicked()
	{
		HideImmediate();
		LoadSceneAsync(_inGameSceneName).Forget();
	}

	public void OnTitleButtonClicked()
	{
		HideImmediate();
		LoadSceneAsync(_titleSceneName).Forget();
	}

	void HideImmediate()
	{
		if (_panel != null)
		{
			_panel.SetActive(false);
		}
	}

	string CalculateRank(float cost)
	{
		if (cost <= _rankSThreshold) return "S";
		if (cost <= _rankAThreshold) return "A";
		if (cost <= _rankBThreshold) return "B";
		return "C";
	}

	Color GetRankColor(string rank)
	{
		switch (rank)
		{
			case "S": return new Color(1f, 0.93f, 0.45f);
			case "A": return new Color(0.45f, 0.95f, 0.65f);
			case "B": return new Color(0.55f, 0.85f, 1f);
			default: return new Color(1f, 0.55f, 0.55f);
		}
	}

	string GetComment(string rank)
	{
		switch (rank)
		{
			case "S": return "見回り完璧。電気のムダがほとんどありませんでした。";
			case "A": return "かなり優秀。あと少しで節電マスターです。";
			case "B": return "まずまず。消し忘れをもう少し減らせそうです。";
			default: return "部屋がかなり明るいままでした。次はもっと急いで消灯しましょう。";
		}
	}

	async UniTaskVoid LoadSceneAsync(string sceneName)
	{
		if (TryLoadWithSceneService(sceneName))
		{
			return;
		}

		SceneManager.LoadScene(sceneName);
	}

	bool TryLoadWithSceneService(string sceneName)
	{
		try
		{
			ServiceLocator.Get<ISceneService>().LoadAsync(sceneName).Forget();
			return true;
		}
		catch (System.InvalidOperationException)
		{
			return false;
		}
	}

	void EnsureUI()
	{
		if (_panel != null && _costText != null && _rankText != null && _commentText != null)
		{
			return;
		}

		BuildRuntimeUI();
	}

	void BuildRuntimeUI()
	{
		Transform existingCanvas = transform.Find("ResultCanvas");
		if (existingCanvas != null)
		{
			Destroy(existingCanvas.gameObject);
		}

		Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		var canvasObject = new GameObject("ResultCanvas");
		canvasObject.transform.SetParent(transform, false);

		var canvas = canvasObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 100;
		var scaler = canvasObject.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920f, 1080f);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;
		canvasObject.AddComponent<GraphicRaycaster>();

		_panel = CreateUiObject("Panel", canvasObject.transform);
		var panelRect = _panel.GetComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0f, 0f);
		panelRect.anchorMax = new Vector2(1f, 1f);
		panelRect.offsetMin = Vector2.zero;
		panelRect.offsetMax = Vector2.zero;

		var panelImage = _panel.AddComponent<Image>();
		panelImage.color = new Color(0.02f, 0.03f, 0.06f, 0.9f);

		GameObject cardObject = CreateUiObject("Card", _panel.transform);
		var cardRect = cardObject.GetComponent<RectTransform>();
		SetRect(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(820f, 560f));
		var cardImage = cardObject.AddComponent<Image>();
		cardImage.color = new Color(0.08f, 0.11f, 0.18f, 0.96f);

		var titleText = CreateText("TitleText", cardObject.transform, font, 54, FontStyle.Bold, TextAnchor.MiddleCenter);
		titleText.text = "RESULT";
		titleText.color = Color.white;
		SetRect(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(720f, 72f));

		_costText = CreateText("CostText", cardObject.transform, font, 42, FontStyle.Bold, TextAnchor.MiddleCenter);
		_costText.color = new Color(0.96f, 0.98f, 1f);
		SetRect(_costText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 88f), new Vector2(680f, 110f));

		_rankText = CreateText("RankText", cardObject.transform, font, 64, FontStyle.Bold, TextAnchor.MiddleCenter);
		SetRect(_rankText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -12f), new Vector2(720f, 86f));

		_commentText = CreateText("CommentText", cardObject.transform, font, 28, FontStyle.Normal, TextAnchor.MiddleCenter);
		_commentText.color = new Color(0.86f, 0.9f, 0.96f);
		_commentText.horizontalOverflow = HorizontalWrapMode.Wrap;
		_commentText.verticalOverflow = VerticalWrapMode.Overflow;
		_commentText.lineSpacing = 1.1f;
		SetRect(_commentText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(700f, 110f));

		Button retryButton = CreateButton("RetryButton", cardObject.transform, font, "RETRY", new Vector2(-150f, -220f));
		retryButton.onClick.AddListener(OnRetryButtonClicked);

		Button titleButton = CreateButton("TitleButton", cardObject.transform, font, "TITLE", new Vector2(150f, -220f));
		titleButton.onClick.AddListener(OnTitleButtonClicked);

		_panel.SetActive(false);
	}

	Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchoredPosition)
	{
		GameObject buttonObject = CreateUiObject(name, parent);
		var image = buttonObject.AddComponent<Image>();
		image.color = new Color(0.14f, 0.18f, 0.27f, 0.96f);

		var button = buttonObject.AddComponent<Button>();
		ColorBlock colors = button.colors;
		colors.normalColor = image.color;
		colors.highlightedColor = new Color(0.22f, 0.28f, 0.4f, 1f);
		colors.pressedColor = new Color(0.08f, 0.12f, 0.2f, 1f);
		colors.selectedColor = colors.highlightedColor;
		colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
		button.colors = colors;

		SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(240f, 64f));

		Text buttonText = CreateText("Label", buttonObject.transform, font, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
		buttonText.text = label;
		buttonText.color = Color.white;
		SetRect(buttonText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

		return button;
	}

	Text CreateText(string name, Transform parent, Font font, int fontSize, FontStyle fontStyle, TextAnchor alignment)
	{
		GameObject textObject = CreateUiObject(name, parent);
		var text = textObject.AddComponent<Text>();
		text.font = font;
		text.fontSize = fontSize;
		text.fontStyle = fontStyle;
		text.alignment = alignment;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.text = string.Empty;
		return text;
	}

	GameObject CreateUiObject(string name, Transform parent)
	{
		var go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.AddComponent<RectTransform>();
		return go;
	}

	void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
	{
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = sizeDelta;
	}
}
