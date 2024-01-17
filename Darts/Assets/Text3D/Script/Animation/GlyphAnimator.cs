using Assets.Text3D.Script.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Animation
{
    [Serializable]
    public class GlyphAnimator : MonoBehaviour
    {
        public AnimationEntry InsertAnimation;
        public AnimationEntry DeleteAnimation;

        public float MoveTime = 0.5f;
        public AnimationCurve MoveCurve;

        public void TakeFrom(GlyphAnimator main)
        {
            InsertAnimation = main.InsertAnimation;
            DeleteAnimation = main.DeleteAnimation;
            MoveTime = main.MoveTime;
            MoveCurve = main.MoveCurve;
        } 

        float StartTime;
        float MoveStartTime;
        AnimationEntry  Current;
        GlyphEvents.GlyphEventArgs Args;
        Vector3 RealPosition;
        Vector3 MoveToPosition;
        DynamicTextElement Parent;
        bool Deleteing = false;


        public void Animate(GlyphEvents.GlyphEventArgs args)
        {
            IsAnimating = true;
            RealPosition = args.Position;
            MoveToPosition = RealPosition;
            Args = args;
            StartTime = Time.time + args.Delay;
            Parent = args.Parent;
            Current = args.Animation;   
            if (args.Animate == false && Current != null)
                StartTime = Time.time - Current.Time;
            UpdateAnimation();
        }

        public void Move(GlyphEvents.GlyphEventArgs args)
        {
            if (Deleteing)
                return;
            RealPosition = GetInterpolatedPosition();// MoveToPosition;
            MoveToPosition = args.Position;
            MoveStartTime = Time.time + args.Delay;
        }

        public void Enter(GlyphEvents.GlyphEventArgs args)
        {
            IsAnimating = true;
            Deleteing = false;
            RealPosition = args.Position;
            MoveToPosition = RealPosition;
            Args = args;
            StartTime = Time.time + args.Delay;
            Parent = args.Parent;
            Current = InsertAnimation;
            if (args.Animation != null)
                Current = args.Animation;
            if (args.Animate == false && Current != null)
                StartTime = Time.time - Current.Time;
            UpdateAnimation();
        }

        public void Exit(GlyphEvents.GlyphEventArgs args)
        {
            IsAnimating = true;
            RealPosition = GetInterpolatedPosition();// MoveToPosition;
            MoveToPosition = RealPosition;
            Deleteing = true;
            Args = args;
            StartTime = Time.time + args.Delay;
            Parent = args.Parent;
            Current = DeleteAnimation;
            if (args.Animation != null)
                Current = args.Animation;
            if (args.Animate == false && Current != null)
                StartTime = Time.time - Current.Time;
            UpdateAnimation();
        }

        private void Start()
        {
          //  if ((DeleteAnimation != null && MoveTime > DeleteAnimation.Time) || (InsertAnimation != null && MoveTime > InsertAnimation.Time))
          //      Debug.LogWarning("Glyph Animator - Move time cannot be larger then delete time or add time. Check your glyph prefab to fix this issue");
        }

        Vector3 GetInterpolatedPosition()
        {
            Vector3 interpolatedRealPosition = MoveToPosition;
            if (MoveTime > 0f)
            {
                float time = Time.time;
                float currentMoveTime = time - MoveStartTime;
                float moveFactor = currentMoveTime / MoveTime;
                if (moveFactor > 1)
                    RealPosition = MoveToPosition;
                moveFactor = Mathf.Clamp(moveFactor, 0f, 1f);
                moveFactor = MoveCurve.Evaluate(moveFactor);
                interpolatedRealPosition = Vector3.Lerp(RealPosition, MoveToPosition, moveFactor);
            }
            return interpolatedRealPosition;
        }

        public bool IsAnimating
        {
            get;
            private set;
        }

        void UpdateAnimation()
        {
            if (Args == null)
                return;
            if (DeleteAnimation !=null && MoveTime > DeleteAnimation.Time)
                MoveTime = DeleteAnimation.Time;
            if (InsertAnimation != null && MoveTime > InsertAnimation.Time)
                MoveTime = InsertAnimation.Time;
            float time = Time.time;
            Vector3 interpolatedRealPosition = GetInterpolatedPosition();
            if (MoveTime <= 0f)
                RealPosition = MoveToPosition;
            if (Current == null || Current.Animation == null)
            {
                transform.localPosition = interpolatedRealPosition;
                transform.localScale = new Vector3(Args.Parent.fontSize, Args.Parent.fontSize, Args.Parent.fontSize);
                transform.localRotation = Quaternion.identity;
                IsAnimating = false;
            }
            else
            {
                float currnetTime = time - StartTime;
                float factor = 1f;
                if (Current.Time > 0f)
                    factor = Mathf.Clamp(currnetTime / Current.Time, 0f, 1f);
                if (factor == 1f)
                    IsAnimating = false;
                Vector3 targetPosition = interpolatedRealPosition;
                Vector3 targetScale = new Vector3(Args.Parent.fontSize, Args.Parent.fontSize, Args.Parent.fontSize);
                Vector3 startPosition = targetPosition - Current.Animation.Translation;
                Vector3 startScale = Current.Animation.Scale * Args.Parent.fontSize;
                Vector3 targetRotation = Vector3.zero;
                Vector3 startRotation = Current.Animation.rotation;
                if(Current != null && ((Current.Mode & (AnimationEntry.AnimationMode.Reveresed)) != AnimationEntry.AnimationMode.None))
                    factor = 1f - factor;
                float rotationFactor = Current.Animation.RotationCurve.Evaluate(factor);
                float scaleFactor = Current.Animation.ScaleCurve.Evaluate(factor);
                float translateFacotr = Current.Animation.TranslationCurve.Evaluate(factor);

                if (Current != null && ((Current.Mode & (AnimationEntry.AnimationMode.Inverted)) != AnimationEntry.AnimationMode.None))
                {
                    rotationFactor = 1f - rotationFactor;
                    scaleFactor = 1f - scaleFactor;
                    translateFacotr = 1f - translateFacotr;
                }

                transform.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, translateFacotr);
                transform.localScale = targetScale;
                transform.localRotation = Quaternion.identity;

                Vector3 anchor = new Vector3(Current.Animation.Anchor.x * Args.Size.x, Current.Animation.Anchor.y * Args.Size.y, Current.Animation.Anchor.z * Args.Size.z);
                var center = transform.TransformPoint(anchor);

                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleFactor);
                transform.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(startRotation, targetRotation, rotationFactor));
                var transformed = transform.TransformPoint(anchor);

                var diff = Parent.transform.InverseTransformVector(center - transformed);
                transform.localPosition += diff;
            }
        }

        public void Update()
        {
            UpdateAnimation();
        }
    }
}
