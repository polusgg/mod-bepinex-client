using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Coroutine = PolusggSlim.Utils.Coroutine;
using Encoding = System.Text.Encoding;

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

        private GameObject _topButtonBar;
        private GameObject _loggedInMenu;

        private GameObject _openLogInModalButton;
        private GameObject _createAccountButton;
        
        private GameObject _logOutButton;
        
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
            var loggedInMenuPrefab = bundle.LoadAsset("Assets/Mods/LoginMenu/LoggedInMenu.prefab").Cast<GameObject>();
            _glyphRenderer = new GameObject("GlyphRendererFix").AddComponent<SpriteRenderer>();


            var nameTextBar = _normalMenu.FindRecursive(go => go.name.Contains("NameText"));
            var nameTextBarPosition = nameTextBar.transform.position;
            nameTextBar.active = false; 

            _topButtonBar = Instantiate(topButtonBarPrefab);
            _topButtonBar.transform.position = nameTextBarPosition;

            _loggedInMenu = Instantiate(loggedInMenuPrefab);
            _loggedInMenu.transform.position = nameTextBarPosition;

            _openLogInModalButton = _topButtonBar.FindRecursive(go => go.name.Contains("Login"));
            _createAccountButton = _topButtonBar.FindRecursive(go => go.name.Contains("Create"));

            _openLogInModalButton.MakePassiveButton(() =>
            {
                try
                {
                    var menuObj = Instantiate(bundle
                        .LoadAsset("Assets/Mods/LoginMenu/PolusggAccountMenu.prefab")).Cast<GameObject>();
                    menuObj.transform.localPosition = new Vector3(0, 0, -500f);

                    var emailObj = menuObj.FindRecursive(x => x.name.Contains("EmailTextBox"));
                    var passwordObj = menuObj.FindRecursive(x => x.name.Contains("PasswordTextBox"));
                    var loginButton = menuObj.FindRecursive(x => x.name.Contains("Login"));

                    var background = menuObj.FindRecursive(x => x.name.Contains("Background"));
                    var close = menuObj.FindRecursive(x => x.name.Contains("closeButton"));

                    var emailField = CreateTextBoxTMP(emailObj);
                    var password = CreateTextBoxTMP(passwordObj);

                    emailField.AllowEmail = true;
                    emailField.AllowPaste = true;

                    password.allowAllCharacters = true;
                    password.AllowPaste = true;
                    password.AllowSymbols = true;
                    password.AllowEmail = true;

                    password.OnChange.AddListener(new Action(() =>
                    {
                        var starText = string.Join("", Enumerable.Repeat("*", password.text.Length));
                        password.outputText.text = starText + "<color=#FF0000>" + password.compoText + "</color>";
                        password.outputText.ForceMeshUpdate(true, true);
                    }));

                    void LoginAction()
                    {
                        AsyncCoroutine
                            .CoContinueTaskWith(Task.Run(async () =>
                            {
                                PggLog.Message($"Thread {Thread.CurrentThread.ManagedThreadId}. IsThreadPool {Thread.CurrentThread.IsThreadPoolThread}");
                                return await Login(emailField.text, password.text);
                            }), () =>
                            {
                                _loggedInMenu.active = true;
                                _topButtonBar.active = false;

                                UpdateGameSettingsWithName(_authContext.DisplayName);
                                Destroy(menuObj);
                            })
                            .StartAsCoroutine();
                    }

                    emailField.OnEnter.AddListener((Action) LoginAction);
                    password.OnEnter.AddListener((Action) LoginAction);
                    loginButton.MakePassiveButton(LoginAction);

                    background.MakePassiveButton(() => { }, false);

                    close.MakePassiveButton(() => Destroy(menuObj));

                    menuObj.AddComponent<TransitionOpen>().duration = 0.2f;
                }
                catch (Exception e)
                {
                    PggLog.Error($"Error: {e.Message}, Stack Trace: {e.StackTrace}");
                }
            });
            _createAccountButton.MakePassiveButton(() => { Application.OpenURL("https://account.polus.gg"); });

            _logOutButton = _loggedInMenu.FindRecursive(go => go.name.Contains("LogOut"));
            _logOutButton.MakePassiveButton(LogOut);
            
            _loggedInMenu.active = _authContext.LoggedIn;
            _topButtonBar.active = !_authContext.LoggedIn;
            if (_authContext.LoggedIn)
                UpdateGameSettingsWithName(_authContext.DisplayName);
        }

        private async Task<bool> Login(string email, string password)
        {
            var result = await _authContext.ApiClient
                .LogIn(email, password);
            if (result != null)
            {
                _authContext.ParseClientIdAsUuid(result.Data.ClientId);
                _authContext.ClientToken = result.Data.ClientToken;
                _authContext.DisplayName = result.Data.DisplayName;
                _authContext.Perks = result.Data.Perks;
                
                // TODO: Reorganize code
                var filePath = Path.Combine(Paths.GameRootPath, "api.txt");
                await File.WriteAllTextAsync(filePath, 
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new SavedAuthModel
                        {
                            ClientId = _authContext.ClientId,
                            ClientToken = _authContext.ClientToken,
                            DisplayName = _authContext.DisplayName,
                            Perks = _authContext.Perks
                        })
                    ))
                );
            }

            return result != null;
        }
        
        private void LogOut()
        {
            _authContext.ClientId = new byte[0];
            _authContext.ClientToken = "";
            _authContext.DisplayName = "";
            _authContext.Perks = new string[0];
            
            UpdateGameSettingsWithName("Guest");
            
            _loggedInMenu.active = false;
            _topButtonBar.active = true;
            
            //TODO: Reorganize Code
            var filePath = Path.Combine(Paths.GameRootPath, "api.txt");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        [HideFromIl2Cpp]
        private void UpdateGameSettingsWithName(string displayName)
        {
            SaveManager.PlayerName = displayName;
            SaveManager.lastPlayerName = displayName;
            
            var tmp = _loggedInMenu.GetComponentInChildren<TextMeshPro>();
            tmp.m_text = $"Welcome, {displayName}";
            
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
            if (context.LoggedIn && SceneManager.GetActiveScene().name == "MMOnline")
                __instance.nameText.m_text = $"Logged in as: {context.DisplayName}";

            return false;
        }
    }
}