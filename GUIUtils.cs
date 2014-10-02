using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace KSPModDebugUtils
{

    class GUIControls
    {

        static float fieldSize = 128;

        static public void FloatField(string name, ref float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);

            float oldValue = value;
            string oldValueString = value.ToString();
            string newValue = GUILayout.TextField(oldValueString, GUILayout.Width(fieldSize));

            if (oldValueString != newValue && !float.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static public void IntField(string name, ref int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);

            int oldValue = value;

            string newValue = GUILayout.TextField(value.ToString(), GUILayout.Width(fieldSize));
            if (!int.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static public void StringField(string name, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            value = GUILayout.TextField(value, GUILayout.Width(fieldSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static public void BoolField(string name, ref bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            value = GUILayout.Toggle(value, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }

}
