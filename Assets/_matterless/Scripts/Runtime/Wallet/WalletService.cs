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
        private readonly WalletSettings m_WalletSettings;
        private IUnityEventDispatcher m_UnityEventDispatcher;
        private bool m_IsInitialized = false;

        // Add events for UI to subscribe to
        public event Action onWalletConnected;
        public event Action onWalletDisconnected;

        public WalletService(WalletSettings walletSettings, IUnityEventDispatcher unityEventDispatcher)
        {
            m_WalletSettings = walletSettings;
            m_UnityEventDispatcher = unityEventDispatcher;

            m_UnityEventDispatcher.unityOnApplicationFocus += OnApplicationFocus;
            m_UnityEventDispatcher.unityOnApplicationPause += OnApplicationPause;

            InstantiateAppKitPrefab();
            InitializeWallet();
        }

        public void Disconnect()
        {
            onWalletDisconnected?.Invoke();
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

            Debug.Log("tomicz: AppKit config created");
            Debug.Log("tomicz: " + m_WalletSettings.projectId);
            Debug.Log("tomicz: " + m_WalletSettings.projectName);
            Debug.Log("tomicz: " + m_WalletSettings.projectDescription);
            Debug.Log("tomicz: " + m_WalletSettings.projectUrl);
            Debug.Log("tomicz: " + m_WalletSettings.projectIconUrl);
            
            await AppKit.InitializeAsync(config);
            await OnAppKitInitialized();

        }

        private async Task OnAppKitInitialized(){
            Debug.Log($"tomicz: Checking if AppKit is initialized - {AppKit.IsInitialized}");
            if(AppKit.IsInitialized){
                Debug.Log("tomicz: AppKit initialized");

                AppKit.AccountConnected += OnAccountConnected;
                Debug.Log("tomicz: Account connected event subscribed");
            }
        }

        public void Connect(){
            if(!AppKit.IsInitialized){
                Debug.LogError("tomicz: AppKit not initialized");
                return;
            }

            AppKit.OpenModal(ViewType.Connect);
        }

        private void OnAccountConnected(object sender, Connector.AccountConnectedEventArgs e)
        {
            Debug.Log("tomicz: Account connected");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log("tomicz: Is AppKit initialized: " + AppKit.IsInitialized);
            Debug.Log("tomicz: Application focus changed to: " + hasFocus);
            Debug.Log("tomicz: IsModalOpen: " + AppKit.IsModalOpen);

            if (AppKit.IsInitialized)
            {
                Debug.Log("tomicz: App gained focus - checking connection");
                Debug.Log("tomicz: IsAccountConnected: " + AppKit.IsAccountConnected);
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("tomicz: Application pause changed to: " + isPaused);
        }
        
        private void InstantiateAppKitPrefab()
        {
            GameObject appKitPrefab = Resources.Load<GameObject>("Wallet/Reown AppKit");
            
            if (appKitPrefab == null)
            {
                Debug.LogError("Reown AppKit prefab not found at Resources/Wallet/Reown AppKit.prefab");
                return;
            }

            UnityEngine.Object.Instantiate(appKitPrefab);
        }
    }
}