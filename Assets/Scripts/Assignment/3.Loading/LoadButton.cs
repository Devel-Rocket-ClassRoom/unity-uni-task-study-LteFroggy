using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class LoadButton : MonoBehaviour {
	private Image _buttonImage;
	private TMP_Text _buttonText;
	private Button _button;
	
	private readonly Color _originalColor = new Color(132f / 255f, 255 / 255f, 97 / 255f);
	private readonly string _originalText = "로딩 시작";
	
	private readonly Color _cancelColor = new Color(255/)
	
	private readonly Color _resumeColor = new Color(255f / 255f, 243f / 255f, 119f / 255f);
	private readonly string _resumeText = "로딩 재개";
	
	public void AddListener(UnityAction action) { _button.onClick.AddListener(action); }
	public void RemoveListener(UnityAction action) { _button.onClick.RemoveListener(action); }

	private void Awake() {
		_button = GetComponent<Button>();
		_buttonImage = GetComponent<Image>();
		_buttonText = GetComponentInChildren<TMP_Text>();
	}
	
	public void SetButtonEnableState(bool state) {
		_button.interactable = state;
	}
	
	public void ResetButton() {
		SetButtonColorAndText(_originalColor, _originalText);
	}
	
	public void SetAsResumeButton() {
		SetButtonColorAndText(_resumeColor, _resumeText);
	}
	
	private void SetButtonColorAndText(Color color, string text) {
		_buttonImage.color = color;
		_buttonText.text = text;
	}
}