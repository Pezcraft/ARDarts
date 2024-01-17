using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTypingTutorial : MonoBehaviour
{

    public DynamicTextElement TextElement; // the text element we will be operating on
    public float ModifyTime = 1f; // a time peroid for each modifiction
    public int ModificationsEachTime = 2;
    string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ "; // this is a list of random charecters we are going to select from
    float modifyTimer; // timer for each operation

	// Use this for initialization
	void Start () {
        modifyTimer = ModifyTime; // set up the timer

    }
	
	// Update is called once per frame
	void Update () {
        modifyTimer -= Time.deltaTime; // update the timer
        if(modifyTimer<0f) // if the timer is past
        {
            
            modifyTimer = ModifyTime; // restart the timer
            for (int i = 0; i < ModificationsEachTime; i++)  // for each modification
            {
                if (Random.value >= 0.5f) // half a chance to add an item
                {
                    int insertPosition = Random.Range(0, TextElement.Text.Length); // choose a random position for the insertion
                    char insertChar = Chars[Random.Range(0, Chars.Length)]; // choose a random char
                    
                    TextElement.InsertText(insertPosition, insertChar.ToString()); // the insert text method animates a text insertion at the specified index. The animation is set in the GlyphAnimator component of the text gameobject
                }
                else // other half a chance to remove an item
                {
                    if (TextElement.Text.Length > 0)
                    {
                        int removePosition = Random.Range(0, TextElement.Text.Length); // choose a random remove position
                        TextElement.RemoveAt(removePosition); // call remove at to animate the remove op. The animation is set in the GlyphAnimator component of the text gameobject
                    }

                }
            }
        }
	}
}
