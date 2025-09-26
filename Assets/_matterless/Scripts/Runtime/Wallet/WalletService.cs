using System;
using System.Linq;
using System.Threading.Tasks;
using Reown.AppKit.Unity;
using UnityEngine;

namespace Matterless.Floorcraft
{
    public class WalletService
    {
        private readonly WalletSettings m_WalletSettings;
        private bool m_IsInitialized = false;

        // Add events for UI to subscribe to
        public event Action onWalletConnected;
        public event Action onWalletDisconnected;

        public WalletService(WalletSettings walletSettings)
        {
            m_WalletSettings = walletSettings;
        }

        public async void ConnectWallet()
        {
            if (!m_IsInitialized)
            {
                await InitializeWallet();
            }
            else
            {
                await ResumeSession();
            }
        }

        public void DisconnectWallet()
        {
            onWalletDisconnected?.Invoke();
        }

        private async Task InitializeWallet()
        {
            try
            {
                AppKitConfig config = new AppKitConfig(
                    projectId: m_WalletSettings.projectId, 
                    new Metadata(
                        name: m_WalletSettings.projectName,
                        description: m_WalletSettings.projectDescription,
                        url: m_WalletSettings.projectUrl,
                        iconUrl: m_WalletSettings.projectIconUrl
                    )
                );
                
                await AppKit.InitializeAsync(config);
                m_IsInitialized = true;
                
                await ResumeSession();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing wallet: {ex.Message}");
            }
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
    }
}