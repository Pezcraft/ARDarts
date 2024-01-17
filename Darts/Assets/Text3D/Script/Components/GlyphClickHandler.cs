using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlyphClickHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void OnMouseDown()
    {
        var text = GetComponentInParent<DynamicTextElement>();
        if (text != null)
            text.ItemClicked(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
