using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterText : MonoBehaviour
{
    public int secondsToWait;
    void Start()
    {
        StartCoroutine(Timer());
    }
    IEnumerator Timer()
    {
        Text text = GetComponent<Text>();
        for (int i = secondsToWait; i > 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }
}
