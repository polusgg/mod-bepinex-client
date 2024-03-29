using System;
using System.Linq;
using System.Threading.Tasks;
using Il2CppSystem.Text;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;
using PolusggSlim.Utils.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Coroutine = PolusggSlim.Coroutines.Coroutine;

namespace PolusggSlim.Auth.Behaviours
{
    [RegisterInIl2Cpp]
    public class AccountLoginBehaviour : MonoBehaviour
    {
        private const string LOGIN_SCENE = "MMOnline";

        private static GameObject _positionAnchorObject;

        private static readonly Action<Scene, LoadSceneMode> ShowAccountMenuHook = (scene, _) =>
        {
            if (scene.name == LOGIN_SCENE)
            {
                _positionAnchorObject = new GameObject("PggAccountsAnchor");
                var aspectPosition = _positionAnchorObject.AddComponent<AspectPosition>();

                aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
                aspectPosition.DistanceFromEdge = new Vector3(0f, 0.5f);
                aspectPosition.AdjustPosition();

                _positionAnchorObject.AddComponent<AccountLoginBehaviour>();
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
            if (_positionAnchorObject != null)
                Destroy(_positionAnchorObject);

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
            SetupGUI();
            
            _loggedInMenu.active = _authContext.LoggedIn;
            _topButtonBar.active = !_authContext.LoggedIn;
            if (_authContext.LoggedIn)
                UpdateGameSettingsWithName(_authContext.DisplayName);

            UpdateJoinGameButtonState(_authContext.LoggedIn);
            
            CheckLogin();
        }

        private void SetupGUI()
        {
            GameObject.Find("NormalMenu/HostGameButton").transform.localPosition = new Vector3(0f, 1.2f, -1f);
            GameObject.Find("NormalMenu/FindGameButton").transform.localPosition = new Vector3(0f, -0.4f, -1f);
            GameObject.Find("NormalMenu/JoinGameButton").transform.localPosition = new Vector3(0f, -2f, -1f); 
            
            
            var bundle = ResourceManager.GetAssetBundle("accountsmenu");

            var topButtonBarPrefab = bundle.LoadAsset("Assets/Mods/LoginMenu/TopAccount.prefab").Cast<GameObject>();
            var loggedInMenuPrefab = bundle.LoadAsset("Assets/Mods/LoginMenu/LoggedInMenu.prefab").Cast<GameObject>();
            _glyphRenderer = new GameObject("GlyphRendererFix").AddComponent<SpriteRenderer>();


            _topButtonBar = Instantiate(topButtonBarPrefab, _positionAnchorObject.transform);
            _topButtonBar.transform.localPosition = Vector3.zero;

            _loggedInMenu = Instantiate(loggedInMenuPrefab, _positionAnchorObject.transform);
            _loggedInMenu.transform.localPosition = Vector3.zero;

            _openLogInModalButton = _topButtonBar.FindRecursive(go => go.name.Contains("Login"));
            _createAccountButton = _topButtonBar.FindRecursive(go => go.name.Contains("Create"));

            _openLogInModalButton.MakePassiveButton(() =>
            {
                try
                {
                    CreateLoginMenu();
                }
                catch (Exception e)
                {
                    PggLog.Error($"Error: {e.Message}, Stack Trace: {e.StackTrace}");
                }
            });
            _createAccountButton.MakePassiveButton(() => { Application.OpenURL("https://account.polus.gg"); });

            _logOutButton = _loggedInMenu.FindRecursive(go => go.name.Contains("LogOut"));
            _logOutButton.MakePassiveButton(LogOut);
        }

        private void CreateLoginMenu()
        {
            var bundle = ResourceManager.GetAssetBundle("accountsmenu");
            
            var menuObj = Instantiate(
                bundle.LoadAsset("Assets/Mods/LoginMenu/PolusggAccountMenu.prefab")
            ).Cast<GameObject>();

            menuObj.transform.localPosition = new Vector3(0, 0, -500f);

            var emailObj = menuObj.FindRecursive(x => x.name.Contains("EmailTextBox"));
            var passwordObj = menuObj.FindRecursive(x => x.name.Contains("PasswordTextBox"));
            var loginButton = menuObj.FindRecursive(x => x.name.Contains("Login"));

            var background = menuObj.FindRecursive(x => x.name.Contains("Background"));
            var close = menuObj.FindRecursive(x => x.name.Contains("closeButton"));

            var emailField = CreateTextBoxTMP(emailObj);
            var passwordField = CreateTextBoxTMP(passwordObj);

            emailField.AllowEmail = true;
            emailField.AllowPaste = true;

            passwordField.allowAllCharacters = true;
            passwordField.AllowPaste = true;
            passwordField.AllowSymbols = true;
            passwordField.AllowEmail = true;
            
            
            var focusSwitcher = menuObj.AddComponent<FocusSwitcher>();
            focusSwitcher.TextBoxes.Add(emailField);
            focusSwitcher.TextBoxes.Add(passwordField);

            passwordField.OnChange.AddListener(new Action(() =>
            {
                var starText = string.Join("", Enumerable.Repeat("*", passwordField.text.Length));
                passwordField.outputText.text = starText;
                passwordField.outputText.ForceMeshUpdate(true, true);
            }));

            void LoginAction()
            {
                var loginButtonRenderer = loginButton.GetComponent<SpriteRenderer>();
                var textRenderer = loginButton.GetComponentInChildren<TextMeshPro>();
                loginButtonRenderer.color = textRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                
                AsyncCoroutine
                    .CoContinueTaskWith(Task.Run(async () =>
                    {
                        return await Login(emailField.text, passwordField.text);
                    }), loggedIn =>
                    {
                        loginButtonRenderer.color = textRenderer.color = new Color(1f, 1f, 1f, 1f);
                        if (loggedIn)
                        {
                            _loggedInMenu.active = true;
                            _topButtonBar.active = false;
                            
                            UpdateJoinGameButtonState(true);
                            
                            UpdateGameSettingsWithName(_authContext.DisplayName);
                            Destroy(menuObj);
                        }
                        else
                        {
                            passwordField.text = "";
                            passwordField.outputText.text = "";
                            Coroutine.Start(new Coroutine.Il2CppEnumeratorWrapper(
                                Effects.SwayX(loginButton.transform, 0.75f, 0.25f)
                            ));
                        }
                    })
                    .StartAsCoroutine();
            }

            emailField.OnEnter.AddListener((Action) LoginAction);
            passwordField.OnEnter.AddListener((Action) LoginAction);
            loginButton.MakePassiveButton(LoginAction);

            background.MakePassiveButton(() => { }, false);

            close.MakePassiveButton(() => Destroy(menuObj));

            menuObj.AddComponent<TransitionOpen>().duration = 0.2f;
        }

        private void CheckLogin()
        {
            if (!_authContext.LoggedIn || _authContext.LoggedInDateTime > DateTime.Now.AddMinutes(-10))
                return;

            AsyncCoroutine.CoContinueTaskWith(Task.Run(async () =>
            {
                var response = await _authContext.ApiClient.CheckToken(
                    _authContext.ClientIdString,
                    _authContext.ClientToken
                );

                if (response != null)
                {
                    _authContext.DisplayName = response.Data.DisplayName;
                    _authContext.Perks = response.Data.Perks;
                    _authContext.LoggedInDateTime = DateTime.Now;

                    _authContext.SaveToFile();
                }

                return response == null;

            }), result =>
            {
                if (result)
                    LogOut();
            }).StartAsCoroutine();
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
                _authContext.LoggedInDateTime = DateTime.Now;
                
                _authContext.SaveToFile();
            }

            return result != null;
        }
        
