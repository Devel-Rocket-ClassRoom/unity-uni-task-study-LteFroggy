using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSystemController : MonoBehaviour {
	private int _timeLeft;
	private int TimeLeft {
		get => _timeLeft;
		set {
			_timeLeft = value;
			_timerText.text = $"{_timeLeft / 60:D2} : {_timeLeft % 60:D2}";
		}
	}

	[Header("=== 하단 버튼 등록 ===")]
	[SerializeField] private StartButton _startButton;
	[SerializeField] private Button _stopButton;
	[SerializeField] private Button _resetButton;
	
	[Header("=== 타이머 값이 들어갈 텍스트 ===")]
	[SerializeField] private TMP_Text _timerText;
	
	[Header("=== 상태 표시할 텍스트 ===")]
	[SerializeField] private TMP_Text _statusText;
	
	private CancellationTokenSource _timerCts;
	private readonly Color _startButtonOriginalColor;
	
	private void Start() {
		_startButton.AddListener(OnStartButtonPressed);
		_stopButton.onClick.AddListener(OnStopButtonPressed);
		_resetButton.onClick.AddListener(OnResetButtonPressed);
		
		TimeLeft = 10;
		
		_statusText.text = "준비";
	}
	
	private void OnStartButtonPressed() {
		_startButton.SetAsResume();
		_startButton.SetButtonEnableState(false);
		
		// 타이머 시작한다.
		_timerCts?.Cancel();
		_timerCts?.Dispose();
		_timerCts = new CancellationTokenSource();
		
		TimerAsync(_timerCts.Token).Forget();
	}
	
	private async UniTask TimerAsync(CancellationToken ct) {
		try {
			// 시간이 0초가 될때까지 1초씩 줄인다.
			_statusText.text = "타이머 실행 중";
			while (TimeLeft > 0) {
				await UniTask.Delay(1000, cancellationToken : ct);
				TimeLeft--;
			}
			
			_statusText.text = "완료!";
		} catch (OperationCanceledException) {
			Debug.Log($"중지됨");
			_statusText.text = "타이머 정지";
		}
	}
	
	private void OnStopButtonPressed() {
		_timerCts?.Cancel();
		_timerCts?.Dispose();
		_timerCts = null;
		
		_startButton.SetButtonEnableState(true);
	}
	
	private void OnResetButtonPressed() {
		_startButton.ResetButton();
		TimeLeft = 10;
		
		_timerCts?.Cancel();
		_timerCts?.Dispose();
		_timerCts = null;
		
		_startButton.SetButtonEnableState(true);
		_statusText.text = "준비";
	}
}
