using Assets.Script.Animation;
using Assets.Text3D.Script.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class DynamicTextElement : TextElement
{
    protected const int QueueAnimate = 0;
    protected const int QueueAnimateDontOverride = 4;
    protected const int QueueAddAnimation = 1;
    protected const int QueueRemoveAnimation = 2;
    protected const int QueueInvalidate = 3;

    public Material FaceMaterial;
    public Material SideMaterial;
    public MeshFilter LetterPrefab;
    string mPrevText = null;
    string mPreviewText = null;

    class RecycleEntry
    {
        public RecycleEntry(GameObject obj, float recycleTime)
        {
            Object = obj;
            RecycleTime = recycleTime;
        }
        public GameObject Object;
        public float RecycleTime;
    }
    [Serializable]
    public class ClickEventArgs
    {
        public ClickEventArgs(int index,int wordStart,int wordEnd,string word,string letter)
        {
            Index = index;
            WordStartIndex = wordStart;
            WordEndIndex = wordEnd;
            Word = word;
            Letter = letter;
        }
        public int Index { get; private set; }
        public int WordStartIndex { get; private set; }
        public int WordEndIndex { get; private set; }
        public string Word { get; private set; }
        public string Letter { get; private set; }
    }
    [Serializable]
    public class ClickEvent : UnityEvent<ClickEventArgs>
    {



    }

    public ClickEvent OnGlyphClicked = new ClickEvent();
    List<GameObject> mObjectPool = new List<GameObject>();
    List<GameObject> mActiveGlyphs = new List<GameObject>();
    List<RecycleEntry> mRecycleList = new List<RecycleEntry>();
    List<GlyphEvents> mTmpEvents = new List<GlyphEvents>();

    AnimationQueue mSequencedQueue = new AnimationQueue();
    AnimationQueue mImidiateQueue = new AnimationQueue();
    float mMoveDelay = 0f;
    void ApplyEntryToPreviewText(AnimationQueue.AnimationQueueEntry entry)
    {
        switch (entry.ActionType)
        {
            case QueueAddAnimation:
                mPreviewText = mPreviewText.Insert(entry.Index, entry.Text);
                break;
            case QueueRemoveAnimation:
                mPreviewText = mPreviewText.Remove(entry.Index, entry.Count);
                break;
        }
    }
    void EnsurePreviewText()
    {
        if (mPreviewText == null)
        {
            mPreviewText = text;
            foreach (AnimationQueue.AnimationQueueEntry entry in mSequencedQueue.mAnimationQueue)
                ApplyEntryToPreviewText(entry);
        }
    }

    public static void ShowTextEffect(GameObject prefab,string text)
    {
        GameObject instance = GameObject.Instantiate(prefab.gameObject); // create a new instance of the prefab.
        DynamicTextElement textElement = instance.GetComponentInChildren<DynamicTextElement>(); // find the underlaying dynamic text object
        if (textElement == null)
            return; // no text element with this prefab. 

        // the prefabs that come with this asset have a self destory component . so there is nothing additional that should be done except instanciating them
        textElement.Text = text; // format the text and assign it to the ready made animated prefab
    }

    public override void Invalidate()
    {
        //do not invalidate in the standard way
        //base.Invalidate();
        // instead add this to the imidiate animation queue
        mImidiateQueue.ClearQueue(); // all the previous operations do not apply anymore , so they are removed
        mPrevText = text; // the prev text is now the current text
        AnimationQueue.AnimationQueueEntry entry = new AnimationQueue.AnimationQueueEntry();
        entry.ActionType = QueueInvalidate;
        entry.Text = text;
        mImidiateQueue.EnqueueEntry(entry);
    }

    public void SetCaretPosition(int index,ref Vector3 position)
    {
        if (text.Length == 0 || index < 0 || index >= mCaretPositions.Count) // the caret position is not available yet
            return;

        var glyph = mCaretPositions[index];
        position = transform.TransformPoint(glyph);
    }

    public override void Awake()
    {

        DeleteAll();
        base.Awake();
        // Invalidate();
        string initText = text;
        text = "";
        mPrevText = "";
        AppendText(initText);
    }

    protected override void OnTextChanged()
    {
        mImidiateQueue.ClearQueue();
        mPrevText = text;
        Invalidate();
    }

    void DeleteAll()
    {
        mObjectPool.Clear();
        mActiveGlyphs.Clear();
        mRecycleList.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var gameObject = transform.GetChild(i).gameObject;
            if (gameObject == null || gameObject.GetComponent<TextEffectsItem>() == null)
                continue;
            CommonMethods.SafeDestroy(gameObject);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    void ClearTransform()
    {
        mObjectPool.Clear();
        mActiveGlyphs.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var gameObject = transform.GetChild(i).gameObject;
            if (gameObject == null || gameObject.GetComponent<TextEffectsItem>() == null)
                continue;
            gameObject.SetActive(false);
            var events = gameObject.GetComponent<GlyphEvents>();
            if (events != null)
                events.Reset(this);
            mObjectPool.Add(gameObject);
        }
    }

    void RecycleGlyph(GameObject obj)
    {
        obj.SetActive(false);
        var events = obj.GetComponent<GlyphEvents>();
        if (events != null)
            events.Reset(this);
        mObjectPool.Add(obj);
    }
    public void RemoveAt(int index, AnimationEntry animation = null)
    {
        RemoveText(index, 1, animation);
    }
    public void RemoveLast(AnimationEntry animation = null)
    {
        if (mPreviewText.Length == 0)
            return;
        RemoveText(mPreviewText.Length - 1, 1);
    }

    public void RemoveText(int index, int count, AnimationEntry animation = null)
    {
        InnerQueueRemoveText(index, count, mImidiateQueue, ref text, animation);
    }

    public void InsertText(int index, string str, AnimationEntry animation = null)
    {
        InnerQueueInsertText(index, str, mImidiateQueue, ref text, animation);
    }

    public void AppendText(string str, AnimationEntry animation = null)
    {
        InnnerQueueAppendText(str, mImidiateQueue, ref text, animation);

    }
    public void QueueRemoveAt(int index, AnimationEntry animation = null)
    {
        QueueRemoveText(index, 1, animation);
    }

    public void QueueRemoveLast(AnimationEntry animation = null)
    {
        EnsurePreviewText();
        if (mPreviewText.Length == 0)
            return;
        QueueRemoveText(mPreviewText.Length - 1, 1);
    }

    public void QueueRemoveText(int index, int count, AnimationEntry animation = null)
    {
        EnsurePreviewText();
        InnerQueueRemoveText(index, count, mSequencedQueue, ref mPreviewText, animation);
    }

    public void QueueInsertText(int index, string str, AnimationEntry animation = null)
    {
        EnsurePreviewText();
        InnerQueueInsertText(index, str, mSequencedQueue, ref mPreviewText, animation);
    }

    public void QueueAppendText(string str, AnimationEntry animation = null)
    {
        EnsurePreviewText();
        InnnerQueueAppendText(str, mSequencedQueue, ref mPreviewText, animation);
    }

    void InnerQueueInsertText(int index, string str, AnimationQueue queue, ref string changedText, AnimationEntry animation)
    {
        if (index > changedText.Length)
        {
            Debug.LogWarning("Queue insert text does not match the preview text, make sure the index parameter is within the text string length. Queue operation aborted");
            return;
        }
        changedText = changedText.Insert(index, str);
        var glyphAnimator = GetComponent<GlyphAnimator>();
        if (animation == null && glyphAnimator != null)
            animation = GetComponent<GlyphAnimator>().InsertAnimation;
        var entry = new AnimationQueue.AnimationQueueEntry();
        entry.ActionType = QueueAddAnimation;
        entry.Index = index;
        entry.Text = str;
        if (animation != null)
            entry.Time = animation.Time;
        entry.Animation = animation;
        queue.EnqueueEntry(entry);
    }

    void InnnerQueueAppendText(string str, AnimationQueue queue, ref string changedText, AnimationEntry animation)
    {
        var glyphAnimator = GetComponent<GlyphAnimator>();
        if (animation == null && glyphAnimator != null)
            animation = GetComponent<GlyphAnimator>().InsertAnimation;
        var entry = new AnimationQueue.AnimationQueueEntry();
        entry.Index = changedText.Length;
        changedText = changedText.Insert(changedText.Length, str);
        entry.ActionType = QueueAddAnimation;

        entry.Text = str;
        if (animation != null)
            entry.Time = animation.Time;
        entry.Animation = animation;
        queue.EnqueueEntry(entry);
    }

    void InnerQueueRemoveText(int index, int count, AnimationQueue queue, ref string changedText, AnimationEntry animation)
    {
        if (index + count > changedText.Length)
        {
            Debug.LogWarning("Queue remove text does not match the preview text, make sure the index and count parameters are within the text string legnth. Queue operation aborted");
            return;
        }
        changedText = changedText.Remove(index, count);
        var glyphAnimator = GetComponent<GlyphAnimator>();
        if (animation == null && glyphAnimator != null)
            animation = GetComponent<GlyphAnimator>().InsertAnimation;
        var entry = new AnimationQueue.AnimationQueueEntry();
        entry.ActionType = QueueRemoveAnimation;
        entry.Index = index;
        entry.Count = count;
        if (animation != null)
            entry.Time = animation.Time;
        entry.Animation = animation;
        queue.EnqueueEntry(entry);
    }

    public void InnerAnimateGlyph(int index, AnimationEntry animation, bool dontOverride)
    {
        if (FontInstance == null)
            return;
        var obj = mActiveGlyphs[index];
        if (dontOverride)
        {
            var animator = obj.GetComponent<GlyphAnimator>();
            if (animator != null)
                if (animator.IsAnimating) // dont override currently playing animation
                    return;
        }
        if (obj != null)
        {
            var events = obj.GetComponent<GlyphEvents>();
            events.Animate(animation);
        }
    }

    public void AnimateGlyph(int index, AnimationEntry animation, bool dontOverride = false)
    {

        if (animation == null)
        {
            Debug.LogWarning("cannot animate null animation, AnimateGlyph aborted");
            return;
        }
        if (index < 0 || index >= text.Length)
        {
            Debug.LogWarning("the selected index is outside the bounds of the text, AnimateGlyph aborted");
            return;
        }
        if (FontInstance == null)
            return;

        var entry = new AnimationQueue.AnimationQueueEntry();
        entry.ActionType = QueueAnimate;
        if (dontOverride)
            entry.ActionType = QueueAnimateDontOverride;
        entry.Index = index;
        entry.Animation = animation;
        mImidiateQueue.EnqueueEntry(entry);
    }

    public void InnerRemoveText(int index, int count, AnimationEntry animation = null)
    {
        for (int i = 0; i < count; i++)
        {
            InnerRemoveAt(index);
        }

        GlyphAnimator animator = GetComponent<GlyphAnimator>();
        float delay = 0f;

        if (animator != null)
            delay = animator.MoveTime;

        delay *= GetMoveDelayMultiplier(animation, false);

        mMoveDelay = delay;
        GenerateText(false, mPrevText);
        mMoveDelay = 0f;


    }

    float GetMoveDelayMultiplier(AnimationEntry animation, bool isAdd)
    {
        if (animation == null)
        {
            GlyphAnimator animator = GetComponent<GlyphAnimator>();
            if (animator != null)
            {
                if (isAdd && animator.InsertAnimation != null && animator.InsertAnimation.Animation != null)
                    return animator.InsertAnimation.Animation.MoveGlyphMultiplier;
                if (isAdd == false && animator.DeleteAnimation != null && animator.DeleteAnimation.Animation != null)
                    return animator.DeleteAnimation.Animation.MoveGlyphMultiplier;
            }
            return 1f;
        }
        if (animation.Animation == null)
            return 1f;
        return animation.Animation.MoveGlyphMultiplier;
    }
    void InnerInsertText(string str, int position, AnimationEntry animation = null)
    {
        for (int i = str.Length - 1; i >= 0; i--)
            InnerInserGlyph(str[i], position, animation);

        GenerateText(false, mPrevText);

        GlyphAnimator animator = GetComponent<GlyphAnimator>();
        float delay = 0f;
        if (animator != null)
            delay = animator.MoveTime;
        delay *= GetMoveDelayMultiplier(animation, true);
        for (int i = 0; i < mTmpEvents.Count; i++)
        {
            var events = mTmpEvents[i];
            events.MoveImidiate = false;
            events.Add(events.Index, events.GlyphText, events.Position, true, delay, animation);
        }
        mTmpEvents.Clear();
    }

    void InnerInserGlyph(char glyph, int position, AnimationEntry animation = null)
    {
        if (FontInstance == null)
            return;
        mPrevText = mPrevText.Insert(position, glyph.ToString());
       // FontInstance.EnsureGlyphs(glyph.ToString());
        var gameobj = UpdateGlyph(position, glyph, Vector3.zero, true);
        GlyphEvents events = null;
        if (gameobj != null)
            events = gameobj.GetComponent<GlyphEvents>();
        if (events != null)
        {
            events.MoveImidiate = true;
            mTmpEvents.Add(events);
        }

    }

    void InnerRemoveAt(int index, AnimationEntry animation = null)
    {
        if (FontInstance == null)
            return;
        var obj = mActiveGlyphs[index];
        mActiveGlyphs.RemoveAt(index);
        if (obj != null)
        {
            var events = obj.GetComponent<GlyphEvents>();
            if (events == null)
                RecycleGlyph(obj);
            else
            {
                RecycleEntry entry = new RecycleEntry(obj, Time.time + events.RemoveTime);
                mRecycleList.Add(entry);
                events.Delete(0f, animation);
            }
        }
        mPrevText = mPrevText.Remove(index, 1);
    }

    protected override void GenerateText(bool fullRefresh, string replaceText = null)
    {
        if (fullRefresh)
        {
            ClearTransform();
            mPrevText = text;
        }
        if (FontInstance == null)
            return;
        if (FontInstance.FontLoaded == false)
            return;

        if (LetterPrefab == null)
        {
            Debug.LogWarning("DynamicTextElement's Letter prefab cannot be null");
            return;
        }
        base.GenerateText(fullRefresh, replaceText);
    }

    ClickEventArgs CreateEventArgs(int index)
    {
        int wordStart = 0;
        int wordEnd = text.Length;
        string letter = text[index].ToString();
        if(Char.IsWhiteSpace(letter[0]))
            return new ClickEventArgs(index, index, index, letter, letter);
        for(int i=0; i<index; i++)
        {
            char ch = text[i];
            if (Char.IsWhiteSpace(ch))
                wordStart = i+1;
        }
        wordEnd = wordStart;
        while(wordEnd<text.Length && !Char.IsWhiteSpace(text[wordEnd]))
            wordEnd++;
        if (wordStart < text.Length && wordEnd <= text.Length)
            return new ClickEventArgs(index, wordStart, wordEnd, text.Substring(wordStart, wordEnd - wordStart), letter);
        return new ClickEventArgs(index, index, index, letter, letter);
    }
    public void ItemClicked(GameObject obj)
    {
        var events =obj.GetComponent<GlyphEvents>();
        if(events != null)
        {
            if (OnGlyphClicked != null)
                OnGlyphClicked.Invoke(CreateEventArgs(events.Index));
        }
    }

    void EnsureAnimator(GameObject gameObj)
    {
        var mainAnimator = GetComponent<GlyphAnimator>();
        if (mainAnimator != null)
        {
            var subAnimator = gameObj.GetComponent<GlyphAnimator>();
            if (subAnimator == null)
                subAnimator = gameObj.AddComponent<GlyphAnimator>();
            subAnimator.TakeFrom(mainAnimator);
        }
    }


    protected override GameObject UpdateGlyph(int index, int codePoint, Vector3 position, bool fullRefresh)
    {
        if (LetterPrefab == null)
            throw new Exception("Letter prefab cannot be null");
        var mesh = FontInstance.GetSharedMesh(codePoint);
      //  Debug.Log(mesh.vertexCount + " vertex");
        GameObject obj = null;
        if (fullRefresh)
        {
            if (codePoint != '\n' && codePoint != '\r')
            {
                if (mObjectPool.Count > 0)
                {
                    obj = mObjectPool[mObjectPool.Count - 1];
                    mObjectPool.RemoveAt(mObjectPool.Count - 1);
                    obj.SetActive(true);
                }
                else
                {
                    obj = GameObject.Instantiate(LetterPrefab.gameObject);
                    obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.HideAndDontSave;
                    obj.tag = "EditorOnly";
                    if (obj.GetComponent<TextEffectsItem>() == null)
                        obj.AddComponent<TextEffectsItem>();
                    if (obj.GetComponent<GlyphClickHandler>() == null)
                        obj.AddComponent<GlyphClickHandler>();
                }
            }

            mActiveGlyphs.Insert(index,obj);
            if (obj != null)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.materials = new Material[] { FaceMaterial, SideMaterial };
                EnsureAnimator(obj);
                var filter = obj.GetComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                obj.transform.SetParent(transform, false);
                var events = obj.GetComponent<GlyphEvents>();
                if (events != null)
                {
                    events.Reset(this);
                    if (mesh != null)
                        events.Size = mesh.bounds.size;
                    else
                        events.Size = new Vector3(1f, 1f, 1f);
                    events.Add(index, ((char)codePoint).ToString(), position, false);
                    events.MoveImidiate = true;
                    events.SetPosition(position, index);
                    events.MoveImidiate = false;
                }
                else
                    obj.transform.localPosition = position;
                var collider = obj.GetComponent<BoxCollider>();
                if (collider == null)
                    collider = obj.AddComponent<BoxCollider>();
                var metrics = FontInstance.GetMetrics(codePoint);
                Vector3 size = new Vector3();
                if (mesh != null)
                    size = mesh.bounds.size;
                size.x = metrics.AdvanceWidth * fontSize * spaceMultipler;
                collider.size = size;
                collider.center = collider.size*0.5f;
                if(GetComponent<GlyphAnimator>() != null && Application.isPlaying)
                    obj.transform.localScale = new Vector3(0f, 0f, 0f);
                else
                    obj.transform.localScale = new Vector3(fontSize, fontSize, fontSize);
            }
        }
        else
        {
            obj = mActiveGlyphs[index];
            if (obj != null)
            {
                var events = obj.GetComponent<GlyphEvents>();
                if (events != null)
                    events.SetPosition(position, index,mMoveDelay);
                else
                    obj.transform.localPosition = position;
            }
        }

        return obj;
    }

    public bool IsAnimaitionQueueEmpty { get { return mSequencedQueue.mAnimationQueue.Count == 0; } }

    void ProcessQueueEntry(AnimationQueue.AnimationQueueEntry entry)
    {
        switch(entry.ActionType)
        {
            case QueueInvalidate:
                GenerateText(true,entry.Text);
                mPrevText = entry.Text;
                break;
            case QueueAnimateDontOverride:
                InnerAnimateGlyph(entry.Index, entry.Animation,true);
                break;
            case QueueAnimate:
                InnerAnimateGlyph(entry.Index, entry.Animation,false);
                break;
            case QueueAddAnimation:
                InnerInsertText(entry.Text, entry.Index, entry.Animation);
                break;
            case QueueRemoveAnimation:
                InnerRemoveText(entry.Index, entry.Count, entry.Animation);
                break;
        }

    }
    public void AnimateTextChange(string newText)
    {
        RemoveText(0, text.Length);
        AppendText(newText);
    }

    public override void Update()
    {
        base.Update();
        mRecycleList.RemoveAll(x =>
        {
            if (x.RecycleTime < Time.time)
            {
                RecycleGlyph(x.Object);
                return true;
            }
            return false;
        });
        mImidiateQueue.OnProcessEntry = ProcessQueueEntry;
        mImidiateQueue.CleanQueue();
        if(text.Length != mActiveGlyphs.Count)
        {
         //   Debug.LogWarning("Text recreated");
            GenerateText(true, text);
            mPrevText = text;
        }
        mSequencedQueue.OnProcessEntry = ProcessQueueEntry;
        mSequencedQueue.UpdateAnimationQueue();
    }

    //public GameObject GetCharacterObject(int index)
    //{

    //}
}

