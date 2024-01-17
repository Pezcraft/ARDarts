using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteInEditMode]
public abstract class TextElement : MonoBehaviour, ISerializationCallbackReceiver
{
    protected List<Vector3> mPositions = new List<Vector3>();
    protected List<Vector3> mCaretPositions = new List<Vector3>();

    [SerializeField]
    public Font3DManager font3d;
     



    float mTextLeftOffset = 0f;
    float mTextTopOffset = 0f;
    float mTextWidth = 1f;
    float mTextHeight = 1f;

    public Rect GetTextRect()
    {
        return new Rect(mTextLeftOffset, mTextTopOffset, mTextWidth, mTextHeight);
    }


    protected Font3DManager FontInstance
    {
        get
        {

            return font3d;
        }
    }


    public Font3DManager Font
    {
        get
        {
            return font3d;
        }
        set
        {
            UnHookFontObject();
            font3d = value;
            HookFontObject();

        }
    }

    public abstract void Invalidate();

    void UnHookFontObject()
    {
        if (FontInstance != null)
        {
            ((IPrivateFontManager)FontInstance).FontRefresh -= TextElement_FontRefresh;
        }
    }

    protected virtual void OnValidate()
    {
        Invalidate();
    }
    void HookFontObject()
    {
        if(FontInstance != null)
        {
            ((IPrivateFontManager)FontInstance).FontRefresh -= TextElement_FontRefresh;
            ((IPrivateFontManager)FontInstance).FontRefresh += TextElement_FontRefresh;
        }
    }

    private void TextElement_FontRefresh()
    {
    //    GenerateText(true);
    }

