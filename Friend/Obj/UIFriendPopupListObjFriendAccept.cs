using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriendAccept : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        ButtonBase m_RefuseButton;

        [SerializeField]
        ButtonBase m_AcceptButton;

        [SerializeField]
        TMPro.TextMeshProUGUI m_connectText;

        public override void Init(FriendBaseData friendData)
        {
            base.Init(friendData);
            m_connectText.text = CurrentConnectJudge(friendData.LoginUnixTime, friendData.LogoutUnixTime);
            TextInit();

            m_RefuseButton.onClick.RemoveAllListeners();
            m_AcceptButton.onClick.RemoveAllListeners();
            m_RefuseButton.onClick.AddListener(OnClickRefuseButton);
            m_AcceptButton.onClick.AddListener(OnClickAcceptButton);

            SetRefuseButtonInteractable(true);
            SetAcceptButtonInteractable(true);

            //레드닷 처리
            var redDot = GetComponentInChildren<NewRedDot>();
            if (null != redDot)
            {
                redDot.SetRedDot(GDT.ReddotMainType.RMT_Friend, GDT.ReddotSubType.RST_NewFriendApply, friendData.CUID);
            }

            isNetAccept = false;
        }

        public override void Refresh()
        {
            base.Refresh();
            if (FriendBaseData != null)
                m_connectText.text = CurrentConnectJudge(FriendBaseData.LoginUnixTime, FriendBaseData.LogoutUnixTime);
        }

        protected override void CallBackInit()
        {
            base.CallBackInit();

            if (isNetAccept)
            {
                Managers.NetworkMgr.GameClient.ResFriendAcceptAction -= Res_FriendAccept;
                isNetAccept = false;
            }
        }

        public void OnClickRefuseButton()
        {
            var messageData =
                Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                    .POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_REFUSAL);
            Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                messageData.ShowChat,
                messageData.LanguageID, Managers.FriendMgr.GetFriendAcceptWaitDataDic(Cuid).Name);
            RefuseUpdate();
        }

        bool isNetAccept = false;

        public void SetRefuseButtonInteractable(bool interactable)
        {
            m_RefuseButton.interactable = interactable;
        }
        
        public void SetAcceptButtonInteractable(bool interactable)
        {
            m_AcceptButton.interactable = interactable;
        }
        
        public void OnClickAcceptButton()
        {
            if (isNetAccept)
            {
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                    .GetAcceptFriendObjDic(Cuid, FriendObjType.CARD).SetAcceptButtonInteractable(false);
                Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                    .GetAcceptFriendObjDic(Cuid, FriendObjType.LIST).SetAcceptButtonInteractable(false);
                return;
            }

            isNetAccept = true;
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                .GetAcceptFriendObjDic(Cuid, FriendObjType.CARD).SetAcceptButtonInteractable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                .GetAcceptFriendObjDic(Cuid, FriendObjType.LIST).SetAcceptButtonInteractable(false);

            Managers.NetworkMgr.GameClient.ResFriendAcceptAction += Res_FriendAccept;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendAccept(Cuid);
        }

        void Res_FriendAccept(ErrorResult result, FriendData friendData)
        {
            Managers.NetworkMgr.GameClient.ResFriendAcceptAction -= Res_FriendAccept;

            isNetAccept = false;
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                .GetAcceptFriendObjDic(Cuid, FriendObjType.CARD)?.SetAcceptButtonInteractable(true);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Accept
                .GetAcceptFriendObjDic(Cuid, FriendObjType.LIST)?.SetAcceptButtonInteractable(true);
            
            if (result == ErrorResult.FRIEND_MAX)
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_GET_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
            }
            else if (result != ErrorResult.SUCCESS)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Bottom, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.SYSTEM_MESSAGE_UNKNOWN_ERROR);
                
                RefuseUpdate();
            }
            else
            {
                Managers.FriendMgr.LocalUpdateFriendAcceptWaitDicRemove(friendData.BaseData.CUID);
                Managers.FriendMgr.LocalUpdateFriendDataDicAdd(friendData);

                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_AGREE);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID, friendData.BaseData.Name);
            }
        }

        void RefuseUpdate()
        {
            Managers.FriendMgr.LocalUpdateFriendAcceptWaitDicRemove(Cuid);
            Managers.NetworkMgr.GameClient.Proxy.Send_FriendReject(Cuid);
        }

        void TextInit()
        {
            m_RefuseButton.SetText(UI_Localize.LanguageType.UI, FRIEND_RECEPTIONLIST_REFUSAL_BUTTON);
            m_AcceptButton.SetText(UI_Localize.LanguageType.UI, FRIEND_RECEPTIONLIST_ACCEPT_BUTTON);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
