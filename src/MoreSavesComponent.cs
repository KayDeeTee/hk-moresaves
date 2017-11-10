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

        private static float lastPageTransition;
        private static float transistionTime = 0.5f;

        private static bool setupHooks = false;

        public void Start()
        {
            gm = GameManager.instance;
            uim = UIManager.instance;

            maxPages = PlayerPrefs.GetInt("MaxPages", 1);
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
            MoreSaves.pageLabel.enabled = uim.menuState == GlobalEnums.MainMenuState.SAVE_PROFILES;

            if (uim.menuState == GlobalEnums.MainMenuState.SAVE_PROFILES && Time.time - lastPageTransition > transistionTime * 2)
            {
                bool updateSaves = false;
                if (gm.inputHandler.inputActions.paneRight.WasPressed)
                {
                    currentPage++;
                    updateSaves = true;
                }
                if (gm.inputHandler.inputActions.paneLeft.WasPressed)
                {
                    currentPage--;
                    updateSaves = true;
                }
                currentPage = currentPage % maxPages;
                if (currentPage < 0)
                    currentPage = maxPages - 1;

                if (updateSaves)
                {
                    MoreSaves.pageLabel.text = String.Format("Page {0}/{1}", currentPage + 1, maxPages);
                    lastPageTransition = Time.time;
                    //Instantly
                    hideAllSaves();

                    //After all faded
                    Invoke("showAllSaves", transistionTime);
                }
            }
        }

        public void hideAllSaves()
        {
            uim.slotOne.HideSaveSlot();
            uim.slotTwo.HideSaveSlot();
            uim.slotThree.HideSaveSlot();
            uim.slotFour.HideSaveSlot();
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

            uim.slotOne.ShowSaveSlot();
            uim.slotTwo.ShowSaveSlot();
            uim.slotThree.ShowSaveSlot();
            uim.slotFour.ShowSaveSlot();
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
