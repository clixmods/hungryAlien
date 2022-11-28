using UnityEngine;
using UnityEditor;
 
[CustomPropertyDrawer(typeof(PrefixAttribute))]
public class PrefixDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.PropertyField(position, property, new GUIContent( (attribute as PrefixAttribute).prefixName + label ));
    }
}