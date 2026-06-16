using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour {
	private TMP_Text _text;
	private Slider _slider;
	
	public TMP_Text Text => _text;
	public Slider Slider => _slider;
	
	public void Init() {
		_text = GetComponentInChildren<TMP_Text>();
		_slider = GetComponentInChildren<Slider>();
	}
	
	public void UpdateLoadingBar(int idx, float t) {
		_text.text = $"{(idx < 0 ? "전체" : $"리소스 {idx}")} : {(int)(t * 100)}%";
		_slider.value = t;
	}
}