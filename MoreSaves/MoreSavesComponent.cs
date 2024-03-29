﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlobalEnums;
using ModCommon.Util;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.SaveSlotButton;
using static UnityEngine.UI.SaveSlotButton.SlotState;
using Logger = Modding.Logger;

namespace MoreSaves
{
    internal class MoreSavesComponent : MonoBehaviour
    {
        private const int MIN_PAGES = 2;

        private const float TRANSISTION_TIME = 0.5f;

        private const float INPUT_WINDOW = 0.4f;

        private int _currentPage;

        private int _maxPages;

        private bool _pagesHidden;

        private float _lastPageTransition;

        private float _lastInput;

        private float _firstInput;

        private int _queueRight;

        private int _queueLeft;

        private InputHandler _ih;

        private SaveSlotButton _selectedSaveSlot;

        private string scene = Constants.MENU_SCENE;

        private static IEnumerable<SaveSlotButton> Slots => new[]
        {
            _uim.slotOne, _uim.slotTwo, _uim.slotThree, _uim.slotFour
        };

        private static GameManager _gm => GameManager.instance;

        private static UIManager _uim => UIManager.instance;

        private void Start()
        {
            _pagesHidden = false;

            _maxPages = PlayerPrefs.GetInt("MaxPages", MIN_PAGES);

            _maxPages = Math.Max(_maxPages, MIN_PAGES);

            MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";

            DontDestroyOnLoad(this);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
            ModHooks.Instance.GetSaveFileNameHook -= GetFilename;
            ModHooks.Instance.SavegameSaveHook -= CheckAddMaxPages;
            ModHooks.Instance.SavegameClearHook -= CheckRemoveMaxPages;
            On.UnityEngine.UI.SaveSlotButton.OnSelect -= this.SaveSlotButton_OnSelect;
            On.UnityEngine.UI.SaveSlotButton.OnDeselect -= this.SaveSlotButton_OnDeselect;
            On.UnityEngine.UI.SaveSlotButton.AnimateToSlotState -= SaveSlotButton_AnimateToSlotState;


            On.UnityEngine.UI.SaveSlotButton.AnimateToSlotState += SaveSlotButton_AnimateToSlotState;
            On.UnityEngine.UI.SaveSlotButton.OnDeselect += this.SaveSlotButton_OnDeselect;
            On.UnityEngine.UI.SaveSlotButton.OnSelect += this.SaveSlotButton_OnSelect;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
            ModHooks.Instance.GetSaveFileNameHook += GetFilename;
            ModHooks.Instance.SavegameSaveHook += CheckAddMaxPages;
            ModHooks.Instance.SavegameClearHook += CheckRemoveMaxPages;
        }

        private IEnumerator SaveSlotButton_AnimateToSlotState(On.UnityEngine.UI.SaveSlotButton.orig_AnimateToSlotState orig, SaveSlotButton self, SlotState nextState)
        {
            yield return orig(self, nextState);

            //probably a better way to wait for the transition to end but good enough TM
            yield return new WaitForSeconds(0.8f);

            string filepath = Application.persistentDataPath + GetLockFilename((int)self.saveSlot);
            if (File.Exists(filepath))
            {
                self.clearSaveButton.alpha = 0;
                self.clearSaveButton.interactable = false;
                self.clearSaveButton.gameObject.SetActive(false);
                self.StartCoroutine(_uim.FadeOutCanvasGroup(self.clearSaveButton));
            }
            
            yield break;
        }

        private void SaveSlotButton_OnDeselect(On.UnityEngine.UI.SaveSlotButton.orig_OnDeselect orig, SaveSlotButton self, UnityEngine.EventSystems.BaseEventData eventData)
        {
            _selectedSaveSlot = null;
            orig(self, eventData);
        }

        private void SaveSlotButton_OnSelect(On.UnityEngine.UI.SaveSlotButton.orig_OnSelect orig, SaveSlotButton self, UnityEngine.EventSystems.BaseEventData eventData)
        {
            _selectedSaveSlot = self;
            orig(self, eventData);
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            scene = arg1.name;
        }

