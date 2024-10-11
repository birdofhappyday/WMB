using System;
using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Wallet;
using Cysharp.Threading.Tasks;
using GDT;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Evm;

namespace AnotherWorld.Wallet
{
    using WalletConnectUnity.Core.Networking;

    public class OKXWallet : BaseWallet
    {
        private string _gasLimitToHex;
        
        /// <summary>
        /// 지갑에 대한 정보를 얻어오는 부분
        /// </summary>
        /// <param name="walletEnum"></param>
        /// <returns></returns>
        private Wallet GetWalletObject(WalletType walletEnum)
        {
            Wallet result = null;

            switch (walletEnum)
            {
                case WalletType.WT_OKX:
                    result = new Wallet()
                    {
                        Id = "971e689d0a5be527bac79629b4ee9b925e82208e5168b733496a09c0faed0709",
                        Name = "OKX Wallet",
                        Homepage = "https://www.okx.com/web3",
                        ImageId = "45f2f08e-fc0c-4d62-3e63-404e72170500",
                        WebappLink = "https://www.okx.com/download",
                        MobileLink = "okex://main",
                        AppStore = "https://apps.apple.com/us/app/okx-buy-bitcoin-eth-crypto/id1327268470",
                        PlayStore = "https://play.google.com/store/apps/details?id=com.okinc.okex.gp"
                    };
                    break;

                case WalletType.WT_MetaMask:
                    result = new Wallet()
                    {
                        Id = "c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96",
                        Name = "MetaMask",
                        Homepage = "https://metamask.io/",
                        ImageId = "5195e9db-94d8-4579-6f11-ef553be95100",
                        WebappLink = "",
                        MobileLink = "metamask://",
                        AppStore = "https://apps.apple.com/us/app/metamask/id1438144202",
                        PlayStore = "https://play.google.com/store/apps/details?id=io.metamask"
                    };
                    break;
            }

            return result;
        }

        public override async UniTask<bool> Initialize(int gasLimit, int gasPrice, string coinABI, string erc721ABI, string erc1155ABI)
        {
            if (false == await base.Initialize(gasLimit, gasPrice, coinABI, erc721ABI, erc1155ABI))
            {
                return false;
            }
            
            _gasLimitToHex = $"0x{gasLimit:X}";

            if (WalletConnectUnity.Core.WalletConnect.Instance.IsInitialized)
            {
                AnotherWorld.Util.Debug.Log($"WalletConnect already initialized");
                return true;
            }

            var wallet = await WalletConnectUnity.Core.WalletConnect.Instance.InitializeAsync();
            if (null == wallet)
            {
                AnotherWorld.Util.Debug.LogError($"Failed to initialize a WalletConnect");
                return false;
            }

            WalletConnectUnity.Core.WalletConnect.Instance.SessionConnected += (_, @struct) =>
            {
                if (string.IsNullOrEmpty(@struct.Topic))
                    return;

                AnotherWorld.Util.Debug.Log($"Session connected. Topic: {@struct.Topic}");
            };

            // Invoked after wallet connected
            WalletConnectUnity.Core.WalletConnect.Instance.ActiveSessionChanged += (_, @struct) =>
            {
                if (string.IsNullOrEmpty(@struct.Topic))
                    return;

                AnotherWorld.Util.Debug.Log($"Active session changed. Topic: {@struct.Topic}");
            };

            // Invoked after wallet disconnected
            WalletConnectUnity.Core.WalletConnect.Instance.SessionDisconnected += (_, _) =>
            {
                AnotherWorld.Util.Debug.Log($" Session disconnected.");
            };

            return true;
        }

        public override async UniTask<string> Sign(WalletType type)
        {
            if (false == WalletConnectUnity.Core.WalletConnect.Instance.IsInitialized)
            {
                AnotherWorld.Util.Debug.LogError($"Failed to sign - initialize first");
                // return null;
            }

            try
            {
                var optionalNamespaces = new Dictionary<string, ProposedNamespace>();

                var methods = new[]
                {
                    "wallet_switchEthereumChain", "wallet_addEthereumChain", "eth_sendTransaction", "personal_sign"
                };

                var events = new[] { "chainChanged", "accountsChanged", "connect", "disconnect" };

                var chain = $"{ChainConstants.Namespaces.Evm}:{_chainId}";
                AnotherWorld.Util.Debug.Log($"Chain - {chain}");
                optionalNamespaces.Add(ChainConstants.Namespaces.Evm,
                    new ProposedNamespace
                    {
                        Chains = new[] { chain },
                        Events = events,
                        Methods = methods
                    });

                var connectOptions = new ConnectOptions { OptionalNamespaces = optionalNamespaces };

                var connectedData = await WalletConnectUnity.Core.WalletConnect.Instance.ConnectAsync(connectOptions);
                // PC에서는 딥링크를 사용할 수가 없다.
                Linker.OpenSessionProposalDeepLink(connectedData.Uri, GetWalletObject(type));
                
                await connectedData.Approval;

                var session = WalletConnectUnity.Core.WalletConnect.Instance.ActiveSession;
                var sessionNamespace = session.Namespaces;

                var address = WalletConnectUnity.Core.WalletConnect.Instance.ActiveSession
                    .CurrentAddress(sessionNamespace.Keys.FirstOrDefault())
                    .Address;

                AnotherWorld.Util.Debug.Log($"result address - {address}");
                // IsSignedWallet = (false == string.IsNullOrEmpty(address));
                if (SignCheck(address, type))
                {
                    IsSignedWallet = true;
                    CurrentWalletType = type;
                }
                else
                {
                    address = string.Empty;
                }

                return address;
            }
            catch (Exception e)
            {
                AnotherWorld.Util.Debug.LogError($"Failed to sign - {e.Message}");
                throw;
            }
        }

