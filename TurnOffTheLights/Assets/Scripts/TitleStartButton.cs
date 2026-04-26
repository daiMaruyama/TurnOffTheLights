using UnityEngine;
using Cysharp.Threading.Tasks;
using GameJamCore;
using GameJamScene;

public class TitleStartButton : MonoBehaviour
{
	[SerializeField] string _nextSceneName = "InGame";

	public void OnStartButtonClicked()
	{
		LoadNextSceneAsync().Forget();
	}

	async UniTaskVoid LoadNextSceneAsync()
	{
		await ServiceLocator.Get<ISceneService>().LoadAsync(_nextSceneName);
	}
}
