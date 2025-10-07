using System;
using System.Linq;
using System.Threading.Tasks;
using Reown.AppKit.Unity;
using UnityEngine;
using Matterless.Inject;
using Matterless.UTools;

namespace Matterless.Floorcraft
{
    public class WalletService
    {
        public event Action onWalletConnected;
        public event Action onWalletDisconnected;
        public event Action<bool> onModalStateChanged;

        private readonly WalletSettings m_WalletSettings;
        private GameObject m_AppKitPrefab;
        private bool m_IsInitialized = false;


        public WalletService(WalletSettings walletSettings)
        {
            m_WalletSettings = walletSettings;

            InstantiateAppKitPrefab();
            InitializeWallet();
        }

        public async void Disconnect()
        {
            await AppKit.DisconnectAsync();
        }

        public string GetConnectedAddress()
        {
            if (AppKit.IsAccountConnected)
            {
                return AppKit.Account.Address;
            }
            return string.Empty;
        }

        private async Task InitializeWallet()
        {
            AppKitConfig config = new AppKitConfig(
                projectId: m_WalletSettings.projectId, 
                new Metadata(
                    name: m_WalletSettings.projectName,
                    description: m_WalletSettings.projectDescription,
                    url: m_WalletSettings.projectUrl,
                    iconUrl: m_WalletSettings.projectIconUrl,
                    new RedirectData
                    {
                        Native = "unity-floorcraft-app://"
                    }   
                )
            );
            
            await AppKit.InitializeAsync(config);
            await OnAppKitInitialized();

        }

        private async Task OnAppKitInitialized(){
            if(AppKit.IsInitialized){
                AppKit.AccountConnected += OnAccountConnected;
                AppKit.AccountDisconnected += OnAccountDisconnected;
                AppKit.ModalController.OpenStateChanged += OnModalStateChanged;
            }
        }

        public void Connect(){
            if(!AppKit.IsInitialized){
                Debug.LogError("AppKit not initialized");
                return;
            }

            AppKit.OpenModal(ViewType.Connect);
        }

        private void OnAccountConnected(object sender, Connector.AccountConnectedEventArgs e)
        {
            onWalletConnected?.Invoke();
        }
        
        private void InstantiateAppKitPrefab()
        {
            GameObject appKitPrefab = Resources.Load<GameObject>("Wallet/Reown AppKit");
            
            if (appKitPrefab == null)
            {
                Debug.LogError("Reown AppKit prefab not found at Resources/Wallet/Reown AppKit.prefab");
                return;
            }

            m_AppKitPrefab = UnityEngine.Object.Instantiate(appKitPrefab);
        }

        private void OnAccountDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
        {
            onWalletDisconnected?.Invoke();
        }

        private void OnModalStateChanged(object sender, ModalOpenStateChangedEventArgs e)
        {
            onModalStateChanged?.Invoke(e.IsOpen);
        }
    }
}