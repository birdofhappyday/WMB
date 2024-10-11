using System.Threading;
using Cysharp.Threading.Tasks;
using GDT;

namespace AnotherWorld.Wallet
{
    public class WalletConnectInfo
    {
        public bool Result;
        public string Address;
        public WalletType Type;
        public bool Error;

        public static WalletConnectInfo Empty()
        {
            WalletConnectInfo info = new();
            info.Result = false;
            info.Address = string.Empty;
            info.Type = WalletType.WT_None;
            info.Error = false;

            return info;
        }

        public static WalletConnectInfo Exception()
        {
            WalletConnectInfo info = new();
            info.Result = false;
            info.Address = string.Empty;
            info.Type = WalletType.WT_None;
            info.Error = true;

            return info;
        }
    }

    public abstract class BaseWallet
    {
        #region Constant

        public const string EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT = "EXCHANGEGOODS_POPUP_SEND_FAIL_DESC_TEXT";
        public const string EXCHANGEGOODS_POPUP_SUCCESS_DEPOSIT_TEXT = "EXCHANGEGOODS_POPUP_SUCCESS_DEPOSIT_TEXT";
        public const string EXCHANGEGOODS_POPUP_SEND_FAIL_EXIST_TEXT = "EXCHANGEGOODS_POPUP_SEND_FAIL_EXIST_TEXT";

        protected const string CONNECT_WALLET_POPUP_CONNECT_SIGN = "CONNECT_WALLET_POPUP_CONNECT_SIGN";

        // 지갑 sign 실패의 경우 띄우는 팝업
        protected const string EXCHANGEGOODS_POPUP_SEND_FAIL_TEXT_02 = "EXCHANGEGOODS_POPUP_SEND_FAIL_TEXT_02";

        private const int Thousand = 1000;

        #endregion

        #region Fields/Properties

        public bool IsTransfer { get; protected set; }

        protected CancellationTokenSource _cancellationTokenSource = new();

        protected string _coinABI;
        protected string _erc721ABI;
        protected string _erc1155ABI;

        protected string _chainId;

        protected CurrencySubType _currencyCoinType;

        public string GasLimit { get; protected set; }
        public string GasPrice { get; protected set; }

        public string CurrentWalletAddress { get; protected set; }

        public bool IsSignedWallet { get; protected set; }

        /// <summary>
        /// NOTE(Justin): 추후 OKX로만 연결할 경우 메타마스크도 OKX 지갑으로 연결할 예정이므로 필요함.
        /// </summary>
        public WalletType CurrentWalletType { get; protected set; }

        #endregion

        #region Default Functions

        public virtual async UniTask<bool> Initialize(int gasLimit, int gasPrice, string coinABI, string erc721ABI,
            string erc1155ABI)
        {
            if (string.IsNullOrEmpty(coinABI)
                || string.IsNullOrEmpty(erc721ABI)
                || string.IsNullOrEmpty(erc1155ABI)
               )
            {
                await UniTask.Yield();
                return false;
            }

            GasLimit = gasLimit.ToString();
            GasPrice = gasPrice == 0 ? string.Empty : gasPrice.ToString();

            _coinABI = coinABI;
            _erc721ABI = erc721ABI;
            _erc1155ABI = erc1155ABI;

            _chainId = Managers.ConfigMgr.AWConfig.GameServer.ChainID;
            IsSignedWallet = false;

            return true;
        }

        public void SetCurrencySubType(CurrencySubType type)
        {
            _currencyCoinType = type;
        }

        /// <summary>
        /// NOTE(Justin): 전송을 기다리기 위한 함수. 잘 정리하면 필요없어질 수도 있을듯?
        /// </summary>
        protected async UniTask WaitForTransfer()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new();

            var waitTime = Managers.TableMgr.CommonGDT.CommonTable.ExchangeDepositCoolTime / Thousand;
            var isCancelled = await UniTask.WaitForSeconds(waitTime, cancellationToken: _cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCancelled == true)
            {
                return;
            }

            IsTransfer = false;

            AnotherWorld.Util.Debug.Log("End of WaitForTransfer");
        }

        public void ResetWaitForTransfer()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            IsTransfer = false;
        }

        /// <summary>
        /// 지갑에 현재 지갑 타입과 주소 연결여부를 세팅하는 부분
        /// </summary>
        /// <param name="walletAddress"></param>
        /// <param name="isConnect"></param>
        /// <param name="walletType"></param>
        public void SetAddress(string walletAddress, bool isConnect, WalletType walletType)
        {
            CurrentWalletAddress = walletAddress;

            // IsSignedWallet = isConnect;

            CurrentWalletType = walletType;

            if (!isConnect)
            {
                return;
            }

            var questCompletes = Managers.QuestMgr.GetQuestComplete(QuestConditionType.QT_WalletConnection);
            foreach (var questComplete in questCompletes)
            {
                if (questComplete.QuestCompletePossibleCheck())
                {
                    Managers.NetworkMgr.GameClient.Proxy.Send_Quest_Complete_WalletConnection(questComplete
                        .GetQuestCompleteID);
                }
            }
        }

        #endregion

        #region Abstract Functions

        public abstract UniTask<string> Sign(WalletType type);

        /// <summary>
        /// 지갑 연결
        /// </summary>
        /// <returns></returns>
        public abstract UniTask<WalletConnectInfo> Connect(WalletType type, bool isNewConnect = true);

        /// <summary>
        /// 지갑 해제
        /// </summary>
        /// <returns></returns>
        public abstract UniTask<bool> Disconnect();

        /// <summary>
        /// 코인 전송
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public abstract UniTask<bool> Transfer(string amount);

        /// <summary>
        /// NFT(ERC721) 전송
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public abstract UniTask<bool> Transfer(string tokenAddress, int tokenId);

        /// <summary>
        /// NFT(ERC1155) 전송
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="tokenId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public abstract UniTask<bool> Transfer(string tokenAddress, int tokenId, long amount);

        #endregion

        #region Check Functions

        /// <summary>
        /// 사인한 지갑의 주소가 현재 지갑 주소와 같은지 체크하는 함수.
        /// true일때는 같고 false 일때는 다르다.
        /// 주소가 없거나 주소가 다를 경우 false를 뱉는다.
        /// 다를 경우 팝업을 띄운다.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected bool SignCheck(string address, WalletType type)
        {
            bool result = true;
            
            if (string.IsNullOrEmpty(address))
            {
                result = false;
            }
            else if (!string.IsNullOrEmpty(CurrentWalletAddress) && type == CurrentWalletType && !CurrentWalletAddress.Equals(address))
            {
                result = false;
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                    EXCHANGEGOODS_POPUP_SEND_FAIL_TEXT_02, false);
            }


            return result;
        }

        #endregion
    }
}
