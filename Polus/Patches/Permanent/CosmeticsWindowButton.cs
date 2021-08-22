using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Polus.Patches.Permanent
{
    public static class CosmeticsWindowButton
    {
        public static void CreateCosmeticsButton()
        {
            GameObject storeObj = GameObject.Find("StoreButton");
            storeObj = Object.Instantiate(storeObj, GameObject.Find("BottomButtons").transform);
            PassiveButton storeButton = storeObj.GetComponent<PassiveButton>();
            SpriteRenderer storeSprite = storeObj.GetComponent<SpriteRenderer>();

            (storeButton.OnClick = new Button.ButtonClickedEvent())
                .AddListener(new Action(() => Application.OpenURL("steam://run/1653240//--window=cosmetics/")));
        }

        public static void Load()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                // if (scene.name == "MainMenu") ;
                // CreateCosmeticsButton();
            }));
        }
    }
}