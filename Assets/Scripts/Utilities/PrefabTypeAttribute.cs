using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabTypeAttribute : PropertyAttribute
{
    public System.Type RequiredType {  get; }
    public PrefabTypeAttribute(System.Type requiredType)
    {
        RequiredType = requiredType;
    }
}
