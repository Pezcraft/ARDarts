using Assets.Text3D.Script.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Text3D.Script.Components
{
    [Serializable]
    class GlyphRange
    {
#pragma warning disable 0649
        [HexChar]
        public ushort startUnicode;
        [HexChar]
        public ushort endUnicode;
#pragma warning restore 0649
        public IEnumerable<int> Glyphs()
        {
            ushort start = startUnicode;
            ushort end = endUnicode;
            if(startUnicode > endUnicode)
            {
                start = endUnicode;
                end = startUnicode;
            }
            for (int i = start; i <= end; i++)
                yield return i;
        }
    }
}
