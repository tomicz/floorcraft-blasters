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
        [SerializeField] private TMP_Text m_WalletBalanceText;

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

        public void SetWalletAddressText(string walletAddress)
        {
            m_WalletAddressText.text = walletAddress;
        }

        public void SetWalletBalanceText(string walletBalance)
        {
            m_WalletBalanceText.text = walletBalance;
        }

        public void ShowWalletInfo(){
            m_WalletAddressText.gameObject.SetActive(true);
            m_WalletBalanceText.gameObject.SetActive(true);
        }

        public void HideWalletInfo(){
            m_WalletAddressText.gameObject.SetActive(false);
            m_WalletBalanceText.gameObject.SetActive(false);
        }
    }
}