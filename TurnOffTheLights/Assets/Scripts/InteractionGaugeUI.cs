using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのドアインタラクション中に表示するゲージUI。
/// PlayerInteraction.HoldProgress を読んで fillAmount に反映する。
/// hover 中のみ表示される。
/// _root が設定されていれば SetActive で全体を、未設定なら Image.enabled で制御する。
/// </summary>
public class InteractionGaugeUI : MonoBehaviour
{
	[Header("References")]
	[Tooltip("対象のPlayerInteraction")]
	[SerializeField] PlayerInteraction _playerInteraction;

	[Tooltip("fillAmountを更新するImage（Image Type: Filled）")]
	[SerializeField] Image _gaugeImage;

	[Tooltip("ゲージ全体のRoot。SetActiveで切り替える。スクリプトと同じGameObjectは指定不可")]
	[SerializeField] GameObject _root;

	void Awake()
	{
		if (_root == gameObject)
		{
			Debug.LogError("InteractionGaugeUI: _root をこのスクリプトと同じGameObjectに設定すると自滅します。別オブジェクトを指定してください。", this);
			_root = null;
		}
	}

	void LateUpdate()
	{
		if (_playerInteraction == null || _gaugeImage == null)
		{
			return;
		}

		bool isHovering = _playerInteraction.HoveredRoomIndex >= 0;

		_gaugeImage.fillAmount = isHovering ? _playerInteraction.HoldProgress : 0f;

		if (_root != null)
		{
			if (_root.activeSelf != isHovering)
			{
				_root.SetActive(isHovering);
			}
		}
		else
		{
			if (_gaugeImage.enabled != isHovering)
			{
				_gaugeImage.enabled = isHovering;
			}
		}
	}
}
