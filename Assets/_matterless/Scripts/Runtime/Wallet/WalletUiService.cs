using System;
using System.Collections.Generic;
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
        
        // NFT sprite cache - key is token ID, value is sprite (null for video NFTs)
        private readonly Dictionary<string, Sprite> m_NFTSpriteCache = new Dictionary<string, Sprite>();
        
        // NFT name cache - key is token ID, value is name (for video NFTs that can't display images)
        private readonly Dictionary<string, string> m_NFTNameCache = new Dictionary<string, string>();

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
            
            // Re-display cached NFT images
            DisplayCachedNFTImages();
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

            // Clear NFT containers and caches
            m_View.ClearNFTContainers();
            m_NFTSpriteCache.Clear();
            m_NFTNameCache.Clear();

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
                
                // Use ERC-721 service for Floorcraft NFTs
                var nftService = new NFT721Service(m_WalletService.chainSettings.nft721ContractAddress, m_WalletService.chainSettings.rpcUrl);
                
                for (int i = 0; i < ownedTokenIds.Count; i++)
                {
                    try
                    {
                        string tokenId = ownedTokenIds[i];
                        
                        // Check if already cached (either as sprite or name)
                        if (m_NFTSpriteCache.ContainsKey(tokenId) || m_NFTNameCache.ContainsKey(tokenId))
                        {
                            Debug.Log($"Token {tokenId} already cached, skipping download");
                            continue;
                        }
                        
                        // Check if this is a video NFT
                        bool isVideo = await nftService.IsVideoNFT(tokenId);
                        
                        if (isVideo)
                        {
                            // Video NFT - get the name to display as text
                            string nftName = await nftService.GetNFTName(tokenId);
                            m_NFTNameCache[tokenId] = nftName;
                            Debug.Log($"Token {tokenId} is video NFT, cached name: {nftName}");
                        }
                        else
                        {
                            // Image NFT - load the sprite
                            Sprite sprite = await nftService.LoadNFTImage(tokenId);
                            if (sprite != null)
                            {
                                // Store in cache
                                m_NFTSpriteCache[tokenId] = sprite;
                            }
                            else
                            {
                                // Fallback: if image fails to load, get name instead
                                string nftName = await nftService.GetNFTName(tokenId);
                                m_NFTNameCache[tokenId] = nftName;
                                Debug.LogWarning($"Failed to load sprite for token {tokenId}, using name: {nftName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to load NFT data for token {ownedTokenIds[i]}: {ex.Message}");
                    }
                }
                
                // Display cached images/names after download completes
                DisplayCachedNFTImages();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading NFT images: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Display cached NFT images or names in containers
        /// </summary>
        private void DisplayCachedNFTImages()
        {
            try
            {
                var ownedTokenIds = m_WalletService.GetOwnedTokenIds();
                
                if (ownedTokenIds.Count == 0 || m_View.m_InstantiatedContainersPublic.Count == 0)
                {
                    return;
                }
                
                int displayCount = Mathf.Min(ownedTokenIds.Count, m_View.m_InstantiatedContainersPublic.Count);
                
                for (int i = 0; i < displayCount; i++)
                {
                    string tokenId = ownedTokenIds[i];
                    
                    // Check if sprite is cached (image NFT)
                    if (m_NFTSpriteCache.TryGetValue(tokenId, out Sprite sprite))
                    {
                        m_View.SetNFTImage(i, sprite);
                    }
                    // Check if name is cached (video NFT or failed image load)
                    else if (m_NFTNameCache.TryGetValue(tokenId, out string nftName))
                    {
                        m_View.SetNFTText(i, nftName);
                    }
                    else
                    {
                        Debug.LogWarning($"No cached data for token {tokenId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error displaying cached NFT data: {ex.Message}");
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