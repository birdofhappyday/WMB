using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AnotherWorld.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AnotherWorld.UI.Friend.Popup
{
    /// <summary>
    /// 팝업에서 주요 부분 표시하는 부분.
    /// 탭마다 스크립트를 따로 나눠서 여기서 특별히 하는 기능은 없다.
    /// 글자설정과 Instance를 둬서 갱신시 활용하려고 한다.
    /// </summary>
    public class UIFriendPopup : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = true;
        
        [SerializeField]
        private UI_Localize m_titleText;

        const string FRIEND_TITLE = "FRIEND_TITLE";
        const string FRIEND_TAB_FRIENDLIST = "FRIEND_TAB_FRIENDLIST";
        const string FRIEND_TAB_FRIENDREQUESTLIST = "FRIEND_TAB_FRIENDREQUESTLIST";
        const string FRIEND_TAB_RECEPTIONLIST = "FRIEND_TAB_RECEPTIONLIST";
        const string FRIEND_TAB_RECOMMENDATION = "FRIEND_TAB_RECOMMENDATION";
        const string FRIEND_TAB_BLOCKLIST = "FRIEND_TAB_BLOCKLIST";
        public const string POPUP_CANCEL_BUTTON = "POPUP_CANCEL_BUTTON";

        public UIFriendPopupFriendTab tab_Friend;
        public UIFriendPopupRequestTab tab_Request;
        public UIFriendPopupAcceptTab tab_Accept;
        public UIFriendPopupRecommendTab tab_Recommend;
        public UIFriendPopupBlockTab tab_Block;

        public UIButton HomeButton;
        public UIButton BackButton;

        private UITabFriend _tabFriend = new UITabFriend();

        public string[] _Tab_Text = new string[5]
        {
            FRIEND_TAB_FRIENDLIST, FRIEND_TAB_FRIENDREQUESTLIST, FRIEND_TAB_RECEPTIONLIST,
            FRIEND_TAB_RECOMMENDATION, FRIEND_TAB_BLOCKLIST
        };

        [SerializeField]
        private ButtonBase[] friendTabArray;

        public override void Init()
        {
            TextInit();

            HomeButton.AddButtonClick(OnHome);
            BackButton.AddButtonClick(BackButtonEvent);

            //CurrencyCoin.Init();

            backButtonBg.onClick.AddListener(OnClickCardButtonGroupClose);
            backButtonBg.gameObject.SetActive(false);

            //레드닷이 있는 경우에만
            foreach (var friendTab in friendTabArray)
            {
                //레드닷 처리
                var redDot = friendTab.GetComponentInChildren<NewRedDot>();
                if (null != redDot)
                {
                    redDot.SetRedDot(GDT.ReddotMainType.RMT_Friend, redDot.SubType, 0);
                    
                    friendTab.onToggleValueChanged +=
                        (isOn) =>
                        {
                            if (isOn)
                            {
                                if (redDot.SubType == GDT.ReddotSubType.RST_FriendTab)
                                {
                                    Managers.NewRedDotMgr.FriendAllOff();
                                }
                                else
                                {
                                    Managers.NewRedDotMgr.AcceptWaitFriendAllOff();
                                }
                            }
                        };
                }
            }
                
            base.Init();
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            tab_Friend.GameToLobby();
            tab_Request.GameToLobby();
            tab_Accept.GameToLobby();
            tab_Recommend.GameToLobby();
            tab_Block.GameToLobby();
        }

        public override void BackButtonEvent()
        {
            base.BackButtonEvent();
            ClosePopup();
        }

        void TextInit()
        {
            m_titleText.OnTxt(UI_Localize.LanguageType.UI, "FRIEND_TITLE");
            
            int i = 0;
            foreach (var friendTab in friendTabArray)
            {
                int j = i;
                friendTab.SetText(UI_Localize.LanguageType.UI, _Tab_Text[i]);
                //friendTab.onClick.AddListener(() => { OnClickFriendTab(j); });
                friendTab.onToggleValueChanged =
                    (isOn) =>
                    {
                        FriendTabActive(j, isOn);
                    };
                ++i;
            }

            OnClickFriendTab(0);
        }

        public void OnClickFriendTab(int i)
        {
            for (int j = 0; j < _Tab_Text.Length; j++)
            {
                FriendTabActive(j, i == j);
            }
        }

        public void FriendTabActive(int i, bool isOn)
        {
            switch (i)
            {
                case 0:
                    tab_Friend.IsOpen(isOn);
                    break;

                case 1:
                    tab_Request.IsOpen(isOn);
                    break;

                case 2:
                    tab_Accept.IsOpen(isOn);
                    break;

                case 3:
                    tab_Recommend.IsOpen(isOn);
                    break;

                case 4:
                    tab_Block.IsOpen(isOn);
                    break;
            }
        }

        private System.Action _onClickCardAddButtonClose = null;

        public void SetOnClickCardButtonGroupCloseAction(System.Action onClickAction)
        {
            backButtonBg.gameObject.SetActive(true);
            _onClickCardAddButtonClose += onClickAction;
        }
        
        public void RemoveOnClickCardButtonGroupCloseAction(System.Action onClickAction)
        {
            backButtonBg.gameObject.SetActive(false);
            _onClickCardAddButtonClose -= onClickAction;
        }

        [SerializeField]    
        private ButtonBase backButtonBg;

        public void OnClickCardButtonGroupClose()
        {
            backButtonBg.gameObject.SetActive(false);
            cardDeleteAndBlockButton.SetActive(false);
            _onClickCardAddButtonClose?.Invoke();
            _onClickCardAddButtonClose = null;
        }

        [SerializeField]
        private GameObject cardDeleteAndBlockButton;

        [SerializeField]
        private ButtonBase m_DeleteButton;

        [SerializeField]
        private ButtonBase m_BlockButton;

        [SerializeField]
        private ButtonBase m_WhisperingButton;

        public void SetPosCardDeleteAndBlockButton(Vector3 pos, UnityAction OnClickDelete,
            UnityAction OnClickFriendBlock, UnityAction OnClickWhispering)
        {
            cardDeleteAndBlockButton.SetActive(true);

            m_DeleteButton.onClick.RemoveAllListeners();
            m_BlockButton.onClick.RemoveAllListeners();
            m_WhisperingButton.onClick.RemoveAllListeners();

            cardDeleteAndBlockButton.SetPosition(pos);
            m_DeleteButton.onClick.AddListener(OnClickDelete);
            m_DeleteButton.onClick.AddListener(OnClickCardButtonGroupClose);
            m_BlockButton.onClick.AddListener(OnClickFriendBlock);
            m_BlockButton.onClick.AddListener(OnClickCardButtonGroupClose);
            m_WhisperingButton.onClick.AddListener(OnClickWhispering);
            m_WhisperingButton.onClick.AddListener(OnClickCardButtonGroupClose);
        }
    }
}
