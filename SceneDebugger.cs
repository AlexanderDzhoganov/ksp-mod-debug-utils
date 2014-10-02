using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

namespace KSPModDebugUtils
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SceneDebugger : MonoBehaviour
    {

        private Rect m_WindowRect = new Rect(32, 32, 768, 512);

        private float m_TreeIdentSpacing = 16.0f;

        private HashSet<int> m_Expanded = new HashSet<int>();
        private HashSet<int> m_ExpandedComponents = new HashSet<int>();

        private WatchWindow m_WatchWindow = new WatchWindow();
        private TimeScaleWindow m_TimeScaleWindow = new TimeScaleWindow();

        private HashSet<GameObject> m_SceneRoots = new HashSet<GameObject>();

        private bool m_UIHidden = false;
        private bool m_UIActive = true;

        private Toolbar.IButton m_ToolbarButton = null;

        private Vector2 m_ScrollPosition = new Vector2(0.0f, 0.0f);

        public void Awake()
        {
            InitializeToolbarButton();
            RegisterCallbacks();
            m_SceneRoots = FindSceneRoots();
            print("Scene Debugger: initialized");
        }

        private void InitializeToolbarButton()
        {
            if (Toolbar.ToolbarManager.Instance == null)
            {
                print("Scene Debugger: toolbar instance not available");
                return;
            }

            m_ToolbarButton = Toolbar.ToolbarManager.Instance.add("scenedebugger", "mainButton");
            m_ToolbarButton.TexturePath = "000_Toolbar/img_buttonSceneDebugger";
            m_ToolbarButton.ToolTip = "Scene Debugger";
            m_ToolbarButton.Visibility = new Toolbar.GameScenesVisibility(GameScenes.FLIGHT);
            m_ToolbarButton.OnClick += new Toolbar.ClickHandler(OnToolbarButtonClick);
        }

        void OnToolbarButtonClick(Toolbar.ClickEvent ev)
        {
            this.gameObject.SetActive(true);
            m_UIActive = true;
            m_UIHidden = false;
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }

        void OnGUIRecoveryDialogSpawn(MissionRecoveryDialog dialog)
        {
            m_UIHidden = true;
        }

        void OnGamePause()
        {
            m_UIHidden = true;
        }

        void OnGameUnpause()
        {
            m_UIHidden = false;
        }

        private void OnShowUI()
        {
            m_UIHidden = false;
        }

        private void OnHideUI()
        {
            m_UIHidden = true;
        }

        private void RegisterCallbacks()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onGUIRecoveryDialogSpawn.Add(new EventData<MissionRecoveryDialog>.OnEvent(OnGUIRecoveryDialogSpawn));
            GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnGamePause));
            GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnGameUnpause));
        }

        private void UnregisterCallbacks()
        {
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onGUIRecoveryDialogSpawn.Remove(OnGUIRecoveryDialogSpawn);
            GameEvents.onGamePause.Remove(OnGamePause);
            GameEvents.onGameUnpause.Remove(OnGameUnpause);
        }

        private HashSet<GameObject> FindSceneRoots()
        {
            HashSet<GameObject> roots = new HashSet<GameObject>();

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                roots.Add(obj.transform.root.gameObject);
            }

            return roots;
        }

        private void OnSceneTreeReflectField(System.Object obj, FieldInfo field, int ident)
        {
            if (obj == null || field == null)
            {
                GUILayout.Label("null");
                return;
            }

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

            GUILayout.Label("field ");
            GUILayout.Label(field.FieldType.ToString() + " ");
            GUILayout.Label(field.Name);
            var value = field.GetValue(obj);
            GUILayout.Label(value == null ? "null" : value.ToString());

            if (GUILayout.Button("Watch"))
            {
                m_WatchWindow.AddWatch(obj, field);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectProperty(System.Object obj, PropertyInfo property, int ident)
        {
            if (obj == null || property == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_TreeIdentSpacing * ident);

            GUILayout.Label("property ");
            GUILayout.Label(property.PropertyType.ToString() + " ");

            GUILayout.Label(property.Name);
            var value = property.GetValue(obj, null);
            GUILayout.Label(value == null ? "null" : value.ToString());

            if (GUILayout.Button("Watch"))
            {
                m_WatchWindow.AddWatch(obj, property);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectMethod(System.Object obj, MethodInfo method, int ident)
        {
            if (obj == null || method == null)
            {
                GUILayout.Label("null");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_TreeIdentSpacing * ident);

            GUILayout.Label("method ");
            string signature = method.ReturnType.ToString() + " " + method.Name + "(";

            bool first = true;
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                if (!first)
                {
                    signature += ", ";
                }
                else
                {
                    first = false;
                }

                signature += param.ParameterType.ToString() + " " + param.Name;
            }

            signature += ")";
            
            GUILayout.Label(signature);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectUnityEngineVector2(string name, ref UnityEngine.Vector2 vec, int ident)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(m_TreeIdentSpacing * ident);

            GUILayout.Label("Vector2");
            GUILayout.Label(name);

            GUILayout.BeginVertical();
            GUIControls.FloatField("x", ref vec.x);
            GUIControls.FloatField("y", ref vec.y);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectUnityEngineVector3(string name, ref UnityEngine.Vector3 vec, int ident)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(m_TreeIdentSpacing * ident);

            GUILayout.Label("Vector3");
            GUILayout.Label(name);

            GUILayout.BeginVertical();
            GUIControls.FloatField("x", ref vec.x);
            GUIControls.FloatField("y", ref vec.y);
            GUIControls.FloatField("z", ref vec.z);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectUnityEngineVector4(string name, ref UnityEngine.Vector4 vec, int ident)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(m_TreeIdentSpacing * ident);

            GUILayout.Label("Vector4");
            GUILayout.Label(name);

            GUILayout.BeginVertical();
            GUIControls.FloatField("x", ref vec.x);
            GUIControls.FloatField("y", ref vec.y);
            GUIControls.FloatField("z", ref vec.z);
            GUIControls.FloatField("w", ref vec.w);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectFloat(string name, ref float value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(m_TreeIdentSpacing * ident);
            GUILayout.Label("float");
            GUIControls.FloatField(name, ref value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectInt(string name, ref int value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(m_TreeIdentSpacing * ident);
            GUILayout.Label("int");
            GUIControls.IntField(name, ref value);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectBool(string name, ref bool value, int ident)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(m_TreeIdentSpacing * ident);
            GUILayout.Label("bool");
            GUILayout.FlexibleSpace();

            GUIControls.BoolField(name, ref value);
            GUILayout.EndHorizontal();
        }

        private void OnSceneTreeReflectUnityEngineTransform(UnityEngine.Transform transform, int ident)
        {
            if (transform == null)
            {
                GUILayout.Label("null");
                return;
            }

            var childCount = transform.childCount;
            OnSceneTreeReflectInt("childCount", ref childCount, ident + 1);

            var eulerAngles = transform.eulerAngles;
            OnSceneTreeReflectUnityEngineVector3("eulerAngles", ref eulerAngles, ident + 1);
            transform.eulerAngles = eulerAngles;

            var forward = transform.forward;
            OnSceneTreeReflectUnityEngineVector3("forward", ref forward, ident + 1);
            transform.forward = forward;

            var hasChanged = transform.hasChanged;
            OnSceneTreeReflectBool("hasChanged", ref hasChanged, ident + 1);
            transform.hasChanged = hasChanged;

            var localEulerAngles = transform.localEulerAngles;
            OnSceneTreeReflectUnityEngineVector3("localEulerAngles", ref eulerAngles, ident + 1);
            transform.localEulerAngles = localEulerAngles;

            var localPosition = transform.localPosition;
            OnSceneTreeReflectUnityEngineVector3("localPosition", ref localPosition, ident + 1);
            transform.localPosition = localPosition;

            var localRotation = transform.localRotation;

            var localScale = transform.localScale;
            OnSceneTreeReflectUnityEngineVector3("localScale", ref localScale, ident + 1);
            transform.localScale = localScale;

            var localToWorldMatrix = transform.localToWorldMatrix;

            var lossyScale = transform.lossyScale;
            OnSceneTreeReflectUnityEngineVector3("lossyScale", ref lossyScale, ident + 1);

            var right = transform.right;
            OnSceneTreeReflectUnityEngineVector3("right", ref lossyScale, ident + 1);

            var rotation = transform.rotation;

            var up = transform.up;
            OnSceneTreeReflectUnityEngineVector3("up", ref up, ident + 1);

            var worldToLocalMatrix = transform.worldToLocalMatrix;
        }

        private static bool IsEnumerable(object myProperty)
        {
            if (typeof(IEnumerable).IsAssignableFrom(myProperty.GetType())
                || typeof(IEnumerable<>).IsAssignableFrom(myProperty.GetType()))
                return true;

            return false;
        }

        private void OnSceneTreeReflectIEnumerable(System.Object myProperty, int ident)
        {
            var ie = myProperty as IEnumerable;
            string s = string.Empty;
            if (null != ie)
            {
                bool first = true;
                foreach (var p in ie)
                {
                    if (!first)
                        s += ", ";

                    if (myProperty is GameObject)
                    {
                        OnSceneTreeRecursive((GameObject)myProperty, ident + 1);
                    }
                    else
                    {
                        OnSceneTreeReflect(myProperty, ident + 1);
                    }

                    first = false;
                }
            }
        }

        private void OnSceneTreeReflect(System.Object obj, int ident)
        {
            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            Type type = obj.GetType();

            if (type == typeof(UnityEngine.Transform))
            {
                OnSceneTreeReflectUnityEngineTransform((UnityEngine.Transform)obj, ident);
                return;
            }
            else if (IsEnumerable(obj))
            {
                OnSceneTreeReflectIEnumerable(obj, ident);
            }

            MemberInfo[] fields = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (MemberInfo member in fields)
            {
                FieldInfo field = null;
                PropertyInfo property = null;
                MethodInfo method = null;

                if (member.MemberType == MemberTypes.Field)
                {
                    field = (FieldInfo)member;
                    OnSceneTreeReflectField(obj, field, ident);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    property = (PropertyInfo) member;
                    OnSceneTreeReflectProperty(obj, property, ident);
                }
                else if (member.MemberType == MemberTypes.Method)
                {
                    method = (MethodInfo) member;
                    OnSceneTreeReflectMethod(obj, method, ident);
                }

                if (field == null)
                {
                    continue;
                }
            }
        }

        private void OnSceneTreeComponents(GameObject obj, int ident)
        {
            var components = obj.GetComponents(typeof(Component));
            foreach(var component in components)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(m_TreeIdentSpacing * ident);

                if (m_ExpandedComponents.Contains(component.GetHashCode()))
                {
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        m_ExpandedComponents.Remove(component.GetHashCode());
                    }

                    GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");

                    GUILayout.EndHorizontal();
         
                    OnSceneTreeReflect(component, ident + 1);
                }
                else
                {
                    if (GUILayout.Button("+", GUILayout.Width(16)))
                    {
                        m_ExpandedComponents.Add(component.GetHashCode());
                    }

                    GUILayout.Label(component.name + " (" + component.GetType().ToString() + ")");
                    GUILayout.EndHorizontal();
                }      
            }
        }

        private void OnSceneTreeRecursive(GameObject obj, int ident = 0)
        {
            if (obj == null)
            {
                GUILayout.Label("null");
                return;
            }

            if (m_Expanded.Contains(obj.GetHashCode()))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(m_TreeIdentSpacing * ident);

                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    m_Expanded.Remove(obj.GetHashCode());
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
                GUILayout.Space(m_TreeIdentSpacing * ident);

                if (GUILayout.Button("+", GUILayout.Width(16)))
                {
                    m_Expanded.Add(obj.GetHashCode());
                }

                GUILayout.Label(obj.name);
                GUILayout.EndHorizontal();
            }
        }

        private void OnSceneDebuggerWindow(int index)
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);

            if (GUILayout.Button("Refresh"))
            {
                m_SceneRoots = FindSceneRoots();
            }

            foreach (GameObject obj in m_SceneRoots)
            {
                OnSceneTreeRecursive(obj);
            }

            GUI.DragWindow();

            GUILayout.EndScrollView();
        }

        private void OnGUI()
        {
            if (m_UIHidden || !m_UIActive)
            {
                return;
            }

            m_WindowRect = GUI.Window(612621, m_WindowRect, OnSceneDebuggerWindow, "Scene Debugger");
            m_WatchWindow.OnGUI();
            m_TimeScaleWindow.OnGUI();
        }

    }

}
