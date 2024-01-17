using Assets.Script.GlyphProccessing;
using Assets.Text3D.Script.Components;
using SharpFont.CustomAdditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Typography.OpenFont;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class Font3DManager : ScriptableObject, IPrivateFontManager, ISerializationCallbackReceiver
{
    [ThreadStatic]
    static GlyphPipeline mGlyphPipeline = new GlyphPipeline();

    [SerializeField]
    private GlyphSet glyphSet = GlyphSet.Minimal;

#pragma warning disable 0649
    [SerializeField]
    private GlyphRange[] glyphRange;
#pragma warning restore 0649

    [NonSerialized]
    [HideInInspector]
    public bool tagged = false;

    public GlyphSet GlyphSet
    {
        get
        {
            return glyphSet;
        }
        set
        {
            glyphSet = value;

            AddGlyphs();
        }
    }


#if UNITY_EDITOR
    [SerializeField]
    ModelImporterMeshCompression MeshCompression = ModelImporterMeshCompression.Off;
#endif
    [SerializeField]
    private byte[] fontData = null;


#pragma warning disable 0414
    [SerializeField]
    private string fontPath = "";
#pragma warning restore 0414

    [SerializeField]
    private int smoothing = 40;
    public int Smoothing
    {
        get
        {
            return smoothing;
        }
        set
        {
            smoothing = value;
            AddGlyphs();
        }
    }

    [SerializeField]
    private float arrowAngle = 0;
    [SerializeField]
    private float depth = 0.3f;

    Typeface mFontFace;
    [SerializeField]
    public float LineGap = 1f;
    MeshComposer mComposer = new MeshComposer();
    float mFontScaleToOne = 1.0f;

    [Serializable]
    public struct GlyphMetrics
    {
        public float AdvanceWidth;
        public Rect Bounds;
    }

    [Serializable]
    class MeshEntry
    {
        public char codePoint;
        public Mesh mesh;
        public GlyphMetrics metrics;
    }

    Dictionary<int, MeshEntry> mMeshes = new Dictionary<int, MeshEntry>();

    [SerializeField]
    MeshEntry[] LetterMeshes = null;

    Thread mLoadThread;
    volatile float mLoadingProgress;
    public bool FontLoaded { get { return true; } } 
   // event Action InnerFontRefresh;


    void IPrivateFontManager.SetData(byte[] data)
    {
        fontData = data;
    }

    event Action IPrivateFontManager.FontRefresh
    {
        add
        {
        //    InnerFontRefresh += value;
        }

        remove
        {
         //   InnerFontRefresh -= value;
        }
    }

    void Clear()
    {
        foreach (var mesh in mMeshes.Values)
            CommonMethods.SafeDestroy(mesh.mesh);
        mMeshes.Clear();
        mComposer.ClearGlyphs();
    }

    void RefreshSettings()
    {
        mComposer.Dynamic = true;
    }

    private void LoadFromFile(string filePath)
    {
        Clear();
        InnerLoad(filePath);
    }

    void LoadRaw()
    {
       // Debug.Log("raw");
        Clear();
        try
        {
         //   mLoadedPath = fontPath;
          //  mSmoothing = smoothing;
            OpenFontReader reader = new OpenFontReader();
            using (var stream = new MemoryStream(fontData))
                mFontFace = reader.Read(stream);
            if (mFontFace == null)
                throw new Exception();
            ApplyEffect();
            AddGlyphs();
            //mError = false;


        }
        catch (Exception)
        {
            Debug.LogWarning("Failed loading the selected font. Please make sure the font format is supported");
            //mError = true;
            //font loading error
        }
    }

    void InnerLoad(string filePath)
    {
        try
        {
          //  mLoadedPath = filePath;
         //   mSmoothing = smoothing;
            OpenFontReader reader = new OpenFontReader();
            using (var stream = File.OpenRead(filePath))
                mFontFace = reader.Read(stream);
            if (mFontFace == null)
                throw new Exception();
            fontPath = filePath;
            AddGlyphs();



        }
        catch (Exception e)
        {
            throw e;
            //font loading error
        }
    }

    public GlyphMetrics GetMetrics(int codePoint)
    {
        MeshEntry entry;
        if (mMeshes.TryGetValue(codePoint, out entry))
            return entry.metrics;
        GlyphMetrics def = new GlyphMetrics();
        def.AdvanceWidth = 1f;
        def.Bounds = new Rect(0f, 0f, 1f, 1f);
        return def;
    }

    public Mesh GetSharedMesh(int codePoint)
    {
        MeshEntry mesh;
        if (mMeshes.TryGetValue(codePoint, out mesh))
            return mesh.mesh;
        return null;
    }

    private void OnDestroy()
    {
        Clear();
    }

    public float GetKerning(int from,int to)
    {
//        var fromIdx = mFontFace.LookupIndex(from);
//        var toIdx = mFontFace.LookupIndex(to);
        return 0f;
//        return ((float)mFontFace.GetKernDistance(fromIdx, toIdx)) * mFontScaleToOne;
    }
    MeshEntry GetEntry(int codePoint)
    {
        MeshEntry res;
        if(mMeshes.TryGetValue(codePoint,out res) == false)
        {
            res = new MeshEntry();
            res.codePoint = (char)codePoint;
            mMeshes[codePoint] = res;
        }
        return res;
    }

    void TakeGlyph(Glyph glyph,ushort glyphIndex,int codePoint)
    {

        if (mGlyphPipeline == null)
            mGlyphPipeline = new GlyphPipeline();
        var pipeline = mGlyphPipeline;
        pipeline.VerticesPerUnit = smoothing;
        var entry = GetEntry(codePoint);
        GlyphMetrics metrics = new GlyphMetrics();
        metrics.Bounds = new Rect(glyph.Bounds.XMin * mFontScaleToOne, glyph.Bounds.YMin * mFontScaleToOne, (glyph.Bounds.XMax - glyph.Bounds.XMin) * mFontScaleToOne, (glyph.Bounds.YMax - glyph.Bounds.YMin) * mFontScaleToOne);
        metrics.AdvanceWidth = ((float)mFontFace.GetHAdvanceWidthFromGlyphIndex(glyphIndex)) * mFontScaleToOne;
        entry.metrics = metrics;
        if (glyph != null)
        {
            if (mComposer.HasGlyph(glyphIndex) == false)
            {
                var composition = pipeline.RunPipeline(glyph.GlyphPoints, glyph.EndPoints, mFontScaleToOne);
                mComposer.AddGlyph(glyphIndex, composition);
            }

            entry.mesh = mComposer.CreateMesh(glyphIndex);
            if (entry.mesh != null)
            {
                if (entry.mesh.vertexCount > 0)
                {
#if UNITY_EDITOR
                    MeshUtility.Optimize(entry.mesh);
                    MeshUtility.SetMeshCompression(entry.mesh, MeshCompression);
                    AssetDatabase.AddObjectToAsset(entry.mesh, this);
#endif
                }
                else
                {
                    entry.mesh = null;
                }
            }
        }
    }

    void ApplyEffect()
    {
        mComposer.ClearNodes();
        NodeSettings node = new NodeSettings();
        node.Fill = FillType.Front;
        node.TransitionType = EdgeType.Sharp;
        node.TextureV = 0f;
        node.FillOffset = 0f;// fillOffset;
        node.HoleOffset = 0f;// holeOffset;
        node.Rotation = Quaternion.AngleAxis(arrowAngle, Vector3.right);
        mComposer.AddNode(node);

        node = new NodeSettings();
        node.TextureV = 1f;
        node.Fill = FillType.Back;
        node.TransitionType = EdgeType.None;
        node.Rotation = Quaternion.AngleAxis(-arrowAngle, Vector3.right);
        node.FillOffset = 0f;// fillOffset;
        node.HoleOffset = 0f;// holeOffset;
        node.Translate = new Vector3(0f, 0f, depth);
        mComposer.AddNode(node);

//        var effect = GetComponent<FontManager3DEffect>();
//        effect.ApplyToFont();
    }

    public void EnsureLoaded()
    {
        //if (mFontFace == null && mError == false)
        //    LoadRaw();
    }

    IEnumerable<int> RangeEnumeration()
    {
        foreach (GlyphRange r in glyphRange)
            foreach (int i in r.Glyphs())
                yield return i;
    }
    
    void AddGlyphs()
    {
        Clear();
        float ScaleToOne = 1f / (mFontFace.Bounds.YMax - mFontFace.Bounds.YMin);
        mFontScaleToOne = ScaleToOne;
        LineGap = (mFontFace.LineGap + Math.Abs(mFontFace.Ascender) + Math.Abs(mFontFace.Descender)) * ScaleToOne;
        var rangeEnum = GlyphSetEnumerable.GetGlyphs(glyphSet); 
        if (glyphSet == GlyphSet.Dynamic)
            rangeEnum = RangeEnumeration();
        int total = rangeEnum.Count();
        int current = 0;
        foreach (int codePoint in rangeEnum)
        {
            current++;
#if UNITY_EDITOR
            double progress = (double)current / (double)total;
            if (EditorUtility.DisplayCancelableProgressBar(
"Creating font meshes",
"This may take a momenet , depending on the amount of generated glyphs",
(float)progress))
            {
                return;
            }
#endif
            ushort index = mFontFace.LookupIndex(codePoint);
            if (index == 0)
                continue;
            TakeGlyph(mFontFace.Glyphs[index], index,codePoint);
        }
    }

    MeshComposer IPrivateFontManager.GetComposer()
    {
        return mComposer;
    }

    public void ShowDebug()
    {
      
    }

    void IPrivateFontManager.GenerateFont()
    {
        if (fontData == null || fontData.Length == 0)
        {
            Debug.LogWarning("No font selected. Aborting");
            return;
        }
#if UNITY_EDITOR

            foreach (var entry in mMeshes.Values)
        {
            GameObject.DestroyImmediate(entry.mesh,true);
        }
        AssetDatabase.SaveAssets();
        if (EditorUtility.DisplayCancelableProgressBar(
    "Creating font meshes",
    "This may take a momenet , depending on the amount of generated glyphs",
    0f))
        {
            return;
        }
#endif
        LoadRaw();
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
#endif
    }

    public void OnBeforeSerialize()
    {
        LetterMeshes = mMeshes.Values.ToArray();
    }

    public void OnAfterDeserialize()
    {
        if (LetterMeshes != null)
        {
            foreach (MeshEntry entry in LetterMeshes)
            {
                mMeshes[entry.codePoint] = entry;
            }
        }
    }
}

