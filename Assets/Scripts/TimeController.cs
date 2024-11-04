using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeScaleDisplay;

    public void OnTimeScaleUpdated(float timeScale){
        Time.timeScale = timeScale;
        if (timeScaleDisplay != null) { timeScaleDisplay.text = $"{timeScale}"; }
    }
}
