using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriendSearch : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        TMPro.TextMeshProUGUI m_connectText;

        [SerializeField]
        ButtonBase m_BlockButton;

        [SerializeField]
        ButtonBase m_AddButton;

        public override void Init(FriendBaseData friendBaseData)
        {
            base.Init(friendBaseData);
            m_connectText.text = CurrentConnectJudge(friendBaseData.LoginUnixTime, friendBaseData.LogoutUnixTime);

            m_BlockButton.onClick.RemoveAllListeners();
            m_AddButton.onClick.RemoveAllListeners();

            m_BlockButton.onClick.AddListener(OnClickFriendBlock);
            m_AddButton.onClick.AddListener(OnClickFriendRequest);

            SetAddButtonInterable(true);
            SetBlockButtonInterable(true);

            isNetBlock = false;

            TextInit();
        }

        protected override void CallBackInit()
        {
            base.CallBackInit();
            if (isNetBlock)
            {
                Managers.NetworkMgr.GameClient.ResFriendBlockAddAction -= ResFriendBlockAction;
                isNetBlock = false;
            }
        }

        public void OnClickFriendRequest()
        {
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                .GetFriendSearchPopupListObj(Cuid, FriendObjType.LIST)?.SetAddButtonInterable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                .GetFriendSearchPopupListObj(Cuid, FriendObjType.CARD)?.SetAddButtonInterable(false);

            Managers.FriendMgr.RequestFriend(Cuid);
        }

        bool isNetBlock = false;

        public void OnClickFriendBlock()
        {
            if (isNetBlock)
            {
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                    .GetFriendSearchPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInterable(false);
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                    .GetFriendSearchPopupListObj(Cuid, FriendObjType.CARD)?.SetBlockButtonInterable(false);
                return;
            }

            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                .GetFriendSearchPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInterable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                .GetFriendSearchPopupListObj(Cuid, FriendObjType.CARD)?.SetBlockButtonInterable(false);

            GDT.PopupMessageT messageData = null;
            if (Managers.FriendMgr.CountFriendBlockDataDic() >=
                Managers.TableMgr.CommonGDT.CommonTable.FriendBlockListMaxCount)
            {
                messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_BLOCKLIST_MAX_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
                return;
            }
            else if (Managers.FriendMgr.ContainFriendBlockDataDic(Cuid))
            {
                messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_BLOCK_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
                return;
            }

            messageData =
                Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager.POPUP_FRIEND_DELETE_FRIENDBLOCKLIST_TEXT);
            Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE,
                messageData.LanguageID,
                OnNoticeBlockOkAction,
                () =>
                {
                    Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                        .GetFriendSearchPopupListObj(Cuid, FriendObjType.LIST)?.SetBlockButtonInterable(true);
                    Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend
                        .GetFriendSearchPopupListObj(Cuid, FriendObjType.CARD)?.SetBlockButtonInterable(true);
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

        public void SetAddButtonInterable(bool enable)
        {
            m_AddButton.interactable = enable;
        }

        public void SetBlockButtonInterable(bool enable)
        {
            m_BlockButton.interactable = enable;
        }

        void TextInit()
        {
            m_AddButton.SetText(UI_Localize.LanguageType.UI, FRIEND_RECOMMENDATION_BUTTON);
            m_BlockButton.SetText(UI_Localize.LanguageType.UI, FRIEND_BLOCKLIST_BUTTON);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
