using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.UI;
using static Modding.CanvasUtil;
using static MoreSaves.MoreSavesComponent;

namespace MoreSaves
{
    [UsedImplicitly]
    public class MoreSaves : Mod
    {
        private const string VERSION = "0.4.3";

        private static GameObject _canvas;

        public static Text PageLabel;

        public override string GetVersion()
        {
            return VERSION;
        }

        public override bool IsCurrent()
        {
            return true;
        }

        public override void Initialize()
        {
            Log("Initializing MoreSaves");

            CreateFonts();

            _canvas = CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920f, 1080f));

            PageLabel = CreateTextPanel
                (
                    _canvas, "Page 1/?", 29, TextAnchor.MiddleCenter,
                    new RectData
                    (
                        new Vector2(200f, 200f),
                        new Vector2(1240f, 870f),
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f)
                    )
                )
                .GetComponent<Text>();

            PageLabel.enabled = true;

            FadeOut(0f);
            _canvas.AddComponent<MoreSavesComponent>();
            Object.DontDestroyOnLoad(_canvas);
            Log("Initialized MoreSaves");

        }

        private static void FadeOut(float t)
        {
            PageLabel.CrossFadeAlpha(0f, t, true);
        }
    }
}