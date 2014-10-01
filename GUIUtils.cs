using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace KSPSceneDebugger
{

    class GUIControls
    {

        static float nameSize = 256;
        static float spacing = 8;

        static public void FloatField(string name, ref float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameSize));
            GUILayout.Space(spacing);

            float oldValue = value;

            string newValue = GUILayout.TextField(value.ToString("0.00"));
            if (!float.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
        }

        static public void IntField(string name, ref int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameSize));
            GUILayout.Space(spacing);

            int oldValue = value;

            string newValue = GUILayout.TextField(value.ToString());
            if (!int.TryParse(newValue, out value))
            {
                value = oldValue;
            }

            GUILayout.EndHorizontal();
        }

        static public void StringField(string name, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameSize));
            GUILayout.Space(spacing);
            value =  GUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }

        static public void BoolField(string name, ref bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameSize));
            GUILayout.Space(spacing);
            value = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();
        }

    }

}
