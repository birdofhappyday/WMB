using System;
using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Friend.Obj;
using GDT;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using ObservableExtensions = UniRx.ObservableExtensions;

namespace AnotherWorld.UI.Friend.Popup
{
    public class UIFriendPopupTopSetting : MonoBehaviour
    {
        GameCurrency _friendPointCurrency;

        private void OnDestroy()
        {
            DestoryInit();
        }

        private bool _isInit = false;

        private UIFriendPopupTab _uiFriendPopupTab;

        public void Init(UIFriendPopupTab uiFriendPopupTab)
        {
            if (_isInit)
                return;

            FriendSortTabInit();
            FriendPointCurrencySetting();
            SetPointImage();
            sendPointText.OnTxt(UI_Localize.LanguageType.UI, FriendManager.FRIEND_FRIENDSHIPPOINT_DAILY_TEXT);

            Point = 0;
            Managers.FriendMgr.FriendPointSendCountChange += SendPointCountAction;
            sortToggle.onToggleValueChanged = OnClickSortOrder;

            _uiFriendPopupTab = uiFriendPopupTab;
            SendPointCountAction(Managers.FriendMgr.FriendPointSendCount);

            onClickObjShapeChangeButton.onToggleValueChanged = null;
            onClickObjShapeChangeButton.onToggleValueChanged = OnClickObjShapeChange;
            
            shapeButtonChangeText.OnTxt(UI_Localize.LanguageType.UI, FRIEND_LIST_TYPE_01);
            _isInit = true;
        }

        public void Open()
        {
            FriendPointCurrencySetting();
        }

        [SerializeField]
        private ButtonBase onClickObjShapeChangeButton;
        [SerializeField]
        private UI_Localize shapeButtonChangeText;
        
        [SerializeField]
        TMP_Dropdown sortDropdown;

        [SerializeField]
        private string[] sortTextList;

        public void FriendSortTabInit()
        {
            if (sortTextList.Length == 0)
            {
                sortDropdown.gameObject.SetActive(false);
                return;
            }

            List<string> optionDataList = new List<string>();

            foreach (var text in sortTextList)
            {
                optionDataList.Add(UI_Localize.GetLanguage(text));
            }

            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(optionDataList);

            sortDropdown.onValueChanged.AddListener(OnClickSortStandard);
        }

        void FriendPointCurrencySetting()
        {
            if (_friendPointCurrency == null)
            {
                _friendPointCurrency = Managers.CurrencyMgr.GetCurrency(CurrencySubType.CST_FriendPoint);
                ObservableExtensions.Subscribe(_friendPointCurrency.Amount.ObserveEveryValueChanged(d => d.Value), amount => Point = amount).AddTo(this);
            }

            _currencyInfo = Managers.TableMgr.CommonGDT.GetCurrencyInfo(CurrencySubType.CST_FriendPoint);
        }

        public void GameToLobby()
        {
            _friendPointCurrency = null;
            _isInit = false;
        }

        public void DestoryInit()
        {
            if (Managers.FriendMgr != null)
            {
                Managers.FriendMgr.FriendPointSendCountChange -= SendPointCountAction;
            }
        }

        long point = 0;

        public long Point
        {
            get => point;
            set
            {
                point = value;
                SetFriendPoint(value);
            }
        }

        [SerializeField]
        UI_Localize friendPointText;

        public void SetFriendPoint(long friendPoint)
        {
            friendPointText.OnTxt(UI_Localize.LanguageType.UI, FriendManager.FRIEND_FRIENDSHIPPOINT_DATA,
                friendPoint.ToString());
        }

        /// <summary>
        /// 이미지 변경등 내용을 담기 위한 변수.
        /// </summary>
        CurrencyInfoT _currencyInfo;

        [SerializeField]
        Image[] pointImage;

        void SetPointImage()
        {
            if (!string.IsNullOrEmpty(_currencyInfo.SmallIcon))
            {
                foreach (var img in pointImage)
                {
                    img.sprite = Managers.ResourceMgr.LoadSprite(_currencyInfo.SmallIcon);
                }
            }
        }

        [SerializeField]
        UI_Localize sendPointText;

        [SerializeField]
        private UI_Localize sendPointCountText;

        void SendPointCountAction(int count)
        {
            sendPointCountText.OnTxt(UI_Localize.LanguageType.UI, FriendManager.FRIEND_FRIENDSHIPPOINT_DAILY_COUNT,
                count.ToString(), Managers.TableMgr.CommonGDT.CommonTable.FriendFriendListMaxCount.ToString());
        }

