using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlendMaterial : MonoBehaviour
{
    [Range(0f,1f)] public float blend = 0.5f;

    void Update()
    {
        RTCollection.BlendMaterial(blend);
    }
}
