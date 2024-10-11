using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SharedCode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace AnotherWorld.UI.Friend.Popup
{
    public enum FriendPopupRecommendTabMode
    {
        RECOMMEND,
        SEARCH,
    }

    public class UIFriendPopupRecommendTab : UIFriendPopupTab
    {
        const string _recommendListPath = "UI/Friend/UIFriendPopupListObjFriendRecommend";
        const string _recommendCardPath = "UI/Friend/UIFriendPopupListObjFriendRecommend_Card";

        private Dictionary<long, UIFriendPopupListObjFriendRecommend> _recommendListDic =
            new Dictionary<long, UIFriendPopupListObjFriendRecommend>();

        private Dictionary<long, UIFriendPopupListObjFriendRecommend> _recommendCardDic =
            new Dictionary<long, UIFriendPopupListObjFriendRecommend>();


        public override List<UIFriendPopupListObj> GetListObj()
        {
            if (ObjType == FriendObjType.LIST)
                return _recommendListDic.Values.Cast<UIFriendPopupListObj>().ToList();
            else
                return _recommendCardDic.Values.Cast<UIFriendPopupListObj>().ToList();
        }

        public UIFriendPopupListObjFriendRecommend GetRecommendListObj(long cuid, FriendObjType friendObjType)
        {
            UIFriendPopupListObjFriendRecommend result;

            if (friendObjType == FriendObjType.LIST)
                _recommendListDic.TryGetValue(cuid, out result);
            else
                _recommendCardDic.TryGetValue(cuid, out result);


            return result;
        }

        private void ClearRecommendObjs()
        {
            foreach (var friendRecommendData in _recommendListDic.Values)
            {
                friendRecommendData.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendRecommendData.gameObject);
            }

            foreach (var friendRecommendData in _recommendCardDic.Values)
            {
                friendRecommendData.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendRecommendData.gameObject);
            }

            _recommendListDic.Clear();
            _recommendCardDic.Clear();
        }

        private Vector2 _initRecommendConetentsAnchoredPosition = Vector2.zero;

        public override void Refresh(params object[] valueArray)
        {
            ClearRecommendObjs();
            contentGroup[0].list.content.anchoredPosition = _initRecommendConetentsAnchoredPosition;

            List<FriendBaseData> recommandDatas = (List<FriendBaseData>)valueArray[0];
            foreach (FriendBaseData data in recommandDatas)
            {
                if (!Managers.FriendMgr.CurrentFriendRequestPossible(data.CUID))
                    continue;

                UIFriendPopupListObjFriendRecommend friendObj = Managers.ResourceMgr
                    .Instantiate(_recommendListPath, contentGroup[0].list.content.transform)
                    .GetComponent<UIFriendPopupListObjFriendRecommend>();
                friendObj.Init(data);

                _recommendListDic.Add(data.CUID, friendObj);

                //ObjTask(_recommendListPath, contentGroup[0].list.content.transform, data, FriendObjType.LIST).Forget();
            }

            foreach (FriendBaseData data in recommandDatas)
            {
                if (!Managers.FriendMgr.CurrentFriendRequestPossible(data.CUID))
                    continue;

                UIFriendPopupListObjFriendRecommend friendObj = Managers.ResourceMgr
                    .Instantiate(_recommendCardPath, contentGroup[0].card.content.transform)
                    .GetComponent<UIFriendPopupListObjFriendRecommend>();
                friendObj.Init(data);

                _recommendCardDic.Add(data.CUID, friendObj);

                // ObjTask(_recommendCardPath, contentGroup[0].card.content.transform, data, FriendObjType.CARD).Forget();
            }

            uiFriendPopupTopSettings[0].SortListObj();
        }

        async UniTask ObjTask(string popupPath, Transform parent, FriendBaseData data, FriendObjType friendObjType)
        {
            var friendObj = await Managers.ResourceMgr.InstantiateTask(popupPath, parent);
            UIFriendPopupListObjFriendRecommend recommendFriendObj =
                friendObj.GetComponent<UIFriendPopupListObjFriendRecommend>();
            recommendFriendObj.Init(data);

            if (friendObjType == FriendObjType.LIST)
                _recommendListDic.Add(data.CUID, recommendFriendObj);
            else
            {
                _recommendCardDic.Add(data.CUID, recommendFriendObj);
            }

            uiFriendPopupTopSettings[0].SortListObj();
        }

        protected override void Init()
        {
            base.Init();
            TextInit();
            ListClear();
            refreshButton.onClick.AddListener(OnClickRecommendRefreshButton);
            searchRefreshButton.onClick.AddListener(OnClickSearchRefreshButton);
            Managers.NetworkMgr.GameClient.ResFriendRecommendListAction += Res_FriendRecommendList;

            _initRecommendConetentsAnchoredPosition = new Vector2(contentGroup[0].list.content.anchoredPosition.x, 0);
            searchButton.interactable = false;
            searchButton.onClick.AddListener(OnClickSearchButton);
            searchText.onValueChanged.AddListener(InputCheck);

            Managers.NetworkMgr.GameClient.ResFriendSearchAction += ResFriendSearchAction;

            uiFriendPopupTopSettings[0].SetOnClickObjChangeAction(
                (isOn) =>
                {
                    contentGroup[0].list.gameObject.SetActive(!isOn);
                    contentGroup[0].card.gameObject.SetActive(isOn);

                    if (!isOn)
                        ObjType = FriendObjType.LIST;
                    else
                        ObjType = FriendObjType.CARD;

                    uiFriendPopupTopSettings[0].SortListObj();
                });

            uiFriendPopupTopSettings[1].SetOnClickObjChangeAction(
                (isOn) =>
                {
                    contentGroup[1].list.gameObject.SetActive(!isOn);
                    contentGroup[1].card.gameObject.SetActive(isOn);

                    if (!isOn)
                        ObjType = FriendObjType.LIST;
                    else
                        ObjType = FriendObjType.CARD;
                });
        }

        protected override bool DestoryInit()
        {
            if (base.DestoryInit())
            {
                if (Managers.Instance == null)
                    return true;

                if (Managers.NetworkMgr == null)
                    return true;

                Managers.NetworkMgr.GameClient.ResFriendRecommendListAction -= Res_FriendRecommendList;
                Managers.NetworkMgr.GameClient.ResFriendSearchAction -= ResFriendSearchAction;

                _searchCancellationTokenSource?.Cancel();
                return true;
            }

            return false;
        }

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);

            CurrentPopupMode = FriendPopupRecommendTabMode.RECOMMEND;

            if (refreshButton.interactable == true)
                OnClickRecommendRefreshButton();
            else
                RecommandRefreshObj();
        }

        void RecommandRefreshObj()
        {
            foreach (var recommandPrepab in _recommendListDic.Values)
            {
                recommandPrepab.Refresh();
            }

            foreach (var recommandPrepab in _recommendCardDic.Values)
            {
                recommandPrepab.Refresh();
            }
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            ClearRecommendObjs();
            _searchCancellationTokenSource?.Cancel();
            isOpen = false;
        }

        private FriendPopupRecommendTabMode _currentPopupMode;

        public FriendPopupRecommendTabMode CurrentPopupMode
        {
            get => _currentPopupMode;
            set
            {
                _currentPopupMode = value;
                ChangeTabMode(_currentPopupMode);
            }
        }

        void Res_FriendRecommendList(List<FriendBaseData> recommendDatas)
        {
            Refresh(recommendDatas);
            RefreshButtonUniTask(_refreshCoolTime).Forget();
        }

        void TextInit()
        {
            TextSettingInputField();
            TextInitSearchButton();
            InitTextSearchTitle();

            refreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
            searchRefreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
        }

        [SerializeField]
        TMPro.TextMeshProUGUI titleText;

        public void OnClickRecommendRefreshButton()
        {
            RefreshListActive();
        }

        void RefreshListActive()
        {
            RefreshButtonInteractable(false);
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendRecommendList();
        }

        [SerializeField]
        ButtonBase searchButton;

        public void InputCheck(string text)
        {
            if (CurrentPopupMode == FriendPopupRecommendTabMode.RECOMMEND)
            {
                if (!string.IsNullOrEmpty(text))
                    searchButton.interactable = true;
                else
                    searchButton.interactable = false;
            }
            else
            {
                if (string.IsNullOrEmpty(text))
                {
                    CurrentPopupMode = FriendPopupRecommendTabMode.RECOMMEND;
                    searchButton.interactable = false;
                }
            }
        }

        [SerializeField]
        private GameObject recommandTab;

        [SerializeField]
        GameObject searchTab;

        void ChangeTabMode(FriendPopupRecommendTabMode uIfriendpopuplistobjmode)
        {
            switch (uIfriendpopuplistobjmode)
            {
                case FriendPopupRecommendTabMode.RECOMMEND:
                    recommandTab.SetActive(true);
                    searchTab.SetActive(false);
                    searchText.text = string.Empty;
                    break;

                case FriendPopupRecommendTabMode.SEARCH:
                    recommandTab.SetActive(false);
                    searchTab.SetActive(true);
                    break;
            }
        }

        [SerializeField]
        UI_Localize searchTitle;

        void InitTextSearchTitle()
        {
            searchTitle.OnTxt(FriendManager.FRIEND_SEARCH_RESULT);
        }

        [SerializeField]
        UI_Localize searchInitText;

        void TextSettingInputField()
        {
            searchInitText.OnTxt(FriendManager.FRIEND_SEARCH_TEXT);
        }

        void TextInitSearchButton()
        {
            searchButton.SetText(UI_Localize.LanguageType.UI, FriendManager.FRIEND_SEARCH_BUTTON);
        }

        [SerializeField]
        UI_Localize noneSearchFriendText;

        [SerializeField]
        TMPro.TMP_InputField searchText;

        void ResFriendSearchAction(ErrorResult result, FriendBaseData searchData)
        {
            bool success = result == ErrorResult.SUCCESS;

            CurrentPopupMode = FriendPopupRecommendTabMode.SEARCH;

            ActiveSearchListObj(success);

            if (success)
            {
                SearchDataInit(searchData);

                uiFriendPopupTopSettings[1].SortListObj();
            }
        }

        const string _searchListPath = "UI/Friend/UIFriendPopupListObjFriendSearch";
        const string _searchCardPath = "UI/Friend/UIFriendPopupListObjFriendSearch_Card";

        private Dictionary<long, UIFriendPopupListObjFriendSearch> _searchListObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendSearch>();

        private Dictionary<long, UIFriendPopupListObjFriendSearch> _searchCardObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendSearch>();

        public void AddFriendSearchPopupListObj(long cuid, UIFriendPopupListObjFriendSearch uiFriendPopupListObjFriend,
            FriendObjType friendObjType)
        {
            if (friendObjType == FriendObjType.LIST)
                _searchListObjDic.Add(cuid, uiFriendPopupListObjFriend);
            else if (friendObjType == FriendObjType.CARD)
                _searchCardObjDic.Add(cuid, uiFriendPopupListObjFriend);
        }

        public UIFriendPopupListObjFriendSearch GetFriendSearchPopupListObj(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_searchListObjDic.ContainsKey(cuid))
                    return null;

                return _searchListObjDic[cuid];
            }
            else
            {
                if (!_searchCardObjDic.ContainsKey(cuid))
                    return null;

                return _searchCardObjDic[cuid];
            }
        }

        public void ClearSearchPopupListObj()
        {
            foreach (var obj in _searchListObjDic)
            {
                obj.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(obj.Value.gameObject);
            }

            _searchListObjDic.Clear();

            foreach (var obj in _searchCardObjDic)
            {
                obj.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(obj.Value.gameObject);
            }

            _searchCardObjDic.Clear();
        }

        void SearchDataInit(FriendBaseData searchData)
        {
            ClearSearchPopupListObj();

            UIFriendPopupListObjFriendSearch friendListObj = Managers.ResourceMgr
                .Instantiate(_searchListPath, contentGroup[1].list.content.transform)
                .GetComponent<UIFriendPopupListObjFriendSearch>();
            friendListObj.Init(searchData);

            AddFriendSearchPopupListObj(searchData.CUID, friendListObj, FriendObjType.LIST);

            UIFriendPopupListObjFriendSearch friendCardObj = Managers.ResourceMgr
                .Instantiate(_searchCardPath, contentGroup[1].card.content.transform)
                .GetComponent<UIFriendPopupListObjFriendSearch>();
            friendCardObj.Init(searchData);

            AddFriendSearchPopupListObj(searchData.CUID, friendCardObj, FriendObjType.CARD);
        }

        void ActiveSearchListObj(bool active)
        {
            contentGroup[1].list.transform.parent.gameObject.SetActive(active);
            noneSearchFriendText.gameObject.SetActive(!active);
        }

        private string _searchStr = string.Empty;

        public void OnClickSearchButton()
        {
            if (_Me.EnterChar.Name.Equals(searchText.text))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_SEARCH_MYSELF);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);
                return;
            }

            _searchStr = searchText.text;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendSearch(searchText.text);
        }

        [SerializeField]
        private ButtonBase searchRefreshButton;

        [SerializeField]
        private Image searchRefreshDimmedImg;

        [SerializeField]
        private UI_Localize searchRefreshDimmedTxt;

        void OnClickSearchRefreshButton()
        {
            SearchRefreshButtonInteractable(false);
            searchText.text = _searchStr;
            OnClickSearchButton();
            SearchRefreshButtonUniTask(_refreshCoolTime).Forget();
        }

        void SearchRefreshButtonInteractable(bool active)
        {
            searchRefreshButton.interactable = active;
        }

        private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

        async UniTask SearchRefreshButtonUniTask(float time)
        {
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new();

            if (searchRefreshDimmedImg != null)
            {
                searchRefreshDimmedImg.DOKill();
                searchRefreshDimmedImg.fillAmount = 1;
                searchRefreshDimmedImg.DOFillAmount(0, time).SetEase(Ease.Linear).SetAutoKill(true);
            }

            while (time > 0)
            {
                if (searchRefreshDimmedTxt != null)
                {
                    searchRefreshDimmedTxt.OnTxt(UI_Localize.LanguageType.UI, BUTTON_FRIENDLIST_REFRESHTIME_TEXT,
                        time.ToString());
                }

                var isCancelled = await UniTask
                    .WaitForSeconds(1, cancellationToken: _searchCancellationTokenSource.Token)
                    .SuppressCancellationThrow();
                if (isCancelled == true)
                {
                    break;
                }

                --time;
            }


            SearchRefreshTimeCheck(true);
        }

        private void SearchRefreshTimeCheck(bool isActive)
        {
            if (isActive)
            {
                SearchRefreshButtonInteractable(true);
            }
            else
            {
                SearchRefreshButtonInteractable(false);
            }
        }
    }
}
