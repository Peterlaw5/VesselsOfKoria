using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultValueAttribute : PropertyAttribute
{
    public string defValue;
    public bool invert;
    //Pass a default value for that variable (string).   
    public DefaultValueAttribute(string defaultValue, bool isInverted)
    {
        defValue = defaultValue;
        invert = isInverted;
    }
    //public DefaultValueAttribute (string defaultValue)
    //{
    //    defValue = defaultValue;
    //}
}
