using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour {
	[Header("=== 버튼 입력받기 ===")]
	[SerializeField] private LoadButton _loadButton;
	[SerializeField] private Button _cancelButton;
	
	[Header("=== 로딩바들 등록 ===")]
	[SerializeField] private LoadingBar[] _loadingBars;
	[SerializeField] private LoadingBar _totalLoadingBar;
	
	private readonly float[] _loadingBarDurations = { 3f, 4f, 5f };
	private readonly float[] _loadingBarElapsed = new float[3];
	
	private CancellationTokenSource _loadingCts;

	private void Start() {
		foreach (var bar in _loadingBars) { bar.Init(); }
		_totalLoadingBar.Init();
		
		// 시작하면 클릭 가능 및 로딩 시작 버튼으로 변경
		_loadButton.SetButtonEnableState(true);
		_loadButton.ResetButton();
		
		for (int i = 0; i < _loadingBars.Length; i++) {
			_loadingBarElapsed[i] = 0f;
		}
		
		_loadButton.AddListener(OnStartButtonPressed);
		_cancelButton.onClick.AddListener(OnCancelButtonPressed);
	}
	
	private void OnStartButtonPressed() {
		_loadButton.SetAsResumeButton();
		
		_loadingCts?.Cancel();
		_loadingCts?.Dispose();
		_loadingCts = new CancellationTokenSource();
		
		StartLoading(_loadingCts.Token).Forget();
	}
	
	private async UniTask StartLoading(CancellationToken ct) {
		await UniTask.WhenAll(
			StartLoadingBarAsync(ct, 0),
			StartLoadingBarAsync(ct, 1),
			StartLoadingBarAsync(ct, 2)
		);
	}
	
	private async UniTask StartLoadingBarAsync(CancellationToken ct, int index) {
		var progress = Progress.Create<float>(p => {
			_loadingBars[index].UpdateLoadingBar(index, p);
			UpdateTotalProgressBar();
		});
		
		try {
			// 자기 자신이 남은 시간 확인.
			while (_loadingBarElapsed[index] < _loadingBarDurations[index]) {
				ct.ThrowIfCancellationRequested();
				_loadingBarElapsed[index] += Time.deltaTime;
				
				progress.Report(Mathf.Lerp(0f, 1f, _loadingBarElapsed[index] / _loadingBarDurations[index]));
				
				await UniTask.Yield(ct);
			}	
		} catch (OperationCanceledException) {
			Debug.Log($"취소됨");
		}
	}
	
	private void OnCancelButtonPressed() {
		_loadingCts?.Cancel();
		_loadingCts?.Dispose();
		_loadingCts = null;
		
		// 리셋 버튼 누르면 클릭 가능 및 로딩 시작 버튼으로 변경
		_loadButton.SetButtonEnableState(true);
		_loadButton.ResetButton();
		
		// 로딩바 초기화
		for (int i = 0; i < _loadingBars.Length; i++) {
			_loadingBarElapsed[i] = 0f;
			_loadingBars[i].UpdateLoadingBar(i, 0f);
		}
		_totalLoadingBar.UpdateLoadingBar(-1, 0f);
	}
	
	private void UpdateTotalProgressBar() {
		// 각각의 진행률을 모두 더한다.
		float sum = 0f;
		for (int i = 0; i < 3; i++) { sum += _loadingBarElapsed[i] / _loadingBarDurations[i]; }
		
		_totalLoadingBar.UpdateLoadingBar(-1, sum / 3f);
	}
}