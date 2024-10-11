using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using SharedCode;
using UnityEngine;

namespace AnotherWorld.UI.Friend.Popup
{
    public class UIFriendPopupBlockTab : UIFriendPopupTab
    {
        Dictionary<long, UIFriendPopupListObjFriendBlock> _blockPopupListObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendBlock>();

        Dictionary<long, UIFriendPopupListObjFriendBlock> _blockPopupCardObjDic =
            new Dictionary<long, UIFriendPopupListObjFriendBlock>();

        public override List<UIFriendPopupListObj> GetListObj()
        {
            if (ObjType == FriendObjType.LIST)
                return _blockPopupListObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
            else
                return _blockPopupCardObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
        }

        public UIFriendPopupListObjFriendBlock GetBlockPopupListObj(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_blockPopupListObjDic.ContainsKey(cuid))
                    return null;

                return _blockPopupListObjDic[cuid];
            }
            else
            {
                if (!_blockPopupCardObjDic.ContainsKey(cuid))
                    return null;

                return _blockPopupCardObjDic[cuid];
            }
        }

        public void AddBlockPopupListObj(long cuid, UIFriendPopupListObjFriendBlock uiRequestPopupListObjFriend,
            FriendObjType friendObjType)
        {
            if (friendObjType == FriendObjType.LIST)
                _blockPopupListObjDic.Add(cuid, uiRequestPopupListObjFriend);
            else if (friendObjType == FriendObjType.CARD)
                _blockPopupCardObjDic.Add(cuid, uiRequestPopupListObjFriend);
        }

        public void RemoveBlockPopupListObj(long cuid)
        {
            UIFriendPopupListObjFriendBlock requestPopupObj = GetBlockPopupListObj(cuid, FriendObjType.LIST);

            if (requestPopupObj == null)
                return;
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _blockPopupListObjDic.Remove(cuid);

            requestPopupObj = GetBlockPopupListObj(cuid, FriendObjType.CARD);
            
            requestPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(requestPopupObj.gameObject);

            _blockPopupCardObjDic.Remove(cuid);
        }

        private void ClearBlockObjDic()
        {
            foreach (var friendBlockData in _blockPopupListObjDic)
            {
                friendBlockData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendBlockData.Value.gameObject);
            }

            foreach (var friendBlockData in _blockPopupCardObjDic)
            {
                friendBlockData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(friendBlockData.Value.gameObject);
            }

            _blockPopupListObjDic.Clear();
            _blockPopupCardObjDic.Clear();
        }

        public override void Refresh(params object[] valueArray)
        {
            foreach (var blockFriend in _blockPopupListObjDic)
            {
                blockFriend.Value.Refresh();
            }

            foreach (var blockFriend in _blockPopupCardObjDic)
            {
                blockFriend.Value.Refresh();
            }
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            ClearBlockObjDic();
            isOpen = false;
        }

        protected override void Init()
        {
            base.Init();
            TextInit();
            InitCallBack();

            refreshButton.onClick.AddListener(OnClickRefreshButton);

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

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);
            Refresh();
            InitObjData();
        }

        protected override bool DestoryInit()
        {
            if (base.DestoryInit())
            {
                Managers.FriendMgr.LocalUpdateFriendBlockDicAddAction -= LocalUpdateFriendBlockDicAddAction;
                Managers.FriendMgr.LocalUpdateFriendBlockDicRemoveAction -= LocalUpdateFriendBlockDicRemoveAction;
                return true;
            }

            return false;
        }

        void TextInit()
        {
            refreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
        }

        void InitCallBack()
        {
            Managers.FriendMgr.LocalUpdateFriendBlockDicAddAction += LocalUpdateFriendBlockDicAddAction;
            Managers.FriendMgr.LocalUpdateFriendBlockDicRemoveAction += LocalUpdateFriendBlockDicRemoveAction;
        }

        [SerializeField]
        TMPro.TextMeshProUGUI noneFriendText;

        void CountSetting()
        {
            InitTitleText();
            ActiveListObject();
        }

        [SerializeField]
        TMPro.TextMeshProUGUI titleText;

        void InitTitleText()
        {
            titleText.text = string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_BLOCKLIST_MAX)}",
                Managers.FriendMgr.CountFriendBlockDataDic(),
                Managers.TableMgr.CommonGDT.CommonTable.FriendBlockListMaxCount);
        }

        [SerializeField]
        GameObject listObject;

        void ActiveListObject()
        {
            bool _active = Managers.FriendMgr.CountFriendBlockDataDic() != 0;

            listObject.SetActive(_active);
            noneFriendText.gameObject.SetActive(!_active);
        }

        const string _listObjPath = "UI/Friend/UIFriendPopupListObjFriendBlock";
        const string _cardObjPath = "UI/Friend/UIFriendPopupListObjFriendBlock_Card";

        protected override void InitObjData()
        {
            base.InitObjData();

            InitListData();
            InitCardData();

            CountSetting();
            uiFriendPopupTopSettings[0].SortListObj();
        }

        void InitListData()
        {
            foreach (FriendBlockData friendBlockData in Managers.FriendMgr.GetFriendBlockDataDic().Values)
            {
                if (_blockPopupListObjDic.ContainsKey(friendBlockData.CUID))
                    continue;

                AddListData(friendBlockData);
            }
        }

        void InitCardData()
        {
            foreach (FriendBlockData friendBlockData in Managers.FriendMgr.GetFriendBlockDataDic().Values)
            {
                if (_blockPopupCardObjDic.ContainsKey(friendBlockData.CUID))
                    continue;

                AddCardData(friendBlockData);
            }
        }

        void AddObjData(FriendBlockData friendBlockData)
        {
            AddListData(friendBlockData);
            AddCardData(friendBlockData);

            uiFriendPopupTopSettings[0].SortListObj();
        }

        void AddListData(FriendBlockData friendBlockData)
        {
            UIFriendPopupListObjFriendBlock friendObj = Managers.ResourceMgr
                .Instantiate(_listObjPath, contentGroup[0].list.content.transform)
                .GetComponent<UIFriendPopupListObjFriendBlock>();
            friendObj.Init(friendBlockData);

            AddBlockPopupListObj(friendBlockData.CUID, friendObj, FriendObjType.LIST);
        }

        void AddCardData(FriendBlockData friendBlockData)
        {
            UIFriendPopupListObjFriendBlock friendObj = Managers.ResourceMgr
                .Instantiate(_cardObjPath, contentGroup[0].card.content.transform)
                .GetComponent<UIFriendPopupListObjFriendBlock>();
            friendObj.Init(friendBlockData);

            AddBlockPopupListObj(friendBlockData.CUID, friendObj, FriendObjType.CARD);
        }

        void OnClickRefreshButton()
        {
            RefreshButtonInteractable(false);
            Refresh();
            RefreshButtonUniTask(_refreshCoolTime).Forget();
        }

        #region 콜백

        void LocalUpdateFriendBlockDicAddAction(FriendBlockData friendBlockData)
        {
            AddObjData(friendBlockData);

            CountSetting();
        }

        void LocalUpdateFriendBlockDicRemoveAction(long cuid)
        {
            RemoveBlockPopupListObj(cuid);

            CountSetting();
        }

        #endregion
    }
}
