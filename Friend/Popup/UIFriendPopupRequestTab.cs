using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Popup
{
    public class UIFriendPopupRequestTab : UIFriendPopupTab
    {
        private Dictionary<long, UIFriendPopupListObjFriendRequest> _requestPopupListObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendRequest>();

        private Dictionary<long, UIFriendPopupListObjFriendRequest> _requestPopupCardObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendRequest>();

        public override List<UIFriendPopupListObj> GetListObj()
        {
            if (ObjType == FriendObjType.LIST)
                return _requestPopupListObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
            else
                return _requestPopupCardObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
        }

        public UIFriendPopupListObjFriendRequest GetRequestPopupListObj(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_requestPopupListObjDic.ContainsKey(cuid))
                    return null;

                return _requestPopupListObjDic[cuid];
            }
            else
            {
                if (!_requestPopupCardObjDic.ContainsKey(cuid))
                    return null;

                return _requestPopupCardObjDic[cuid];
            }
        }

        public void AddRequestPopupListObj(long cuid, UIFriendPopupListObjFriendRequest uiRequestPopupListObjFriend,
            FriendObjType friendObjType)
        {
            if (friendObjType == FriendObjType.LIST)
                _requestPopupListObjDic.Add(cuid, uiRequestPopupListObjFriend);
            else if (friendObjType == FriendObjType.CARD)
                _requestPopupCardObjDic.Add(cuid, uiRequestPopupListObjFriend);
        }

        public void RemoveRequestPopupListObj(long cuid)
        {
            UIFriendPopupListObjFriendRequest requestPopupObj = GetRequestPopupListObj(cuid, FriendObjType.LIST);

            if (requestPopupObj == null)
                return;
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _requestPopupListObjDic.Remove(cuid);
            
            requestPopupObj = GetRequestPopupListObj(cuid, FriendObjType.CARD);
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _requestPopupCardObjDic.Remove(cuid);
        }

        void ClearRequestPopupObjDic()
        {
            foreach (var requestFriend in _requestPopupListObjDic)
            {
                requestFriend.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(requestFriend.Value.gameObject);
            }

            foreach (var requestFriend in _requestPopupCardObjDic)
            {
                requestFriend.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(requestFriend.Value.gameObject);
            }

            _requestPopupListObjDic.Clear();
            _requestPopupCardObjDic.Clear();
        }

        public override void Refresh(params object[] valueArray)
        {
            foreach (var requestobj in _requestPopupListObjDic)
            {
                requestobj.Value.Refresh();
            }

            foreach (var requestobj in _requestPopupCardObjDic)
            {
                requestobj.Value.Refresh();
            }
        }

        protected override void Init()
        {
            base.Init();

            TextInit();

            refreshButton.onClick.AddListener(OnClickRefreshButton);

            Managers.FriendMgr.ServerUpdateFriendRequestWaitDataDicRemoveAction +=
                ServerUpdateFriendRequestWaitDataDicRemoveAction;
            Managers.FriendMgr.LocalUpdateFriendRequestWaitDicRemoveAction += LocalUpdateFriendRequestWaitDicRemoveAction;
            Managers.FriendMgr.LocalUpdateFriendRequestWaitDicAddAction += LocalUpdateFriendRequestWaitDicAddAction;

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
        }

        protected override bool DestoryInit()
        {
            if (base.DestoryInit())
            {
                if (Managers.Instance == null)
                    return true;

                if (Managers.NetworkMgr == null)
                    return true;

                Managers.FriendMgr.ServerUpdateFriendRequestWaitDataDicRemoveAction -=
                    ServerUpdateFriendRequestWaitDataDicRemoveAction;
                Managers.FriendMgr.LocalUpdateFriendRequestWaitDicRemoveAction -=
                    LocalUpdateFriendRequestWaitDicRemoveAction;
                Managers.FriendMgr.LocalUpdateFriendRequestWaitDicAddAction -= LocalUpdateFriendRequestWaitDicAddAction;
                return true;
            }

            return false;
        }

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);

            InitObjData();
            SetFriendListTitleText();
            ActiveRequestListObject();
            Refresh();
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            ClearRequestPopupObjDic();
            isOpen = false;
        }

        void TextInit()
        {
            refreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
        }

        [SerializeField]
        TMPro.TextMeshProUGUI requestCountText;

        const string _requestListObjPrefabPath = "UI/Friend/UIFriendPopupListObjFriendRequest";
        const string _requestCardObjPrefabPath = "UI/Friend/UIFriendPopupListObjFriendRequest_Card";


        protected override void InitObjData()
        {
            RequestListFriendDataInit();
            RequestCardFriendDataInit();

            SetFriendListTitleText();
            uiFriendPopupTopSettings[0].SortListObj();
        }

        void RequestListFriendDataInit()
        {
            foreach (var friendBaseData in Managers.FriendMgr.GetRequestWaitDataDic().Values)
            {
                if (_requestPopupListObjDic.ContainsKey(friendBaseData.CUID))
                    continue;

                AddRequestListFriendData(friendBaseData);
            }
        }

        void RequestCardFriendDataInit()
        {
            foreach (var friendBaseData in Managers.FriendMgr.GetRequestWaitDataDic().Values)
            {
                if (_requestPopupCardObjDic.ContainsKey(friendBaseData.CUID))
                    continue;

                AddRequestCardFriendData(friendBaseData);
            }
        }

        void AddObjData(FriendBaseData friendBaseData)
        {
            AddRequestListFriendData(friendBaseData);
            AddRequestCardFriendData(friendBaseData);
        }

        void AddRequestListFriendData(FriendBaseData friendBaseData)
        {
            UIFriendPopupListObjFriendRequest friendObj = Managers.ResourceMgr
                .Instantiate(_requestListObjPrefabPath, contentGroup[0].list.content.transform)
                .GetComponent<UIFriendPopupListObjFriendRequest>();
            friendObj.Init(friendBaseData);
            
            AddRequestPopupListObj(friendBaseData.CUID, friendObj, FriendObjType.LIST);

            uiFriendPopupTopSettings[0].SortListObj();
        }

        void AddRequestCardFriendData(FriendBaseData friendBaseData)
        {
            UIFriendPopupListObjFriendRequest friendObj = Managers.ResourceMgr
                .Instantiate(_requestCardObjPrefabPath, contentGroup[0].card.content.transform)
                .GetComponent<UIFriendPopupListObjFriendRequest>();
            friendObj.Init(friendBaseData);
            
            AddRequestPopupListObj(friendBaseData.CUID, friendObj, FriendObjType.CARD);

            uiFriendPopupTopSettings[0].SortListObj();
        }

        public void SetFriendListTitleText()
        {
            requestCountText.text =
                string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_FRIENDREQUESTLIST_COUNT)}",
                    Managers.FriendMgr.GetRequestWaitDataDic().Count,
                    Managers.TableMgr.CommonGDT.CommonTable.FriendRequestListMaxCount);
        }

        [SerializeField]
        GameObject requestListObject;

        [SerializeField]
        TMPro.TextMeshProUGUI noneRequestFriendText;

        void ActiveRequestListObject()
        {
            bool enable = Managers.FriendMgr.GetRequestWaitDataDic().Count > 0 ? true : false;
            requestListObject.SetActive(enable);
            noneRequestFriendText.gameObject.SetActive(!enable);
        }

        void OnClickRefreshButton()
        {
            RefreshButtonInteractable(false);
            Refresh();
            RefreshButtonUniTask(_refreshCoolTime).Forget();
        }

        #region 콜백

        void ServerUpdateFriendRequestWaitDataDicRemoveAction(long cuid)
        {
            RemoveRequestPopupListObj(cuid);
            ActiveRequestListObject();
            SetFriendListTitleText();
        }

        void LocalUpdateFriendRequestWaitDicRemoveAction(long cuid)
        {
            RemoveRequestPopupListObj(cuid);
            ActiveRequestListObject();
            SetFriendListTitleText();
        }

        void LocalUpdateFriendRequestWaitDicAddAction(FriendBaseData friendBaseData)
        {
            AddObjData(friendBaseData);
            ActiveRequestListObject();
            SetFriendListTitleText();
        }

        #endregion
    }
}
