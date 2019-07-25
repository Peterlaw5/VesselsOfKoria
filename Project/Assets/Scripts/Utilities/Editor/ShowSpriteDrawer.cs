using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(ShowSpriteAttribute))]
public class ShowSpriteDrawer : PropertyDrawer
{
    private static GUIStyle style = new GUIStyle();
    public float heightCustom = 0f;
    public bool defaultHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowSpriteAttribute showSprite = attribute as ShowSpriteAttribute;

        // Don't make child fields be indented
        var ident = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        //////////////////////////////////////////////////////// Tooltip Attribute ///////////////////////////////////////////////////////////////
        EditorGUI.BeginProperty(position, label, property);

        string tooltip = property.tooltip;
        var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
        if (attributes != null && attributes.Length > 0)
        {
            tooltip = ((TooltipAttribute)attributes[0]).tooltip;
        }
        else
        {
            tooltip = null;
        }

        label.tooltip = tooltip;

        Rect tooltipRect;

        if (property.objectReferenceValue != null)
        {
            tooltipRect = new Rect(position.x - 50f, position.y - 64f, position.width, position.height);
        }
        else
        {
            tooltipRect = new Rect(position.x - 50f, position.y, position.width, position.height);
        }

        EditorGUI.LabelField(tooltipRect, new GUIContent("", tooltip));


        ////////////////////////////////////////////////////////// Show Sprite ////////////////////////////////////////////////////////////////
        //create object field for the sprite
        Rect spriteRect;
        spriteRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.objectReferenceValue = EditorGUI.ObjectField(spriteRect, property.name, property.objectReferenceValue, typeof(Sprite), false);


        //if this is not a repain or the property is null exit now
        if (Event.current.type != EventType.Repaint || property.objectReferenceValue == null)
        {
            return;
        }

        //draw a sprite
        Sprite sp = property.objectReferenceValue as Sprite;

        //sprite- draw position
        spriteRect.y += EditorGUIUtility.singleLineHeight + 4;

        //Draw size (text) of sprite
        Rect sizeDrawRect = new Rect(position.x, position.y, position.width, position.height);

        sizeDrawRect.y += position.height / 2f;
        sizeDrawRect.width = 100f;
        sizeDrawRect.height = 16f;

        //CUSTOM VALUES (width, height)
        if (showSprite.spriteWidth != 0 || showSprite.spriteHeight != 0)
        {
            defaultHeight = false;
            heightCustom = showSprite.spriteWidth;


            //Position of text px label Rect            
            sizeDrawRect.y = EditorGUIUtility.singleLineHeight - 4f;
            sizeDrawRect.width = 100f;
            sizeDrawRect.height = 16f;

            //spriteRect.x = position.x;
            spriteRect.width = showSprite.spriteWidth;
            spriteRect.height = showSprite.spriteHeight;


            //Check/Fix large values (width, height)
            if (showSprite.spriteHeight > 128f || showSprite.spriteWidth > 128f)
            {
                //set sprite left
                spriteRect.x = position.x;
                sizeDrawRect.x = position.x;
                //need check
                sizeDrawRect.y -= EditorGUIUtility.singleLineHeight + (showSprite.spriteHeight / 2.3f);
                heightCustom = showSprite.spriteHeight;
            }
            else
            {
                //set sprite center
                spriteRect.x = position.width / 2;
                sizeDrawRect.x = position.x;
                heightCustom = showSprite.spriteHeight;
            }

        }
        else //default value
        {
            defaultHeight = true;

            //Position of text px label Rect
            sizeDrawRect.y += EditorGUIUtility.singleLineHeight - 4f;
            sizeDrawRect.width = 100f;
            sizeDrawRect.height = 16f;

            //Position of sprite Rect
            spriteRect.x = position.width / 2;
            spriteRect.width = 64f;
            spriteRect.height = 64f;
            heightCustom = spriteRect.height;

        }
        style.normal.background = sp.texture;

        //Draw sprite only if field has a sprite(!= null)
        if (property.objectReferenceValue != null)
        {
            style.Draw(spriteRect, GUIContent.none, false, false, false, false);

            GUIContent content = new GUIContent();
            content.text = "<color=#ffffff>" + spriteRect.width.ToString() + " x " + spriteRect.height.ToString() + " px</color>";
            style.normal.background = null;
            //EditorGUI.TextField(sizeDrawRect, content.text);
            Rect danRect = new Rect(position.x, position.y + position.height * 0.5f, 100f, 16f);
            style.Draw(danRect, content, false, false, false, false);
            //EditorGUI.LabelField(sizeDrawRect, content.text);
        }
        EditorGUI.indentLevel = ident;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Object has a sprite, add a value to default height function
        if (property.objectReferenceValue != null)
        {
            if (defaultHeight)
            {
                return base.GetPropertyHeight(property, label) + (int)64;
            }
            else
            {
                return base.GetPropertyHeight(property, label) + (int)heightCustom;
            }


        }
        //Object hasn't a sprite, return default height
        else
        {
            return base.GetPropertyHeight(property, label);
        }


    }

}
