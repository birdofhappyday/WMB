using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupCardObjFriend : UIFriendPopupListObjFriend
    {
        [SerializeField]
        private ButtonBase addToggle;

        public override void Init(FriendData friendData)
        {
            addToggle.onToggleValueChanged = AddButtonAction;
            TextInit();
            base.Init(friendData);
        }

        void TextInit()
        {
            m_heartSendButton.SetText(UI_Localize.LanguageType.UI, FRIEND_FRIENDSHIPPOINT_SEND_BUTTON);
            m_heartReceiveButton.SetText(UI_Localize.LanguageType.UI, FRIEND_FRIENDSHIPPOINT_RECEPTION);
        }

        [SerializeField]
        private Transform deleteBlockGameObject;
        
        public void AddButtonAction(bool isOn)
        {
            if (isOn)
            {
                Managers.UIMgr.GetPopup<UIFriendPopup>().SetOnClickCardButtonGroupCloseAction(CloseAddToggle);
                Managers.UIMgr.GetPopup<UIFriendPopup>().SetPosCardDeleteAndBlockButton(deleteBlockGameObject.position,
                    OnClickDelete, OnClickFriendBlock, OnClickFriendWhispering);
            }
        }

        public void CloseAddToggle()
        {
            addToggle.isOn = false;
        }
    }
}
