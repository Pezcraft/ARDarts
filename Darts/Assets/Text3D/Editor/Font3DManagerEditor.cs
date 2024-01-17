using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Font3DManager))]
public class Font3DManagerEditor : Editor
{

    private static readonly string[] exculde = new string[] { "m_Script" ,"fontData","fontPath"};
    private static readonly string[] exculdeWithRange = new string[] { "m_Script", "fontData", "fontPath","glyphRange"};


    public byte[] FileToByteArray(string fileName)
    {
        byte[] buff = null;
        FileStream fs = new FileStream(fileName,
                                       FileMode.Open,
                                       FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        long numBytes = new FileInfo(fileName).Length;
        buff = br.ReadBytes((int)numBytes);
        return buff;
    }

    public string DropFont()
    {
        Rect rectPos = GUILayoutUtility.GetRect(0.0f, 65.0f, GUILayout.ExpandWidth(true));
        GUI.Box(rectPos, "Drag and drop a unity font here");

        switch (Event.current.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!rectPos.Contains(Event.current.mousePosition))
                    return null;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    if(DragAndDrop.objectReferences.Length >0)
                    {
                        Font f = DragAndDrop.objectReferences[0] as Font;
                        if (f == null)
                            return null;
                        return AssetDatabase.GetAssetPath(f);
                    }
                }
                break;
        }
        return null;
    }
    public override void OnInspectorGUI()
    {
        var fontPathProp = serializedObject.FindProperty("fontPath");

        string fontName = fontPathProp.stringValue;
        if (fontName == null)
            fontName = "";
        var splits = fontName.Split('/', '\\');
        fontName = splits[splits.Length - 1];
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        GUILayout.TextField(fontName);
        var font3D = target as IPrivateFontManager;
        byte[] bytes = null;
        string path = null;
        path = DropFont();
        if (GUILayout.Button("Load from file...") || path != null)
        {
            if(path == null)
                path = EditorUtility.OpenFilePanel("Select the base font for the 3D mesh", Application.dataPath, "ttf");
            try
            {
                bytes = FileToByteArray(path);
                font3D = target as IPrivateFontManager;
                font3D.SetData(bytes);
                EditorUtility.SetDirty(target);
            }
            catch(Exception )
            {
                bytes = null;
                path = null;
                EditorUtility.DisplayDialog("File cannot be loaded", "The file at the path \"" + path + "\" is either missing or corrupt", "Ok");
            }

        }
        var showArray = exculdeWithRange;
        var prop = serializedObject.FindProperty("glyphSet");
        if (prop != null && (GlyphSet)(Enum.GetValues(typeof(GlyphSet)).GetValue(prop.enumValueIndex)) == GlyphSet.Dynamic)
            showArray = exculde;
        serializedObject.Update();
        if(path != null)
            fontPathProp.stringValue = path;
        font3D.ShowDebug();
        if(GUILayout.Button("Generate Font Meshes"))
        {
            if (font3D != null)
                font3D.GenerateFont();
        }
        EditorGUILayout.Separator();
        
        DrawPropertiesExcluding(serializedObject, showArray);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        serializedObject.ApplyModifiedProperties();


    }
}
