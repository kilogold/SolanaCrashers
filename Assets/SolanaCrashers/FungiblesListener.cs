using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using SolCrashersOnChain;
using SolCrashersOnChain.Program;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitDemo
{
    public class FungiblesListener : MonoBehaviour
    {
        private const string goldATA = "6d8tL1FJPcJ8mn4wekbiQQzSV2rMGYSY7ab3xHMR4Kt9";
        private const string gemsATA = "DXqGMepER7SeJmsfPgkb7eoewQQZXfbgHGB7ka9ZHJy1";
        public struct Fungibles
        {
            public uint gameCurrency;
            public uint premiumCurrency;

            public override string ToString()
            {
                return $"Gold: {gameCurrency}, Gems: {premiumCurrency}";
            }
        }
        public static event Action<Fungibles> FungiblesUpdated;
        private Fungibles subscriptionCache;
        private SubscriptionState subscriptionGold;
        private SubscriptionState subscriptionGems;
        
        async void Awake()
        {
            subscriptionGold = await Web3.WsRpc.SubscribeTokenAccountAsync(goldATA, (state, value) =>
            {
                subscriptionCache.gameCurrency = (uint)value.Value.Data.Parsed.Info.TokenAmount.AmountUlong;
                Debug.Log("On-chain token GOLD balance changed to: " + subscriptionCache.gameCurrency);
                TriggerFungiblesUpdate(subscriptionCache);
            }, Commitment.Confirmed);
            
            subscriptionGems = await Web3.WsRpc.SubscribeTokenAccountAsync(gemsATA, (state, value) =>
            {
                subscriptionCache.premiumCurrency = (uint)value.Value.Data.Parsed.Info.TokenAmount.AmountUlong;
                Debug.Log("On-chain token GEMS balance changed to: " + subscriptionCache.gameCurrency);
                TriggerFungiblesUpdate(subscriptionCache);
            }, Commitment.Confirmed);

            SaveManager.GameDataLoaded += async _ =>
            {
                var output = await Web3.Rpc.GetTokenAccountBalanceAsync(goldATA);
                if (!output.WasSuccessful)
                {
                    Debug.LogError(output.Reason);
                }

                subscriptionCache.gameCurrency = (uint)output.Result.Value.AmountUlong;
            
                output = await Web3.Rpc.GetTokenAccountBalanceAsync(gemsATA);
                if (!output.WasSuccessful)
                {
                    Debug.LogError(output.Reason);
                }

                subscriptionCache.premiumCurrency = (uint)output.Result.Value.AmountUlong;

                Debug.Log("On-chain token balance loaded: " + subscriptionCache);

                TriggerFungiblesUpdate(subscriptionCache);
            };
        }
        
        void OnDestroy()
        {
            //Web3.WsRpc.Unsubscribe(subscriptionGems);
            //Web3.WsRpc.Unsubscribe(subscriptionGold);
            Web3.WsRpc.DisconnectAsync();
        }

        void TriggerFungiblesUpdate(Fungibles funge)
        {
            FungiblesUpdated?.Invoke(funge);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                var client = new  SolCrashersOnChain.SolCrashersOnChainClient(
                    Web3.Rpc,
                    Web3.WsRpc,
                    new PublicKey("4LaDUjM73BdtSxjAH15rJmGv4aLYGHSMRtXDK6BWhhyq")
                );

                var accts = new PrintGoldAccounts
                {
                    DstAta = new PublicKey("6d8tL1FJPcJ8mn4wekbiQQzSV2rMGYSY7ab3xHMR4Kt9"),
                    Payer = Web3.Wallet.Account.PublicKey,
                    Mint = new PublicKey("7syYaUinTxXedKUQEBU8yVeR8ffhpCJkbDYraPbpardL"),
                    AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey,
                    SystemProgram = SystemProgram.ProgramIdKey,
                    TokenProgram = TokenProgram.ProgramIdKey,
                    Rent = SysVars.RentKey
                };
                
                SendPrintGoldAsync(client, accts).Forget();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                var client = new  SolCrashersOnChain.SolCrashersOnChainClient(
                    Web3.Rpc,
                    Web3.WsRpc,
                    new PublicKey("4LaDUjM73BdtSxjAH15rJmGv4aLYGHSMRtXDK6BWhhyq")
                );

                var accts = new PrintGemsAccounts()
                {
                    DstAta = new PublicKey("DXqGMepER7SeJmsfPgkb7eoewQQZXfbgHGB7ka9ZHJy1"),
                    Payer = Web3.Wallet.Account.PublicKey,
                    Mint = new PublicKey("4krPZJTGHFX1UpYsqz6ndE5vEo54E9qiyTG9ENwqRvmz"),
                    SystemProgram = SystemProgram.ProgramIdKey,
                    TokenProgram = TokenProgram.ProgramIdKey,
                    Rent = SysVars.RentKey
                };
                
                SendPrintGemsAsync(client, accts).Forget();
            }
        }

        private async UniTask SendPrintGoldAsync(SolCrashersOnChainClient client, PrintGoldAccounts accts)
        {
            var result = await client.SendPrintGoldAsync(
                accts,
                45,
                Web3.Wallet.Account.PublicKey,
                (bytes, key) => Web3.Wallet.Account.Sign(bytes),
                client.ProgramIdKey
            );
            
            Debug.Log(result.Result);
            Debug.Log(result.Reason);
        }
        
        private async UniTask SendPrintGemsAsync(SolCrashersOnChainClient client, PrintGemsAccounts accts)
        {
            var result = await client.SendPrintGemsAsync(
                accts,
                12,
                Web3.Wallet.Account.PublicKey,
                (bytes, key) => Web3.Wallet.Account.Sign(bytes),
                client.ProgramIdKey
            );
            
            Debug.Log(result.Result);
            Debug.Log(result.Reason);
        }
    }
}