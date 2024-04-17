using System;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using SolCrashersOnChain;
using UnityEngine;

namespace UIToolkitDemo
{
    public class FungiblesListener : MonoBehaviour
    {
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
            SolCrashersOnChainClient.DeriveMintAndTokenAccountsFor("gold", out _, out PublicKey goldATA);
            SolCrashersOnChainClient.DeriveMintAndTokenAccountsFor("gems", out _, out PublicKey gemsATA);

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

            // Refresh currency when game data gets loaded.
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
            
            // This may be the bugfix that's missing from the UnitySDK.
            Application.quitting += () => Web3.WsRpc.DisconnectAsync();
        }
        
        void OnDestroy()
        {
            //Web3.WsRpc.Unsubscribe(subscriptionGems);
            //Web3.WsRpc.Unsubscribe(subscriptionGold);
            
        }

        void TriggerFungiblesUpdate(Fungibles funge)
        {
            FungiblesUpdated?.Invoke(funge);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SolCrashersOnChainClient.PrintGold(20);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                SolCrashersOnChainClient.PrintGems(1);

            }
        }
    }
}