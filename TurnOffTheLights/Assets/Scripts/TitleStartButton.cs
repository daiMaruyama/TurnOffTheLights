using Cysharp.Threading.Tasks;
using GameJamCore;
using GameJamScene;
using UnityEngine;
using UnityEngine.UI;

public class TitleStartButton : MonoBehaviour
{
	[SerializeField] string _nextSceneName = "InGame";

	Button _button;
	Text _buttonText;
	Image _buttonImage;
	RectTransform _runtimeRoot;
	Text _titleText;
	Text _subtitleText;
	Text _hintText;

	void Start()
	{
		_button = FindAnyObjectByType<Button>();
		if (_button != null)
		{
			_buttonText = _button.GetComponentInChildren<Text>();
			_buttonImage = _button.GetComponent<Image>();
		}

		BuildTitlePresentation();
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

		Font font = ResolveFont();
		_runtimeRoot = CreateOrFindRoot(canvas.transform);

		if (_buttonText != null)
		{
			_buttonText.text = "START SHIFT";
			_buttonText.fontSize = 40;
			_buttonText.fontStyle = FontStyle.Bold;
		}

		if (_buttonImage != null)
		{
			_buttonImage.type = Image.Type.Sliced;
		}

		_titleText = CreateOrFindText("RuntimeTitleText", _runtimeRoot, font, 82, FontStyle.Bold, TextAnchor.UpperCenter);
		_titleText.text = "Turn Off\nThe Lights";
		_titleText.color = new Color(0.94f, 0.97f, 1f);
		SetRect(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(900f, 190f));

		_subtitleText = CreateOrFindText("RuntimeSubtitleText", _runtimeRoot, font, 28, FontStyle.Normal, TextAnchor.UpperCenter);
		_subtitleText.text = "子どもたちが次々と電気をつける夜の寮を、\n時間内に見回って節電するステルス清掃ゲーム";
		_subtitleText.color = new Color(0.76f, 0.84f, 0.96f);
		SetRect(_subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(920f, 90f));

		_hintText = CreateOrFindText("RuntimeHintText", _runtimeRoot, font, 24, FontStyle.Italic, TextAnchor.LowerCenter);
		_hintText.text = "WASD で移動  /  E 長押しで消灯";
		_hintText.color = new Color(0.92f, 0.95f, 1f, 0.75f);
		SetRect(_hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(720f, 40f));
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
