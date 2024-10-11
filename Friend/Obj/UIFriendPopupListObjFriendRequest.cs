using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriendRequest : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        TMPro.TextMeshProUGUI m_connectText;

        [SerializeField]
        ButtonBase m_RequestButtonCancel;

        public override void Init(FriendBaseData friendData)
        {
            base.Init(friendData);
            m_connectText.text = CurrentConnectJudge(friendData.LoginUnixTime, friendData.LogoutUnixTime);
            m_RequestButtonCancel.onClick.RemoveAllListeners();
            m_RequestButtonCancel.onClick.AddListener(OnClickRequestCancelButton);

            m_RequestButtonCancel.interactable = true;

            TextInit();
        }

        public override void Refresh()
        {
            base.Refresh();
            if (FriendBaseData != null)
                m_connectText.text = CurrentConnectJudge(FriendBaseData.LoginUnixTime, FriendBaseData.LogoutUnixTime);
        }

        public void OnClickRequestCancelButton()
        {
            var messageData =
                Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                    .POPUP_FRIEND_TOASTMESSAGE_FRIENDREQUEST_CANCEL);
            Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                messageData.ShowChat,
                messageData.LanguageID);
            Managers.NetworkMgr.GameClient.Proxy.Send_FriendCancel(Cuid);
            Managers.FriendMgr.LocalUpdateFriendRequestWaitDicRemove(Cuid);
        }

        void TextInit()
        {
            m_RequestButtonCancel.SetText(UI_Localize.LanguageType.UI, UIFriendPopup.POPUP_CANCEL_BUTTON);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
