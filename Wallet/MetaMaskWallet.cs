using System;
using AnotherWorld.UI.Wallet;
using Cysharp.Threading.Tasks;
using GDT;
using Newtonsoft.Json;

namespace AnotherWorld.Wallet
{
    public class MetaMaskWallet : BaseWallet
    {
        public override async UniTask<string> Sign(WalletType type)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                Web3Wallet._cancellationTokenSource?.Cancel();
                WaitForTransfer().Forget();

                // set message
                var message =
                    UI_Localize.GetLanguage(Managers.TableMgr.CommonGDT.GetConnectWallet(type)?
                        .SignatureMsg);
                // sign message
                var signature = await (Web3Wallet.Sign(message));

                // reject
                if (string.IsNullOrEmpty(signature))
                {
                    return string.Empty;
                }

                // verify account
                var address = await EVM.Verify(message, signature);

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
                // get current timestamp
                var timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
                // set expiration time
                var expirationTime = timestamp + 3600;
                var address = await Sign(type);

                if (string.IsNullOrEmpty(address))
                    return WalletConnectInfo.Exception();

                var now = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
                // validate
                if (address.Length == 42 && expirationTime >= now)
                {
                    // NOTE(Justin): 같은 지갑에서 다른 아이디로 로그인할 수도 있으므로...
                    if (isNewConnect
                        && string.IsNullOrEmpty(CurrentWalletAddress))
                    {
                        var receivedCallback = false;
                        Managers.NetworkMgr.GameClient.WalletConnectAction = (success, walletType, addr) =>
                        {
                            result = new() { Result = success, Address = addr, Type = walletType, };

                            receivedCallback = true;
                        };

                        Managers.NetworkMgr.GameClient.Proxy.Req_BlockChain_WalletConnect(
                            (byte)WalletType.WT_MetaMask,
                            address
                        );

                        await UniTask.WaitWhile(() => false == receivedCallback);
                    }
                    else
                    {
                        var receivedCallback = false;

                        Managers.NetworkMgr.GameClient.WalletChangeAction = (success, walletType, addr) =>
                        {
                            result = new() { Result = success, Address = addr, Type = walletType, };

                            receivedCallback = true;
                        };

                        Managers.NetworkMgr.GameClient.Proxy.Req_BlockChain_WalletChange(
                            (byte)WalletType.WT_MetaMask,
                            address
                        );

                        await UniTask.WaitWhile(() => false == receivedCallback);
                    }
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
            SetAddress(string.Empty, false, WalletType.WT_None);
            IsSignedWallet = false;
            return true;
        }

        /// <summary>
        /// 전송 전 사전 체크
        /// </summary>
        /// <returns></returns>
        private async UniTask<bool> PreTransfer()
        {
            if (string.IsNullOrEmpty(CurrentWalletAddress))
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(
                    Popup_Notice.POPUP_TITLE,
                    CONNECT_WALLET_POPUP_CONNECT_SIGN,
                    () => Connect(CurrentWalletType).AttachExternalCancellation(_cancellationTokenSource.Token)
                        .Forget(),
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
                    () => { checkConfirm = true; },
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
            WaitForTransfer().Forget();
            Web3Wallet._cancellationTokenSource?.Cancel();

            return true;
        }

        /// <summary>
        /// AWM/Klay 전송
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public override async UniTask<bool> Transfer(string amount)
        {
            var isSuccess = false;
            try
            {
                if (false == await PreTransfer())
                {
                    return false;
                }

                var response = string.Empty;

                // AWM은 토큰이라 바로 보내지 못하고 거져서 보내는 것이 있다.
                if (CurrencySubType.CST_Awm == _currencyCoinType)
                {
                    var value = "0";

                    var args = JsonConvert.SerializeObject(new string[]
                    {
                        Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress, amount
                    });

                    var data = await EVM.CreateContractData(_coinABI, "transfer", args);
                    response =
                        await Web3Wallet.SendTransaction(
                            _chainId,
                            Managers.TableMgr.CommonGDT.GetCoinInfo(Managers.WalletMgr.CurrentCoinType.Value)
                                .CoinTokenID,
                            value,
                            data
                        );
                }
                // 코인은 바로 보낼 수 있어서 보내는 방식이 다르다.
                else if (CurrencySubType.CST_Klay == _currencyCoinType)
                {
                    var value = amount;

                    response =
                        await Web3Wallet.SendTransaction(
                            _chainId,
                            Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress,
                            value
                        );
                }

                AnotherWorld.Util.Debug.Log($"approve tx hash : {response}");

                if (false == string.IsNullOrEmpty(response))
                {
                    var isConfirm = false;
                    Managers.UIMgr.GetOpenPopup<UI_Swap_Exchange_Popup>()?.ClosePopup();
                    Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                        EXCHANGEGOODS_POPUP_SUCCESS_DEPOSIT_TEXT, () => isConfirm = true, false);

                    await UniTask.WaitWhile(() => false == isConfirm);

                    isSuccess = true;
                }
            }
            catch (Exception)
            {
                var isConfirm = false;

                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT, () => isConfirm = true, false);

                await UniTask.WaitWhile(() => false == isConfirm);
            }
            finally
            {
                AnotherWorld.Util.Debug.Log($"Transfer({_currencyCoinType} End");
                IsTransfer = false;
            }

            return isSuccess;
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
            var response =
                await Web3Wallet.SendTransaction(
                    _chainId,
                    tokenAddress,
                    "0",
                    data,
                    GasLimit,
                    GasPrice
                );

            AnotherWorld.Util.Debug.Log($"{method} tx hash : {response}");

            if (string.IsNullOrEmpty(response))
            {
                return string.Empty;
            }

            return response;
        }

        /// <summary>
        /// ERC721 전송
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async UniTask<bool> Transfer(string tokenAddress, int tokenId)
        {
            var isSuccess = false;
            try
            {
                if (false == await PreTransfer())
                {
                    return false;
                }

                var response = await TransferInternal(
                    _erc721ABI,
                    tokenAddress,
                    "transferFrom", // NOTE(Justin): 전송
                    CurrentWalletAddress, // From
                    Managers.ConfigMgr.AWConfig.GameServer.BlockChainServerAddress, // To
                    tokenId.ToString() // TokenId
                );

                Debug.Log($"transferFrom tx hash : {response}");

                if (string.IsNullOrEmpty(response))
                {
                    throw new System.Exception($"Failed to transferFrom - {tokenAddress}");
                }

                isSuccess = true;
            }
            catch (Exception)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(
                    Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT,
                    false
                );
            }
            finally
            {
                AnotherWorld.Util.Debug.Log("NFT Transfer End");
                IsTransfer = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// ERC1155 전송
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="tokenId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async UniTask<bool> Transfer(string tokenAddress, int tokenId, long amount)
        {
            var isSuccess = false;
            try
            {
                if (false == await PreTransfer())
                {
                    return false;
                }

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
                    throw new Exception($"Failed to safeTransferFrom - {tokenAddress}");
                }

                isSuccess = true;
            }
            catch (Exception)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(
                    Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT,
                    false
                );
            }
            finally
            {
                AnotherWorld.Util.Debug.Log("NFT Transfer End");
                IsTransfer = false;
            }

            return isSuccess;
        }
    }
}
