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

        private Rect m_WindowRect = new Rect(512, 32, 512, 256);

        private List<KeyValuePair<System.Object, FieldInfo>> m_FieldWatches = new List<KeyValuePair<object, FieldInfo>>();
        private List<KeyValuePair<System.Object, PropertyInfo>> m_PropertyWatches = new List<KeyValuePair<object, PropertyInfo>>(); 

        public void AddWatch(System.Object obj, FieldInfo field)
        {
            m_FieldWatches.Add(new KeyValuePair<object, FieldInfo>(obj, field));
        }

        public void AddWatch(System.Object obj, PropertyInfo field)
        {
            m_PropertyWatches.Add(new KeyValuePair<object, PropertyInfo>(obj, field));
        }

        public bool RemoveWatch(System.Object obj, FieldInfo field)
        {
            for (int i = 0; i < m_FieldWatches.Count; i++)
            {
                if (m_FieldWatches[i].Key == obj && m_FieldWatches[i].Value == field)
                {
                    m_FieldWatches.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveWatch(System.Object obj, PropertyInfo field)
        {
            for (int i = 0; i < m_PropertyWatches.Count; i++)
            {
                if (m_PropertyWatches[i].Key == obj && m_PropertyWatches[i].Value == field)
                {
                    m_PropertyWatches.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void OnWatchWindow(int index)
        {
            foreach (var watch in m_FieldWatches)
            {
                FieldInfo field = watch.Value;
                System.Object obj = watch.Key;

                GUILayout.BeginHorizontal();
                    
                Type objType = obj.GetType();
                GUILayout.Label(objType.ToString());

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
                else
                {
                    GUILayout.Label(field.Name + " " + field.GetValue(watch.Key).ToString());
                }

                if (GUILayout.Button("X"))
                {
                    RemoveWatch(obj, field);
                    return;
                }

                GUILayout.EndHorizontal();
            }

            foreach (var watch in m_PropertyWatches)
            {
                PropertyInfo field = watch.Value;
                System.Object obj = watch.Key;

                GUILayout.BeginHorizontal();

                Type objType = obj.GetType();
                GUILayout.Label(objType.ToString());

                if (field.GetValue(obj, null) == null)
                {
                    GUILayout.Label("null");
                }
                else if (field.PropertyType == typeof(float))
                {
                    float value = (float)field.GetValue(obj, null);
                    GUIControls.FloatField(field.Name, ref value);
                    field.SetValue(obj, value, null);
                }
                else if (field.PropertyType == typeof(int))
                {
                    int value = (int)field.GetValue(obj, null);
                    GUIControls.IntField(field.Name, ref value);
                    field.SetValue(obj, value, null);
                }
                else if (field.PropertyType == typeof(string))
                {
                    string value = (string)field.GetValue(obj, null);
                    GUIControls.StringField(field.Name, ref value);
                    field.SetValue(obj, value, null);
                }
                else if (field.PropertyType == typeof(bool))
                {
                    bool value = (bool)field.GetValue(obj, null);
                    GUIControls.BoolField(field.Name, ref value);
                    field.SetValue(obj, value, null);
                }
                else
                {
                    GUILayout.Label(field.Name + " " + field.GetValue(watch.Key, null).ToString());
                }

                if (GUILayout.Button("X"))
                {
                    RemoveWatch(obj, field);
                    return;
                }

                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }

        public void OnGUI()
        {
            m_WindowRect = GUI.Window(512521, m_WindowRect, OnWatchWindow, "Watches (Scene Debugger)");
        }

    }

}
