using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[Serializable]
public class Text3DSimpleAnimation : MonoBehaviour
{
    public Vector3 Translation = Vector3.zero;
    public AnimationCurve TranslationCurve;
    public Vector3 Scale = Vector3.zero;
    public AnimationCurve ScaleCurve;
    public Vector3 rotation;
    public AnimationCurve RotationCurve;
    public Vector3 Anchor = new Vector3(0.5f, 0.5f, 0.5f);
    public float MoveGlyphMultiplier = 1f;
}
