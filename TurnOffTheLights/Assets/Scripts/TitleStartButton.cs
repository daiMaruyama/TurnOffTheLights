using Cysharp.Threading.Tasks;
using GameJamCore;
using GameJamScene;
using UnityEngine;
using UnityEngine.UI;

public class TitleStartButton : MonoBehaviour
{
	[SerializeField] string _nextSceneName = "InGame";
	[SerializeField] Vector2 _buttonPosition = new Vector2(0f, -165f);
	[SerializeField] Vector2 _buttonSize = new Vector2(320f, 72f);
	[SerializeField] Vector2 _titlePosition = new Vector2(0f, 210f);
	[SerializeField] Vector2 _titleSize = new Vector2(920f, 200f);
	[SerializeField] Vector2 _subtitlePosition = new Vector2(0f, 70f);
	[SerializeField] Vector2 _subtitleSize = new Vector2(860f, 90f);
	[SerializeField] Vector2 _hintPosition = new Vector2(0f, -270f);
	[SerializeField] Vector2 _hintSize = new Vector2(720f, 40f);

	Button _button;
	Text _buttonText;
	Image _buttonImage;
	RectTransform _runtimeRoot;
	Image _heroBandImage;
	Text _titleText;
	Text _subtitleText;
	Text _hintText;

	void Start()
	{
		Debug.Log($"[TitleStartButton] Start called on {gameObject.name}");
		_button = FindAnyObjectByType<Button>(FindObjectsInactive.Include);
		Debug.Log($"[TitleStartButton] Button found: {(_button != null ? _button.name : "NULL")}");
		if (_button != null)
		{
			_button.gameObject.SetActive(true);
			_button.interactable = true;
			_button.onClick.RemoveListener(OnStartButtonClicked);
			_button.onClick.AddListener(OnStartButtonClicked);
			_buttonText = _button.GetComponentInChildren<Text>(true);
			_buttonImage = _button.GetComponent<Image>();

			if (_buttonImage != null)
			{
				_buttonImage.enabled = true;
			}
			if (_buttonText != null)
			{
				_buttonText.enabled = true;
			}
		}

		BuildTitlePresentation();
		Debug.Log($"[TitleStartButton] BuildTitlePresentation done. _runtimeRoot={(_runtimeRoot == null ? "NULL" : _runtimeRoot.name)}");
	}

	void Update()
	{
		float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.5f;

		if (_buttonImage != null)
		{
			_buttonImage.color = Color.Lerp(
				new Color(0.1f, 0.14f, 0.22f, 0.92f),
				new Color(0.22f, 0.31f, 0.48f, 1f),
				pulse);
		}

		if (_buttonText != null)
		{
			_buttonText.color = Color.Lerp(
				new Color(0.85f, 0.92f, 1f),
				Color.white,
				pulse);
		}

		if (_hintText != null)
		{
			Color hintColor = _hintText.color;
			hintColor.a = Mathf.Lerp(0.45f, 0.95f, pulse);
			_hintText.color = hintColor;
		}
	}

	public void OnStartButtonClicked()
	{
		if (_button != null)
		{
			_button.interactable = false;
		}
		if (_buttonImage != null)
		{
			_buttonImage.enabled = false;
		}
		if (_buttonText != null)
		{
			_buttonText.enabled = false;
		}
		LoadNextSceneAsync().Forget();
	}

	async UniTaskVoid LoadNextSceneAsync()
	{
		CleanupRuntimePresentation();
		await ServiceLocator.Get<ISceneService>().LoadAsync(_nextSceneName);
	}

