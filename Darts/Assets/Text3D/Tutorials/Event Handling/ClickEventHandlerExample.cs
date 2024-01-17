using Assets.Text3D.Script.Animation;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClickEventHandlerExample : MonoBehaviour
{
    public DynamicTextElement Element;
    public AnimationEntry Animation;
    public AnimationEntry RemoveAnimation;
    float Timer = -1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void RemoveFirstWord()
    {
        var match = Regex.Match(Element.Text, "[A-Za-z0-9]+");
        if (match != null)
        {
            Element.RemoveText(0, match.Index + match.Length, RemoveAnimation);
        }
    }
        void AnimateFirstWord()
    {
        var match = Regex.Match(Element.Text, "[A-Za-z0-9]+");
        if(match != null)
        {
            int lastIndex = match.Index + match.Length;
            for (int i = 0; i < lastIndex; i++)
                Element.AnimateGlyph(i, Animation);
        }
        
    }
    public void ItemClicked(DynamicTextElement.ClickEventArgs args)
    {
        RemoveFirstWord();
        Debug.Log(args.Word);
    }
    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if(Timer < 0f)
        {
            AnimateFirstWord();
            Timer = Animation.Time;
        }
    }
}
