using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlendMaterial : MonoBehaviour
{
    [Range(0f,1f)] public float blend = 0.5f;
    public Material[] materialList;

    void Update()
    {
        if(materialList == null) return;
        for(int i =0; i<materialList.Length; i++)
        {
            if(materialList[i] != null) materialList[i].SetFloat("_Blend",blend);
        }
    }
}
