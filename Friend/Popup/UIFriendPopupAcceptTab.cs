using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Popup
{
    public class UIFriendPopupAcceptTab : UIFriendPopupTab
    {
        private Dictionary<long, UIFriendPopupListObjFriendAccept> _acceptPopupListObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendAccept>();

        private Dictionary<long, UIFriendPopupListObjFriendAccept> _acceptPopupCardObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendAccept>();

        public override List<UIFriendPopupListObj> GetListObj()
        {
            if (ObjType == FriendObjType.LIST)
                return _acceptPopupListObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
            else
                return _acceptPopupCardObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
        }

        public UIFriendPopupListObjFriendAccept GetAcceptPopupListObj(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_acceptPopupListObjDic.ContainsKey(cuid))
                    return null;

                return _acceptPopupListObjDic[cuid];
            }
            else
            {
                if (!_acceptPopupCardObjDic.ContainsKey(cuid))
                    return null;

                return _acceptPopupCardObjDic[cuid];
            }
        }

        public void AddAcceptPopupListObj(long cuid, UIFriendPopupListObjFriendAccept uiAcceptPopupListObjFriend,
            FriendObjType friendObjType)
        {
            if (friendObjType == FriendObjType.LIST)
                _acceptPopupListObjDic.Add(cuid, uiAcceptPopupListObjFriend);
            else if (friendObjType == FriendObjType.CARD)
                _acceptPopupCardObjDic.Add(cuid, uiAcceptPopupListObjFriend);
        }

        public void RemoveAcceptPopupListObj(long cuid)
        {
            UIFriendPopupListObjFriendAccept requestPopupObj = GetAcceptPopupListObj(cuid, FriendObjType.LIST);

            if (requestPopupObj == null)
                return;
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _acceptPopupListObjDic.Remove(cuid);

            requestPopupObj = GetAcceptPopupListObj(cuid, FriendObjType.CARD);
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _acceptPopupCardObjDic.Remove(cuid);
        }
        
        public UIFriendPopupListObjFriendAccept GetAcceptFriendObjDic(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_acceptPopupListObjDic.ContainsKey(cuid))
                    return null;

                return _acceptPopupListObjDic[cuid];
            }
            else
            {
                if (!_acceptPopupCardObjDic.ContainsKey(cuid))
                    return null;

                return _acceptPopupCardObjDic[cuid];
            }
        }

        private void ClearAcceptFriendObjDic()
        {
            foreach (var friendAcceptData in _acceptPopupListObjDic)
            {
                friendAcceptData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendAcceptData.Value.gameObject);
            }

            foreach (var friendAcceptData in _acceptPopupCardObjDic)
            {
                friendAcceptData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendAcceptData.Value.gameObject);
            }

            _acceptPopupListObjDic.Clear();
            _acceptPopupCardObjDic.Clear();
        }

        public override void Refresh(params object[] valueArray)
        {
            foreach (var acceptFriend in _acceptPopupListObjDic)
            {
                acceptFriend.Value.Refresh();
            }

            foreach (var acceptFriend in _acceptPopupCardObjDic)
            {
                acceptFriend.Value.Refresh();
            }
        }

        protected override void Init()
        {
            base.Init();

            TextInit();
            FriendCountSetting();
            refreshButton.onClick.AddListener(OnClcikRefreshButton);
            Managers.FriendMgr.ServerUpdateFriendAcceptWaitDataDicAddAction += ServerUpdateFriendAcceptWaitDataDicAddAction;
            Managers.FriendMgr.ServerUpdateFriendAcceptWaitDataDicRemoveAction +=
                ServerUpdateFriendAcceptWaitDataDicRemoveAction;
            Managers.FriendMgr.LocalUpdateFriendAcceptWaitDicRemoveAction += LocalUpdateFriendAcceptWaitDicRemoveAction;

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

        public override void GameToLobby()
        {
            base.GameToLobby();
            ClearAcceptFriendObjDic();
            isOpen = false;
        }

        protected override bool DestoryInit()
        {
            if (base.DestoryInit())
            {
                if (Managers.FriendMgr == null)
                    return false;

                Managers.FriendMgr.ServerUpdateFriendAcceptWaitDataDicAddAction -=
                    ServerUpdateFriendAcceptWaitDataDicAddAction;
                Managers.FriendMgr.ServerUpdateFriendAcceptWaitDataDicRemoveAction -=
                    ServerUpdateFriendAcceptWaitDataDicRemoveAction;
                Managers.FriendMgr.LocalUpdateFriendAcceptWaitDicRemoveAction -= LocalUpdateFriendAcceptWaitDicRemoveAction;
                return true;
            }

            return false;
        }

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);
            Refresh();
            InitObjData();
        }

        void TextInit()
        {
            refreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
        }

        const string listObjPath = "UI/Friend/UIFriendPopupListObjFriendAccept";
        const string cardObjPath = "UI/Friend/UIFriendPopupListObjFriendAccept_Card";

        protected override void InitObjData()
        {
            InitListData();
            InitCardData();
            FriendCountSetting();
            uiFriendPopupTopSettings[0].SortListObj();
        }

        void InitListData()
        {
            foreach (FriendBaseData friendBaseData in Managers.FriendMgr.GetAcceptWaitDataDic().Values)
            {
                if (_acceptPopupListObjDic.ContainsKey(friendBaseData.CUID))
                    continue;

                AddListData(friendBaseData);
            }
        }

        void InitCardData()
        {
            foreach (FriendBaseData friendBaseData in Managers.FriendMgr.GetAcceptWaitDataDic().Values)
            {
                if (_acceptPopupCardObjDic.ContainsKey(friendBaseData.CUID))
                    continue;

                AddCardData(friendBaseData);
            }
        }

        void AddObjData(FriendBaseData friendBaseData)
        {
            AddListData(friendBaseData);
            AddCardData(friendBaseData);

            uiFriendPopupTopSettings[0].SortListObj();
        }

        void AddListData(FriendBaseData friendBaseData)
        {
            UIFriendPopupListObjFriendAccept friendObj = Managers.ResourceMgr
                .Instantiate(listObjPath, contentGroup[0].list.content.transform)
                .GetComponent<UIFriendPopupListObjFriendAccept>();
            friendObj.Init(friendBaseData);

            AddAcceptPopupListObj(friendBaseData.CUID, friendObj, FriendObjType.LIST);
        }

        void AddCardData(FriendBaseData friendBaseData)
        {
            UIFriendPopupListObjFriendAccept friendObj = Managers.ResourceMgr
                .Instantiate(cardObjPath, contentGroup[0].card.content.transform)
                .GetComponent<UIFriendPopupListObjFriendAccept>();
            friendObj.Init(friendBaseData);

            AddAcceptPopupListObj(friendBaseData.CUID, friendObj, FriendObjType.CARD);
        }

        [SerializeField]
        TMPro.TextMeshProUGUI requestQuestCountText;

        void InitTextTitle(int count)
        {
            requestQuestCountText.text = string.Format(
                $"{UI_Localize.GetLanguage(FriendManager.FRIEND_RECEPTIONLIST_MAX)}", count,
                Managers.TableMgr.CommonGDT.CommonTable.FriendReceptionListMaxCount);
        }

        [SerializeField]
        TMPro.TextMeshProUGUI noneFriendText;

        [SerializeField]
        GameObject listObj;

        void ActiveListObj(bool active)
        {
            listObj.SetActive(active);
            noneFriendText.gameObject.SetActive(!active);
        }

        void FriendCountSetting()
        {
            InitTextTitle(Managers.FriendMgr.GetAcceptWaitDataDic().Count);
            ActiveListObj(Managers.FriendMgr.GetAcceptWaitDataDic().Count != 0);
        }

        void ServerUpdateFriendAcceptWaitDataDicRemoveAction(long cuid)
        {
            RemoveAcceptPopupListObj(cuid);
            FriendCountSetting();
        }

        void ServerUpdateFriendAcceptWaitDataDicAddAction(FriendBaseData friendBaseData)
        {
            AddObjData(friendBaseData);
            FriendCountSetting();
        }

        void LocalUpdateFriendAcceptWaitDicRemoveAction(long cuid)
        {
            RemoveAcceptPopupListObj(cuid);
            FriendCountSetting();
        }

        void OnClcikRefreshButton()
        {
            RefreshButtonInteractable(false);
            Refresh(null);
            RefreshButtonUniTask(_refreshCoolTime).Forget();
        }
    }
}
