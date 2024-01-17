using Assets.Text3D.Script.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Script.Animation
{
    public class GlyphEvents : MonoBehaviour
    {
        public float RemoveTime = 1f;

        public Vector3 Position { get; private set; }
        public string GlyphText { get; private set; }
        public int Index { get; private set; }

        DynamicTextElement ParentElement;
        public void Reset(DynamicTextElement element)
        {
            ParentElement = element;
            if (Initialize != null)
                Initialize.Invoke();
        }
        public bool MoveImidiate { get; set; }

        public Vector3 Size { get; set; }
        public void Add(int index, string glyphText,Vector3 position,bool dynamicChange,float delay = 0f, AnimationEntry animation = null)
        {
            Position = position;
            Index = index;
            GlyphText = glyphText;
            if (Added != null)
            {
                var args = new GlyphEventArgs(ParentElement,Position, Index, GlyphText,Size,animation, delay, dynamicChange);
                Added.Invoke(args);
            }
        }

        public void SetPosition(Vector3 position,int newIndex,float delay =0f)
        {
            Position = position;
            Index = newIndex;
            if(MoveImidiate)
            {
                transform.localPosition = position;
                return;
            }
            if (Move != null)
            {
                var args = new GlyphEventArgs(ParentElement,Position, Index, GlyphText, Size,null, delay);
                Move.Invoke(args);
            }
        }
        public void Animate(AnimationEntry animation, float delay = 0f)
        {
            if (Animated != null)
            {
                var args = new GlyphEventArgs(ParentElement, Position, Index, GlyphText, Size, animation,delay);
                Animated.Invoke(args);
            }
        }
        public void Delete(float delay =0f,AnimationEntry animation = null)
        {
            if (Deleted != null)
            {
                var args = new GlyphEventArgs(ParentElement,Position, Index, GlyphText, Size,animation,delay);
                Deleted.Invoke(args);
            }
        }

        public class GlyphEventArgs : EventArgs
        {
            public GlyphEventArgs(DynamicTextElement element,Vector3 position,int index,string glyphText, Vector3 size, AnimationEntry animation ,float delay, bool animate = true)
            {
                Parent = element;
                Position = position;
                WorldPosition = element.transform.TransformPoint(position);
                Index = index;
                GlyphText = glyphText;
                Animate = animate;
                Size = size;
                Animation = animation;
                Delay = delay;
            }
            public float Delay { get; private set; }
            public AnimationEntry Animation { get; private set; }
            public DynamicTextElement Parent;
            public Vector3 Size { get; private set; }
            public Vector3 Position { get; private set; }
            public Vector3 WorldPosition { get; private set; }
            public int Index { get; private set; }
            public bool Animate { get; private set; }
            public string GlyphText { get; private set; }
        }

        [Serializable]
        public class GlyphEvent : UnityEvent<GlyphEventArgs>
        {
        }

        public UnityEvent Initialize;
        public GlyphEvent Added;
        public GlyphEvent Animated;
        public GlyphEvent Deleted;
        public GlyphEvent Move;
    }
}
