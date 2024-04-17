using System.Text;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Wallet;
using SolCrashersOnChain.Program;
using SolCrashersOnChain.Errors;
using Cysharp.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.SDK;
using UnityEngine;

namespace SolCrashersOnChain
{
    public partial class SolCrashersOnChainClient : TransactionalBaseClient<SolCrashersOnChainErrorKind>
    {
        private static readonly PublicKey PROGRAM_ID = new("4LaDUjM73BdtSxjAH15rJmGv4aLYGHSMRtXDK6BWhhyq");
        private static SolCrashersOnChainClient _clientInstance = null;
        private static SolCrashersOnChainClient _client =>
            _clientInstance ??= new SolCrashersOnChainClient(
                Web3.Rpc,
                Web3.WsRpc,
                PROGRAM_ID
            );

        public static bool DeriveMintAndTokenAccountsFor(string seed, out PublicKey mint, out PublicKey token)
        {
            if(!PublicKey.TryFindProgramAddress(
                   new []{ Encoding.UTF8.GetBytes(seed), Web3.Wallet.Account.PublicKey.KeyBytes}, 
                   PROGRAM_ID, 
                   out PublicKey _token, out _))
            {
                Debug.LogError("Failed TokenAccount derivation.");
                token = null;
                mint = null;
                return false;
            }

            if (!PublicKey.TryFindProgramAddress(
                    new[] { Encoding.UTF8.GetBytes("mint"), Encoding.UTF8.GetBytes(seed)},
                    PROGRAM_ID,
                    out PublicKey _mint, out _))
            {
                Debug.LogError("Failed MintAccount derivation.");
                token = null;
                mint = null;
                return false;
            }

            token = _token;
            mint = _mint;
            return true;
        }

        public static void PrintGold(uint amount)
        {
            _client.SendPrintGoldAsync(amount).Forget();
        }
        
        public static void PrintGems(uint amount)
        {
            _client.SendPrintGemsAsync(amount).Forget();
        }

        private async UniTask SendPrintGoldAsync(uint amount)
        {
            if (!DeriveMintAndTokenAccountsFor("gold", out PublicKey mint, out PublicKey dstAta))
                return;

            var accts = new PrintGoldAccounts
            {
                DstAta = dstAta,
                Payer = Web3.Wallet.Account.PublicKey,
                Mint = mint,
                AssociatedTokenProgram = AssociatedTokenAccountProgram.ProgramIdKey,
                SystemProgram = SystemProgram.ProgramIdKey,
                TokenProgram = TokenProgram.ProgramIdKey,
                Rent = SysVars.RentKey
            };
            
            var result = await SendPrintGoldAsync(
                accts,
                amount,
                Web3.Wallet.Account.PublicKey,
                (bytes, key) => Web3.Wallet.Account.Sign(bytes),
                ProgramIdKey
            );
            
            Debug.Log(result.Result);
            Debug.Log(result.Reason);
        }
        
        private async UniTask SendPrintGemsAsync(uint amount)
        {
            if (!DeriveMintAndTokenAccountsFor("gems", out PublicKey mint, out PublicKey dstAta))
                return;
            
            var accts = new PrintGemsAccounts()
            {
                DstAta = dstAta,
                Payer = Web3.Wallet.Account.PublicKey,
                Mint = mint,
                SystemProgram = SystemProgram.ProgramIdKey,
                TokenProgram = TokenProgram.ProgramIdKey,
                Rent = SysVars.RentKey
            };

            var result = await SendPrintGemsAsync(
                accts,
                amount,
                Web3.Wallet.Account.PublicKey,
                (bytes, key) => Web3.Wallet.Account.Sign(bytes),
                ProgramIdKey
            );
            
            Debug.Log(result.Result);
            Debug.Log(result.Reason);
        }

    }
}