using System.Collections.Generic;
using PolusGG.Api;
using PolusGG.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusGG {
    public static class AccountMenu {
        private static bool loaded;
        private static List<GameObject> _objects = new();
        public static void InitializeAccountMenu(Scene scene) {
            GameObject gameObject = GameObject.Find("NormalMenu");
            if (!loaded) {
                _objects.Add(PogusPlugin.Bundle.LoadAsset("Assets/Mods/LoginMenu/PogusAccountMenu.prefab").Cast<GameObject>().DontDestroy());
                _objects.Add(PogusPlugin.Bundle.LoadAsset("Assets/Mods/LoginMenu/TopAccount.prefab").Cast<GameObject>().DontDestroy());
                loaded = true;
            }
            PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
            GameObject nameText = gameObject.FindRecursive(x => x.name.Contains("NameText"));
            GameObject passwordText = gameObject.FindRecursive(x => x.name.Contains("PasswordText"));
            if (!PolusAuth.IsPlayerSignedIn)
            {
                var name = nameText.GetComponent<TextMeshPro>();
                var password = passwordText.GetComponent<TextMeshPro>();
                PolusAuth.Login(name.text, password.text);
            }
        }
    }
}