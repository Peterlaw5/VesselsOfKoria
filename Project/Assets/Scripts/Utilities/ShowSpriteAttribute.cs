using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ShowSpriteAttribute : PropertyAttribute
{
    //width and height of spriteRect.
    public float spriteWidth;
    public float spriteHeight;

    public ShowSpriteAttribute()
    {
        //default value is 64 x 64px
    }

    public ShowSpriteAttribute(float sizeWidth, float sizeHeight)
    {
        this.spriteWidth = sizeWidth;
        this.spriteHeight = sizeHeight;
    }
}
