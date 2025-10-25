using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Matterless.Floorcraft
{
    /// <summary>
    /// High-level NFT service for Floorcraft Blasters.
    /// Handles game integration and business logic.
    /// Uses ERC1155Contract for blockchain communication.
    /// </summary>
    public class NFTService
    {
        private readonly ERC1155Contract m_Contract;
        
        /// <summary>
        /// Initialize NFT Service
        /// </summary>
        /// <param name="contract">ERC1155Contract instance</param>
        public NFTService(ERC1155Contract contract)
        {
            m_Contract = contract;
        }
        
        /// <summary>
        /// Initialize NFT Service with contract address and RPC URL
        /// </summary>
        /// <param name="contractAddress">Your ERC-1155 contract address (e.g., "0xABC...")</param>
        /// <param name="rpcUrl">RPC endpoint with API key (e.g., "https://base-mainnet.g.alchemy.com/v2/YOUR_KEY")</param>
        public NFTService(string contractAddress, string rpcUrl)
            : this(new ERC1155Contract(contractAddress, rpcUrl))
        {
        }
        
        /// <summary>
        /// Get all NFT token IDs owned by an address
        /// Uses ERC-1155 batch query method
        /// </summary>
        /// <param name="ownerAddress">Wallet address to check</param>
        /// <returns>List of token IDs as strings</returns>
        public async Task<List<string>> GetOwnedTokenIds(string ownerAddress)
        {
            try
            {
                // Use ERC-1155 batch query method
                var ownedTokenIds = await m_Contract.GetOwnedTokenIds(ownerAddress);
                
                return ownedTokenIds;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NFT Service] Error querying NFTs: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Get metadata URI for a specific token
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        /// <returns>Metadata URI (usually IPFS link)</returns>
        public async Task<string> GetTokenURI(string tokenId)
        {
            try
            {
                var uri = await m_Contract.URI(BigInteger.Parse(tokenId));
                return uri;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NFT Service] Error getting tokenURI for {tokenId}: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Check if a specific token is owned by an address
        /// </summary>
        /// <param name="ownerAddress">Wallet address to check</param>
        /// <param name="tokenId">Token ID to verify</param>
        /// <returns>True if owner owns the token</returns>
        public async Task<bool> OwnsToken(string ownerAddress, string tokenId)
        {
            try
            {
                var balance = await m_Contract.BalanceOf(ownerAddress, BigInteger.Parse(tokenId));
                return balance > 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NFT Service] Error checking token ownership: {ex.Message}");
                return false;
            }
        }
    }
}

