using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriendBlock : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        ButtonBase removeButton;

        public override void Init(FriendBlockData friendBlockData)
        {
            base.Init(friendBlockData);

            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnClockBlockRemoveButton);
            removeButton.interactable = true;

            TextInit();
        }

        public void OnClockBlockRemoveButton()
        {
            var messageData =
                Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                    .POPUP_FRIEND_TOASTMESSAGE_BLOCKRELEASE);
            Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                messageData.ShowChat,
                messageData.LanguageID, FriendBlockData.Name);

            Managers.NetworkMgr.GameClient.Proxy.Send_FriendBlockRemove(Cuid);
            Managers.FriendMgr.LocalUpdateFriendBlockDicRemove(Cuid);
        }

        void TextInit()
        {
            removeButton.SetText(UI_Localize.LanguageType.UI, FRIEND_BLOCKLIST_UNLOCK_BUTTON);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
