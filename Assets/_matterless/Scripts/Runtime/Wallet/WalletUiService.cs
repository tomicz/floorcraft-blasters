using System;
using Matterless.Inject;
using System.Threading.Tasks;
using UnityEngine;

namespace Matterless.Floorcraft
{
    public class WalletUiService
    {
        private readonly WalletService m_WalletService;
        private readonly AudioUiService m_AudioUiService;
        private readonly IAnalyticsService m_AnalyticsService;
        private readonly INotificationService m_NotificationService;
        private readonly WalletUiView m_View;

        public WalletUiService(WalletService walletService, AudioUiService audioUiService, IAnalyticsService analyticsService, INotificationService notificationService)
        {
            m_WalletService = walletService;
            m_AudioUiService = audioUiService;
            m_AnalyticsService = analyticsService;
            m_NotificationService = notificationService;

            // Create the view
            m_View = WalletUiView.Create("UIPrefabs/UIP_WalletView").Init();

            // Wire up events
            m_View.onConnectWalletButtonClicked += OnConnectWalletButtonClicked;
            m_View.onOpenWalletButtonClicked += OnOpenWalletButtonClicked;
            m_View.onHideWalletButtonClicked += OnHideWalletButtonClicked;
            m_View.onDisconnectWalletButtonClicked += OnDisconnectWalletButtonClicked;

            // Subscribe to wallet state changes
            m_WalletService.onWalletConnected += OnWalletConnected;
            m_WalletService.onWalletDisconnected += OnWalletDisconnected;
            m_WalletService.onModalStateChanged += OnModalStateChanged;

            // Hide by default - will be shown by UiFlowService when in Intro state
            m_View.Hide();

            // Set initial state (show connect button, hide open wallet button, hide wallet info)
            m_View.SetConnectButtonVisibility(true);
            m_View.SetOpenWalletButtonVisibility(false);
        }

        private void OnConnectWalletButtonClicked()
        {
            m_WalletService.Connect();
            m_AudioUiService.PlaySelectSound();
            m_View.SetConnectButtonInteractability(false);
        }

        private void OnOpenWalletButtonClicked()
        {
            // Open wallet info container
            m_AudioUiService.PlaySelectSound();
            m_View.ShowWalletInfo();
        }

        private async void OnWalletConnected()
        {
            m_View.SetConnectButtonVisibility(false);
            m_View.SetOpenWalletButtonVisibility(true);

            string address = m_WalletService.GetConnectedAddress();
            m_View.SetWalletAddress(address);

            // Track wallet connection for user analytics
            m_AnalyticsService.SetWalletAddress(address);

            // Show wallet connected notification
            m_NotificationService.ShowMessage(NotificationType.WalletConnected);

            // Do NOT show wallet info automatically - only on Open Wallet button click
            m_View.HideWalletInfo();
            m_View.SetConnectButtonInteractability(true);
            m_View.SetOpenWalletButtonInteractability(true);
            
            ShowBalance();
            
            // Wait for NFT cache to initialize
            await System.Threading.Tasks.Task.Delay(2000);
            
            // Create NFT containers based on owned NFT count
            int nftCount = m_WalletService.GetOwnedNFTCount();
            m_View.InitializeNFTContainers(nftCount);
            
            // Load NFT images
            LoadNFTImages();
        }

        private void OnWalletDisconnected()
        {
            // Track wallet disconnection for user analytics
            m_AnalyticsService.ClearWalletAddress();

            // Show wallet disconnected notification
            m_NotificationService.ShowMessage(NotificationType.WalletDisconnected);

            // Clear NFT containers
            m_View.ClearNFTContainers();

            m_View.SetConnectButtonVisibility(true);
            m_View.SetOpenWalletButtonVisibility(false);
            m_View.SetConnectButtonInteractability(true);
            m_View.SetOpenWalletButtonInteractability(false);
            m_View.ResetCanvasSortingOrder();
        }

        private async void ShowBalance(){
            // Fetch ETH balance
            string ethBalance = await m_WalletService.GetWalletNativeBalanceAsync();
            m_View.SetEthBalanceText(ethBalance);
            
            // Fetch AUKI balance
            string aukiBalance = await m_WalletService.GetAukiBalanceAsync();
            m_View.SetAukiBalanceText(aukiBalance);
        }
        
        private async void LoadNFTImages()
        {
            try
            {
                var ownedTokenIds = m_WalletService.GetOwnedTokenIds();
                int nftCount = ownedTokenIds.Count;
                
                if (nftCount == 0)
                {
                    return;
                }
                
                if (nftCount > m_View.m_InstantiatedContainersPublic.Count)
                {
                    Debug.LogWarning($"Wallet owns {nftCount} NFTs but only {m_View.m_InstantiatedContainersPublic.Count} containers available");
                    nftCount = m_View.m_InstantiatedContainersPublic.Count;
                }
                
                var nftService = new NFTService(m_WalletService.chainSettings.nftContractAddress, m_WalletService.chainSettings.rpcUrl);
                
                for (int i = 0; i < nftCount; i++)
                {
                    try
                    {
                        Sprite sprite = await nftService.LoadNFTImage(ownedTokenIds[i]);
                        if (sprite != null)
                        {
                            m_View.SetNFTImage(i, sprite);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load sprite for token {ownedTokenIds[i]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to load NFT image for token {ownedTokenIds[i]}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading NFT images: {ex.Message}");
            }
        }

        public void Show()
        {
            m_View.Show();
        }

        public void Hide()
        {
            m_View.Hide();
        }

        private void OnHideWalletButtonClicked()
        {
            // Hide wallet info container
            m_AudioUiService.PlaySelectSound();
            m_View.HideWalletInfo();
        }

        private void OnDisconnectWalletButtonClicked()
        {
            // Disconnect wallet and hide wallet info container
            m_AudioUiService.PlaySelectSound();
            
            // Disable open wallet button during disconnection process
            m_View.SetOpenWalletButtonInteractability(false);
            
            m_WalletService.Disconnect();
            m_View.HideWalletInfo();
        }

        private void OnModalStateChanged(bool isOpen)
        {
            m_View.SetConnectButtonInteractability(!isOpen);
            m_View.SetOpenWalletButtonInteractability(!isOpen);
        }
    }
}