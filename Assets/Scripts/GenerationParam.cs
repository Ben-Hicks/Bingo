using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GenerationParam : MonoBehaviour {

    public Slider slider;
    public Text txtValue;
    public Text txtMinValue;
    public Text txtMaxValue;

    public float fValue;
    public float fMax;
    public float fMin;

    public bool bInt;

    public void SetValue(float _fValue) {
        fValue = _fValue;
        UpdateSlider();
    }

    public void UpdateSlider() {

        slider.minValue = fMin;
        txtMinValue.text = string.Format("{0:0.#}", fMin);

        slider.maxValue = fMax;
        txtMaxValue.text = string.Format("{0:0.#}", fMax);
        slider.wholeNumbers = bInt;

        slider.SetValueWithoutNotify(fValue);
        txtValue.text = string.Format("{0:0.#}", fValue);

    }

    public void OnSliderUpdate() {

        fValue = slider.value;
        txtValue.text = string.Format("{0:0.#}", fValue);
    }

    public void Start() {
        OnSliderUpdate();
    }

    [CustomEditor(typeof(GenerationParam))]
    public class MyScriptEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Update Slider")) {
                ((GenerationParam)target).UpdateSlider();
            }
        }
    }
}
