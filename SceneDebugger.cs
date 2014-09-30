using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using UnityEngine;

namespace KSPSceneDebugger
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SceneDebugger
    {

        private bool m_AutoRefresh = false;

        private Rect m_WindowRect = new Rect(32, 32, 512, 512);

        private float m_TreeIdentSpacing = 4.0f;

        private HashSet<GameObject> m_Expanded = new HashSet<GameObject>();

        public void Awake()
        {

        }

        private void Update()
        {

        }

        private List<GameObject> FindSceneRoots()
        {
            List<GameObject> roots = new List<GameObject>();

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                if (obj.transform.parent == null)
                {
                    roots.Add(obj);
                }
            }

            return roots;
        }

        private void OnSceneTreeReflect(System.Object obj, int ident)
        {
            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields();

            foreach (var field in fields)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(m_TreeIdentSpacing * ident);

                if (field.IsPublic)
                {
                    GUILayout.Label("public ");
                }
                else if (field.IsPrivate)
                {
                    GUILayout.Label("private ");
                }

                if (field.MemberType == MemberTypes.Method)
                {
                    GUILayout.Label("method ");
                }
                else if (field.MemberType == MemberTypes.Field)
                {
                    GUILayout.Label("field ");
                    GUILayout.Label(field.FieldType.ToString() + " ");
                }
                else if (field.MemberType == MemberTypes.Property)
                {
                    GUILayout.Label("property ");
                    GUILayout.Label(field.FieldType.ToString() + " ");
                }

                GUILayout.Label(field.Name);
                GUILayout.EndHorizontal();
            }
        }

        private void OnSceneTreeComponents(GameObject obj, int ident)
        {
            var components = obj.GetComponents(typeof(Component));
            foreach(var component in components)
            {
                OnSceneTreeReflect(component, ident);
            }
        }

        private void OnSceneTreeRecursive(GameObject obj, int ident = 0)
        {
            if (m_Expanded.Contains(obj))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(m_TreeIdentSpacing * ident);

                if (GUILayout.Button("-"))
                {
                    m_Expanded.Remove(obj);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();

                OnSceneTreeComponents(obj, ident + 1);

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    OnSceneTreeRecursive(obj.transform.GetChild(i).gameObject, ident + 1);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("+"))
                {
                    m_Expanded.Add(obj);
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();
            }
        }

        private void OnSceneDebuggerWindow(int index)
        {
            GUI.DragWindow();

            var rootGameObjects = FindSceneRoots();

            foreach (GameObject obj in rootGameObjects)
            {
                OnSceneTreeRecursive(obj);
            }   
        }

        private void OnGUI()
        {
            m_WindowRect = GUI.Window(1423, m_WindowRect, OnSceneDebuggerWindow, "Scene Debugger");
        }

    }

}
