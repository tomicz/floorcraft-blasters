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
            if(AppKit.IsInitialized){
                Debug.Log("tomicz: AppKit initialized");
            }
        }

        public void Connect(){
            if(!AppKit.IsInitialized){
                Debug.LogError("tomicz: AppKit not initialized");
                return;
            }

            AppKit.OpenModal(ViewType.Connect);
        }

        private void OnAccountConnected(object sender, EventArgs e)
        {
            Debug.Log("tomicz: Account connected");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log("tomicz: Application focus changed to: " + hasFocus);
            if(AppKit.IsInitialized){
                AppKit.AccountConnected += OnAccountConnected;
                Debug.Log("tomicz: Account connected event subscribed");
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("tomicz: Application pause changed to: " + isPaused);
        }

        public async Task ResumeSession()
        {
            bool resumed = await AppKit.ConnectorController.TryResumeSessionAsync();

            if (resumed)
            {
                MyAccountConnectedHandler();
            }
            else
            {
                AppKit.AccountConnected += (_, e) => MyAccountConnectedHandler();
                AppKit.OpenModal();
            }
        }

        public string GetWalletAddress()
        {
            try
            {
                var accounts = AppKit.ConnectorController.Accounts;
                if (accounts != null && accounts.Any())
                {
                    // Return the first connected account's address
                    return accounts.First().AccountId;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting wallet address: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetWalletBalanceAsync()
        {
            try
            {
                var address = GetWalletAddress();
                if (string.IsNullOrEmpty(address))
                {
                    return "0";
                }

                return "0.0";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting wallet balance: {ex.Message}");
                return "0";
            }
        }

        public string GetWalletBalance()
        {
            try
            {
                var address = GetWalletAddress();
                if (string.IsNullOrEmpty(address))
                {
                    return "0";
                }

                return "0.0";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting wallet balance: {ex.Message}");
                return "0";
            }
        }

        public string[] GetAllWalletAddresses()
        {
            try
            {
                var accounts = AppKit.ConnectorController.Accounts;
                if (accounts != null && accounts.Any())
                {
                    return accounts.Select(account => account.AccountId).ToArray();
                }
                return new string[0];
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting wallet addresses: {ex.Message}");
                return new string[0];
            }
        }

        public bool IsWalletConnected()
        {
            try
            {
                var accounts = AppKit.ConnectorController.Accounts;
                return accounts != null && accounts.Any();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking wallet connection: {ex.Message}");
                return false;
            }
        }

        private void MyAccountConnectedHandler()
        {
            Debug.Log("Account connected successfully!");
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

            UnityEngine.Object.Instantiate(appKitPrefab);
        }
    }
}