        public void Update()
        {
            if (scene != Constants.MENU_SCENE) return;

            float t = Time.realtimeSinceStartup;

            if (_uim.menuState != MainMenuState.SAVE_PROFILES)
            {
                MoreSaves.PageLabel.CrossFadeAlpha(0, 0.25f, false);

                return;
            }

            _ih = _ih ? _ih : _uim.GetAttr<UIManager, InputHandler>("ih");

            HeroActions heroActions = _ih.inputActions;

            bool updateSaves = false;

            bool holdingLeft = heroActions.paneLeft.IsPressed;
            bool holdingRight = heroActions.paneRight.IsPressed;
            bool pressedDN = heroActions.dreamNail.WasPressed;

            if (pressedDN)
            {
                if(_selectedSaveSlot != null)
                {
                    string filepath = Application.persistentDataPath + GetLockFilename((int)_selectedSaveSlot.saveSlot);
                    if (File.Exists(filepath))
                    {
                        _selectedSaveSlot.StartCoroutine(_uim.FadeInCanvasGroup(_selectedSaveSlot.clearSaveButton));
                        File.Delete(filepath);
                    } else
                    {
                        _selectedSaveSlot.StartCoroutine(_uim.FadeOutCanvasGroup(_selectedSaveSlot.clearSaveButton));
                        FileStream fs = File.Create(filepath);
                        fs.Close();
                    }
                }
            }

            if (heroActions.paneRight.WasPressed && t - _lastInput > 0.05f)
            {
                _firstInput = t;
                _queueRight++;
            }

            if (heroActions.paneLeft.WasPressed && t - _lastInput > 0.05f)
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

                _currentPage %= _maxPages;

                if (_currentPage < 0) _currentPage = _maxPages - 1;

                MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";
            }

            if (!_pagesHidden && updateSaves && t - _lastPageTransition > TRANSISTION_TIME)
            {
                _lastPageTransition = t;
                _pagesHidden = true;
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
                MoreSaves.PageLabel.CrossFadeAlpha(1, 0.25f, false);
            else
                MoreSaves.PageLabel.CrossFadeAlpha(0, 0.25f, false);
        }

        public void HideOne()
        {
            _uim.slotOne.HideSaveSlot();
        }

        public void HideTwo()
        {
            _uim.slotTwo.HideSaveSlot();
        }

        public void HideThree()
        {
            _uim.slotThree.HideSaveSlot();
        }

        public void HideFour()
        {
            _uim.slotFour.HideSaveSlot();
        }

        public void HideAllSaves()
        {
            Invoke(nameof(HideOne), 0);
            Invoke(nameof(HideTwo), 0);
            Invoke(nameof(HideThree), 0);
            Invoke(nameof(HideFour), 0);
        }

        public void ShowAllSaves()
        {
            Logger.Log("[MoreSaves] Showing All Saves");

            foreach (SaveSlotButton s in Slots)
            {
                s._prepare(_gm);
                s.ShowRelevantModeForSaveFileState();
            }

            _uim.StartCoroutine(_uim.GoToProfileMenu());
        }

        private void CheckAddMaxPages(int x)
        {
            if (_currentPage == _maxPages - 1) _maxPages++;

            PlayerPrefs.SetInt("MaxPages", _maxPages);
        }

        private void CheckRemoveMaxPages(int x)
        {
            if
            (
                (_currentPage == _maxPages || _currentPage == _maxPages - 1) &&
                Enumerable.Range(1, 8).Any(i => File.Exists($"{Application.persistentDataPath}/user{(_maxPages - 1) * 4 + i}.dat"))
            )
                return;

            PlayerPrefs.SetInt("MaxPages", --_maxPages);
            MoreSaves.PageLabel.text = $"Page {_currentPage + 1}/{_maxPages}";
        }

        private string GetLockFilename(int x)
        {
            x = x % 4 == 0 ? 4 : x % 4;

            return "/user" + (_currentPage * 4 + x) + ".protecc";
        }

        private string GetFilename(int x)
        {
            x = x % 4 == 0 ? 4 : x % 4;

            return "user" + (_currentPage * 4 + x) + ".dat";
        }
    }

    internal static class SaveExtensions
    {
        private static void ChangeSaveFileState(this SaveSlotButton self, SaveFileStates nextSaveFileState)
        {
            self.saveFileState = nextSaveFileState;

            if (self.isActiveAndEnabled) self.ShowRelevantModeForSaveFileState();
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