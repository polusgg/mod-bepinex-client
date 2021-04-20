using System;
using System.Linq;
using Il2CppSystem.Text;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;
using PolusggSlim.Utils.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PolusggSlim.Behaviours
{
    [RegisterInIl2Cpp]
    public class AccountLoginBehaviour : MonoBehaviour
    {
        private const string LOGIN_SCENE = "MMOnline";
        
        private static GameObject? _normalMenu;
        
        private static Action<Scene, LoadSceneMode> ShowAccountMenuHook = (scene, _) =>
        {
            if (scene.name == LOGIN_SCENE)
            {
                PggLog.Message("Loading AccountLoginBehaviour");
                _normalMenu = GameObject.Find("NormalMenu");
                _normalMenu?.AddComponent<AccountLoginBehaviour>();
            }
        };

        public static void Load()
        {
            ShowAccountMenuHook.Invoke(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            SceneManager.add_sceneLoaded(ShowAccountMenuHook);   
        }
        
        public static void Unload()
        {
            _normalMenu?.TryDestroyComponent<AccountLoginBehaviour>();
            SceneManager.remove_sceneLoaded(ShowAccountMenuHook);
        }
        
        
        
        private GameObject _topButtonBar;
        private GameObject _openLogInModalButton;
        private GameObject _createAccountButton;
        
        private SpriteRenderer _glyphRenderer;

        public AccountLoginBehaviour(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            if (_normalMenu is null)
                return;
            
            var bundle = ResourceManager.GetAssetBundle("bepinexresources");

            var topButtonBarPrefab = bundle.LoadAsset("Assets/Mods/LoginMenu/TopAccount.prefab").Cast<GameObject>();
            _glyphRenderer = new GameObject("GlyphRendererFix").AddComponent<SpriteRenderer>();

            
            var nameTextBar = _normalMenu.FindRecursive(go => go.name.Contains("NameText"));
            nameTextBar.active = false;
            _topButtonBar = Instantiate(topButtonBarPrefab);
            _topButtonBar.transform.position = nameTextBar.transform.position;

            _openLogInModalButton = _topButtonBar.FindRecursive(go => go.name.Contains("Login"));
            _createAccountButton = _topButtonBar.FindRecursive(go => go.name.Contains("Create"));

            _openLogInModalButton.MakePassiveButton(() =>
            {
                try
                {
                    var menuObj = Instantiate(bundle
                        .LoadAsset("Assets/Mods/LoginMenu/PolusggAccountMenu.prefab")).Cast<GameObject>();
                    var accountMenuHolder = menuObj.AddComponent<AccountMenuHolder>();
                    menuObj.transform.localPosition = new Vector3(0, 0, -500f);
                
                    GameObject nameObj = menuObj.FindRecursive(x => x.name.Contains("EmailTextBox"));
                    GameObject passwordObj = menuObj.FindRecursive(x => x.name.Contains("PasswordTextBox"));
                    GameObject loginButton = menuObj.FindRecursive(x => x.name.Contains("Login"));

                    GameObject background = menuObj.FindRecursive(x => x.name.Contains("Background"));
                    GameObject close = menuObj.FindRecursive(x => x.name.Contains("closeButton"));

                    var nameField = accountMenuHolder.nameField = CreateTextBoxTMP(nameObj);
                    var password = accountMenuHolder.password = CreateTextBoxTMP(passwordObj);

                    nameField.AllowEmail = true;
                    nameField.AllowPaste = true;

                    password.allowAllCharacters = true;
                    password.AllowPaste = true;
                
                    password.OnChange.AddListener(new Action(() =>
                    {
                        var starText = String.Join("", Enumerable.Repeat("*", password.text.Length));
                        password.outputText.text =  starText  + "<color=#FF0000>" + password.compoText + "</color>";
                        password.outputText.ForceMeshUpdate(true, true);
                    }));

                    loginButton.MakePassiveButton(() =>
                    {
                        if (Login(nameField.text, password.text))
                            Destroy(menuObj);     
                    });
                
                    background.MakePassiveButton(() => Destroy(menuObj), false);

                    close.MakePassiveButton(() => Destroy(menuObj));
                    
                    PggLog.Message("Entered before transition open add function");
                    menuObj.AddComponent<TransitionOpen>().duration = 0.2f;
                }
                catch (Exception e)
                {
                    PggLog.Error($"Error: {e.Message}, Stack: {e.StackTrace}");
                }
            });
            _createAccountButton.MakePassiveButton(() =>
            {
                Application.OpenURL("https://account.polus.gg");
            });
        }

        private bool Login(string email, string password)
        {
            var context = PluginSingleton<PolusggMod>.Instance.AuthContext;
            var result = context.ApiClient
                .LogIn(email, password)
                .GetAwaiter().GetResult();
            if (result != null)
            {
                context.ParseClientIdAsUuid(result.Data.ClientId);
                context.ClientToken = result.Data.ClientToken;
                context.DisplayName = result.Data.DisplayName;
                context.Perks = result.Data.Perks;
            }

            return result != null;
        }


        private TextBoxTMP CreateTextBoxTMP(GameObject main)
        {
            TextBoxTMP box = main.AddComponent<TextBoxTMP>();
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
        
        
        [RegisterInIl2Cpp]
        public class AccountMenuHolder : MonoBehaviour
        {
            public TextBoxTMP nameField;
            public TextBoxTMP password;
            
            public AccountMenuHolder(IntPtr ptr) : base(ptr)  { }
        }
    }
}