        public override async UniTask<WalletConnectInfo> Connect(WalletType type, bool isNewConnect = true)
        {
            WalletConnectInfo result = null;

            try
            {
                var address = await Sign(type);
                if (string.IsNullOrEmpty(address))
                {
                    throw new System.Exception($"address is null or empty");
                }

                // NOTE(Justin): 같은 지갑에서 다른 아이디로 로그인할 수도 있으므로...
                if (isNewConnect
                    && string.IsNullOrEmpty(CurrentWalletAddress))
                {
                    var receivedCallback = false;

                    Managers.NetworkMgr.GameClient.Proxy.Req_BlockChain_WalletConnect((byte)type, address);
                    Managers.NetworkMgr.GameClient.WalletConnectAction = (success, walletType, addr) =>
                    {
                        result = new()
                        {
                            Result = success,
                            Address = addr,
                            Type = walletType,
                        };

                        receivedCallback = true;
                    };

                    await UniTask.WaitWhile(() => false == receivedCallback);
                }
                else
                {
                    var receivedCallback = false;

                    Managers.NetworkMgr.GameClient.WalletChangeAction = (success, walletType, addr) =>
                    {
                        result = new()
                        {
                            Result = success,
                            Address = addr,
                            Type = walletType,
                        };

                        receivedCallback = true;
                    };
                    Managers.NetworkMgr.GameClient.Proxy.Req_BlockChain_WalletChange((byte)type, address);

                    await UniTask.WaitWhile(() => false == receivedCallback);
                }
            }
            catch (Exception)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                    Wallet_Connection_List.CONNECT_WALLET_POPUP_FAIL_DESC_TEXT, false);

                result = WalletConnectInfo.Empty();
            }

