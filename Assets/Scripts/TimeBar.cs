using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    private Slider slider;
    private float targetProgress;

    public float FillSpeed;

    private bool isTimeDone;
    [SerializeField] private GameObject fillKnob;

    private void Awake() {
        slider = gameObject.GetComponent<Slider>();
    }
    // Start is called before the first frame update
    void Start() {
        SetToProgress(1f);
        isTimeDone = false;
    }

    void Update() {
        SetToProgressAnimated(0);
        isTimeDone = slider.value == 0;
        fillKnob.SetActive(!isTimeDone);
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

    public bool CheckIfTimeDone() {
        return isTimeDone;
    }
}
