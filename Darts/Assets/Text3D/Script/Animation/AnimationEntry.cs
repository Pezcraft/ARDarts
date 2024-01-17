using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Text3D.Script.Animation
{
    [Serializable]
    public class AnimationEntry
    {
        [Flags]
        public enum AnimationMode
        {
            None = 0,
            Reveresed = 1,
            Inverted = 2,
            ReveresedAndInverted = 3
        }
        public Text3DSimpleAnimation Animation;
        public float Time;
        public AnimationMode Mode;
    }
}
