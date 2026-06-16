using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Phase {
	FadeIn,
	MoveRight,
	Magnify,
	Rotation,
	Completed
}

public class AnimationSequenceController : MonoBehaviour {
	[Header("=== 버튼들 ===")]
	[SerializeField] private Button _startButton;
	[SerializeField] private Button _resetButton;
	[SerializeField] private Button _cancelButton;

	[Header("=== 상태 텍스트 ===")]
	[SerializeField] private TMP_Text _statusText;

	[Header("=== 이동할 도형 ===")] 
	[SerializeField] private GameObject _target;
	
	private Phase _currentPhase;
	private float _currentPhaseTimeLeft;
	
	private CancellationTokenSource _animationCts;
	
	// 알파값 변경에 사용할 변수
	private const float k_InitAlpha = 0f;
	private const float k_LastAlpha = 1f;
	
	// 도형 이동에 사용할 위치값
	private readonly Vector3 k_InitLoc = new Vector3(-494f, -35f, 0f);
	private readonly Vector3 k_LastLoc = new Vector3(278f, -35f, 0f);
	
	// 도형 확대에 사용할 값
	private const float k_InitScale = 1f;
	private const float k_LastScale = 2f;
	
	// 도형 회전에 사용할 값
	private const float k_InitRotation = 0f;
	private const float k_LastRotation = 360f;

	private void Start() {
		_startButton.onClick.AddListener(OnStartButtonPressed);
		_resetButton.onClick.AddListener(OnResetButtonPressed);
		_cancelButton.onClick.AddListener(OnCancelButtonPressed);
		
		OnResetButtonPressed();
	}
	
	private void OnStartButtonPressed() {
		_animationCts?.Cancel();
		_animationCts?.Dispose();
		_animationCts = new CancellationTokenSource();
		
		PlayAnimationAsync(_animationCts.Token).Forget();
	}
	
	private async UniTask PlayAnimationAsync(CancellationToken token) {
		try {
			_startButton.interactable = false;
			
			if (_currentPhase <= Phase.FadeIn) {
				_statusText.text = $"{_currentPhase} 시작";
				await FadeInAsync(token);
			} 
			if (_currentPhase <= Phase.MoveRight) {
				_statusText.text = $"{_currentPhase} 시작";
				await MoveRightAsync(token);
			}
			if (_currentPhase <= Phase.Magnify) {
				_statusText.text = $"{_currentPhase} 시작";
				await MagnifyAsync(token);
			}
			if (_currentPhase <= Phase.Rotation) {
				_statusText.text = $"{_currentPhase} 시작";
				await RotateAsync(token);
			}
			
			_statusText.text = $"모든 동작 완료됨";
		} catch (OperationCanceledException) {
			Debug.Log($"{_currentPhase} 중지됨");
			_statusText.text = $"{_currentPhase} 중지됨";
			
			_startButton.interactable = true;
		}
	}
	
	private async UniTask FadeInAsync(CancellationToken ct) {
		CanvasGroup targetGroup = _target.GetComponent<CanvasGroup>();
		
		// 대상의 알파값을 0 ~ 1로 바꾼다
		while (_currentPhaseTimeLeft > 0) {
			ct.ThrowIfCancellationRequested();
			
			_currentPhaseTimeLeft -= Time.deltaTime;
			targetGroup.alpha = Mathf.Lerp(k_InitAlpha, k_LastAlpha, 1 - _currentPhaseTimeLeft);
			
			await UniTask.Yield(ct);
		}
		
		targetGroup.alpha = k_LastAlpha;
		
		// 작업 완료했다면, 다음 페이즈로
		_currentPhaseTimeLeft = 1f;
		_currentPhase = Phase.MoveRight;
	}
	
	private async UniTask MoveRightAsync(CancellationToken ct) {
		
		RectTransform targetTransform = _target.GetComponent<RectTransform>();
				
		// 대상의 위치를 옮긴다.
		while (_currentPhaseTimeLeft > 0) {
			ct.ThrowIfCancellationRequested();
			
			_currentPhaseTimeLeft -= Time.deltaTime;
			targetTransform.localPosition = Vector3.Lerp(k_InitLoc, k_LastLoc, 1 - _currentPhaseTimeLeft);
			
			await UniTask.Yield(ct);
		}
		
		targetTransform.localPosition = k_LastLoc;
		
		// 작업 완료했다면, 다음 페이즈로
		_currentPhaseTimeLeft = 1f;
		_currentPhase = Phase.Magnify;
	}
	
	// 확대한다.
	private async UniTask MagnifyAsync(CancellationToken ct) {
				
		RectTransform targetTransform = _target.GetComponent<RectTransform>();
		
		var progress = Progress.Create<float>(p => {
			targetTransform.localScale = new Vector3(p, p, p);
		});
				
		// 대상의 위치를 옮긴다.
		while (_currentPhaseTimeLeft > 0) {
			ct.ThrowIfCancellationRequested();
			
			_currentPhaseTimeLeft -= Time.deltaTime;
			progress.Report(Mathf.Lerp(k_InitScale, k_LastScale, 1 - _currentPhaseTimeLeft));
			
			await UniTask.Yield(ct);
		}
		
		progress.Report(k_LastScale);
		
		// 작업 완료했다면, 다음 페이즈로
		_currentPhaseTimeLeft = 1f;
		_currentPhase = Phase.Rotation;
	}
	
	private async UniTask RotateAsync(CancellationToken ct) {
				
		RectTransform targetTransform = _target.GetComponent<RectTransform>();
		
		var progress = Progress.Create<float>(p => {
			targetTransform.localRotation = Quaternion.Euler(0, 0, p);
		});
				
		// 대상을 회전시킨다.
		while (_currentPhaseTimeLeft > 0) {
			ct.ThrowIfCancellationRequested();
			
			_currentPhaseTimeLeft -= Time.deltaTime;
			progress.Report(Mathf.Lerp(k_InitRotation, k_LastRotation, 1 - _currentPhaseTimeLeft));
			
			await UniTask.Yield(ct);
		}
		
		progress.Report(k_LastRotation);
		
		
		// 완료 페이즈로.
		_currentPhase = Phase.Completed;
	}
	
	private void OnResetButtonPressed() {
		_animationCts?.Cancel();
		_animationCts?.Dispose();
		_animationCts = null;
		
		_currentPhaseTimeLeft = 1f;
		_currentPhase = Phase.FadeIn;
		
		_target.GetComponent<CanvasGroup>().alpha = k_InitAlpha;
		_target.GetComponent<RectTransform>().localPosition = k_InitLoc;
		_target.GetComponent<RectTransform>().localRotation = Quaternion.Euler(k_InitRotation, 0f, 0f);
		_target.GetComponent<RectTransform>().localScale =  new Vector3(k_InitScale, k_InitScale, k_InitScale);
		
		_statusText.text = "준비됨";
		_startButton.interactable = true;
	}
	
	private void OnCancelButtonPressed() {
		_animationCts?.Cancel();
		_animationCts?.Dispose();
		_animationCts = null;
	}
}
