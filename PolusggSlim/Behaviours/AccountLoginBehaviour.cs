using System;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;
using PolusggSlim.Utils.Extensions;
using UnityEngine;

namespace PolusggSlim.Behaviours
{
    [RegisterInIl2Cpp]
    public class AccountLoginBehaviour : MonoBehaviour
    {
        private GameObject _loginModal;
        private GameObject _emailTextBox;
        private GameObject _passwordTextBox;
        private GameObject _loginButton;
        private GameObject _closeButton;

        public AccountLoginBehaviour(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            var bundle = ResourceManager.GetAssetBundle("bepinexresources");
            _loginModal = (GameObject) bundle.LoadAsset("PolusggAccountMenu");
            _loginModal.active = false;

            _emailTextBox = _loginModal.FindRecursive(go => go.name == "EmailTextBox");
            _passwordTextBox = _loginModal.FindRecursive(go => go.name == "PasswordTextBox");
            _loginButton = _loginModal.FindRecursive(go => go.name == "Login");
            _closeButton = _loginModal.FindRecursive(go => go.name == "closeButton");
            
        }

        public void Start()
        {
            var context = PluginSingleton<PolusggMod>.Instance.AuthContext;
            var result = context.ApiClient.LogIn("test", "me").GetAwaiter().GetResult();
            if (result != null)
            {
                context.ParseClientIdAsUuid(result.Data.ClientId);
                context.ClientToken = result.Data.ClientToken;
                context.DisplayName = result.Data.DisplayName;
                context.Perks = result.Data.Perks;
            }
        }
    }
}