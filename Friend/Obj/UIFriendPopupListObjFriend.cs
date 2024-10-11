using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;
using UnityEngine.Events;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriend : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        TMPro.TextMeshProUGUI m_connectText;

        [SerializeField]
        TMPro.TextMeshProUGUI m_friendPointText;

        [SerializeField]
        protected ButtonBase m_heartSendButton;

        [SerializeField]
        protected ButtonBase m_heartReceiveButton;

        [SerializeField]
        ButtonBase m_DeleteButton;

        int _point;

        public int Point
        {
            get => _point;
            set
            {
                _point = value;
                if (_point != Managers.TableMgr.CommonGDT.CommonTable.FriendLikabilityMax)
                {
                    m_friendPointText.text =
                        string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_LIKABILITY)}",
                            (float)Point / Managers.TableMgr.CommonGDT.CommonTable.FriendLikabilityMax * 100);
                }
                else
                    m_friendPointText.text =
                        UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_TAKELIKABILITY_MAX);
            }
        }

        public void UpdatePoint(int point)
        {
            if (_point != Managers.TableMgr.CommonGDT.CommonTable.FriendLikabilityMax)
            {
                m_friendPointText.text =
                    string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_TAKELIKABILITY)}",
                        (float)point / Managers.TableMgr.CommonGDT.CommonTable.FriendLikabilityMax * 100,
                        (float)(point - _point) / Managers.TableMgr.CommonGDT.CommonTable.FriendLikabilityMax * 100);

                _point = point;
            }
        }

        // 친구를 처음 생성할때
        public override void Init(FriendData friendData)
        {
            base.Init(friendData);

            Point = friendData.LikabilityData.Point;
            ButtonOnclickInit();
            SetSendButtonInteractable(SendPossible(friendData.LikabilityData.SendUnixTime));
            SetReceiveButtonInteractable(ReceivePossible(friendData.LikabilityData.ReceiveUnixTime,
                friendData.LikabilityData.RewardUnixTime));
            m_connectText.text =
                CurrentConnectJudge(friendData.BaseData.LoginUnixTime, friendData.BaseData.LogoutUnixTime);

            _isNetSend = false;
            _isNetRecevie = false;

            //레드닷 처리
            var redDot = GetComponentInChildren<NewRedDot>();
            if (null != redDot)
            {
                redDot.SetRedDot(GDT.ReddotMainType.RMT_Friend, GDT.ReddotSubType.RST_NewFriend, friendData.BaseData.CUID);
            }

            Managers.FriendMgr.friendIsDayOff += PointDataUpdate;
        }

        // 온 오프 알림
        public override void Init(FriendBaseData friendBaseData)
        {
            base.Init(friendBaseData);
            FriendBaseDataInit(friendBaseData);

            m_connectText.text = CurrentConnectJudge(friendBaseData.LoginUnixTime, friendBaseData.LogoutUnixTime);
        }

        public override void Refresh()
        {
            base.Refresh();
            Point = FriendData.LikabilityData.Point;
            SendPossible(FriendData.LikabilityData.SendUnixTime);
            ReceivePossible(FriendData.LikabilityData.ReceiveUnixTime,
                FriendData.LikabilityData.RewardUnixTime);

            if (FriendBaseData != null)
                m_connectText.text = CurrentConnectJudge(FriendBaseData.LoginUnixTime, FriendBaseData.LogoutUnixTime);
        }

        // 호감도 포인트
        public void PointDataUpdate(FriendPointData friendPointData)
        {
            if (friendPointData == null)
            {
                Debug.LogError("ksmksmksm 포인트 처리 안 들어올때 들어옴");
                return;
            }

            if (Point != friendPointData.LikabilityData.Point)
            {
                UpdatePoint(friendPointData.LikabilityData.Point);
            }
            // else
            // {
            //     Point = friendPointData.LikabilityData.Point;
            // }

            SetSendButtonInteractable(SendPossible(friendPointData.LikabilityData.SendUnixTime));
            SetReceiveButtonInteractable(ReceivePossible(friendPointData.LikabilityData.ReceiveUnixTime,
                friendPointData.LikabilityData.RewardUnixTime));
            FriendData.LikabilityData = friendPointData.LikabilityData;
        }

        // 호감도 포인트
        public void PointDataUpdate()
        {
            if (FriendData.LikabilityData == null)
            {
                Debug.LogError("ksmksmksm 포인트 처리 안 들어올때 들어옴");
                return;
            }

            Point = FriendData.LikabilityData.Point;
            SetSendButtonInteractable(SendPossible(FriendData.LikabilityData.SendUnixTime));
            SetReceiveButtonInteractable(ReceivePossible(FriendData.LikabilityData.ReceiveUnixTime,
                FriendData.LikabilityData.RewardUnixTime));
        }

        bool _isNetSend = false;


        public void OnClickSendFriendPoint()
        {
            if (_isNetSend)
            {
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                    ?.SetSendButtonInteractable(false);
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                    ?.SetSendButtonInteractable(false);
                return;
            }

            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                ?.SetSendButtonInteractable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                ?.SetSendButtonInteractable(false);

            if (Managers.FriendMgr.FriendPointSendCount >=
                Managers.TableMgr.CommonGDT.CommonTable.FriendFriendShipPointSendMax)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Top, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.FRIEND_FRIENDSHIPPOINT_DAILY_MAX_TEXT);
                return;
            }

            _isNetSend = true;

            Managers.NetworkMgr.GameClient.ResFriendPointSendAction += SendFriendPointAction;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendPointSend(Cuid);
        }

        bool _isNetRecevie = false;

        public void OnClickFriendPointReceive()
        {
            if (_isNetRecevie)
            {
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                    ?.SetReceiveButtonInteractable(false);
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                    ?.SetReceiveButtonInteractable(false);
                return;
            }

            _isNetRecevie = true;
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                ?.SetReceiveButtonInteractable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                ?.SetReceiveButtonInteractable(false);

            Managers.NetworkMgr.GameClient.ResFriendPointReceiveAction += ReceiveFriendPointAction;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendPointReceive(Cuid);
        }

        public void SetSendButtonInteractable(bool enable)
        {
            m_heartSendButton.interactable = enable;
        }

        public void SetReceiveButtonInteractable(bool enable)
        {
            m_heartReceiveButton.interactable = enable;
        }

        public bool GetSendButtonInteractable()
        {
            return m_heartSendButton.interactable;
        }

        public bool GetReceiveButtonInteractable()
        {
            return m_heartReceiveButton.interactable;
        }

        void ButtonOnclickInit()
        {
            m_heartReceiveButton.onClick.AddListener(OnClickFriendPointReceive);
            m_heartSendButton.onClick.AddListener(OnClickSendFriendPoint);
            if (m_DeleteButton != null)
                m_DeleteButton.onClick.AddListener(OnClickDelete);
            if (m_BlockButton != null)
                m_BlockButton.onClick.AddListener(OnClickFriendBlock);
            if (m_whisperingButton != null)
                m_whisperingButton.onClick.AddListener(OnClickFriendWhispering);
        }

        void SendFriendPointAction(ErrorResult result, FriendPointData friendPointData, CurrencyData currency)
        {
            Managers.NetworkMgr.GameClient.ResFriendPointSendAction -= SendFriendPointAction;

            if (result != ErrorResult.SUCCESS)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Bottom, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.SYSTEM_MESSAGE_UNKNOWN_ERROR);
                return;
            }

            //PointDataUpdate(friendPointData);

            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                ?.PointDataUpdate(friendPointData);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                ?.PointDataUpdate(friendPointData);

            Managers.FriendMgr.SetFriendPointCountDelta(1);

            Managers.UIMgr.OpenPopup<Popup_FriendPoint>()
                .OnPopup_FriendPoint(FriendManager.POPUP_FRIEND_FRIENDSHIPPOINT_TEXT, FriendManager.POPUP_CHECK_BUTTON,
                    currency);

            _isNetSend = false;
        }

        void ReceiveFriendPointAction(ErrorResult result, FriendPointData friendPointData, CurrencyData currency)
        {
            Managers.NetworkMgr.GameClient.ResFriendPointReceiveAction -= ReceiveFriendPointAction;

            if (result != ErrorResult.SUCCESS)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Bottom, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.SYSTEM_MESSAGE_UNKNOWN_ERROR);
                return;
            }

            //PointDataUpdate(friendPointData);

            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)
                ?.PointDataUpdate(friendPointData);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.CARD)
                ?.PointDataUpdate(friendPointData);

            Managers.UIMgr.OpenPopup<Popup_FriendPoint>()
                .OnPopup_FriendPoint(FriendManager.POPUP_FRIEND_FRIENDSHIPPOINT_TEXT, FriendManager.POPUP_CHECK_BUTTON,
                    currency);

            _isNetRecevie = false;
        }

        public UnityAction<long, bool> SendPossibleAction = null;

        /// <summary>
        /// 보낼 수 있는지 판단하는 함수.
        /// 테이블에서 체크하는 것은 0보다 큰지로 0이 될 경우에는 포인트 발송이 불가하다.
        /// </summary>
        /// <param name="sendTime"></param>
        /// <returns></returns> 
        bool SendPossible(long sendTime)
        {
            bool result = false;

            long todayUnix = global::Util.CurrentTodayUnixTime();

            if (todayUnix - sendTime >= 0)
            {
                if (Managers.TableMgr.CommonGDT.CommonTable.FriendFriendShipPointSendNum > 0)
                {
                    result = true;
                }
            }

            SendPossibleAction?.Invoke(Cuid, result);
            return result;
        }

        public UnityAction<long, bool> ReceivePossibleAction = null;

        /// <summary>
        /// 받을 수 있는지 판단하는 함수.
        /// </summary>
        /// <param name="receiveTime"></param>
        /// <param name="rewardTime"></param>
        /// <returns></returns>
        bool ReceivePossible(long receiveTime, long rewardTime)
        {
            bool result = false;

            long deltaTime = receiveTime - rewardTime;

            if (deltaTime > 0)
            {
                result = true;
            }

            ReceivePossibleAction?.Invoke(Cuid, result);
            return result;
        }

        public void OnClickDelete()
        {
            Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                FriendManager.POPUP_FRIEND_DELETE_FRIENDLIST_TEXT, OnNoticeRemoveOkAction);
        }

        void OnNoticeRemoveOkAction()
        {
            Managers.NetworkMgr.GameClient.Proxy.Send_FriendRemove(Cuid);
            Managers.FriendMgr.LocalUpdateFriendDataDicRemove(Cuid);
        }

        protected override void CallBackInit()
        {
            base.CallBackInit();
            SendPossibleAction = null;
            ReceivePossibleAction = null;

            if (Managers.FriendMgr != null)
                Managers.FriendMgr.friendIsDayOff -= PointDataUpdate;

            if (_isNetSend)
            {
                Managers.NetworkMgr.GameClient.ResFriendPointSendAction -= SendFriendPointAction;
                _isNetSend = false;
            }

            if (_isNetRecevie)
            {
                Managers.NetworkMgr.GameClient.ResFriendPointReceiveAction -= ReceiveFriendPointAction;
                _isNetRecevie = false;
            }
        }

        void FriendBaseDataInit(FriendBaseData friendBaseData)
        {
            this.FriendData.BaseData = friendBaseData;
            this.FriendBaseData = friendBaseData;
        }

        private bool isNetBlock = false;

        [SerializeField]
        private ButtonBase m_BlockButton;

        public void SetBlockButtonInteractable(bool interactable)
        {
            if (m_BlockButton != null)
                m_BlockButton.interactable = interactable;
        }

        [SerializeField]
        private ButtonBase m_whisperingButton;

        public void OnClickFriendBlock()
        {
            if (isNetBlock)
            {
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend
                    .GetFriendPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInteractable(false);
                return;
            }

            if (Managers.FriendMgr.CountFriendBlockDataDic() >=
                Managers.TableMgr.CommonGDT.CommonTable.FriendBlockListMaxCount)
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_BLOCKLIST_MAX_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
                return;
            }
            else if (Managers.FriendMgr.ContainFriendBlockDataDic(Cuid))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_BLOCK_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
                return;
            }

            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend.GetFriendPopupListObj(Cuid, FriendObjType.LIST)?
                .SetBlockButtonInteractable(false);

            Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                FriendManager.POPUP_FRIEND_DELETE_FRIENDBLOCKLIST_TEXT,
                OnNoticeBlockOkAction,
                () =>
                {
                    Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend
                        .GetFriendPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInteractable(true);
                });
        }

        void OnNoticeBlockOkAction()
        {
            if (Managers.FriendMgr.ContainFriendDataDic(Cuid))
            {
                Managers.NetworkMgr.GameClient.Proxy.Send_FriendRemove(Cuid);
                Managers.FriendMgr.LocalUpdateFriendDataDicRemove(Cuid);
            }
            else if (Managers.FriendMgr.ContainFriendRequestWaitDataDic(Cuid))
            {
                Managers.NetworkMgr.GameClient.Proxy.Send_FriendCancel(Cuid);
                Managers.FriendMgr.LocalUpdateFriendRequestWaitDicRemove(Cuid);
            }
            else if (Managers.FriendMgr.ContainAcceptWaitDataDic(Cuid))
            {
                Managers.NetworkMgr.GameClient.Proxy.Send_FriendReject(Cuid);
                Managers.FriendMgr.LocalUpdateFriendAcceptWaitDicRemove(Cuid);
            }

            isNetBlock = true;

            Managers.NetworkMgr.GameClient.ResFriendBlockAddAction += ResFriendBlockAction;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendBlockAdd(Cuid);
        }

        void ResFriendBlockAction(ErrorResult result, FriendBlockData friendBlockData)
        {
            isNetBlock = false;
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Friend
                .GetFriendPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInteractable(true);

            Managers.NetworkMgr.GameClient.ResFriendBlockAddAction -= ResFriendBlockAction;

            if (result != ErrorResult.SUCCESS)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Bottom, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.SYSTEM_MESSAGE_UNKNOWN_ERROR);
                return;
            }

            Managers.FriendMgr.LocalUpdateFriendBlockDicAdd(friendBlockData);

            var messageData =
                Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                    .POPUP_FRIEND_TOASTMESSAGE_BLOCK);
            Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                messageData.ShowChat,
                messageData.LanguageID, friendBlockData.Name);
        }

        public void OnClickFriendWhispering()
        {
            Managers.UIMgr.GetPopup<UI_Chatting>().ChattingUserName = PlayerName;
            Managers.NetworkMgr.GameClient.Proxy.Req_Whisper_Message_Online_Check(PlayerName);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
