using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

namespace KSPModDebugUtils
{

    class WatchWindow
    {

        private Rect m_WindowRect = new Rect(512, 32, 128, 64);

        private List<KeyValuePair<System.Object, FieldInfo>> m_Watches = new List<KeyValuePair<object, FieldInfo>>();

        public bool IsWatchable(FieldInfo field)
        {
            if (field.MemberType != MemberTypes.Field && field.MemberType != MemberTypes.Property)
            {
                return false;
            }

            return true;
        }

        public bool AddWatch(System.Object obj, FieldInfo field)
        {
            if (field.MemberType != MemberTypes.Field && field.MemberType != MemberTypes.Property)
            {
                return false;
            }

            m_Watches.Add(new KeyValuePair<object, FieldInfo>(obj, field));
            return true;
        }

        public bool RemoveWatch(System.Object obj, FieldInfo field)
        {
            for (int i = 0; i < m_Watches.Count; i++)
            {
                if (m_Watches[i].Key == obj && m_Watches[i].Value == field)
                {
                    m_Watches.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void OnWatchWindow(int index)
        {
            foreach (var watch in m_Watches)
            {
                FieldInfo field = watch.Value;
                System.Object obj = watch.Key;

                GUILayout.BeginHorizontal();
                    
                Type objType = obj.GetType();
                GUILayout.Label(objType.ToString());

                if (field.MemberType == MemberTypes.Field)
                {
                    if (field.FieldType == typeof(float))
                    {
                        float value = (float)field.GetValue(obj);
                        GUIControls.FloatField(field.Name, ref value);
                        field.SetValue(obj, value);
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        int value = (int)field.GetValue(obj);
                        GUIControls.IntField(field.Name, ref value);
                        field.SetValue(obj, value);
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        string value = (string)field.GetValue(obj);
                        GUIControls.StringField(field.Name, ref value);
                        field.SetValue(obj, value);
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        bool value = (bool)field.GetValue(obj);
                        GUIControls.BoolField(field.Name, ref value);
                        field.SetValue(obj, value);
                    }
                }

                if (GUILayout.Button("X"))
                {
                    RemoveWatch(obj, field);
                    return;
                }

                GUILayout.EndHorizontal();
            }
        }

        public void OnGUI()
        {
            m_WindowRect = GUI.Window(5162, m_WindowRect, OnWatchWindow, "Watches (Scene Debugger)");
        }

    }

}
