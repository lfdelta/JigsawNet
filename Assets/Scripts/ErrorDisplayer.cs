using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorDisplayer : MonoBehaviour
{
    public Image Background;
    public Text ErrorText;


    public void DisplayMessage(string Message, float Duration)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayMessageInternal(Message, Duration));
    }


    private IEnumerator DisplayMessageInternal(string Message, float Duration)
    {
        ErrorText.text = Message;
        Background.enabled = true;
        ErrorText.enabled = true;
        yield return new WaitForSecondsRealtime(Duration);
        Background.enabled = false;
        ErrorText.enabled = false;
    }
}
