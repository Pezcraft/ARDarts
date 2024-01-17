using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#pragma warning disable 0414
[CustomEditor(typeof(DynamicTextElement))]
public class DynamicTextElementInspector : Editor {

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
           // var cast = target as DynamicTextElement;
          //  if (cast != null)
          //      cast.Invalidate();
        }
    }

}
