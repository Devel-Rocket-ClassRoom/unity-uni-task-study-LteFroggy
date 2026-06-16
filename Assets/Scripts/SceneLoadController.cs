using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadController : MonoBehaviour {
	[SerializeField] private Button _loadButton;
	[SerializeField] private Slider _loadSlider;
	[SerializeField] private TextMeshProUGUI _loadText;
	[SerializeField] private string targetSceneName;
	
	private FadeController fadeController;

	private const float minimumLoadTime = 2f;
	
	private bool isLoading = false;

	private void Start() {
		_loadButton.onClick.AddListener(OnLoadSceneClicked);
		
		fadeController = FindFirstObjectByType<FadeController>();
		gameObject.SetActive(false);
	}

	private void OnLoadSceneClicked() {
		Debug.Log($"버튼 눌렸어요");
		LoadSceneWithFadeAsync().Forget();
	}
	
	private async UniTask LoadSceneWithFadeAsync() {
		isLoading = true;
		_loadButton.interactable = false;
		await fadeController.FadeOut();
		
		gameObject.SetActive(true);
		
		await LoadSceneAsync();
		
		await UniTask.Delay(300);
		
		await fadeController.FadeIn();
	}
	
	private async UniTask LoadSceneAsync() {
		var progress = Progress.Create<float>(p => {
			_loadSlider.value = p;
			_loadText.text = $"로딩 중... {(int)(p * 100)}%";
		});
		
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
		asyncOperation.allowSceneActivation = false;
		
		float elapsedTime = 0f;
		
		// asyncOperation의 allowSceneActivation을 false로 해두면 진행률이 0.9에서 막힌다. 따라서 0.9면 로딩 다 된 것
		// 그런데 너무 빨라서 로딩 진행률이 잘 안보이므로, minimumLadTime을 이용해 최소 2초는 소요되도록.
		while (elapsedTime < minimumLoadTime || asyncOperation.progress < 0.9f) {
			elapsedTime += Time.deltaTime;
			progress.Report(Mathf.Min(elapsedTime / minimumLoadTime, asyncOperation.progress / 0.9f));
			await UniTask.Yield();
		}
		
		progress.Report(1f);
		_loadText.text = $"완료!";
		asyncOperation.allowSceneActivation = true;
	}
}
