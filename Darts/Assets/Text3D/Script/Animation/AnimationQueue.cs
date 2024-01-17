using Assets.Text3D.Script.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Animation
{
    class AnimationQueue
    {

        float QueueStartTime = -100f;
        public class AnimationQueueEntry
        {
            public AnimationEntry Animation;
            public int ActionType;
            public int Index;
            public int Count;
            public string Text;
            public float Time;
        }
        public Queue<AnimationQueueEntry> mAnimationQueue = new Queue<AnimationQueueEntry>();
        public Action<AnimationQueueEntry> OnProcessEntry;

        void ProcessQueueEntry(AnimationQueueEntry entry)
        {
            if (OnProcessEntry != null)
                OnProcessEntry(entry);
        }
        public void EnqueueEntry(AnimationQueueEntry entry)
        {
            if (mAnimationQueue.Count == 0)
                QueueStartTime = float.MinValue;
            mAnimationQueue.Enqueue(entry);
        }
        public void ClearQueue()
        {
            mAnimationQueue.Clear();
            QueueStartTime = float.MaxValue;
        }
        public void CleanQueue()
        {
            if (mAnimationQueue.Count == 0)
                return;
            while (mAnimationQueue.Count > 0)
            {
                ProcessQueueEntry(mAnimationQueue.Dequeue());
            }
            QueueStartTime = float.MaxValue;
        }
        public void UpdateAnimationQueue()
        {
            float time = Time.time;
            if (mAnimationQueue.Count == 0)
                return;
            var entry = mAnimationQueue.Peek();
            float animationTime = entry.Time;

            if (QueueStartTime < time - animationTime)
            {
                mAnimationQueue.Dequeue();
                if (mAnimationQueue.Count > 0)
                {
                    var newEntry = mAnimationQueue.Peek();
                    QueueStartTime = Time.time;
                    ProcessQueueEntry(newEntry);
                }
            }
        }

    }
}
