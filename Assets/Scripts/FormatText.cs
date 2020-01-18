using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormatText : MonoBehaviour
{
    private string FormatString = "";

    private Text StringText;

    public string[] DefaultArguments;


    void Start()
    {
        StringText = GetComponent<Text>();
        FormatString = StringText.text;
        Format(DefaultArguments);
    }


    public void Format(int Value)
    {
        StringText.text = string.Format(FormatString, Value);
    }


    public void Format(float Value)
    {
        StringText.text = string.Format(FormatString, Value);
    }


    public void Format(string Value)
    {
        StringText.text = string.Format(FormatString, Value);
    }


    public void Format(object Value)
    {
        StringText.text = string.Format(FormatString, Value);
    }


    public void Format(object[] Values)
    {
        StringText.text = string.Format(FormatString, Values);
    }
}