            return result;
        }

        public override async UniTask<bool> Disconnect()
        {
            if (WalletType.WT_OKX == CurrentWalletType)
            {
                try
                {
                    await WalletConnectUnity.Core.WalletConnect.Instance.DisconnectAsync();
                    SetAddress(string.Empty, false, WalletType.WT_None);
                    IsSignedWallet = false;
                }
                catch (Exception e)
                {
                    AnotherWorld.Util.Debug.LogError(e.Message);
                    return false;
                }
            }

            return true;
        }

        public override async UniTask<bool> Transfer(string amount)
        {
            if (false == await PreTransfer())
            {
                return false;
            }

            EthSendTransaction request = null;
            // awm 보내는 방식
            // awm은 토큰으로 data를 만들어주어야 한다.
            // WalletConnect SDK는 다른 방식으로 data를 만드는데 현재 플러그인 충동로 metamask용으로 사용중
            // 추후 SDK를 바꾸게 되면 data방식이 바뀔 예정이다.
            if (CurrencySubType.CST_Awm == _currencyCoinType)
            {
                var args = JsonConvert.SerializeObject(new string[]
                {
                    Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress, amount
                });
                var data = await EVM.CreateContractData(_coinABI, "transfer", args);

                request = new EthSendTransaction(new Transaction
                {
                    from = CurrentWalletAddress,
                    to = Managers.TableMgr.CommonGDT.GetCoinInfo(Managers.WalletMgr.CurrentCoinType.Value).CoinTokenID,
                    value = "0",
                    data = data,
                    // gaslimit 16진수
                    gas = _gasLimitToHex,//"0x30D40",
                });
            }
            // klay 보내는 방식
            else if (CurrencySubType.CST_Klay == _currencyCoinType)
            {
                request = new EthSendTransaction(new Transaction
                {
                    from = CurrentWalletAddress,
                    to = Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress,
                    value = amount,
                    gas = _gasLimitToHex,//"0x30D40",
                });
            }

            try
            {
                var result =
                    await WalletConnectUnity.Core.WalletConnect.Instance
                        .RequestAsync<EthSendTransaction, string>(request);

                if (false == string.IsNullOrEmpty(result))
                {
                    var isConfirm = false;
                    Managers.UIMgr.GetOpenPopup<UI_Swap_Exchange_Popup>()?.ClosePopup();
                    Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                        EXCHANGEGOODS_POPUP_SUCCESS_DEPOSIT_TEXT, () => isConfirm = true, false);

                    await UniTask.WaitWhile(() => false == isConfirm);
                }
            }
            catch (Exception e)
            {
                var isConfirm = false;

                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT, () => isConfirm = true, false);

                await UniTask.WaitWhile(() => false == isConfirm);
            }

            return true;
        }

        private async UniTask<bool> PreTransfer()
        {
            if (string.IsNullOrEmpty(CurrentWalletAddress))
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(
                    Popup_Notice.POPUP_TITLE,
                    CONNECT_WALLET_POPUP_CONNECT_SIGN,
                    () => Connect(CurrentWalletType).AttachExternalCancellation(_cancellationTokenSource.Token).Forget(),
                    false
                );

                return false;
            }

            if (false == IsSignedWallet)
            {
                var checkConfirm = false;
                
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(
                    Popup_Notice.POPUP_TITLE,
                    CONNECT_WALLET_POPUP_CONNECT_SIGN,
                    () =>
                    {
                        checkConfirm = true;
                    },
                    false
                );

                await UniTask.WaitWhile(() => false == checkConfirm);

                var address = await Sign(CurrentWalletType);
                if (string.IsNullOrEmpty(address))
                {
                    return false;
                }
            }
            
            if (IsTransfer)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_EXIST_TEXT, false);
                return false;
            }

            IsTransfer = true;
            _cancellationTokenSource?.Cancel();
            Web3Wallet._cancellationTokenSource?.Cancel();
            WaitForTransfer().Forget();

            return true;
        }

        private async UniTask<string> TransferInternal(
            string abi,
            string tokenAddress,
            string method,
            params string[] argsList
        )
        {
            var args = JsonConvert.SerializeObject(argsList);
            var data = await EVM.CreateContractData(abi, method, args);
            
            AnotherWorld.Util.Debug.Log(data);

            var request = new EthSendTransaction(new Transaction
            {
                from = CurrentWalletAddress,
                to = tokenAddress,
                value = "0",
                data = data,
                // gaslimit 16진수
                gas = _gasLimitToHex,// 200000 = "0x30D40",
            });

            try
            {
                var response = await WalletConnectUnity.Core.WalletConnect.Instance
                    .RequestAsync<EthSendTransaction, string>(request);

                AnotherWorld.Util.Debug.Log($"Transaction({method}) Success: {response}");

                return response;
            }
            catch (WalletConnectException e)
            {
                AnotherWorld.Util.Debug.LogError(
                    $"Transaction({method}) Error({e.CodeType}): {e.Message}\ntokenAddr: {tokenAddress}, args: {args}, gas: {GasLimit}({_gasLimitToHex})");
                throw;
            }
        }

        public override async UniTask<bool> Transfer(string tokenAddress, int tokenId)
        {
            if (false == await PreTransfer())
            {
                return false;
            }

            var success = false;
            try
            {
                var response = await TransferInternal(
                    _erc721ABI,
                    tokenAddress,
                    "transferFrom",
                    CurrentWalletAddress, // From
                    Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress, // To
                    tokenId.ToString() // TokenId
                );

                if (string.IsNullOrEmpty(response))
                {
                    // return false;
                    throw new Exception(
                        $"Failed to send transaction to transferFrom - tokenAddr: {tokenAddress}, tokenId: {tokenId}");
                }

                success = true;
            }
            catch
            {
                success = false;
            }
            finally
            {
                IsTransfer = false;
            }

            return success;
        }

        public override async UniTask<bool> Transfer(string tokenAddress, int tokenId, long amount)
        {
            if (false == await PreTransfer())
            {
                return false;
            }

            var success = false;
            try
            {
                var response = await TransferInternal(
                    _erc1155ABI,
                    tokenAddress,
                    "safeTransferFrom",
                    CurrentWalletAddress, // NOTE(Justin): From
                    Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress, // NOTE(Justin): To
                    tokenId < 1 ? "0" : tokenId.ToString(), // NOTE(Justin): TokenId
                    amount.ToString(), // NOTE(Justin): amount
                    "0x00" // NOTE(Justin): data (빈 값으로 보냄 from Joy)
                );

                if (string.IsNullOrEmpty(response))
                {
                    // return false;
                    throw new Exception(
                        $"Failed to send transaction to safeTransferFrom - tokenAddr: {tokenAddress}, tokenId: {tokenId}, amount: {amount}");
                }

                success = true;
            }
            catch
            {
                success = false;
            }
            finally
            {
                IsTransfer = false;
            }

            return success;
        }
    }
}
