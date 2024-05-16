using Ludus.SDK.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SombraFixaUmItem : FaseLayoutFixoUmItem
{
    public int indiceSombra;
    protected override void PopularSombras(List<int> indiceSelecionado)
    {
       List<int> list = new List<int>();    
       list.Add(indiceSombra);  
        base.PopularSombras (list); 
    }    
}
