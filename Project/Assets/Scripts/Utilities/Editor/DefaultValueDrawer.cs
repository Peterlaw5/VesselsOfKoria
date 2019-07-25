using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DefaultValueAttribute))]
public class DefaultValueDrawer : PropertyDrawer
{
    private static GUIStyle style = new GUIStyle();
    string text;
    int result;
    bool isActive = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DefaultValueAttribute defAttribute = attribute as DefaultValueAttribute;

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        var attributeText = defAttribute.defValue;

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect defLabelRect = new Rect(position.x, position.y, position.width, position.height);

        //property field
        Rect propertyRect = new Rect(position.x, position.y, position.width, 17);
        EditorGUI.PropertyField(propertyRect, property);

        ////if this is not a repain or the property is null exit now (use for style.draw)
        //if (Event.current.type != EventType.Repaint || property == null)
        //{
        //    return;
        //}        


        var target = property.serializedObject.targetObject;
        var targetObjectClassType = target.GetType();
        var fieldTarget = targetObjectClassType.GetField(property.propertyPath);

        //if (defAttribute.defValue == null && fieldTarget == null)
        //{
        //    text = String.Format("Default Value of " + property.name + " is: " + "<color=#ff0000>" + "Empy/null" + "</color>");
        //}
        if (fieldTarget != null)
        {
            //Get the type of variable (es. string, float, etc)
            var value = fieldTarget.GetValue(target);

            if (value != null)
            {
                var stringValue = value.ToString();

                //Compare two strings
                result = string.Compare(stringValue, attributeText, true);

                //isInverted logic is false
                if (defAttribute.invert)
                {

                    //result = 0 , same string
                    if (result == 0)
                    {
                        isActive = true;
                        return;
                    }
                    else
                    {
                        text = String.Format("<color=#ffff00>(default value: <color=#00cc00>" + attributeText + "</color>)</color>");
                        //Style options
                        style.richText = true;

                        //text = String.Format("Default Value of " + property.name + " is: <#ff0000>" + attributeText + "</color>");
                        defLabelRect.x = position.x;
                        defLabelRect.y = position.y + 17;
                        defLabelRect.width = position.width / 2;
                        defLabelRect.height = position.height;

                        //Color text
                        style.normal.textColor = Color.black;
                        //style.Draw(new Rect(position.x, position.y + 17, 10, position.height), GUIContent.none, false, false, false, false);

                        //Draw LabelField 
                        EditorGUI.LabelField(defLabelRect, text, style);
                        isActive = false;
                    }
                }
                //isInverted logic is true
                else
                {
                    //result = 0 , same string
                    if (result == 0)
                    {
                        text = String.Format("<color=#ff0000> This field can't be empty </color>");
                        //Style options
                        style.richText = true;

                        //text = String.Format("Default Value of " + property.name + " is: <#ff0000>" + attributeText + "</color>");
                        defLabelRect.x = position.x;
                        defLabelRect.y = position.y + 17;
                        defLabelRect.width = position.width / 2;
                        defLabelRect.height = position.height;

                        //Color text
                        style.normal.textColor = Color.black;
                        //style.Draw(new Rect(position.x, position.y + 17, 10, position.height), GUIContent.none, false, false, false, false);

                        //Draw LabelField 
                        EditorGUI.LabelField(defLabelRect, text, style);
                        isActive = false;

                    }
                    else
                    {
                        isActive = true;
                        return;
                    }
                }
            }

            else
            {

                return;
            }



        }



        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();

    }

    //Change height
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (isActive)
        {
            return base.GetPropertyHeight(property, label);
        }
        else
        {
            return base.GetPropertyHeight(property, label) + 17;
        }
    }
}