        [SerializeField]
        private ButtonBase sortToggle;

        FriendPopupOrder _friendPopupOrder = FriendPopupOrder.ASCENDING;

        public void OnClickSortOrder(bool friendPopupOrder)
        {
            if (friendPopupOrder)
                this._friendPopupOrder = FriendPopupOrder.DESCENDING;
            else
                this._friendPopupOrder = FriendPopupOrder.ASCENDING;

            SortListObj();
        }

        FriendPopupStandard _popupStandard = FriendPopupStandard.TIME;

        public void OnClickSortStandard(int friendPopupStandard)
        {
            this._popupStandard = (FriendPopupStandard)friendPopupStandard;

            SortListObj();
        }

        List<UIFriendPopupListObj> _currentConnectFriend =
            new List<UIFriendPopupListObj>();

        List<UIFriendPopupListObj> _currentDisConnectFriend =
            new List<UIFriendPopupListObj>();

        public void SortListObj()
        {
            if (_uiFriendPopupTab.GetListObj().Count == 0)
                return;

            List<UIFriendPopupListObj> uiFriendPopupListObjs = null;

            if (_popupStandard == FriendPopupStandard.TIME)
            {
                _currentConnectFriend.Clear();
                _currentDisConnectFriend.Clear();

                foreach (var friend in _uiFriendPopupTab.GetListObj())
                {
                    if (friend.LoginTime > friend.LogoutTime)
                        _currentConnectFriend.Add(friend);
                    else
                        _currentDisConnectFriend.Add(friend);
                }

                if (_friendPopupOrder == FriendPopupOrder.ASCENDING)
                {
                    uiFriendPopupListObjs = _currentConnectFriend.OrderBy(x => x.PlayerName).ToList();
                    uiFriendPopupListObjs.AddRange(_currentDisConnectFriend
                        .OrderByDescending(x => x.LogoutTime).ThenBy(x => x.PlayerName).ToList());
                }
                else
                {
                    uiFriendPopupListObjs = _currentDisConnectFriend.OrderBy(x => x.LogoutTime).ThenByDescending(x => x.PlayerName).ToList();
                    uiFriendPopupListObjs.AddRange(_currentConnectFriend.OrderByDescending(x => x.PlayerName).ToList());
                }
            }
            else if (_popupStandard == FriendPopupStandard.POINT)
            {
                if (_friendPopupOrder == FriendPopupOrder.ASCENDING)
                {
                    uiFriendPopupListObjs = _uiFriendPopupTab.GetListObj()
                        .OrderBy(x => x.FriendData?.LikabilityData.Point).ToList();
                }
                else
                {
                    uiFriendPopupListObjs = _uiFriendPopupTab.GetListObj()
                        .OrderByDescending(x => x.FriendData?.LikabilityData.Point).ToList();
                }
            }
            // else if (_popupStandard == FriendPopupStandard.PLAYERNAME)
            // {
            //     if (_friendPopupOrder == FriendPopupOrder.ASCENDING)
            //     {
            //         uiFriendPopupListObjs = _uiFriendPopupTab.GetListObj()
            //             .OrderBy(x => x.PlayerName).ToList();
            //     }
            //     else
            //     {
            //         uiFriendPopupListObjs = _uiFriendPopupTab.GetListObj()
            //             .OrderByDescending(x => x.PlayerName).ToList();
            //     }
            // }

            int index = 0;
            foreach (var obj in uiFriendPopupListObjs)
            {
                obj.transform.SetSiblingIndex(index);
                ++index;
            }
        }

        private Action<bool> _onClickObjChangeAction;

        public void SetOnClickObjChangeAction(Action<bool> cardChangeAction)
        {
            _onClickObjChangeAction = cardChangeAction;
        }

        private const string FRIEND_LIST_TYPE_01 = "FRIEND_LIST_TYPE_01";
        private const string FRIEND_LIST_TYPE_02 = "FRIEND_LIST_TYPE_02";
        void OnClickObjShapeChange(bool isOn)
        {
            _onClickObjChangeAction?.Invoke(isOn);

            if (isOn)
                shapeButtonChangeText.OnTxt(UI_Localize.LanguageType.UI, FRIEND_LIST_TYPE_02);
            else
                shapeButtonChangeText.OnTxt(UI_Localize.LanguageType.UI, FRIEND_LIST_TYPE_01);
        }
    }
}
