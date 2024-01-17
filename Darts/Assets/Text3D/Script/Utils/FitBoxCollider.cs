using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitBoxCollider : MonoBehaviour {

    public float ZSize = 1f;
    Rect Current = new Rect();
	// Use this for initialization
	void Start () {
        Fit();
	}
	
    void Fit()
    {
        var element = GetComponent<DynamicTextElement>();
        if (element == null)
            return;
        
        var rect = element.GetTextRect();
        if(rect != Current)
        {
            var box = GetComponent<BoxCollider>();
            if (box == null)
                return;
            Current = rect;
            box.size = new Vector3(rect.width, rect.height, ZSize);
            box.center = new Vector3(rect.x, rect.y, 0f);
        }
    }
	// Update is called once per frame
	void Update () {
        Fit();
	}
}
