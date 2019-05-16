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
            
            PageLabel = CreateTextPanel(_canvas, "Page 1/?", 29, TextAnchor.MiddleCenter,
                new RectData(
                    new Vector2(200f, 200f),
                    new Vector2(1240f, 870f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f)))
                .GetComponent<Text>();
            
            PageLabel.enabled = true;
            
            _imageLeft = CreateImagePanel(_canvas, NullSprite(),
                    new RectData(
                        new Vector2(60f, 60f),
                        new Vector2(440f, 873f),
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f)))
                .GetComponent<Image>();
            _imageRight = CreateImagePanel(_canvas, NullSprite(),
                    new RectData(
                        new Vector2(60f, 60f),
                        new Vector2(1480f, 873f),
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f)))
                .GetComponent<Image>();
            
            _imageLeft.gameObject.GetComponent<RectTransform>();
            _imageRight.gameObject.GetComponent<RectTransform>();
            
            _textLeft = CreateTextPanel(_canvas, "", 24, TextAnchor.MiddleCenter,
                    new RectData(
                        new Vector2(60f, 60f), new Vector2(440f, 873f),
                        new Vector2(0f, 0f), new Vector2(0f, 0f)))
                .GetComponent<Text>();

            _textRight = CreateTextPanel(_canvas, "", 24, TextAnchor.MiddleCenter,
                    new RectData(
                        new Vector2(60f, 60f),
                        new Vector2(1480f, 873f),
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f)))
                .GetComponent<Text>();

            FadeOut(0f);
            _canvas.AddComponent<MoreSavesComponent>();
            Object.DontDestroyOnLoad(_canvas);
            Log("Initialized MoreSaves");
            
            ModHooks.Instance.GetSaveFileNameHook -= GetFilename;
            ModHooks.Instance.SavegameSaveHook -= CheckAddMaxPages;
            ModHooks.Instance.SavegameClearHook -= CheckRemoveMaxPages;
            
            ModHooks.Instance.GetSaveFileNameHook += GetFilename;
            ModHooks.Instance.SavegameSaveHook += CheckAddMaxPages;
            ModHooks.Instance.SavegameClearHook += CheckRemoveMaxPages;
        }

        private static void FadeOut(float t)
        {
            PageLabel.CrossFadeAlpha(0f, t, true);
            _imageLeft.CrossFadeAlpha(0f, t, true);
            _imageRight.CrossFadeAlpha(0f, t, true);
            _textLeft.CrossFadeAlpha(0f, t, true);
            _textRight.CrossFadeAlpha(0f, t, true);
        }

        private const string VERSION = "0.4.2";

        private static GameObject _canvas;

        public static Text PageLabel;

        private static Image _imageLeft;

        private static Image _imageRight;

        private static Text _textLeft;

        private static Text _textRight;
    }
}