using Assets.Text3D.Script.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Animation
{
    [Serializable]
    public class GameObjectAnimator : MonoBehaviour
    {
        public GameObject DirectChildObject;
        public AnimationEntry Animation;
        public float StartDelay = 0f;
        public bool Repeat = true;

        float StartTime;
        DynamicTextElement Parent;
        Vector3 mStartPosition;
        Vector3 mTargetScale;

        public void Start()

        {
            mStartPosition = DirectChildObject.transform.localPosition;
            mTargetScale = DirectChildObject.transform.localScale;
            StartTime = Time.time + StartDelay;
        }

        void UpdateAnimation()
        {
            float time = Time.time;
            if (Animation == null || Animation.Animation == null)
            {
                DirectChildObject.transform.localPosition = mStartPosition;
                DirectChildObject.transform.localScale = mTargetScale;
                DirectChildObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                float currnetTime = time - StartTime;
                float factor = 1f;
                if(Animation.Time >0f)
                    factor = Mathf.Clamp(currnetTime / Animation.Time, 0f, 1f);
                if (factor == 1f && Repeat)
                    StartTime = Time.time;
                Vector3 targetPosition = mStartPosition;
                Vector3 targetScale = mTargetScale;
                Vector3 startPosition = targetPosition - Animation.Animation.Translation;
                Vector3 startScale = CommonMethods.MemberwiseMultiply(mTargetScale,Animation.Animation.Scale);
                Vector3 targetRotation = Vector3.zero;
                Vector3 startRotation = Animation.Animation.rotation;
                if (Animation != null && ((Animation.Mode & (AnimationEntry.AnimationMode.Reveresed)) != AnimationEntry.AnimationMode.None))
                    factor = 1f - factor;
                float rotationFactor = Animation.Animation.RotationCurve.Evaluate(factor);
                float scaleFactor = Animation.Animation.ScaleCurve.Evaluate(factor);
                float translateFacotr = Animation.Animation.TranslationCurve.Evaluate(factor);

                if (Animation != null && ((Animation.Mode & (AnimationEntry.AnimationMode.Inverted)) != AnimationEntry.AnimationMode.None))
                {
                    rotationFactor = 1f - rotationFactor;
                    scaleFactor = 1f - scaleFactor;
                    translateFacotr = 1f - translateFacotr;
                }

                DirectChildObject.transform.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, translateFacotr);
                DirectChildObject.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleFactor);
                DirectChildObject.transform.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(startRotation, targetRotation, rotationFactor));
            }
        }

        public void Update()
        {
            UpdateAnimation();
        }
    }
}
