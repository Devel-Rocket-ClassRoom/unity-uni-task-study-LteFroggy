using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveController : MonoBehaviour {
	[SerializeField] private TMP_InputField _inputField;
	[SerializeField] private Button _manualSaveButton;
	[SerializeField] private TMP_Text _lastSaveText;
	[SerializeField] private TMP_Text _statusText;
	[SerializeField] private Toggle _autoSaveToggle;
	
	private const float k_SaveDuration = 2f;
	private const float k_AutoSaveInterval = 10f;
	private const float k_DebounceDuration = 3f;
	
	private bool isSaving = false;
	private string saveData = string.Empty;
	
	private CancellationTokenSource debounceCTS;
	
	private void Start() {
		_manualSaveButton.onClick.AddListener(OnClickManualSave);
		_inputField.onValueChanged.AddListener(OnInputValueChanged);
		_autoSaveToggle.isOn = true;
		_autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);
		
		AutoSaveLoop().Forget();
		
		_lastSaveText.text = string.Empty;
		_statusText.text = "준비";
	}

	private void OnDestroy() {
		debounceCTS?.Cancel();
		debounceCTS?.Dispose();
	}

	private void OnClickManualSave() {
		debounceCTS?.Cancel();
		debounceCTS?.Dispose();
	}
	
	private async UniTask DebounceAndSave(CancellationToken ct) {
		try {
			ct.ThrowIfCancellationRequested();
			
			await UniTask.Delay(TimeSpan.FromSeconds(k_DebounceDuration), cancellationToken : ct);
			await SaveAsync("디바운스 저장");
			
		} catch (OperationCanceledException _) {
			
		}
	}
	
	private void OnInputValueChanged(string value) {
		debounceCTS = new CancellationTokenSource();
		DebounceAndSave(debounceCTS.Token).Forget();
	}
	
	private void OnAutoSaveToggleChanged(bool value) { }
	
	private async UniTask AutoSaveLoop() {
		CancellationToken ct = this.GetCancellationTokenOnDestroy();
		
		try {
			while (true) {
				await UniTask.Delay(TimeSpan.FromSeconds(k_AutoSaveInterval), cancellationToken: ct);
			
				if (_autoSaveToggle.isOn) {
					await SaveAsync(_inputField.text);
				}	
			}
		} catch (OperationCanceledException e) {
			Debug.LogError($"Auto Save Loop Cancelled");			
		}
	}
	
	private async UniTask SaveAsync(string message) {
		if (isSaving) {
			_statusText.text = "이미 저장중입니다.";
			return;
		}
		
		isSaving = true;
		
		try {
			_statusText.text = $"저장 중...";
			await UniTask.Delay(TimeSpan.FromSeconds(k_SaveDuration));
			
			saveData = message;
			
			_statusText.text = $"{message} : 저장 완료...";
			_lastSaveText.text = $"마지막 저장 시간 : {DateTime.Now:HH:mm:ss}";
		} catch (OperationCanceledException e) {
			_statusText.text = $"저장 실패!";
		} finally {
			isSaving = false;	
		}
	}
}