        private void LogOut()
        {
            _authContext.ClientId = new byte[0];
            _authContext.ClientIdString = string.Empty;
            _authContext.ClientToken = string.Empty;
            _authContext.DisplayName = string.Empty;
            _authContext.Perks = new string[0];
            
            UpdateGameSettingsWithName("Guest");
            
            _loggedInMenu.active = _authContext.LoggedIn;
            _topButtonBar.active = !_authContext.LoggedIn;

            UpdateJoinGameButtonState(_authContext.LoggedIn);
            
            _authContext.DeleteSaveFile();
        }

        [HideFromIl2Cpp]
        private void UpdateGameSettingsWithName(string displayName)
        {
            SaveManager.PlayerName = displayName;
            SaveManager.lastPlayerName = displayName;
            
            var tmp = _loggedInMenu.GetComponentInChildren<TextMeshPro>();
            tmp.text = $"Welcome, {displayName}";
        }

        private void UpdateJoinGameButtonState(bool enabled)
        {
            var rendererColor = new Color(1f, 1f, 1f, enabled ? 1f : 0.3f);
            
            var button = GameObject.Find("NormalMenu/HostGameButton/CreateGameButton");
            button.GetComponent<PassiveButton>().enabled = enabled;
            button.GetComponent<SpriteRenderer>().color = rendererColor;
            

            button = GameObject.Find("NormalMenu/FindGameButton/FindGameButton");
            button.GetComponent<PassiveButton>().enabled = enabled;
            button.GetComponent<SpriteRenderer>().color = rendererColor;
            
            var gameIdField = GameObject.Find("NormalMenu/JoinGameButton/GameIdText");
            gameIdField.transform.Find("Background").GetComponent<SpriteRenderer>().color = rendererColor;
            gameIdField.transform.Find("arrowEnter").gameObject.active = enabled;
            gameIdField.GetComponent<PassiveButton>().enabled = enabled;
            gameIdField.GetComponent<TextBoxTMP>().enabled = enabled;
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
}
