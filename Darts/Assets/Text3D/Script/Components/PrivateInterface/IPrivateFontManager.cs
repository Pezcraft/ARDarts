using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface IPrivateFontManager
{
    MeshComposer GetComposer();
    event Action FontRefresh;
    void SetData(byte[] data);
    void ShowDebug();
    void GenerateFont();
}

