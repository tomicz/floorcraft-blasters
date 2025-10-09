using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Matterless.Floorcraft
{
    public class WalletUiView : UIView<WalletUiView>
    {
        public event Action onConnectWalletButtonClicked;
        public event Action onDisconnectWalletButtonClicked;

        [SerializeField] private Button m_ConnectWalletButton;
        [SerializeField] private Button m_DisconnectWalletButton;
        [SerializeField] private TMP_Text m_WalletAddressText;

        public override WalletUiView Init()
        {
            AddListeners();
            return this;
        }

        private void AddListeners()
        {
            m_ConnectWalletButton.onClick.AddListener(() => onConnectWalletButtonClicked?.Invoke());
            m_DisconnectWalletButton.onClick.AddListener(() => onDisconnectWalletButtonClicked?.Invoke());
        }

        public void SetConnectButtonVisibility(bool isVisible)
        {
            m_ConnectWalletButton.gameObject.SetActive(isVisible);
        }

        public void SetDisconnectButtonVisibility(bool isVisible)
        {
            m_DisconnectWalletButton.gameObject.SetActive(isVisible);
        }

        public void SetConnectButtonInteractability(bool isInteractable)
        {
            m_ConnectWalletButton.interactable = isInteractable;
        }

        public void SetDisconnectButtonInteractability(bool isInteractable)
        {
            m_DisconnectWalletButton.interactable = isInteractable;
        }

        public void SetWalletAddressText(string walletAddress)
        {
            m_WalletAddressText.text = FormatWalletAddress(walletAddress);
        }

        private string FormatWalletAddress(string address)
        {
            if (string.IsNullOrEmpty(address) || address.Length < 10)
                return address;

            // Format as 0x1234...5678 (4 characters after 0x, then last 4 characters)
            return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
        }

        public void ShowWalletInfo(){
            m_WalletAddressText.gameObject.SetActive(true);
        }

        public void HideWalletInfo(){
            m_WalletAddressText.gameObject.SetActive(false);
        }
    }
}