using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TimeController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeScaleDisplay;
    public UnityEvent<float> timeScaleUpdated;

    public void OnTimeScaleUpdated(float timeScale){
        Time.timeScale = timeScale;
        if (timeScaleDisplay != null) { timeScaleDisplay.text = $"{timeScale}"; }
        timeScaleUpdated.Invoke(timeScale);
    }
}
