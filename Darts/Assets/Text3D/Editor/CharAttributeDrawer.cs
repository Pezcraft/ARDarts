using Assets.Text3D.Script.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Text3D.Editor
{
    [CustomPropertyDrawer(typeof(HexCharAttribute))]
    public class CharAttributeDrawer : PropertyDrawer
    {
        public HexCharAttribute charAttribute
        {
            get { return ((HexCharAttribute)attribute); }
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            string hexValue = EditorGUI.TextField(position, label,
                 property.intValue.ToString("X4"));

            int value = 0;

            if (hexValue.ToLower().StartsWith("0x"))
            {
                try
                {
                    value = Convert.ToInt32(hexValue, 16);
                }
                catch (FormatException)
                {
                    value = 0;
                }
            }
            else
            {
                if (int.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber,
                                            null, out value) == false)
                    value = 0;
            }

            if (EditorGUI.EndChangeCheck())
                property.intValue = value;
        }
    }
}
