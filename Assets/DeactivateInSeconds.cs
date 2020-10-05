using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateInSeconds : MonoBehaviour
{
    public float seconds;
    void Start()
    {
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(seconds);
        gameObject.SetActive(false);
    }
}
