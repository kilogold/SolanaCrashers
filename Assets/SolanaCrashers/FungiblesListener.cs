using System;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitDemo
{
    public class FungiblesListener : MonoBehaviour
    {
        private const string ATA = "8xsmabp5pGT55MsAYMtc333aesDQmL7jG426n99MFjmS";
        public struct Fungibles
        {
            public uint gameCurrency;
            public uint premiumCurrency;
        }
        public static event Action<Fungibles> FungiblesUpdated;
        private SubscriptionState subscription;
        
        async void Awake()
        {
            SaveManager.GameDataLoaded += SaveManagerOnGameDataLoaded;
            
            subscription = await Web3.WsRpc.SubscribeTokenAccountAsync(ATA, (state, value) =>
            {
                var amount = (uint)value.Value.Data.Parsed.Info.TokenAmount.AmountUlong;
                
                Debug.Log("On-chain token balance changed to: " + amount);
                
                TriggerFungiblesUpdate(new Fungibles
                {
                    gameCurrency = amount,
                    premiumCurrency = 0
                });
            }, Commitment.Confirmed);
        }
        
        void OnDestroy()
        {
            SaveManager.GameDataLoaded -= SaveManagerOnGameDataLoaded;
            Web3.WsRpc.Unsubscribe(subscription);
        }

        void TriggerFungiblesUpdate(Fungibles funge)
        {
            FungiblesUpdated?.Invoke(funge);
        }
        
        private async void SaveManagerOnGameDataLoaded(GameData obj)
        {
            var output = await Web3.Rpc.GetTokenAccountBalanceAsync(ATA);
            if (!output.WasSuccessful)
            {
                Debug.LogError(output.Reason);
                return;
            }

            Debug.Log("On-chain token balance loaded: " + output.Result.Value.Amount);

            TriggerFungiblesUpdate(new Fungibles
            {
                gameCurrency = (uint)output.Result.Value.AmountUlong,
                premiumCurrency = 0
            });
        }
    }
}