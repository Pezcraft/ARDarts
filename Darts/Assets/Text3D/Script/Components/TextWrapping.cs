using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public struct TextWrapping
{
    /// <summary>
    /// wraping of the text is enabled. Otherwise the text is single line
    /// </summary>
    public bool enableWrap;
    /// <summary>
    /// the size of a text line. Any text after this line is wrap and goes to the next line
    /// </summary>
    public float lineSpan;
}

