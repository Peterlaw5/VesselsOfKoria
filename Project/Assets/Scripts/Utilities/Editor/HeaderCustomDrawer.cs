using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HeaderCustomAttribute))]
public class HeaderCustomDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        HeaderCustomAttribute headerCustom = attribute as HeaderCustomAttribute;
        //EditorGUILayout.BeginHorizontal();
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        label.text = headerCustom.textString;

        // Precalculate some height values
        float heightHalf = (position.height - 20) * 0.5f;
        float heightOneFourth = (position.height - 20) * 0.25f;


        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;



        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.margin = new RectOffset(0, 0, 33, 44);

        //White texture background
        //style.normal.background = EditorGUIUtility.whiteTexture;
        //Custom texture background
        style.normal.background = CreateTextureBackground(headerCustom.backgroundColor, 2, 2);


        style.normal.textColor = headerCustom.col;
        style.stretchWidth = true;
        style.stretchHeight = true;


        Rect labelRect = new Rect(position.x, position.y, position.width, 17);
        EditorGUI.LabelField(labelRect, headerCustom.textString, style);


        // Set indent back to what it was
        EditorGUI.indentLevel = indent;


        Rect propertyRect = new Rect(position.x, position.y + 17, position.width, position.height - 17);
        EditorGUI.PropertyField(propertyRect, property);

        EditorGUI.EndProperty();

    }

    //Change Height fields
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 17;
    }


    private Texture2D CreateTextureBackground(Color col, int width, int height)
    {
        Color[] pix = new Color[width * height];
        //color (pixel) texture
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pix);
        texture.Apply();
        return texture;


    }
}
