using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Text3D.Script.Animation.TextSquenceAnimations
{
    class AnimationRandomizer : AnimationSequenceBase
    {
        /// <summary>
        /// temporal indices for random letter selection
        /// </summary>
        List<int> mTmpItems = new List<int>();

        /// <summary>
        /// The time between animations is selected randomly between [MinimumAnimationGap,MaximumAnimationGap]
        /// </summary>
        public float MinimumAnimationGap = 1f;
        /// <summary>
        /// The time between animations is selected randomly between [MinimumAnimationGap,MaximumAnimationGap]
        /// </summary>
        public float MaximumAnimationGap = 2f;

        /// <summary>
        /// The number of animated letters each iteration is a random number between [MinimumAnimatedLetters,MaximumAnimatedLetters]
        /// </summary>
        public int MinimumAnimatedLetters = 1;
        /// <summary>
        /// The number of animated letters each iteration is a random number between [MinimumAnimatedLetters,MaximumAnimatedLetters]
        /// </summary>
        public int MaximumAnimatedLetters = 2;

        /// <summary>
        /// The animation presets used with this animation sequence
        /// </summary>
        [Header("Base Animations")]
        public AnimationEntry[] AnimationPresets = null;
        /// <summary>
        /// the selection method for the AnimationPresets array. 
        /// </summary>
        public AnimationArraySampleType AnimationSelectMethod = AnimationArraySampleType.Random;

        /// <summary>
        /// the coroutine running the animation sequence
        /// </summary>
        IEnumerator Coroutine = null;

        protected override void InnerStartAnimation()
        {
            InnerStopAnimation(); // stop any previously started animations
            Coroutine = AnimateRandom(); // store the animation coroutine
            StartCoroutine(Coroutine); // start the coroutine
        }

        protected override void InnerStopAnimation()
        {
            if (Coroutine != null) // if there is a coroutine running then destory it
            {
                StopCoroutine(Coroutine);
                Coroutine = null;
            }

        }

        /// <summary>
        /// This coroutine animates the randomizer animation squence
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateRandom()
        {
            
            while(true) // go until the coroutine is stopped manually or the object is destroyed
            {
                VerifyArguments(false); // verfiy the arguments first
                float gapTime = UnityEngine.Random.Range(MinimumAnimationGap, MaximumAnimationGap); // select the gap time for the next iteration
                yield return new WaitForSeconds(gapTime); // wait for the next iteration
                int randomLetters = UnityEngine.Random.Range(MinimumAnimatedLetters, MaximumAnimatedLetters + 1); // select the amount of letters for this iteration
                int length = mTextElement.Text.Length; 
                mTmpItems.Clear();
                for (int i = 0; i < length; i++) // prepare a list of all indices so we can select the random positions
                    mTmpItems.Add(i);

                if (randomLetters > length)
                    randomLetters = length;

                for(int i=0; i<randomLetters; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, mTmpItems.Count); // select an item from the remaining indices
                    int randomItem = mTmpItems[randomIndex];

                    int lastIndex = mTmpItems.Count - 1;
                    mTmpItems[randomIndex] = mTmpItems[lastIndex]; // put the selected item in the end
                    mTmpItems[lastIndex] = randomItem;
                    mTmpItems.RemoveAt(lastIndex); // remove the selected item

                    AnimateOneGlyph(randomItem, 0); // animate the selected item
                }
            }
        }

        protected override AnimationEntry SampleAnimationArray(int type)
        {
            //use the default animation sampling behaviour
            return SampleAnimationArrayWithParams(AnimationPresets, AnimationSelectMethod);
        }

        protected override void VerifyArguments(bool showWarnings)
        {
            if(showWarnings)
            {
                //show warnings for minimum values
                if (MaximumAnimationGap < 0.1f)
                    Debug.LogWarning("MaximumAnimationGap cannot be less then 0.1f");
                if (MinimumAnimationGap < 0.1f)
                    Debug.LogWarning("MinimumAnimationGap cannot be less then 0.1f");
                if (MinimumAnimatedLetters < 1)
                    Debug.LogWarning("MinimumAnimatedLetters cannot be less then 0.1f");
                if (MaximumAnimatedLetters < 1)
                    Debug.LogWarning("MaximumAnimatedLetters cannot be less then 0.1f");
                // show warnings for maximum values smaller then minimum values
                if (MaximumAnimationGap < MinimumAnimationGap)
                    Debug.LogWarning("MaximumAnimationGap cannot be less then MinimumAnimationGap");
                if (MaximumAnimatedLetters < MinimumAnimatedLetters)
                    Debug.LogWarning("MaximumAnimatedLetters cannot be less then MinimumAnimatedLetters");

            }

            //verfiy that arguments match their minimum value
            MaximumAnimationGap = Mathf.Max(0.1f, MaximumAnimationGap);
            MinimumAnimationGap = Mathf.Max(0.1f, MinimumAnimationGap);
            MinimumAnimatedLetters = Mathf.Max(1, MinimumAnimatedLetters);
            MaximumAnimatedLetters = Mathf.Max(1, MaximumAnimatedLetters);

            //verify that the maximum values are larger then or equal to the minimum values
            if (MaximumAnimationGap < MinimumAnimationGap)
                MaximumAnimationGap = MinimumAnimationGap;
            if (MaximumAnimatedLetters < MinimumAnimatedLetters)
                MaximumAnimatedLetters = MinimumAnimatedLetters;
        }
    }
}
