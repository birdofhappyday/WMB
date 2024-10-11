using AnotherWorld.UI.Friend.Popup;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Obj
{
    public class UIFriendPopupListObjFriendRecommend : UIFriendPopupListObj, IPooledObject
    {
        [SerializeField]
        TMPro.TextMeshProUGUI m_connectText;

        [SerializeField]
        ButtonBase m_requestButton;

        public override void Init(FriendBaseData friendData)
        {
            base.Init(friendData);
            m_connectText.text = CurrentConnectJudge(friendData.LoginUnixTime, friendData.LogoutUnixTime);

            m_requestButton.onClick.RemoveAllListeners();
            m_requestButton.onClick.AddListener(OnClickRequestFriend);
            SetRequestButtonInteractable(true);

            TextInit();
        }

        public override void Refresh()
        {
            base.Refresh();
            if (FriendBaseData != null)
            {
                m_connectText.text = CurrentConnectJudge(FriendBaseData.LoginUnixTime, FriendBaseData.LogoutUnixTime);
            }
        }

        public void SetRequestButtonInteractable(bool enable)
        {
            m_requestButton.interactable = enable;
        }

        public void OnClickRequestFriend()
        {
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend.GetRecommendListObj(Cuid, FriendObjType.LIST)
                .SetRequestButtonInteractable(false);
            Managers.UIMgr.GetOpenPopup<UIFriendPopup>().tab_Recommend.GetRecommendListObj(Cuid, FriendObjType.CARD)
                .SetRequestButtonInteractable(false);

            Managers.FriendMgr.RequestFriend(Cuid);
        }

        void TextInit()
        {
            m_requestButton.SetText(UI_Localize.LanguageType.UI, FRIEND_RECOMMENDATION_BUTTON);
        }

        public void Init() { }

        public void Clear()
        {
            CallBackInit();
        }
    }
}