	void BuildTitlePresentation()
	{
		Canvas canvas = _button != null ? _button.GetComponentInParent<Canvas>() : null;
		if (canvas == null)
		{
			return;
		}

		ConfigureCanvas(canvas);
		Font font = ResolveFont();
		_runtimeRoot = CreateOrFindRoot(canvas.transform);
		EnsureHeroBand();
		ConfigureButtonLayout();

		if (_buttonText != null)
		{
			_buttonText.text = "START SHIFT";
			_buttonText.fontSize = 38;
			_buttonText.fontStyle = FontStyle.Bold;
			_buttonText.alignment = TextAnchor.MiddleCenter;
		}

		if (_buttonImage != null)
		{
			_buttonImage.type = Image.Type.Sliced;
			_buttonImage.color = new Color(0.12f, 0.18f, 0.29f, 0.96f);
		}

		_titleText = CreateOrFindText("RuntimeTitleText", _runtimeRoot, font, 88, FontStyle.Bold, TextAnchor.MiddleCenter);
		_titleText.text = "Turn Off\nThe Lights";
		_titleText.color = new Color(0.94f, 0.97f, 1f);
		SetRect(_titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _titlePosition, _titleSize);

		_subtitleText = CreateOrFindText("RuntimeSubtitleText", _runtimeRoot, font, 28, FontStyle.Normal, TextAnchor.MiddleCenter);
		_subtitleText.text = "子どもがつけた電気を消して、電気代を抑えろ。";
		_subtitleText.color = new Color(0.76f, 0.84f, 0.96f);
		SetRect(_subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _subtitlePosition, _subtitleSize);

		_hintText = CreateOrFindText("RuntimeHintText", _runtimeRoot, font, 24, FontStyle.Italic, TextAnchor.MiddleCenter);
		_hintText.text = "WASD で移動  /  E 長押しで消灯";
		_hintText.color = new Color(0.92f, 0.95f, 1f, 0.75f);
		SetRect(_hintText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _hintPosition, _hintSize);
	}

	void CleanupRuntimePresentation()
	{
		if (_runtimeRoot != null)
		{
			Destroy(_runtimeRoot.gameObject);
			_runtimeRoot = null;
		}
	}

	RectTransform CreateOrFindRoot(Transform parent)
	{
		Transform existing = parent.Find("RuntimeTitleUI");
		GameObject go = existing != null ? existing.gameObject : new GameObject("RuntimeTitleUI");
		if (existing == null)
		{
			go.transform.SetParent(parent, false);
			go.AddComponent<RectTransform>();
		}

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		return rect;
	}

	void ConfigureCanvas(Canvas canvas)
	{
		CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
		if (scaler == null)
		{
			return;
		}

		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920f, 1080f);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;
	}

	void EnsureHeroBand()
	{
		Transform existing = _runtimeRoot.Find("HeroBand");
		GameObject go = existing != null ? existing.gameObject : new GameObject("HeroBand");
		if (existing == null)
		{
			go.transform.SetParent(_runtimeRoot, false);
			go.AddComponent<RectTransform>();
		}

		RectTransform rect = go.GetComponent<RectTransform>();
		SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1120f, 620f));

		if (!go.TryGetComponent(out _heroBandImage))
		{
			_heroBandImage = go.AddComponent<Image>();
		}
		_heroBandImage.color = new Color(0.04f, 0.07f, 0.12f, 0.38f);
		_heroBandImage.raycastTarget = false;
	}

	void ConfigureButtonLayout()
	{
		if (_button == null)
		{
			return;
		}

		RectTransform buttonRect = _button.GetComponent<RectTransform>();
		SetRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _buttonPosition, _buttonSize);
	}

	Text CreateOrFindText(string objectName, Transform parent, Font font, int fontSize, FontStyle fontStyle, TextAnchor alignment)
	{
		Transform existing = parent.Find(objectName);
		GameObject go = existing != null ? existing.gameObject : new GameObject(objectName);
		if (existing == null)
		{
			go.transform.SetParent(parent, false);
			go.AddComponent<RectTransform>();
		}

		Text text = go.GetComponent<Text>();
		if (text == null)
		{
			text = go.AddComponent<Text>();
		}

		text.font = font;
		text.fontSize = fontSize;
		text.fontStyle = fontStyle;
		text.alignment = alignment;
		text.horizontalOverflow = HorizontalWrapMode.Wrap;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.raycastTarget = false;
		return text;
	}

	void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
	{
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = sizeDelta;
	}

	Font ResolveFont()
	{
		if (_buttonText != null && _buttonText.font != null)
		{
			return _buttonText.font;
		}

		return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
	}
}