    [SerializeField]
    [TextArea(3, 10)]
    protected string text = "";

    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            OnTextChanged();
        }
    }

    /// <summary>
    /// the size of the font in worldspace unitys
    /// </summary>
    public float fontSize;


    public float lineGapMultiplier = 1f;
    public float spaceMultipler = 1f;

    public TextWrapping textWrap;
    /// <summary>
    /// the alignment of the text realtive to the gameobject's center
    /// </summary>
    public TextAnchor alignment;

    public virtual void Awake()
    {
        HookFontObject();
    }

    protected abstract GameObject UpdateGlyph(int index,int codePoint, Vector3 position,bool fullRefresh);
    

    void FinishLine(List<Vector3> positions, List<Vector3> caretPositions,float lineSize,ref int index)
    {
        float move = 0f;
        if( alignment == TextAnchor.LowerLeft || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.UpperLeft)
        {
            //do nothing here. 
        }
        else if(alignment == TextAnchor.LowerCenter || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.UpperCenter)
        {
            move = lineSize * 0.5f;
        }
        else
        {
            move = lineSize;
        }
        for (; index < positions.Count; index++)
        {
            var v = positions[index];
            v.x -= move;
            positions[index] = v;

            v = caretPositions[index];
            v.x -= move;
            caretPositions[index] = v;

        }

    }

    void FinishText(List<Vector3> positions,List<Vector3> caretPosition, float height,float offset)
    {
        float move = 0f;
        if(alignment == TextAnchor.LowerCenter || alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerRight)
        {
            move = height;
            mTextTopOffset = height * 0.5f + offset * 0.5f;

        }
        else if(alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.MiddleRight)
        {
            move = height * 0.5f;
            mTextTopOffset = offset*0.5f ;
        }
        else
        {
            mTextTopOffset = -height * 0.5f + offset * 0.5f;


        }

        mTextHeight = height + offset;
        for (int i = 0; i < positions.Count; i++)
        {
            var v = positions[i];
            v.y += move;
            positions[i] = v;
        }

        for (int i = 0; i < caretPosition.Count; i++)
        {
            var v = caretPosition[i];
            v.y += move;
            caretPosition[i] = v;
        }

    }

    int FindNextLine(int from,string generateText)
    {
        if (from > generateText.Length)
            return -1;
        int match = generateText.IndexOf('\n', from);
        if (match == -1)
            match = generateText.Length;
        return match;
    }
    void SetLineSize(float maxLineSize)
    {
        mTextWidth = maxLineSize;
        if (alignment == TextAnchor.LowerRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.UpperRight)
        {
            mTextLeftOffset = -maxLineSize*0.5f;

        }
        else if (alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.LowerCenter || alignment == TextAnchor.UpperCenter)
        {
            mTextLeftOffset =0f;
        }
        else
        {
            mTextLeftOffset = maxLineSize * 0.5f;
        }


    }

    protected virtual void GenerateText(bool fullRefresh, string replaceText = null)
    {
        if (FontInstance.FontLoaded == false)
            return;

        //var watch = System.Diagnostics.Stopwatch.StartNew();
        //FontInstance.EnsureGlyphs(text);
        //Debug.Log("load time : " + watch.ElapsedMilliseconds);
        Vector3 position = new Vector3();

        string generateText = text;
        if (replaceText != null)
            generateText = replaceText;
        int match = FindNextLine(0, generateText);
        int current = 0;
        float wrappingSpan = textWrap.lineSpan;
        if (textWrap.enableWrap == false || wrappingSpan < fontSize)
            wrappingSpan = float.PositiveInfinity;
        var positions = mPositions;
        positions.Clear();
        mCaretPositions.Clear();
        int currentPositionsIndex = 0;
        float lineHeight =0f;
        float MaxLineSize = 0.1f;
        float advance = 0f;
        while (match >= 0)
        {
            float lineSize = 0f;
            int prevCodePoint = -1;
            
            for(int i=current; i< match; i++)
            {
                int codePoint = (int)generateText[i];
                if(codePoint == '\n' || codePoint == '\r')
                {
                    positions.Add(position);
                    mCaretPositions.Add(position);
                    continue;
                }

                var metrics = FontInstance.GetMetrics(codePoint);
             //   lineHeight = Math.Max(metrics.Bounds.height, lineHeight);
                advance = metrics.AdvanceWidth * fontSize * spaceMultipler;

                if( prevCodePoint >=0 )
                {
                    float kerning = FontInstance.GetKerning(prevCodePoint, codePoint) * fontSize * spaceMultipler;
                    advance += kerning;
                }

                if (lineSize +metrics.Bounds.width  >= wrappingSpan) // line endded
                {
                    FinishLine(positions, mCaretPositions, lineSize + metrics.Bounds.width,ref currentPositionsIndex);
                    current = i;
                    MaxLineSize = Math.Max(lineSize, MaxLineSize);
                    lineSize = 0f;
                    position.x = 0f;
                    position.y -= (FontInstance.LineGap * lineGapMultiplier  + lineHeight) * fontSize;
                    lineHeight = 0f;
                }

                positions.Add(position);
                mCaretPositions.Add(position + new Vector3(advance*0.5f,0f,0f));
                lineSize += advance;
                position.x += advance;
                prevCodePoint = codePoint;
            }
            FinishLine(positions,mCaretPositions, lineSize, ref currentPositionsIndex);
            current = match;
            MaxLineSize = Math.Max(lineSize, MaxLineSize);
            lineSize = 0f;
            position.x = 0f;
            position.y -= (FontInstance.LineGap * lineGapMultiplier + lineHeight) * fontSize;
            lineHeight = 0f;
            match = FindNextLine(match + 1, generateText);
        }
        if(mCaretPositions.Count != 0)
            mCaretPositions.Add(mCaretPositions[mCaretPositions.Count-1] +  new Vector3(advance * 0.5f,0f,0f));
        float height = Math.Abs(position.y) - FontInstance.LineGap * lineGapMultiplier * fontSize;
        float offset = FontInstance.LineGap * lineGapMultiplier * fontSize;
        SetLineSize(MaxLineSize);
        FinishText(positions,mCaretPositions, height, offset);
        current = 0;
        for (int i = 0; i < generateText.Length; i++)
        {
            int codePoint = (int)generateText[i];

            UpdateGlyph(i,codePoint, positions[current], fullRefresh);
            current++;
        }

    }

    protected virtual void OnTextChanged()
    {

    }
    public virtual void Update()
    {
        //if(mInvalidated)
        //{
        //    mInvalidated = false;

        //    GenerateText(true);

        //}
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
    //    Invalidate();
    }
}
