using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HeaderCustomAttribute : PropertyAttribute
{
    public Color col;
    public string textString;
    public Color backgroundColor;

    public HeaderCustomAttribute(string text, float red, float green, float blue, float alpha, float redBG, float greenBG, float blueBG, float alphaBG)
    {
        this.col = new Color(red, green, blue, alpha);
        this.backgroundColor = new Color(redBG, greenBG, blueBG, alphaBG);
        this.textString = text;
    }
    public HeaderCustomAttribute(string text, float red, float green, float blue, float alpha)
    {
        this.col = new Color(red, green, blue, alpha);
        this.textString = text;
    }

}
