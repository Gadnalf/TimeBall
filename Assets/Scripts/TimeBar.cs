using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    private Slider slider;
    private float targetProgress;

    public float FillSpeed = 0.01f;

    private void Awake() {
        slider = gameObject.GetComponent<Slider>();
    }
    // Start is called before the first frame update
    void Start() {
        SetToProgress(1f);
    }

    void Update() {
        SetToProgressAnimated(0);
    }

    public void IncreaseProgress(float progressIncrease) {
        slider.value += progressIncrease;
    }

    public void DecreaseProgress(float progressDecrease) {
        slider.value += progressDecrease;
    }

    public void SetToProgress(float targetValue) {
        slider.value = targetValue;
    }

    public void SetToProgressAnimated(float targetValue) {
        targetProgress = targetValue;

        if (slider.value > targetProgress)
            slider.value -= FillSpeed * Time.deltaTime;
        else if (slider.value < targetProgress)
            slider.value += FillSpeed * Time.deltaTime;
    }

    public float CheckTime() {
        return slider.value;
    }
}
