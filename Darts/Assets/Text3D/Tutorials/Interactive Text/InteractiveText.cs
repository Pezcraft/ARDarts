using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveText : MonoBehaviour
{
    // Use this for initialization
    int Count = 0;
    float lastClick =-2f;
    string[] Responses = new string[]
    {
        "You clicked me",
        "And yet again",
        "Oh my!",
        "Thank you",
        "3D text effects grows with\nyour support :)",
        "And we support you as well",
        "support@bitsplash.io",
        "Thanks again!",
    };

	void Start () {
		
	}

    void OnMouseDown()
    {
        float time = Time.time;
        if (time - lastClick < 1f) // accpet new click every one second
            return;
        lastClick = Time.time;
        var element = GetComponent<DynamicTextElement>();
        if(element != null)
        {
            string text = Responses[Count % Responses.Length];
            Count++;
            element.AnimateTextChange(text);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
