using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using GlobalEnums;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace MoreSaves
{
    public class MoreSaves : Mod
    {

        private static string version = "0.3.0";

        public override string GetVersion()
        {
            return version;
        }

        public static GameObject canvas;
        public static Text pageLabel;
        public static RectTransform rect;

        public static Font trajanBold;
        public static Font trajanNormal;

        public static GameObject createTextPanel(GameObject parent, string defaultText, int x, int y, int w, int h, int s)
        {
            GameObject panel = new GameObject();
            panel.transform.parent = parent.transform;
            panel.AddComponent<CanvasRenderer>();
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            rt.anchorMax = new Vector2(0, 0);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(x, y);
            UnityEngine.UI.Text text = panel.AddComponent<UnityEngine.UI.Text>();
            text.font = trajanBold;
            text.text = defaultText;
            text.fontSize = s;
            text.alignment = TextAnchor.MiddleCenter;
            return panel;
        }

        public static GameObject createCanvas(int w, int h)
        {
            GameObject c = new GameObject();
            c.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = c.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(w, h);
            c.AddComponent<GraphicRaycaster>();
            return c;
        }


        public override void Initialize()
        {
            ModHooks.ModLog("Initializing MoreSaves");

            UIManager.instance.gameObject.AddComponent<MoreSavesComponent>();

            foreach (UnityEngine.Font f in UnityEngine.Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold")
                {
                    trajanBold = f;
                }

                if (f != null && f.name == "TrajanPro-Regular")
                {
                    trajanNormal = f;
                }
            }

            canvas = createCanvas(1920,1080);
            pageLabel = createTextPanel(canvas, "Page 1/?", 1240, 870, 200, 200, 29).GetComponent<Text>();
            pageLabel.enabled = false;

            MonoBehaviour.DontDestroyOnLoad(canvas);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += checkComponent;

            ModHooks.ModLog("Initialized MoreSaves");
        }

        public void checkComponent(Scene scene, LoadSceneMode lsm){
            if(UIManager.instance.gameObject.GetComponent<MoreSavesComponent>() == null)
                UIManager.instance.gameObject.AddComponent<MoreSavesComponent>();
        }

        

    }
}
