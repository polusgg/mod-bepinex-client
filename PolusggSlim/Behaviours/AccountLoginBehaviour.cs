using System;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Il2CppSystem.Text;
using Newtonsoft.Json;
using PolusggSlim.Auth;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;
using PolusggSlim.Utils.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PolusggSlim.Behaviours
{
    [RegisterInIl2Cpp]
    public class AccountLoginBehaviour : MonoBehaviour
    {
        private const string LOGIN_SCENE = "MMOnline";

        private static GameObject? _normalMenu;

        private static readonly Action<Scene, LoadSceneMode> ShowAccountMenuHook = (scene, _) =>
        {
            if (scene.name == LOGIN_SCENE)
            {
                _normalMenu = GameObject.Find("NormalMenu");
                _normalMenu?.AddComponent<AccountLoginBehaviour>();
            }
        };

        public static void Load()
        {
            PggLog.Message("Loading AccountLoginBehaviour");
            ShowAccountMenuHook.Invoke(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            SceneManager.add_sceneLoaded(ShowAccountMenuHook);
        }

        public static void Unload()
        {
            _normalMenu?.TryDestroyComponent<AccountLoginBehaviour>();
            SceneManager.remove_sceneLoaded(ShowAccountMenuHook);
        }
        
        
        private SpriteRenderer _glyphRenderer;

        private GameObject _nameTextBar;
        private GameObject _topButtonBar;

        private GameObject _openLogInModalButton;
        private GameObject _createAccountButton;
        
        [HideFromIl2Cpp]
        private AuthContext _authContext => PluginSingleton<PolusggMod>.Instance.AuthContext;

        public AccountLoginBehaviour(IntPtr ptr) : base(ptr)
        {
        }
        

        public void Awake()
        {
            if (_normalMenu is null)
                return;

            var bundle = ResourceManager.GetAssetBundle("accountsmenu");

            var topButtonBarPrefab = bundle.LoadAsset("Assets/Mods/LoginMenu/TopAccount.prefab").Cast<GameObject>();
            _glyphRenderer = new GameObject("GlyphRendererFix").AddComponent<SpriteRenderer>();


            _nameTextBar = _normalMenu.FindRecursive(go => go.name.Contains("NameText"));
            var nameTextButton = _nameTextBar.GetComponent<PassiveButton>();
            _nameTextBar.transform.localPosition = new Vector3(0, 2.32f, 0);
            
            nameTextButton.OnClick.RemoveAllListeners();
            nameTextButton.OnClick.AddListener(new Action(LogOut));

            _topButtonBar = Instantiate(topButtonBarPrefab);
            _topButtonBar.transform.position = _nameTextBar.transform.position;

            _openLogInModalButton = _topButtonBar.FindRecursive(go => go.name.Contains("Login"));
            _createAccountButton = _topButtonBar.FindRecursive(go => go.name.Contains("Create"));

            _openLogInModalButton.MakePassiveButton(() =>
            {
                try
                {
                    var menuObj = Instantiate(bundle
                        .LoadAsset("Assets/Mods/LoginMenu/PolusggAccountMenu.prefab")).Cast<GameObject>();
                    menuObj.transform.localPosition = new Vector3(0, 0, -500f);

                    var nameObj = menuObj.FindRecursive(x => x.name.Contains("EmailTextBox"));
                    var passwordObj = menuObj.FindRecursive(x => x.name.Contains("PasswordTextBox"));
                    var loginButton = menuObj.FindRecursive(x => x.name.Contains("Login"));

                    var background = menuObj.FindRecursive(x => x.name.Contains("Background"));
                    var close = menuObj.FindRecursive(x => x.name.Contains("closeButton"));

                    var nameField = CreateTextBoxTMP(nameObj);
                    var password = CreateTextBoxTMP(passwordObj);

                    nameField.AllowEmail = true;
                    nameField.AllowPaste = true;

                    password.allowAllCharacters = true;
                    password.AllowPaste = true;

                    password.OnChange.AddListener(new Action(() =>
                    {
                        var starText = string.Join("", Enumerable.Repeat("*", password.text.Length));
                        password.outputText.text = starText + "<color=#FF0000>" + password.compoText + "</color>";
                        password.outputText.ForceMeshUpdate(true, true);
                    }));

                    loginButton.MakePassiveButton(() =>
                    {
                        if (Login(nameField.text, password.text))
                            Destroy(menuObj);
                    });

                    background.MakePassiveButton(() => Destroy(menuObj), false);

                    close.MakePassiveButton(() => Destroy(menuObj));

                    menuObj.AddComponent<TransitionOpen>().duration = 0.2f;
                }
                catch (Exception e)
                {
                    PggLog.Error($"Error: {e.Message}, Stack: {e.StackTrace}");
                }
            });
            _createAccountButton.MakePassiveButton(() => { Application.OpenURL("https://account.polus.gg"); });

            
            _nameTextBar.active = _authContext.LoggedIn;
            _topButtonBar.active = !_authContext.LoggedIn;
            if (_authContext.LoggedIn)
                UpdateGameSettingsWithName(_authContext.DisplayName);
        }

        [HideFromIl2Cpp]
        private bool Login(string email, string password)
        {
            var result = _authContext.ApiClient
                .LogIn(email, password)
                .GetAwaiter().GetResult();
            if (result != null)
            {
                _authContext.ParseClientIdAsUuid(result.Data.ClientId);
                _authContext.ClientToken = result.Data.ClientToken;
                _authContext.DisplayName = result.Data.DisplayName;
                _authContext.Perks = result.Data.Perks;

                _nameTextBar.active = true;
                _topButtonBar.active = false;

                UpdateGameSettingsWithName(result.Data.DisplayName);
                
                // TODO: Reorganize code
                var filePath = Path.Combine(Paths.GameRootPath, "api.txt");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(new SavedAuthModel
                {
                    ClientId = _authContext.ClientId,
                    ClientToken = _authContext.ClientToken,
                    DisplayName = _authContext.DisplayName,
                    Perks = _authContext.Perks
                }));
            }

            return result != null;
        }
        
        private void LogOut()
        {
            UpdateGameSettingsWithName("Guest");
            
            _nameTextBar.active = false;
            _topButtonBar.active = true;
        }

        [HideFromIl2Cpp]
        private void UpdateGameSettingsWithName(string displayName)
        {
            SaveManager.PlayerName = displayName;
            SaveManager.lastPlayerName = displayName;
            
            var tmp = _nameTextBar.GetComponentInChildren<TextMeshPro>();
            tmp.m_text = $"Logged in as: {displayName}";
            
            var background = _nameTextBar.FindRecursive(x => x.name.Equals("Background"));
            background.active = false;

            // TODO: AccountManager.Instance.accountTab.UpdateNameDisplay();
        }


        private TextBoxTMP CreateTextBoxTMP(GameObject main)
        {
            var box = main.AddComponent<TextBoxTMP>();
            box.outputText = main.transform.Find("BoxLabel").GetComponent<TextMeshPro>();
            box.outputText.text = "";
            box.text = "";
            box.characterLimit = 72;
            box.quickChatGlyph = _glyphRenderer;
            box.sendButtonGlyph = _glyphRenderer;
            box.Background = main.GetComponent<SpriteRenderer>();
            box.Pipe = main.transform.Find("Pipe").GetComponent<MeshRenderer>();
            box.colliders = new Il2CppReferenceArray<Collider2D>(new Collider2D[] {main.GetComponent<BoxCollider2D>()});
            box.OnEnter = new Button.ButtonClickedEvent();
            box.OnChange = new Button.ButtonClickedEvent();
            box.OnFocusLost = new Button.ButtonClickedEvent();
            box.tempTxt = new StringBuilder();
            main.MakePassiveButton(box.GiveFocus);
            return box;
        }
    }

    [HarmonyPatch(typeof(SetNameText), nameof(SetNameText.Start))]
    public static class SetNameText_Start
    {
        public static bool Prefix(SetNameText __instance)
        {
            var context = PluginSingleton<PolusggMod>.Instance.AuthContext;
            if (context.LoggedIn)
                __instance.nameText.m_text = $"Logged in as: {context.DisplayName}";

            return false;
        }
    }
}