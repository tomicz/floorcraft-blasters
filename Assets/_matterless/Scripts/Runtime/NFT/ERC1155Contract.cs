using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using UnityEngine;

namespace Matterless.Floorcraft
{
    /// <summary>
    /// Low-level wrapper for ERC-1155 smart contract calls.
    /// Pure contract interface - no caching, no game logic.
    /// Reusable for any ERC-1155 contract on any EVM chain.
    /// </summary>
    public class ERC1155Contract
    {
        private readonly string m_ContractAddress;
        private readonly string m_RpcUrl;
        private readonly Web3 m_Web3;
        private readonly Contract m_Contract;
        
        public string ContractAddress => m_ContractAddress;
        public string RpcUrl => m_RpcUrl;
        
        /// <summary>
        /// Initialize ERC-1155 contract wrapper
        /// </summary>
        /// <param name="contractAddress">ERC-1155 contract address</param>
        /// <param name="rpcUrl">Blockchain RPC endpoint</param>
        public ERC1155Contract(string contractAddress, string rpcUrl)
        {
            m_ContractAddress = contractAddress;
            m_RpcUrl = rpcUrl;
            
            // Initialize Nethereum
            m_Web3 = new Web3(m_RpcUrl);
            m_Contract = m_Web3.Eth.GetContract(ERC1155ABI.JSON, m_ContractAddress);
            
            Debug.Log($"[ERC1155Contract] Initialized: {m_ContractAddress}");
        }
        
        /// <summary>
        /// Get the balance of a specific token for an address
        /// </summary>
        /// <param name="account">Wallet address</param>
        /// <param name="tokenId">Token ID</param>
        /// <returns>Balance of the token</returns>
        public async Task<BigInteger> BalanceOf(string account, BigInteger tokenId)
        {
            try
            {
                var balanceFunction = m_Contract.GetFunction("balanceOf");
                var balance = await balanceFunction.CallAsync<BigInteger>(account, tokenId);
                return balance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERC1155Contract] BalanceOf failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get the URI for a specific token
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        /// <returns>Token URI</returns>
        public async Task<string> URI(BigInteger tokenId)
        {
            try
            {
                var function = m_Contract.GetFunction("uri");
                var uri = await function.CallAsync<string>(tokenId);
                return uri;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERC1155Contract] URI failed for token {tokenId}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get balances for multiple tokens (batch query)
        /// </summary>
        /// <param name="accounts">Array of wallet addresses</param>
        /// <param name="tokenIds">Array of token IDs to check</param>
        /// <returns>Array of balances</returns>
        public async Task<BigInteger[]> BalanceOfBatch(string[] accounts, BigInteger[] tokenIds)
        {
            try
            {
                var function = m_Contract.GetFunction("balanceOfBatch");
                var balances = await function.CallAsync<BigInteger[]>(accounts, tokenIds);
                return balances;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERC1155Contract] BalanceOfBatch failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check if an account owns any tokens (useful for vehicle unlock)
        /// </summary>
        /// <param name="account">Wallet address</param>
        /// <param name="maxTokenId">Maximum token ID to check (e.g., 1000)</param>
        /// <returns>True if account owns any tokens</returns>
        public async Task<bool> OwnsAnyTokens(string account, int maxTokenId = 1000)
        {
            try
            {
                // Check first 1000 token IDs (adjust as needed)
                var tokenIds = new BigInteger[maxTokenId];
                var accounts = new string[maxTokenId];
                
                for (int i = 0; i < maxTokenId; i++)
                {
                    tokenIds[i] = i + 1; // Token IDs usually start from 1
                    accounts[i] = account;
                }
                
                var balances = await BalanceOfBatch(accounts, tokenIds);
                
                foreach (var balance in balances)
                {
                    if (balance > 0)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERC1155Contract] OwnsAnyTokens failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get all token IDs owned by an account (up to maxTokenId)
        /// </summary>
        /// <param name="account">Wallet address</param>
        /// <param name="maxTokenId">Maximum token ID to check</param>
        /// <returns>List of owned token IDs</returns>
        public async Task<List<string>> GetOwnedTokenIds(string account, int maxTokenId = 1000)
        {
            var ownedTokens = new List<string>();
            
            try
            {
                var tokenIds = new BigInteger[maxTokenId];
                var accounts = new string[maxTokenId];
                
                for (int i = 0; i < maxTokenId; i++)
                {
                    tokenIds[i] = i + 1; // Token IDs usually start from 1
                    accounts[i] = account;
                }
                
                var balances = await BalanceOfBatch(accounts, tokenIds);
                
                for (int i = 0; i < balances.Length; i++)
                {
                    if (balances[i] > 0)
                    {
                        ownedTokens.Add((i + 1).ToString());
                    }
                }
                
                return ownedTokens;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERC1155Contract] GetOwnedTokenIds failed: {ex.Message}");
                return ownedTokens;
            }
        }
    }
}
