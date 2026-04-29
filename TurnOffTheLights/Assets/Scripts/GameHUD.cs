using System.Collections;
using System.Collections.Generic;
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

	[Header("Layout")]
	[SerializeField] Vector2 _timePosition = new Vector2(-160f, -34f);
	[SerializeField] Vector2 _timeSize = new Vector2(280f, 48f);
	[SerializeField] int _timeFontSize = 52;

	[SerializeField] Vector2 _costPosition = new Vector2(-160f, -82f);
	[SerializeField] Vector2 _costSize = new Vector2(340f, 48f);
	[SerializeField] int _costFontSize = 42;

	[SerializeField] Vector2 _objectivePosition = new Vector2(210f, -36f);
	[SerializeField] Vector2 _objectiveSize = new Vector2(760f, 40f);
	[SerializeField] int _objectiveFontSize = 24;

	[SerializeField] Vector2 _statusPosition = new Vector2(210f, -74f);
	[SerializeField] Vector2 _statusSize = new Vector2(420f, 34f);
	[SerializeField] int _statusFontSize = 22;

	[SerializeField] Vector2 _helpButtonPosition = new Vector2(-132f, -134f);
	[SerializeField] Vector2 _helpButtonSize = new Vector2(132f, 42f);

	[SerializeField] Vector2 _helpPanelPosition = new Vector2(-222f, -312f);
	[SerializeField] Vector2 _helpPanelSize = new Vector2(340f, 240f);
	[SerializeField] int _helpPanelFontSize = 28;

	[SerializeField] Vector2 _miniMapFramePosition = new Vector2(-150f, -274f);
	[SerializeField] Vector2 _miniMapFrameSize = new Vector2(224f, 224f);
	[SerializeField] Vector2 _miniMapViewportPosition = new Vector2(0f, 6f);
	[SerializeField] Vector2 _miniMapViewportSize = new Vector2(188f, 164f);
	[SerializeField] float _miniMapOrthoSize = 18f;
	[SerializeField] bool _rotateMiniMapWithPlayer = true;
	[SerializeField] Vector2 _miniMapMarkerSize = new Vector2(12f, 12f);

	Text _objectiveText;
	Text _statusText;
	Text _centerMessageText;
	Button _helpButton;
	GameObject _helpPanel;
	Text _helpPanelText;
	Text _helpButtonLabel;
	GameObject _miniMapFrame;
	RawImage _miniMapImage;
	Camera _miniMapCamera;
	RenderTexture _miniMapTexture;
	Transform _miniMapTarget;
	RectTransform _miniMapViewportRect;
	readonly List<Image> _miniMapRoomMarkers = new List<Image>();

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

		if (_miniMapTexture != null)
		{
			_miniMapTexture.Release();
		}
	}

	void LateUpdate()
	{
		UpdateMiniMapCamera();
		UpdateMiniMapRoomMarkers();
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

		ConfigureCanvasScaler();

		Font font = ResolveFont();
		ConfigureHudLayout();

		if (_objectiveText == null)
		{
			_objectiveText = CreateOverlayText(
				"ObjectiveText",
				root,
				font,
				_objectiveFontSize,
				FontStyle.Normal,
				TextAnchor.UpperLeft,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				_objectivePosition,
				_objectiveSize);
			_objectiveText.text = "子どもがつけた電気を消して、電気代を抑えろ。";
			_objectiveText.color = new Color(0.9f, 0.94f, 1f);
		}

		if (_statusText == null)
		{
			_statusText = CreateOverlayText(
				"StatusText",
				root,
				font,
				_statusFontSize,
				FontStyle.Normal,
				TextAnchor.UpperLeft,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				_statusPosition,
				_statusSize);
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
			_helpButton = CreateButton("HelpButton", root, font, "ルール", new Vector2(1f, 1f), new Vector2(1f, 1f), _helpButtonPosition, _helpButtonSize);
			_helpButton.onClick.AddListener(ToggleHelpPanel);
			_helpButtonLabel = _helpButton.GetComponentInChildren<Text>();
		}

		if (_helpPanel == null)
		{
			_helpPanel = CreatePanel("HelpPanel", root, new Color(0.05f, 0.08f, 0.14f, 0.94f), new Vector2(1f, 1f), new Vector2(1f, 1f), _helpPanelPosition, _helpPanelSize);
			_helpPanelText = CreateOverlayText(
				"HelpPanelText",
				_helpPanel.transform as RectTransform,
				font,
				_helpPanelFontSize,
				FontStyle.Normal,
				TextAnchor.UpperLeft,
				new Vector2(0f, 0f),
				new Vector2(1f, 1f),
				Vector2.zero,
				Vector2.zero);
			RectTransform helpTextRect = _helpPanelText.rectTransform;
			helpTextRect.pivot = new Vector2(0.5f, 0.5f);
			helpTextRect.offsetMin = new Vector2(20f, 20f);
			helpTextRect.offsetMax = new Vector2(-20f, -20f);
			_helpPanelText.horizontalOverflow = HorizontalWrapMode.Wrap;
			_helpPanelText.verticalOverflow = VerticalWrapMode.Truncate;
			_helpPanelText.lineSpacing = 1.15f;
			_helpPanelText.resizeTextForBestFit = true;
			_helpPanelText.resizeTextMaxSize = _helpPanelFontSize;
			_helpPanelText.resizeTextMinSize = 18;
			_helpPanelText.text =
				"目的\n子どもがつけた電気を消して、電気代を抑えろ。\n\n操作\nWASD : 移動\nE 長押し : 消灯\nR : 初期位置に戻る\n\nポイント\n点灯中の部屋が多いほど電気代が増える";
			_helpPanelText.color = new Color(0.88f, 0.93f, 1f);
			_helpPanel.SetActive(false);
		}

		if (_miniMapFrame == null)
		{
			EnsureMiniMap(root, font);
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
			_helpButtonLabel.text = visible ? "閉じる" : "ルール";
		}
	}

	void SetMainHudVisible(bool visible)
	{
		if (_electricityCostText != null) _electricityCostText.gameObject.SetActive(visible);
		if (_remainingTimeText != null) _remainingTimeText.gameObject.SetActive(visible);
		if (_objectiveText != null) _objectiveText.gameObject.SetActive(visible);
		if (_statusText != null) _statusText.gameObject.SetActive(visible);
		if (_helpButton != null) _helpButton.gameObject.SetActive(visible);
		if (_miniMapFrame != null) _miniMapFrame.SetActive(visible);
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

	GameObject CreateRawImagePanel(
		string objectName,
		RectTransform parent,
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

		go.AddComponent<RawImage>();
		return go;
	}

	void EnsureMiniMap(RectTransform root, Font font)
	{
		_miniMapFrame = CreatePanel("MiniMapFrame", root, new Color(0.05f, 0.08f, 0.14f, 0.92f), new Vector2(1f, 1f), new Vector2(1f, 1f), _miniMapFramePosition, _miniMapFrameSize);

		Text mapLabel = CreateOverlayText(
			"MiniMapLabel",
			_miniMapFrame.transform as RectTransform,
			font,
			18,
			FontStyle.Bold,
			TextAnchor.UpperCenter,
			new Vector2(0.5f, 1f),
			new Vector2(0.5f, 1f),
			new Vector2(0f, -16f),
			new Vector2(120f, 24f));
		mapLabel.text = "MAP";
		mapLabel.color = new Color(0.9f, 0.95f, 1f);

		GameObject viewport = CreateRawImagePanel("MiniMapViewport", _miniMapFrame.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _miniMapViewportPosition, _miniMapViewportSize);
		_miniMapImage = viewport.GetComponent<RawImage>();
		_miniMapViewportRect = viewport.GetComponent<RectTransform>();
		_miniMapImage.color = Color.white;

		_miniMapTexture = new RenderTexture(512, 512, 16)
		{
			name = "MiniMapTexture"
		};
		_miniMapImage.texture = _miniMapTexture;

		GameObject cameraObject = new GameObject("MiniMapCamera");
		_miniMapCamera = cameraObject.AddComponent<Camera>();
		_miniMapCamera.orthographic = true;
		_miniMapCamera.orthographicSize = _miniMapOrthoSize;
		_miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
		_miniMapCamera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
		_miniMapCamera.cullingMask = ~0;
		_miniMapCamera.targetTexture = _miniMapTexture;
		_miniMapCamera.nearClipPlane = 0.3f;
		_miniMapCamera.farClipPlane = 200f;
		_miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		_miniMapCamera.enabled = true;

		BuildMiniMapRoomMarkers();
	}

	void ConfigureHudLayout()
	{
		if (_remainingTimeText != null)
		{
			ConfigureExistingText(
				_remainingTimeText,
				TextAnchor.UpperRight,
				new Vector2(1f, 1f),
				new Vector2(1f, 1f),
				_timePosition,
				_timeSize,
				_timeFontSize);
		}

		if (_electricityCostText != null)
		{
			ConfigureExistingText(
				_electricityCostText,
				TextAnchor.UpperRight,
				new Vector2(1f, 1f),
				new Vector2(1f, 1f),
				_costPosition,
				_costSize,
				_costFontSize);
		}
	}

	void ConfigureCanvasScaler()
	{
		CanvasScaler scaler = GetComponent<CanvasScaler>();
		if (scaler == null)
		{
			return;
		}

		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920f, 1080f);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;
	}

	void ConfigureExistingText(
		Text text,
		TextAnchor alignment,
		Vector2 anchorMin,
		Vector2 anchorMax,
		Vector2 anchoredPosition,
		Vector2 sizeDelta,
		int fontSize)
	{
		RectTransform rect = text.rectTransform;
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = sizeDelta;
		text.alignment = alignment;
		text.fontSize = fontSize;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
	}

	void UpdateMiniMapCamera()
	{
		if (_miniMapCamera == null)
		{
			return;
		}

		if (_miniMapTarget == null)
		{
			_miniMapTarget = ResolveMiniMapTarget();
			if (_miniMapTarget == null)
			{
				return;
			}
		}

		Vector3 targetPosition = _miniMapTarget.position;
		_miniMapCamera.transform.position = new Vector3(targetPosition.x, targetPosition.y + 35f, targetPosition.z);

		float yaw = _rotateMiniMapWithPlayer ? _miniMapTarget.eulerAngles.y : 0f;
		_miniMapCamera.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
	}

	void BuildMiniMapRoomMarkers()
	{
		_miniMapRoomMarkers.Clear();

		if (_miniMapViewportRect == null || RoomManager.Instance == null)
		{
			return;
		}

		for (int i = 0; i < RoomManager.Instance.Rooms.Count; i++)
		{
			GameObject markerObject = new GameObject($"RoomMarker_{i + 1}");
			markerObject.transform.SetParent(_miniMapViewportRect, false);

			RectTransform rect = markerObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = _miniMapMarkerSize;

			Image markerImage = markerObject.AddComponent<Image>();
			markerImage.color = new Color(0.35f, 0.42f, 0.56f, 0.75f);
			_miniMapRoomMarkers.Add(markerImage);
		}
	}

	void UpdateMiniMapRoomMarkers()
	{
		if (_miniMapCamera == null || _miniMapViewportRect == null || RoomManager.Instance == null)
		{
			return;
		}

		var rooms = RoomManager.Instance.Rooms;
		int markerCount = Mathf.Min(rooms.Count, _miniMapRoomMarkers.Count);

		for (int i = 0; i < markerCount; i++)
		{
			RoomData room = rooms[i];
			Image marker = _miniMapRoomMarkers[i];
			if (room == null || room.Door == null || marker == null)
			{
				continue;
			}

			Vector3 viewportPoint = _miniMapCamera.WorldToViewportPoint(room.Door.position);
			bool visible = viewportPoint.z > 0f;
			marker.gameObject.SetActive(visible);
			if (!visible)
			{
				continue;
			}

			RectTransform markerRect = marker.rectTransform;
			markerRect.anchorMin = new Vector2(viewportPoint.x, viewportPoint.y);
			markerRect.anchorMax = new Vector2(viewportPoint.x, viewportPoint.y);
			markerRect.anchoredPosition = Vector2.zero;

			bool isLit = room.RoomLight != null && room.RoomLight.enabled;
			if (!isLit)
			{
				marker.color = new Color(1f, 1f, 1f, 0f);
				markerRect.sizeDelta = _miniMapMarkerSize;
				continue;
			}

			marker.color = new Color(1f, 0.87f, 0.25f, 0.95f);
			markerRect.sizeDelta = _miniMapMarkerSize * 1.35f;
		}
	}

	Transform ResolveMiniMapTarget()
	{
		PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
		if (playerMovement != null)
		{
			return playerMovement.transform;
		}

		if (Camera.main != null)
		{
			return Camera.main.transform;
		}

		return null;
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
