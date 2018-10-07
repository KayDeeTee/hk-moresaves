using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlobalEnums;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.SaveSlotButton;
using static UnityEngine.UI.SaveSlotButton.SlotState;
using Logger = Modding.Logger;

namespace MoreSaves
{
    internal class MoreSavesComponent : MonoBehaviour
    {
        private void Start()
        {
            _gm = GameManager.instance;
            _uim = UIManager.instance;
            _pagesHidden = false;

            _maxPages = PlayerPrefs.GetInt("MaxPages", MIN_PAGES);

            _maxPages = Math.Max(_maxPages, MIN_PAGES);

            MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";
            DontDestroyOnLoad(this);
        }

        public void Update()
        {
            float t = Time.realtimeSinceStartup;
            bool updateSaves = false;

            if (_uim.menuState != MainMenuState.SAVE_PROFILES)
            {
                MoreSaves.PageLabel.CrossFadeAlpha(0, 0.25f, false);
                return;
            }

            bool holdingLeft = _gm.inputHandler.inputActions.paneLeft.IsPressed;
            bool holdingRight = _gm.inputHandler.inputActions.paneRight.IsPressed;

            if (_gm.inputHandler.inputActions.paneRight.WasPressed && t - _lastInput > 0.05f)
            {
                _firstInput = t;
                _queueRight++;
            }

            if (_gm.inputHandler.inputActions.paneLeft.WasPressed && t - _lastInput > 0.05f)
            {
                _firstInput = t;
                _queueLeft++;
            }

            if (_queueRight == 0 && holdingRight && t - _firstInput > INPUT_WINDOW)
                _queueRight = 1;
            if (_queueLeft == 0 && holdingLeft && t - _firstInput > INPUT_WINDOW)
                _queueLeft = 1;

            if (_pagesHidden || !_pagesHidden && t - _lastPageTransition > TRANSISTION_TIME)
            {
                if (_queueRight > 0 && t - _lastInput > INPUT_WINDOW / 2)
                {
                    _lastInput = t;
                    _currentPage += _queueRight;
                    _queueRight = 0;
                    updateSaves = true;
                }

                if (_queueLeft > 0 && t - _lastInput > INPUT_WINDOW / 2)
                {
                    _lastInput = t;
                    _currentPage -= _queueLeft;
                    _queueLeft = 0;
                    updateSaves = true;
                }

                // Logger.Log("(% _maxPages) setting _currentPage to " + _currentPage % _maxPages);
                _currentPage = _currentPage % _maxPages;
                if (_currentPage < 0)
                {
                    _currentPage = _maxPages - 1;
                }

                MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";
            }

            if (!_pagesHidden && updateSaves && t - _lastPageTransition > TRANSISTION_TIME)
            {
                _lastPageTransition = t;
                _pagesHidden = true;
                //Instantly
                HideAllSaves();
            }

            if (_pagesHidden && t - _lastInput > INPUT_WINDOW && t - _lastPageTransition > TRANSISTION_TIME)
            {
                _lastPageTransition = t;
                _pagesHidden = false;
                ShowAllSaves();
            }

            if (t - _lastPageTransition < TRANSISTION_TIME * 2) return;

            if (_pagesHidden || Slots.All(x => x.state != HIDDEN))
            {
                MoreSaves.PageLabel.CrossFadeAlpha(1, 0.25f, false);
            }
            else
            {
                MoreSaves.PageLabel.CrossFadeAlpha(0, 0.25f, false);
            }
        }

        public void HideOne() => _uim.slotOne.HideSaveSlot();
        public void HideTwo() => _uim.slotTwo.HideSaveSlot();
        public void HideThree() => _uim.slotThree.HideSaveSlot();
        public void HideFour() => _uim.slotFour.HideSaveSlot();

        public void HideAllSaves()
        {
            Invoke(nameof(HideOne), 0f);
            Invoke(nameof(HideTwo), 0.0625f);
            Invoke(nameof(HideThree), 0.125f);
            Invoke(nameof(HideFour), 0.1875f);
        }

        public void ShowAllSaves()
        {
            Logger.Log("Showing All Saves");
            foreach (SaveSlotButton s in Slots)
            {
                s._prepare(_gm);
                s.ShowRelevantModeForSaveFileState();
            }

            _uim.StartCoroutine(_uim.GoToProfileMenu());
        }

        private static IEnumerable<SaveSlotButton> Slots => new[] {_uim.slotOne, _uim.slotTwo, _uim.slotThree, _uim.slotFour};

        public static void CheckAddMaxPages(int x)
        {
            if (_currentPage == _maxPages - 1)
            {
                _maxPages++;
            }

            PlayerPrefs.SetInt("MaxPages", _maxPages);
        }

        public static void CheckRemoveMaxPages(int x)
        {
            bool flag = false;

            if (_currentPage == _maxPages || _currentPage == _maxPages - 1)
            {
                flag = Enumerable.Range(1, 8).Any(i => File.Exists($"{Application.persistentDataPath}/user{(_maxPages - 1) * 4 + i}.dat"));
            }

            if (flag) return;

            PlayerPrefs.SetInt("MaxPages", --_maxPages);
            MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";
        }

        public static string GetFilename(int x)
        {
            x = x % 4 == 0 ? 4 : x % 4;
            return "user" + (_currentPage * 4 + x) + ".dat";
        }

        private static GameManager _gm;

        private static UIManager _uim;

        private static int _currentPage;

        private const int MIN_PAGES = 2;

        private static int _maxPages;

        private static bool _pagesHidden;

        private static float _lastPageTransition;

        private const float TRANSISTION_TIME = 0.5f;

        private static float _lastInput;

        private static float _firstInput;

        private const float INPUT_WINDOW = 0.4f;

        private static int _queueRight;

        private static int _queueLeft;
    }

    internal static class SaveExtensions
    {
        private static void ChangeSaveFileState(this SaveSlotButton self, SaveFileStates nextSaveFileState)
        {
            self.saveFileState = nextSaveFileState;
            if (self.isActiveAndEnabled)
            {
                self.ShowRelevantModeForSaveFileState();
            }
        }

        public static void _prepare(this SaveSlotButton self, GameManager gameManager)
        {
            self.ChangeSaveFileState(SaveFileStates.OperationInProgress);

            Platform.Current.IsSaveSlotInUse((int) self.saveSlot + 1, delegate(bool fileExists)
            {
                if (!fileExists)
                {
                    self.ChangeSaveFileState(SaveFileStates.Empty);
                    return;
                }

                gameManager.GetSaveStatsForSlot((int) self.saveSlot + 1, delegate(SaveStats saveStats)
                {
                    if (saveStats == null)
                    {
                        self.ChangeSaveFileState(SaveFileStates.Corrupted);
                    }
                    else
                    {
                        self.SetAttr("saveStats", saveStats);
                        self.ChangeSaveFileState(SaveFileStates.LoadedStats);
                    }
                });
            });
        }
    }
}