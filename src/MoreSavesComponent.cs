using UnityEngine;
using Modding;
using System.IO;
using System;
namespace MoreSaves
{
    class MoreSavesComponent : MonoBehaviour
    {
        private static GameManager gm;
        private static UIManager uim;

        private static int currentPage = 0;
        private static int minPages = 2;
        private static int maxPages;

        private static bool pagesHidden;

        private static float lastPageTransition;
        private static float transistionTime = 0.5f;
        private static float lastInput;
        private static float firstInput;
        private static float inputWindow = 0.4f;

        private static float opacity = 0f;

        private static int queueRight = 0;
        private static int queueLeft = 0;

        private static bool holdingRight;
        private static bool holdingLeft;

        private static bool setupHooks = false;

        public void Start()
        {
            gm = GameManager.instance;
            uim = UIManager.instance;

            pagesHidden = false;
            holdingLeft = false;
            holdingRight = false;

            maxPages = PlayerPrefs.GetInt("MaxPages", minPages);
            if (maxPages < minPages)
                maxPages = minPages;

            MoreSaves.pageLabel.text = String.Format("Page {0}/{1}", currentPage+1, maxPages);

            if (!setupHooks)
            {
                ModHooks.Instance.GetSaveFileNameHook += getFilename;
                ModHooks.Instance.SavegameSaveHook += checkAddMaxPages;
                ModHooks.Instance.SavegameClearHook += checkRemoveMaxPages;
                setupHooks = true;
            }

        }

        public void Update()
        {
            float t = Time.realtimeSinceStartup;
            bool updateSaves = false;
            if (uim.menuState == GlobalEnums.MainMenuState.SAVE_PROFILES)
            {
                holdingLeft = gm.inputHandler.inputActions.paneLeft.IsPressed;
                holdingRight = gm.inputHandler.inputActions.paneRight.IsPressed;
                if (gm.inputHandler.inputActions.paneRight.WasPressed && t - lastInput > 0.05f)
                {
                    firstInput = t;
                    queueRight++;
                }
                if (gm.inputHandler.inputActions.paneLeft.WasPressed && t - lastInput > 0.05f)
                {
                    firstInput = t;
                    queueLeft++;
                }

                if (queueRight == 0 && holdingRight && t - firstInput > inputWindow)
                    queueRight = 1;
                if (queueLeft == 0 && holdingLeft && t - firstInput > inputWindow)
                    queueLeft = 1;

                if(pagesHidden || !pagesHidden && t - lastPageTransition > transistionTime){
                    if (( queueRight > 0 && t - lastInput > inputWindow/2) )
                    {
                        lastInput = t;
                        currentPage += queueRight;
                        queueRight = 0;
                        updateSaves = true;
                    }
                    if ((queueLeft > 0 && t - lastInput > inputWindow / 2))
                    {
                        lastInput = t;
                        currentPage-= queueLeft;
                        queueLeft = 0;
                        updateSaves = true;               
                    }
                    currentPage = currentPage % maxPages;
                    if (currentPage < 0)
                        currentPage = maxPages - 1;

                    MoreSaves.pageLabel.text = String.Format("Page {0}/{1}", currentPage + 1, maxPages);
                }
            }
            else
            {
                MoreSaves.pageLabel.CrossFadeAlpha(0, 0.25f, false);
            }

            if (!pagesHidden && updateSaves && t - lastPageTransition > transistionTime)
            {
                lastPageTransition = t;
                pagesHidden = true;
                //Instantly
                hideAllSaves();
            }
            if (pagesHidden && t - lastInput > inputWindow && t - lastPageTransition > transistionTime)
            {
                lastPageTransition = t;
                pagesHidden = false;
                showAllSaves();
            }
            if (t - lastPageTransition > transistionTime * 2)
            {
                if (pagesHidden || !(uim.slotFour.state == UnityEngine.UI.SaveSlotButton.SlotState.HIDDEN & uim.slotThree.state == UnityEngine.UI.SaveSlotButton.SlotState.HIDDEN & uim.slotTwo.state == UnityEngine.UI.SaveSlotButton.SlotState.HIDDEN & uim.slotOne.state == UnityEngine.UI.SaveSlotButton.SlotState.HIDDEN))
                {
                    MoreSaves.pageLabel.CrossFadeAlpha(1, 0.25f, false);
                }
                else
                {
                    MoreSaves.pageLabel.CrossFadeAlpha(0, 0.25f, false);
                }
            }
        }

        public void hideOne()
        {
            uim.slotOne.HideSaveSlot();
        }
        public void hideTwo()
        {
            uim.slotTwo.HideSaveSlot();
        }
        public void hideThree()
        {
            uim.slotThree.HideSaveSlot();
        }
        public void hideFour()
        {
            uim.slotFour.HideSaveSlot();
        }
        public void showOne()
        {
            uim.slotOne.ShowSaveSlot();
        }
        public void showTwo()
        {
            uim.slotTwo.ShowSaveSlot();
        }
        public void showThree()
        {
            uim.slotThree.ShowSaveSlot();
        }
        public void showFour()
        {
            uim.slotFour.ShowSaveSlot();
        }

        public void hideAllSaves()
        {
            Invoke("hideOne", 0 *(0.5f / 8));
            Invoke("hideTwo", 1 * (0.5f / 8));
            Invoke("hideThree", 2 * (0.5f / 8));
            Invoke("hideFour", 3 * (0.5f / 8));
        }

        public void showAllSaves()
        {
            uim.slotOne.enabled = false;
            uim.slotTwo.enabled = false;
            uim.slotThree.enabled = false;
            uim.slotFour.enabled = false;

            uim.slotOne.enabled = true;
            uim.slotTwo.enabled = true;
            uim.slotThree.enabled = true;
            uim.slotFour.enabled = true;

            Invoke("showOne", 0 * (0.5f / 8));
            Invoke("showTwo", 1 * (0.5f / 8));
            Invoke("showThree", 2 * (0.5f / 8));
            Invoke("showFour", 3 * (0.5f / 8));
        }

        public static void checkAddMaxPages(int x)
        {
            if (currentPage == (maxPages-1))
                maxPages++;
            PlayerPrefs.SetInt("MaxPages", maxPages);
        }

        public static void checkRemoveMaxPages(int x)
        {
            bool file = false;
            if (currentPage == maxPages || currentPage == maxPages - 1)
            {
                for (int i = 1; i <= 8; i++)
                {
                    if (File.Exists(Application.persistentDataPath + "/user" + (((maxPages - 1) * 4) + i) + ".dat"))
                        file = true;
                }
            }
            if (!file)
            {
                maxPages--;
                PlayerPrefs.SetInt("MaxPages", maxPages);
                MoreSaves.pageLabel.text = String.Format("Page {0}/{1}", currentPage + 1, maxPages);
            }

        }

        public static string getFilename(int x)
        {
            return "/user" + ((currentPage * 4) + x) + ".dat";
        }
    }
}
