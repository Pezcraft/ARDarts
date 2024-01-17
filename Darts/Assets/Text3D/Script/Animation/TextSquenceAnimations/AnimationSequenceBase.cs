using Assets.Script.Animation;
using Assets.Text3D.Script.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AnimationSequenceBase : MonoBehaviour
{

    public enum AnimationArraySampleType
    {
        Random,
        Sequential
    }

    protected DynamicTextElement mTextElement;

    [Header("Event Handling")]
    public UnityEvent AnimationStarted;
    public UnityEvent AnimationEnded;

    
    public bool AnimateOnStart = false;
    public bool OverrideAnimation = true;

    int mCurrentArraySquence = 0;

    protected abstract void VerifyArguments(bool showWarning);
    protected abstract AnimationEntry SampleAnimationArray(int type);

    protected AnimationEntry SampleAnimationArrayWithParams(AnimationEntry[] array , AnimationArraySampleType sampleType)
    {
        if (array == null || array.Length == 0)
            return null;
        if (sampleType == AnimationArraySampleType.Random)
        {
            int arrayPosition = Random.Range(0, array.Length);
            return array[arrayPosition];
        }
        else if (sampleType == AnimationArraySampleType.Sequential)
        {
            int seq = mCurrentArraySquence++;
            return array[seq % array.Length];
        }
        return null;
    }

    public void StartAnimation()
    {
        if (AnimationStarted != null)
            AnimationStarted.Invoke();
        InnerStartAnimation();
    }

    public void StopAnimation()
    {
        InnerStopAnimation();
        EndAnimation();
    }

    protected abstract void InnerStartAnimation();
    protected abstract void InnerStopAnimation();

    protected void EndAnimation()
    {
        if (AnimationEnded != null)
            AnimationEnded.Invoke();
    }

    protected void AppendOneGlyph(char glyph, int type)
    {
        if (mTextElement == null)
            return;
        var anim = SampleAnimationArray(type);
        if (anim != null)
            mTextElement.AppendText(glyph.ToString(), anim);
    }

    protected void AnimateOneGlyph(int index, int type)
    {
        if (mTextElement == null)
            return;
        if (index >= mTextElement.Text.Length || index < 0)
            return;
        var anim = SampleAnimationArray(type);
        if (anim != null)
            mTextElement.AnimateGlyph(index, anim, !OverrideAnimation);
    }

    public void StartAnimationOn(DynamicTextElement element)
    {
        mTextElement = element;
        StartAnimation();
    }

    // Use this for initialization
    protected virtual void Start () {
        if(mTextElement ==null)
            mTextElement = GetComponent<DynamicTextElement>();
        if (mTextElement != null)
        {
            if (AnimateOnStart)
                StartAnimation();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
