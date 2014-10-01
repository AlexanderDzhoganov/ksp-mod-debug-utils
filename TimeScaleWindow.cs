using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace KSPSceneDebugger
{

    class TimeScaleWindow
    {

        private Rect m_WindowRect = new Rect(640, 32, 128, 64);

        public void OnTimeScaleWindow(int index)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time scale");
            GUILayout.Label(Time.timeScale.ToString("0.00"));
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0.0f, 1.0f);
            GUILayout.EndHorizontal();
        }

        public void OnGUI()
        {
            m_WindowRect = GUI.Window(21612, m_WindowRect, OnTimeScaleWindow, "Time Scale (Scene Debugger)");
        }

    }

}
