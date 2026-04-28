using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム中のHUD表示を管理する。
/// 電気代と残り時間に加え、開始演出や説明パネルも制御する。
/// </summary>
public class GameHUD : MonoBehaviour
{
	[Header("UI References")]
	[Tooltip("電気代を表示するテキスト")]
	[SerializeField] Text _electricityCostText;

	[Tooltip("残り時間を表示するテキスト")]
	[SerializeField] Text _remainingTimeText;

	Text _objectiveText;
	Text _statusText;
	Text _centerMessageText;
	Button _helpButton;
	GameObject _helpPanel;
	Text _helpPanelText;
	Text _helpButtonLabel;

	void Start()
	{
		EnsureSupportUI();

		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnScoreUpdated.AddListener(UpdateHUD);
			UpdateHUD(ScoreManager.Instance.TotalElectricityCost, ScoreManager.Instance.RemainingTime);
		}
	}

	void OnDestroy()
	{
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.OnScoreUpdated.RemoveListener(UpdateHUD);
		}
	}

	public IEnumerator PlayIntroSequence()
	{
		EnsureSupportUI();
		SetMainHudVisible(false);
		SetHelpPanelVisible(false);

		yield return ShowCenterMessage("3", 0.65f, 108, true);
		yield return ShowCenterMessage("2", 0.65f, 108, true);
		yield return ShowCenterMessage("1", 0.65f, 108, true);
		yield return ShowCenterMessage("START", 0.9f, 92, false);

		SetMainHudVisible(true);
		HideCenterMessage();
	}

	public IEnumerator PlayFinishSequence()
	{
		EnsureSupportUI();
		SetHelpPanelVisible(false);
		yield return ShowCenterMessage("FINISH", 1.1f, 92, false);
		HideCenterMessage();
	}

	void UpdateHUD(float electricityCost, float remainingTime)
	{
		if (_electricityCostText != null)
		{
			_electricityCostText.text = $"COST  {Mathf.FloorToInt(electricityCost):N0}円";
			_electricityCostText.color = electricityCost >= 25000f
				? new Color(1f, 0.45f, 0.45f)
				: new Color(0.82f, 1f, 0.67f);
		}

		if (_remainingTimeText != null)
		{
			int minutes = Mathf.FloorToInt(remainingTime / 60f);
			int seconds = Mathf.FloorToInt(remainingTime % 60f);
			_remainingTimeText.text = $"TIME  {minutes:00}:{seconds:00}";
			_remainingTimeText.color = remainingTime <= 15f
				? new Color(1f, 0.55f, 0.55f)
				: new Color(0.82f, 1f, 0.67f);
		}

		if (_statusText != null && RoomManager.Instance != null)
		{
			int openRooms = RoomManager.Instance.GetOpenRoomCount();
			if (openRooms <= 0)
			{
				_statusText.text = "全部屋消灯中";
				_statusText.color = new Color(0.63f, 1f, 0.82f);
			}
			else
			{
				_statusText.text = $"点灯中の部屋  {openRooms}";
				_statusText.color = openRooms >= 4
					? new Color(1f, 0.6f, 0.45f)
					: new Color(0.96f, 0.92f, 0.65f);
			}
		}
	}

	IEnumerator ShowCenterMessage(string message, float duration, int fontSize, bool warningColor)
	{
		if (_centerMessageText == null)
		{
			yield break;
		}

		_centerMessageText.gameObject.SetActive(true);
		_centerMessageText.text = message;
		_centerMessageText.fontSize = fontSize;
		_centerMessageText.color = warningColor
			? new Color(1f, 0.92f, 0.65f)
			: new Color(0.87f, 0.97f, 1f);

		yield return new WaitForSeconds(duration);
	}

	void EnsureSupportUI()
	{
		RectTransform root = transform as RectTransform;
		if (root == null)
		{
			return;
		}

		Font font = ResolveFont();

		if (_objectiveText == null)
		{
			_objectiveText = CreateOverlayText(
				"ObjectiveText",
				root,
				font,
				26,
				FontStyle.Normal,
				TextAnchor.UpperCenter,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -34f),
				new Vector2(880f, 50f));
			_objectiveText.text = "消し忘れた部屋を見つけて、Eキー長押しで電気を消す";
			_objectiveText.color = new Color(0.9f, 0.94f, 1f);
		}

		if (_statusText == null)
		{
			_statusText = CreateOverlayText(
				"StatusText",
				root,
				font,
				24,
				FontStyle.Normal,
				TextAnchor.UpperCenter,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -74f),
				new Vector2(520f, 40f));
			_statusText.color = new Color(0.96f, 0.92f, 0.65f);
		}

		if (_centerMessageText == null)
		{
			_centerMessageText = CreateOverlayText(
				"CenterMessageText",
				root,
				font,
				108,
				FontStyle.Bold,
				TextAnchor.MiddleCenter,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0f, 30f),
				new Vector2(900f, 180f));
			_centerMessageText.gameObject.SetActive(false);
		}

		if (_helpButton == null)
		{
			_helpButton = CreateButton("HelpButton", root, font, "説明", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-86f, -54f), new Vector2(120f, 44f));
			_helpButton.onClick.AddListener(ToggleHelpPanel);
			_helpButtonLabel = _helpButton.GetComponentInChildren<Text>();
		}

		if (_helpPanel == null)
		{
			_helpPanel = CreatePanel("HelpPanel", root, new Color(0.05f, 0.08f, 0.14f, 0.94f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-210f, -180f), new Vector2(360f, 250f));
			_helpPanelText = CreateOverlayText(
				"HelpPanelText",
				_helpPanel.transform as RectTransform,
				font,
				24,
				FontStyle.Normal,
				TextAnchor.UpperLeft,
				new Vector2(0f, 0f),
				new Vector2(1f, 1f),
				new Vector2(0f, 0f),
				new Vector2(-36f, -36f));
			_helpPanelText.rectTransform.offsetMin = new Vector2(18f, 18f);
			_helpPanelText.rectTransform.offsetMax = new Vector2(-18f, -18f);
			_helpPanelText.text =
				"目的\n消し忘れた部屋の電気を消し、電気代を抑える\n\n操作\nWASD : 移動\nE 長押し : 消灯\nR : 初期位置に戻る\n\nポイント\n点灯中の部屋が多いほど電気代が増える";
			_helpPanelText.color = new Color(0.88f, 0.93f, 1f);
			_helpPanel.SetActive(false);
		}
	}

	void ToggleHelpPanel()
	{
		if (_helpPanel == null)
		{
			return;
		}

		SetHelpPanelVisible(!_helpPanel.activeSelf);
	}

	void SetHelpPanelVisible(bool visible)
	{
		if (_helpPanel != null)
		{
			_helpPanel.SetActive(visible);
		}

		if (_helpButtonLabel != null)
		{
			_helpButtonLabel.text = visible ? "閉じる" : "説明";
		}
	}

	void SetMainHudVisible(bool visible)
	{
		if (_electricityCostText != null) _electricityCostText.gameObject.SetActive(visible);
		if (_remainingTimeText != null) _remainingTimeText.gameObject.SetActive(visible);
		if (_objectiveText != null) _objectiveText.gameObject.SetActive(visible);
		if (_statusText != null) _statusText.gameObject.SetActive(visible);
		if (_helpButton != null) _helpButton.gameObject.SetActive(visible);
	}

	void HideCenterMessage()
	{
		if (_centerMessageText != null)
		{
			_centerMessageText.gameObject.SetActive(false);
		}
	}

	Text CreateOverlayText(
		string objectName,
		RectTransform parent,
		Font font,
		int fontSize,
		FontStyle fontStyle,
		TextAnchor anchor,
		Vector2 anchorMin,
		Vector2 anchorMax,
		Vector2 anchoredPosition,
		Vector2 sizeDelta)
	{
		GameObject go = new GameObject(objectName);
		go.transform.SetParent(parent, false);

		var rect = go.AddComponent<RectTransform>();
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = sizeDelta;

		var text = go.AddComponent<Text>();
		text.font = font;
		text.fontSize = fontSize;
		text.fontStyle = fontStyle;
		text.alignment = anchor;
		text.horizontalOverflow = HorizontalWrapMode.Wrap;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		return text;
	}

	Button CreateButton(
		string objectName,
		RectTransform parent,
		Font font,
		string label,
		Vector2 anchorMin,
		Vector2 anchorMax,
		Vector2 anchoredPosition,
		Vector2 sizeDelta)
	{
		GameObject buttonObject = CreatePanel(objectName, parent, new Color(0.12f, 0.18f, 0.29f, 0.96f), anchorMin, anchorMax, anchoredPosition, sizeDelta);
		var button = buttonObject.AddComponent<Button>();

		ColorBlock colors = button.colors;
		colors.normalColor = new Color(0.12f, 0.18f, 0.29f, 0.96f);
		colors.highlightedColor = new Color(0.2f, 0.29f, 0.42f, 1f);
		colors.pressedColor = new Color(0.08f, 0.12f, 0.2f, 1f);
		colors.selectedColor = colors.highlightedColor;
		colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
		button.colors = colors;

		Text text = CreateOverlayText(
			"Label",
			buttonObject.transform as RectTransform,
			font,
			24,
			FontStyle.Bold,
			TextAnchor.MiddleCenter,
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			Vector2.zero,
			Vector2.zero);
		text.text = label;
		text.color = Color.white;
		return button;
	}

	GameObject CreatePanel(
		string objectName,
		RectTransform parent,
		Color color,
		Vector2 anchorMin,
		Vector2 anchorMax,
		Vector2 anchoredPosition,
		Vector2 sizeDelta)
	{
		GameObject go = new GameObject(objectName);
		go.transform.SetParent(parent, false);

		var rect = go.AddComponent<RectTransform>();
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = sizeDelta;

		var image = go.AddComponent<Image>();
		image.color = color;
		return go;
	}

	Font ResolveFont()
	{
		if (_electricityCostText != null && _electricityCostText.font != null)
		{
			return _electricityCostText.font;
		}

		if (_remainingTimeText != null && _remainingTimeText.font != null)
		{
			return _remainingTimeText.font;
		}

		return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
	}
}
