using UnityEngine;

namespace Matterless.Floorcraft
{
    /// <summary>
    /// NFT blockchain configuration settings.
    /// Configure contract address, RPC endpoint, and API key for blockchain interaction.
    /// Note: Only read operations (no transactions), so mainnet testing is safe and free.
    /// </summary>
    [System.Serializable]
    public class ChainSettings
    {
        [Header("ERC-1155 NFT Contract Configuration (Active)")]
        [Tooltip("ERC-1155 NFT contract address on Base blockchain (Auki Domain NFT)")]
        [SerializeField] private string m_NftContractAddress = "0xc3ff4ce2419b80ad638d1253cd5933045e924155";
        
        [Tooltip("ERC-1155 token ID to check ownership for")]
        [SerializeField] private string m_Nft1155TokenId = "64306102522383898777692056113347394168783302164976809436283082385802875895808";
        
        [Header("ERC-721 NFT Contract Configuration (Secondary)")]
        [Tooltip("ERC-721 NFT contract address on Base blockchain (Floorcraft NFT)")]
        [SerializeField] private string m_Nft721ContractAddress = "0xe664b8B0BE6C4dAeA83C44b77Da6106313728F39";
        
        [Header("Network Configuration")]
        [Tooltip("RPC endpoint URL (e.g., https://base-mainnet.g.alchemy.com/v2/)")]
        [SerializeField] private string m_RpcEndpoint = "https://base-mainnet.g.alchemy.com/v2/";
        
        [Tooltip("Alchemy API key (get from https://alchemy.com)")]
        [SerializeField] private string m_ApiKey = "Get your Alchemy API Key from https://alchemy.com";
        
        // Public accessors
        public string nftContractAddress => m_NftContractAddress;       // ERC-1155 (Active)
        public string nft1155TokenId => m_Nft1155TokenId;               // ERC-1155 token ID
        public string nft721ContractAddress => m_Nft721ContractAddress;  // ERC-721 (Secondary)
        public string rpcEndpoint => m_RpcEndpoint;
        public string apiKey => m_ApiKey;
        public string rpcUrl => $"{m_RpcEndpoint}{m_ApiKey}";
        
        /// <summary>
        /// Check if settings are properly configured (ERC-1155 primary)
        /// </summary>
        public bool IsConfigured()
        {
            if (string.IsNullOrEmpty(m_NftContractAddress) || m_NftContractAddress.StartsWith("Get your"))
            {
                Debug.LogError("[ChainSettings] ERC-1155 NFT contract address is not set!");
                return false;
            }
            
            if (string.IsNullOrEmpty(m_Nft1155TokenId))
            {
                Debug.LogError("[ChainSettings] ERC-1155 token ID is not set!");
                return false;
            }
            
            if (string.IsNullOrEmpty(m_ApiKey) || m_ApiKey.StartsWith("Get your"))
            {
                Debug.LogError("[ChainSettings] Alchemy API key is not set! Get one from https://alchemy.com");
                return false;
            }
            
            if (string.IsNullOrEmpty(m_RpcEndpoint))
            {
                Debug.LogError("[ChainSettings] RPC endpoint is not set!");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if ERC-721 settings are configured (secondary)
        /// </summary>
        public bool IsERC721Configured()
        {
            if (string.IsNullOrEmpty(m_Nft721ContractAddress) || m_Nft721ContractAddress.StartsWith("Get your"))
            {
                return false;
            }
            
            return !string.IsNullOrEmpty(m_ApiKey) && !m_ApiKey.StartsWith("Get your") && !string.IsNullOrEmpty(m_RpcEndpoint);
        }
        
        /// <summary>
        /// Get display info for debugging
        /// </summary>
        public string GetDebugInfo()
        {
            return $"ERC-1155 Contract (Active): {m_NftContractAddress}\n" +
                   $"ERC-1155 Token ID: {m_Nft1155TokenId}\n" +
                   $"ERC-721 Contract (Secondary): {m_Nft721ContractAddress}\n" +
                   $"RPC: {m_RpcEndpoint}***\n" +
                   $"Configured: {IsConfigured()}";
        }
    }
}

