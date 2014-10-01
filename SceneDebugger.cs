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
    public class SceneDebugger : MonoBehaviour
    {

        private Rect m_WindowRect = new Rect(32, 32, 512, 512);

        private float m_TreeIdentSpacing = 4.0f;

        private HashSet<GameObject> m_Expanded = new HashSet<GameObject>();

        private WatchWindow m_WatchWindow = new WatchWindow();
        private TimeScaleWindow m_TimeScaleWindow = new TimeScaleWindow();

        private List<GameObject> m_SceneRoots = new List<GameObject>();

        private bool m_UIHidden = false;
        private bool m_UIActive = false;

        private Toolbar.IButton m_ToolbarButton = null;

        public void Awake()
        {
            InitializeToolbarButton();
            RegisterCallbacks();
            m_SceneRoots = FindSceneRoots();
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

                if (!m_WatchWindow.IsWatchable(field))
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Watch"))
                {
                    m_WatchWindow.AddWatch(obj, field);
                }

                GUI.enabled = true;

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

            if (GUILayout.Button("Refresh"))
            {
                m_SceneRoots = FindSceneRoots();
            }

            foreach (GameObject obj in m_SceneRoots)
            {
                OnSceneTreeRecursive(obj);
            }   
        }

        private void OnGUI()
        {
            if (m_UIHidden || !m_UIActive)
            {
                return;
            }

            m_WindowRect = GUI.Window(1423, m_WindowRect, OnSceneDebuggerWindow, "Scene Debugger");
            m_WatchWindow.OnGUI();
            m_TimeScaleWindow.OnGUI();
        }

    }

}
