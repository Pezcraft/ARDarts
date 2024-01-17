using Assets.Text3D.Script.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationWave : AnimationSequenceBase
{
    public enum WaveAnimationType
    {
        LeftToRight,
        RightToLeft,
        LeftToRightAndBack,
        RightToLeftAndBack,
        RepeatingLeftToRight,
        RepeatingRightToLeft,
        ReaptingBackAndForth        
    }

    /// <summary>
    /// the type of the wave animation
    /// </summary>
    public WaveAnimationType WaveType;
    
    /// <summary>
    /// the gap time between each glyph animation. the first glyph is animated first ,the second is animated after GapTime seconds , the third after 2*Gaptime seconds.
    /// </summary>
    public float GlyphGapTime = 1f;
    /// <summary>
    /// the time gap between whole waves
    /// </summary>
    public float WordPauseTime = 0f;
    /// <summary>
    /// the time gap between whole waves
    /// </summary>
    public float PauseTime = 0f;
    /// <summary>
    /// the intial delay before the animation starts
    /// </summary>
    public float AnimationStartDelay = 0f;
    /// <summary>
    /// The animation presets used with this animation sequence
    /// </summary>
    [Header("Base Animations")]
    public AnimationEntry[] AnimationPresets;
    /// <summary>
    /// the selection method for the AnimationPresets array. 
    /// </summary>
    public AnimationArraySampleType AnimationSelectMethod;

    /// <summary>
    /// the current glyph index for the running wave segment
    /// </summary>
    int currentGlyphIndex = 0;
    /// <summary>
    /// the direction of the current wave segment( 1 is left to right , -1 is right to left)
    /// </summary>
    int direction = 1;
    /// <summary>
    /// the animation corouinte
    /// </summary>
    IEnumerator Coroutine = null;

    protected override AnimationEntry SampleAnimationArray(int type)
    {
        // use the default sampling method
        return SampleAnimationArrayWithParams(AnimationPresets, AnimationSelectMethod);
    }

    protected override void InnerStartAnimation()
    {
        InnerStopAnimation(); // stop any previously running animation
        Coroutine = AnimateWave(); // store the animation coroutine
        SetStartAnimation(); // set the inital animation parameters base on WaveType
        StartCoroutine(Coroutine); // start the animation coroutine
    }

    protected override void InnerStopAnimation()
    {
        if (Coroutine != null) // destory any existing animation coroutine
        {
            StopCoroutine(Coroutine);
            Coroutine = null;
        }

    }

    /// <summary>
    /// true if the current animation direction is from left to right.false if right to left
    /// </summary>
    bool IsLeftToRight { get { return direction == 1; } }

    /// <summary>
    /// sets the animation parameters for a left to right movement
    /// </summary>
    /// <param name="goBack"></param>
    void SetLeftToRight(bool goBack)
    {
        direction = 1;
        currentGlyphIndex = (goBack && PauseTime <=0f) ? 1: 0;
    }

    /// <summary>
    /// sets the animation parameters for a right to left movement
    /// </summary>
    /// <param name="goBack"></param>
    void SetRightToLeft(bool goBack)
    {
        direction = -1;
        if (mTextElement != null)
            currentGlyphIndex = mTextElement.Text.Length - ((goBack && PauseTime <= 0f) ? 2 : 1);
        else
            currentGlyphIndex = 0;
    }

    /// <summary>
    /// marks a wave segment as endded. This method checks WaveType and determines if the animation has ended or not. returns true if the animatino continues to another segment , false otherwise.
    /// </summary>
    /// <returns></returns>
    bool EndSegment()
    {
        bool continueAnimation = false;
        switch(WaveType)
        {
            case WaveAnimationType.LeftToRight:
                EndAnimation(); // only one segment
                break;
            case WaveAnimationType.RightToLeft:
                EndAnimation(); // only one segment
                break;
            case WaveAnimationType.LeftToRightAndBack:
                if (IsLeftToRight) // if this is the first segment
                {
                    SetRightToLeft(true); // then go back
                    continueAnimation = true;
                }
                else
                    EndAnimation(); 
                break;
            case WaveAnimationType.RightToLeftAndBack:
                if (IsLeftToRight == false) // if this is the first segment
                {
                    SetLeftToRight(true); // then go back
                    continueAnimation = true;
                }
                else
                    EndAnimation();
                break;
            case WaveAnimationType.RepeatingLeftToRight:
                SetLeftToRight(false); // restart a left to right wves
                continueAnimation = true;
                break;
            case WaveAnimationType.RepeatingRightToLeft:
                SetRightToLeft(false); // restart a right to left wave
                continueAnimation = true;
                break;
            case WaveAnimationType.ReaptingBackAndForth:
                if (IsLeftToRight) // flip the wave direction
                    SetRightToLeft(true);
                else
                    SetLeftToRight(true);
                continueAnimation = true;
                break;
        }
        return continueAnimation;
    }

    /// <summary>
    /// sets the inital wave paramenters according to WaveType
    /// </summary>
    void SetStartAnimation()
    {
        switch (WaveType)
        {
            case WaveAnimationType.LeftToRight:
                SetLeftToRight(false);
                break;
            case WaveAnimationType.RightToLeft:
                SetRightToLeft(false);
                break;
            case WaveAnimationType.LeftToRightAndBack:
                SetLeftToRight(false);
                break;
            case WaveAnimationType.RightToLeftAndBack:
                SetRightToLeft(false);
                break;
            case WaveAnimationType.RepeatingLeftToRight:
                SetLeftToRight(false);
                break;
            case WaveAnimationType.RepeatingRightToLeft:
                SetRightToLeft(false);
                break;
            case WaveAnimationType.ReaptingBackAndForth:
                SetLeftToRight(false);
                break;
        }
    }

    /// <summary>
    /// animates the wave
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateWave()
    {
        if (mTextElement == null)
            yield break ;
        bool first = true;
        do
        {
            if (first) // there is not pause time for the first wave segement
            {
                yield return new WaitForSeconds(AnimationStartDelay); // wait for the inital animation delay time
                first = false;
            }
            else
                yield return new WaitForSeconds(PauseTime); // pause before the next segment
            bool prevIsWhiteSpace = false;
            while (currentGlyphIndex < mTextElement.Text.Length && currentGlyphIndex >= 0) // continue as long as the glyph index is in bounds of the text
            {
                VerifyArguments(false);
                char glyph = mTextElement.Text[currentGlyphIndex];
                if (char.IsWhiteSpace(glyph)) // skip white spaces
                {
                    currentGlyphIndex += direction;
                    if (prevIsWhiteSpace == false && WordPauseTime > 0f)
                        yield return new WaitForSeconds(WordPauseTime);
                    prevIsWhiteSpace = true;
                }
                else
                {
                    prevIsWhiteSpace = false;
                    AnimateOneGlyph(currentGlyphIndex, 0); // animate the glyph
                    currentGlyphIndex += direction;
                    if (GlyphGapTime > 0f || (WaveType != WaveAnimationType.LeftToRight && WaveType != WaveAnimationType.RightToLeft))
                        yield return new WaitForSeconds(GlyphGapTime); // wait for the next iteration
                }
            }
        }
        while (EndSegment()); // continue as long as there are more segments
        EndAnimation(); // invoke the end animation handler
    }

    protected override void VerifyArguments(bool showWarning)
    {
        if(showWarning) // show warnings for arguments that are less then 0 
        {
            if (GlyphGapTime < 0f)
                Debug.LogWarning("GlyphGapTime cannot be less then 0");
            if (GlyphGapTime <0.01f && WaveType != WaveAnimationType.LeftToRight && WaveType != WaveAnimationType.RightToLeft)
                Debug.LogWarning("GlyphGapTime can be set be less then 0.01f only when WaveType is set to either \"LeftToRight\" or \"RightToLeft\"");
            if (AnimationStartDelay < 0f)
                Debug.LogWarning("AnimationStartDelay can't be less then 0");

            if (PauseTime < 0f)
                Debug.LogWarning("Pause cannot be less then 0");
        }
        AnimationStartDelay = Mathf.Max(AnimationStartDelay, 0f);
        GlyphGapTime = Mathf.Max(GlyphGapTime, 0f);
        PauseTime = Mathf.Max(PauseTime, 0f);
        if(WaveType != WaveAnimationType.LeftToRight && WaveType != WaveAnimationType.RightToLeft)
            GlyphGapTime = Mathf.Max(GlyphGapTime, 0.01f);

    }
}
