using System;
using Matterless.Inject;
using UnityEngine;

namespace Matterless.Floorcraft
{
    public class WalletUiService
    {
        private readonly WalletService m_WalletService;
        private readonly AudioUiService m_AudioUiService;
        private readonly WalletUiView m_View;

        public WalletUiService(
            WalletService walletService,
            AudioUiService audioUiService)
        {
            m_WalletService = walletService;
            m_AudioUiService = audioUiService;
            
            Debug.Log("WalletUiService created");
            // Create the view
            m_View = WalletUiView.Create("UIPrefabs/UIP_WalletView").Init();
            
            // Wire up events
            m_View.onConnectWalletButtonClicked += OnConnectWalletButtonClicked;
            m_View.onDisconnectWalletButtonClicked += OnDisconnectWalletButtonClicked;
            
            // Subscribe to wallet state changes
            m_WalletService.onWalletConnected += OnWalletConnected;
            m_WalletService.onWalletDisconnected += OnWalletDisconnected;
            
            // Show the wallet UI immediately when service is created
            m_View.Show();
            
            // Set initial state (show connect button, hide disconnect button)
            m_View.SetConnectButtonVisibility(true);
            m_View.SetDisconnectButtonVisibility(false);
            m_View.HideWalletInfo();
        }

        private void OnConnectWalletButtonClicked()
        {
            m_WalletService.ConnectWallet();
            m_AudioUiService.PlaySelectSound();
        }

        private void OnDisconnectWalletButtonClicked()
        {
            m_WalletService.DisconnectWallet();
            m_AudioUiService.PlaySelectSound();
        }

        private void OnWalletConnected()
        {
            m_View.SetConnectButtonVisibility(false);
            m_View.SetDisconnectButtonVisibility(true);
            m_View.ShowWalletInfo();
            m_View.SetWalletAddressText(m_WalletService.GetWalletAddress());
            m_View.SetWalletBalanceText(m_WalletService.GetWalletBalance());
        }

        private void OnWalletDisconnected()
        {
            m_View.SetConnectButtonVisibility(true);
            m_View.SetDisconnectButtonVisibility(false);
            m_View.SetWalletAddressText("");
            m_View.SetWalletBalanceText("");
            m_View.HideWalletInfo();
        }

        public void Show()
        {
            m_View.Show();
        }

        public void Hide()
        {
            m_View.Hide();
        }
    }
}