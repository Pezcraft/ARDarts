using Assets.Text3D.Script.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingAnimationSequence : AnimationSequenceBase
{
    [TextArea(3, 10)]
    public string TypeInText = "";
    public bool SkipWhiteSpace = true;
    public float AnimationStartDelay = 0f;
    public float AppendGapTime = 0.5f;


    /// <summary>
    /// The animation presets used with this animation sequence
    /// </summary>
    [Header("Base Animations")]
    public AnimationEntry[] AnimationPresets;
    /// <summary>
    /// the selection method for the AnimationPresets array. 
    /// </summary>
    public AnimationArraySampleType AnimationSelectMethod;

    IEnumerator TypeInCoroutine = null;

    protected override AnimationEntry SampleAnimationArray(int type)
    {
        //use the deault sampling method
        return SampleAnimationArrayWithParams(AnimationPresets, AnimationSelectMethod);
    }

    protected override void InnerStartAnimation()
    {
        InnerStopAnimation(); // stop any previously started animation
        TypeInCoroutine = AnimateTypeIn(); // store the animation coroutine
        StartCoroutine(TypeInCoroutine); // start the animation coroutine
    }

    protected override void InnerStopAnimation()
    {
        if (TypeInCoroutine != null) // destory any existing animatino coroutine
        {
            StopCoroutine(TypeInCoroutine);
            TypeInCoroutine = null;
        }
    }
    /// <summary>
    /// call this to run the typing animatino with a custom text string
    /// </summary>
    /// <param name="text"></param>
    public void StartAnimation(string text)
    {
        TypeInText = text; // set the typing text
        StartAnimation(); // start the animation normally
    }

    IEnumerator AnimateTypeIn()
    {
        string typeInText = TypeInText; // store the typing text in advance , this way any changes to it won't affect it
        yield return new WaitForSeconds(AnimationStartDelay); // wait for the inital animation delay time
        for (int i=0; i< typeInText.Length; i++) 
        {
            char glyph = typeInText[i];
            AppendOneGlyph(glyph,0); // type the new glyph
            if (char.IsWhiteSpace(glyph) == false || SkipWhiteSpace == false)
                yield return new WaitForSeconds(AppendGapTime); // wait for the next iteration
        }
        EndAnimation();
    }

    protected override void VerifyArguments(bool showWarning)
    {
        if(showWarning) // show warning for arguments less then 0
        {
            if (AnimationStartDelay < 0f)
                Debug.LogWarning("AnimationStartDelay can't be less then 0");
            if(AppendGapTime < 0f)
                Debug.LogWarning("AppendGapTime can't be less then 0");
        }
        // fix arguments that are less then 0
        AnimationStartDelay = Mathf.Max(AnimationStartDelay, 0f);
        AppendGapTime = Mathf.Max(AppendGapTime, 0f);
    }
}
