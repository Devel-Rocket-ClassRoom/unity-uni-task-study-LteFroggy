using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FadeController : MonoBehaviour {
	private CanvasGroup canvasGroup;
	
	public float fadeDuration = 1.0f;
	
	private static FadeController instance;

	private void Awake() {
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		
		instance = this;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		if (instance == this) {
			instance = null;
		}
	}
	
	public async UniTask FadeIn(float duration = -1f) {
		await FadeAsync(canvasGroup, 1, 0, 
			duration < 0 ? fadeDuration : duration);
	}
	
	public async UniTask FadeOut(float duration = -1f) {
		await FadeAsync(canvasGroup, 0, 1, 
			duration < 0 ? fadeDuration : duration);
	}
	
	private async UniTask FadeAsync(CanvasGroup canvasGroup, float from, float to, float duration) {
		canvasGroup.alpha = from;
		float elapsed = 0f;
		
		while (elapsed < duration) {
			elapsed += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
			
			await UniTask.Yield();
		}
	}
	
}
