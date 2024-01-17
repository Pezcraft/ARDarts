using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextInputTutorial : MonoBehaviour {

    public DynamicTextElement TextElement; // the text element we will be operating on
    public GameObject Caret;
    public bool AccpetInput = true;
    int CaretPosition;
    TouchScreenKeyboard keyboard;
    // Use this for initialization
    void Start()
    {
        CaretPosition = TextElement.Text.Length;
        if (Application.isMobilePlatform)
            keyboard = TouchScreenKeyboard.Open(TextElement.Text,  TouchScreenKeyboardType.Default);
        
    }
    
    private void OnGUI()
    {
        if (Application.isMobilePlatform)
        {
            if (GUI.Button(new Rect(0, 0, 200, 80), "Show keyboard"))
            {
                keyboard = TouchScreenKeyboard.Open(TextElement.Text, TouchScreenKeyboardType.Default);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

        if (AccpetInput == false) // keyboard is disabled
            return;
        if (Application.isMobilePlatform) // this is a mobile platform
        {
            if (keyboard != null)
            {
                if (keyboard.status == TouchScreenKeyboard.Status.Done) // if the keyboard is done animate the text
                {
                    TextElement.AnimateTextChange(keyboard.text);
                    keyboard = null; // this keyboard is done
                }
            }
            return;
        }

        string str = Input.inputString;
        for (int i=0; i< str.Length; i++)
        {
            char c = str[i];
            if (c == '\b') // has backspace/delete been pressed?
            {

                CaretPosition--;
                if (CaretPosition < 0) // deacrease the caret position
                    CaretPosition = 0;

                if(TextElement.Text.Length > 0)
                    TextElement.RemoveAt(CaretPosition); // call remove at to animate the remove op. The animation is set in the GlyphAnimator component of the text gameobject
            }
            else if (c == '\r')
            {
                if (i < str.Length - 1 && str[i + 1] == '\n')
                    i++;
                TextElement.InsertText(CaretPosition, "\n"); // append the chart to the text
                CaretPosition++; // increase the CaretPosition
            }
            else
            {
                TextElement.InsertText(CaretPosition, c.ToString()); // append the chart to the text
                CaretPosition++; // increase the CaretPosition
            }
        }
        // do arrow input
        if(Input.GetKeyDown( KeyCode.LeftArrow))
        {

            CaretPosition--;
            if (CaretPosition < 0f) // deacrease the caret position
                CaretPosition = 0;
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            CaretPosition++;
            if (CaretPosition >TextElement.Text.Length) // increase the caret position
                CaretPosition = TextElement.Text.Length;
        }

    }

    private void LateUpdate() 
    {
        //the changes in the update method are applied ot the text element only after it's update method is called. So the caret updating must be done in LateUpdate
        if (Caret != null)
        {
            Vector3 pos = Caret.transform.position;
            TextElement.SetCaretPosition(CaretPosition,ref pos);
            Caret.transform.position = pos;
        }
    }
}
