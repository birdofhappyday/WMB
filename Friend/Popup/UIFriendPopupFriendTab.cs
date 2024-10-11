using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using SharedCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnotherWorld.UI.Friend.Popup
{
    /// <summary>
    /// 친구목록 탭 부분
    /// 친구버튼 클릭시 표시해주는 탭이다.
    /// </summary>
    public class UIFriendPopupFriendTab : UIFriendPopupTab
    {
        Dictionary<long, UIFriendPopupListObjFriend> _friendPopupListObjDic =
            new Dictionary<long, UIFriendPopupListObjFriend>();

        Dictionary<long, UIFriendPopupListObjFriend> _friendPopupCardObjDic =
            new Dictionary<long, UIFriendPopupListObjFriend>();

        public override List<UIFriendPopupListObj> GetListObj()
        {
            if (ObjType == FriendObjType.LIST)
                return _friendPopupListObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
            else
                return _friendPopupCardObjDic.Values.Cast<UIFriendPopupListObj>().ToList();
        }

        public UIFriendPopupListObjFriend GetFriendPopupListObj(long cuid, FriendObjType objType)
        {
            if (objType == FriendObjType.LIST)
            {
                if (!_friendPopupListObjDic.ContainsKey(cuid))
                    return null;

                return _friendPopupListObjDic[cuid];
            }
            else
            {
                if (!_friendPopupCardObjDic.ContainsKey(cuid))
                    return null;

                return _friendPopupCardObjDic[cuid];
            }
        }

        public void AddFriendPopupListObj(long cuid, UIFriendPopupListObjFriend uiFriendPopupListObjFriend,
            FriendObjType friendObjType)
        {
            if (friendObjType == FriendObjType.LIST)
                _friendPopupListObjDic.Add(cuid, uiFriendPopupListObjFriend);
            else if (friendObjType == FriendObjType.CARD)
                _friendPopupCardObjDic.Add(cuid, uiFriendPopupListObjFriend);
        }

        public void RemoveFriendPopupListObj(long cuid)
        {
            UIFriendPopupListObjFriend friendPopupObj = GetFriendPopupListObj(cuid, FriendObjType.LIST);

            if (friendPopupObj == null)
                return;

            SendFriendPointListAddAction(cuid, false);
            ReceiveFriendPointListAddAction(cuid, false);
            
            friendPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(friendPopupObj.gameObject);

            _friendPopupListObjDic.Remove(cuid);

            friendPopupObj = GetFriendPopupListObj(cuid, FriendObjType.CARD);
            friendPopupObj.Clear();
            Managers.ResourceMgr.ReleaseInstance(friendPopupObj.gameObject);

            _friendPopupCardObjDic.Remove(cuid);
        }

        public void ClearFriendPopupListObj()
        {
            foreach (var objData in _friendPopupListObjDic)
            {
                objData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(objData.Value.gameObject);
            }

            foreach (var objData in _friendPopupCardObjDic)
            {
                objData.Value.Clear();
                Managers.ResourceMgr.ReleaseInstance(objData.Value.gameObject);
            }

            _friendPopupListObjDic.Clear();
            _friendPopupCardObjDic.Clear();
        }

        #region override

        protected override void Init()
        {
            base.Init();

            TextInit();
            FriendPointAllReceiveAndSendEnable(false);
            ButtonOnClickInit();

            Managers.FriendMgr.ServerUpdateFriendDataDicAddAction += ServerUpdateFriendDataAdd;
            Managers.FriendMgr.ServerUpdateFriendDataDicRemoveAction += ServerUpdateFriendDataRemove;
            Managers.FriendMgr.ServerUpdateFriendDataDicUpdateAction += ServerUpdateFriendDataUpdate;
            Managers.FriendMgr.LocalUpdateFriendDataDicAddAction += LocalUpdateFriendDataAdd;
            Managers.FriendMgr.LocalUpdateFriendDataDicRemoveAction += LocalUpdateFriendDataRemove;
            Managers.FriendMgr.LocalUpdateFriendPointDataAction += LocalUpdateFriendPointData;
            Managers.FriendMgr.ServerUpdateFriendPointDataDicUpdateAction += ServerUpdateFriendPointDataDicUpdateAction;

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
                if (Managers.FriendMgr != null)
                {
                    Managers.FriendMgr.ServerUpdateFriendDataDicAddAction -= ServerUpdateFriendDataAdd;
                    Managers.FriendMgr.ServerUpdateFriendDataDicRemoveAction -= ServerUpdateFriendDataRemove;
                    Managers.FriendMgr.ServerUpdateFriendDataDicUpdateAction -= ServerUpdateFriendDataUpdate;
                    Managers.FriendMgr.LocalUpdateFriendDataDicAddAction -= LocalUpdateFriendDataAdd;
                    Managers.FriendMgr.LocalUpdateFriendDataDicRemoveAction -= LocalUpdateFriendDataRemove;
                    Managers.FriendMgr.LocalUpdateFriendPointDataAction -= LocalUpdateFriendPointData;
                    Managers.FriendMgr.ServerUpdateFriendPointDataDicUpdateAction -=
                        ServerUpdateFriendPointDataDicUpdateAction;
                }

                return true;
            }

            return false;
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            ClearFriendPopupListObj();
            _sendFriendPointCuids.Clear();
            _receiveFriendPointCuids.Clear();
            isOpen = false;
        }

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);

            InitObjData();
            Refresh();
        }

        public override void Refresh(params object[] valueArray)
        {
            foreach (var listObj in _friendPopupListObjDic.Values)
            {
                listObj.Refresh();
            }

            foreach (var listObj in _friendPopupCardObjDic.Values)
            {
                listObj.Refresh();
            }
        }

        #endregion

        #region 텍스트 초기화

        /// <summary>
        /// 한번 설정하면 바꾸지 않을 텍스트들.
        /// 나중에 설정에서 언어변경등이 생기면 한번 더 부를 필요가 있다.
        /// </summary>
        void TextInit()
        {
            allReceiveAndSendFriendPoint.SetText(UI_Localize.LanguageType.UI,
                FriendManager.FRIEND_BUTTON_RECEIVE_ALL_POINT);

            refreshButton.SetText(UI_Localize.LanguageType.UI, FriendManager.REFRESH_BUTTON);
        }

        void ButtonOnClickInit()
        {
            allReceiveAndSendFriendPoint.onClick.RemoveAllListeners();
            refreshButton.onClick.RemoveAllListeners();
            allReceiveAndSendFriendPoint.onClick.AddListener(OnClickAllFriendPointSendAndReceive);
            refreshButton.onClick.AddListener(OnClickRefreshButton);
        }

        #endregion

        #region 가변성 데이터(우정 포인트, 친구수)

        [SerializeField]
        TMPro.TextMeshProUGUI m_friendPopupListTitleText;

        public void SetTextFriendListTitle(int currentCount)
        {
            m_friendPopupListTitleText.text =
                string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_FRIENDLIST_COUNT)}", currentCount,
                    Managers.TableMgr.CommonGDT.CommonTable.FriendFriendListMaxCount);
        }

        #endregion

        #region 버튼 관련

        [SerializeField]
        ButtonBase allReceiveAndSendFriendPoint;

        void FriendPointAllReceiveAndSendEnable(bool enable)
        {
            allReceiveAndSendFriendPoint.interactable = enable;
        }

        List<long> _sendFriendPointCuids = new List<long>();
        List<long> _receiveFriendPointCuids = new List<long>();


        public void OnClickAllFriendPointSendAndReceive()
        {
            FriendPointAllReceiveAndSendEnable(false);

            if (Managers.FriendMgr.FriendPointSendCount >=
                Managers.TableMgr.CommonGDT.CommonTable.FriendFriendShipPointSendMax)
            {
                Managers.UIMgr.OpenPopupMessage(GDT.MessagePosition.MP_Top, UI_Localize.LanguageType.UI, 1, false,
                    FriendManager.FRIEND_FRIENDSHIPPOINT_DAILY_MAX_TEXT);
                return;
            }

            List<long> sendFriendCuids = new List<long>();
            List<long> receiveFriendCuids = new List<long>();
            foreach (var friendObj in GetListObj())
            {
                if (((UIFriendPopupListObjFriend)friendObj).GetSendButtonInteractable())
                {
                    var friendPopupListObj = GetFriendPopupListObj(friendObj.FriendData.BaseData.CUID, FriendObjType.LIST);
                    if (friendPopupListObj is not null)
                    {
                        friendPopupListObj.SetSendButtonInteractable(false);
                        var friendPopupCardObj = GetFriendPopupListObj(friendObj.FriendData.BaseData.CUID, FriendObjType.CARD);
                        friendPopupCardObj.SetSendButtonInteractable(false);

                        sendFriendCuids.Add(friendObj.FriendData.BaseData.CUID);

                        if (Managers.FriendMgr.FriendPointSendCount + sendFriendCuids.Count >=
                            Managers.TableMgr.CommonGDT.CommonTable.FriendFriendShipPointSendMax)
                            break;
                    }
                }
            }

            // foreach (var cuid in _receiveFriendPointCuids)
            // {
            //     var friendPopupListObj = GetFriendPopupListObj(cuid, FriendObjType.LIST);
            //     if (friendPopupListObj != null)
            //     {
            //         friendPopupListObj.SetReceiveButtonInteractable(false);
            //         var friendPopupCardObj = GetFriendPopupListObj(cuid, FriendObjType.CARD);
            //         friendPopupCardObj.SetReceiveButtonInteractable(false);
            //     }
            // }
            
            foreach (var friendObj in GetListObj())
            {
                if (((UIFriendPopupListObjFriend)friendObj).GetReceiveButtonInteractable())
                {
                    var friendPopupListObj = GetFriendPopupListObj(friendObj.FriendData.BaseData.CUID, FriendObjType.LIST);
                    if (friendPopupListObj != null)
                    {
                        friendPopupListObj.SetReceiveButtonInteractable(false);
                        var friendPopupCardObj = GetFriendPopupListObj(friendObj.FriendData.BaseData.CUID, FriendObjType.CARD);
                        friendPopupCardObj.SetReceiveButtonInteractable(false);
                        
                        receiveFriendCuids.Add(friendObj.FriendData.BaseData.CUID);
                    }
                }
            }

            Managers.FriendMgr.SetFriendPointCountDelta(sendFriendCuids.Count);

            Managers.NetworkMgr.GameClient.ResFriendPointAllAction += ResFriendPointAll;
            Managers.NetworkMgr.GameClient.Proxy.Req_FriendPointAll(sendFriendCuids, receiveFriendCuids);
        }

        void ResFriendPointAll(List<FriendPointData> friendPointDatas, CurrencyData currencyData)
        {
            Managers.NetworkMgr.GameClient.ResFriendPointAllAction -= ResFriendPointAll;

            foreach (var friendPointData in friendPointDatas)
            {
                if (_friendPopupListObjDic.TryGetValue(friendPointData.CUID,
                        out UIFriendPopupListObjFriend friendPopupListobj))
                    friendPopupListobj.PointDataUpdate(friendPointData);
                else
                    Debug.LogError("ksmksmksm 없는 cuid");
            }

            Managers.UIMgr.OpenPopup<Popup_FriendPoint>().OnPopup_FriendPoint(
                FriendManager.POPUP_FRIEND_FRIENDSHIPPOINT_TEXT, FriendManager.POPUP_CHECK_BUTTON, currencyData);
        }

        void OnClickRefreshButton()
        {
            RefreshButtonInteractable(false);
            Refresh();
            RefreshButtonUniTask(_refreshCoolTime).Forget();
        }

        #endregion

        #region 콜백

        void SendFriendPointListAddAction(long cuid, bool add)
        {
            if (add)
            {
                if (_sendFriendPointCuids.Contains(cuid))
                    return;

                _sendFriendPointCuids.Add(cuid);
            }
            else
            {
                if (!_sendFriendPointCuids.Contains(cuid))
                    return;

                _sendFriendPointCuids.Remove(cuid);
            }

            FriendPointAllReceiveAndSendEnable((_sendFriendPointCuids.Count != 0 && Managers.FriendMgr.FriendPointSendCount <
                                                   Managers.TableMgr.CommonGDT.CommonTable
                                                       .FriendFriendShipPointSendMax) ||
                                               _receiveFriendPointCuids.Count != 0);
        }

        void ReceiveFriendPointListAddAction(long cuid, bool add)
        {
            if (add)
            {
                if (_receiveFriendPointCuids.Contains(cuid))
                    return;

                _receiveFriendPointCuids.Add(cuid);
            }
            else
            {
                if (!_receiveFriendPointCuids.Contains(cuid))
                    return;

                _receiveFriendPointCuids.Remove(cuid);
            }

            FriendPointAllReceiveAndSendEnable((_sendFriendPointCuids.Count != 0 && Managers.FriendMgr.FriendPointSendCount <
                                                   Managers.TableMgr.CommonGDT.CommonTable
                                                       .FriendFriendShipPointSendMax) ||
                                               _receiveFriendPointCuids.Count != 0);
        }

        #endregion

        #region 그외

        const string _listPrePabPath = "UI/Friend/UIFriendPopupListObjFriend";
        const string _cardPrePabPath = "UI/Friend/UIFriendPopupListObjFriend_Card";


        protected override void InitObjData()
        {
            base.InitObjData();

            LineListInit();
            CardListInit();
            FriendCountSetting();

            uiFriendPopupTopSettings[0].SortListObj();
        }

        void LineListInit()
        {
            foreach (var friendData in Managers.FriendMgr.GetFriendDataDic().Values)
            {
                if (_friendPopupListObjDic.ContainsKey(friendData.BaseData.CUID))
                    continue;

                LineAddData(friendData);
            }
        }

        void CardListInit()
        {
            foreach (var friendData in Managers.FriendMgr.GetFriendDataDic().Values)
            {
                if (_friendPopupCardObjDic.ContainsKey(friendData.BaseData.CUID))
                    continue;

                CardAddData(friendData);
            }
        }

        void AddObjData(FriendData friendData)
        {
            LineAddData(friendData);
            CardAddData(friendData);

            uiFriendPopupTopSettings[0].SortListObj();
        }

        void LineAddData(FriendData friendData)
        {
            UIFriendPopupListObjFriend friendObj = Managers.ResourceMgr
                .Instantiate(_listPrePabPath, contentGroup[0].list.content.transform)
                .GetComponent<UIFriendPopupListObjFriend>();
            friendObj.SendPossibleAction = SendFriendPointListAddAction;
            friendObj.ReceivePossibleAction = ReceiveFriendPointListAddAction;
            friendObj.Init(friendData);
            AddFriendPopupListObj(friendData.BaseData.CUID, friendObj, FriendObjType.LIST);
        }

        void CardAddData(FriendData friendData)
        {
            UIFriendPopupCardObjFriend friendObj = Managers.ResourceMgr
                .Instantiate(_cardPrePabPath, contentGroup[0].card.content.transform)
                .GetComponent<UIFriendPopupCardObjFriend>();
            friendObj.SendPossibleAction = SendFriendPointListAddAction;
            friendObj.ReceivePossibleAction = ReceiveFriendPointListAddAction;
            friendObj.Init(friendData);
            AddFriendPopupListObj(friendData.BaseData.CUID, friendObj, FriendObjType.CARD);
        }

        [SerializeField]
        GameObject friendList =
            null;

        [SerializeField]
        TMPro.TextMeshProUGUI noneFriendText =
            null;

        void ActiveFriendList(bool active)
        {
            friendList.SetActive(active);
            noneFriendText.gameObject.SetActive(!active);
        }

        bool FriendCountSetting()
        {
            bool active = Managers.FriendMgr.GetFriendDataDic().Count != 0;

            ActiveFriendList(active);
            //EnableFriendDelete(_active);
            SetTextFriendListTitle(Managers.FriendMgr.GetFriendDataDic().Count);

            return active;
        }

        void ServerUpdateFriendDataAdd(FriendData friendData)
        {
            AddObjData(friendData);
            FriendCountSetting();
        }

        void ServerUpdateFriendDataRemove(long cuid)
        {
            RemoveFriendPopupListObj(cuid);

            FriendCountSetting();
        }

        void ServerUpdateFriendDataUpdate(FriendBaseData friendData)
        {
            GetFriendPopupListObj(friendData.CUID, FriendObjType.LIST)?.Init(friendData);
            GetFriendPopupListObj(friendData.CUID, FriendObjType.CARD)?.Init(friendData);
        }

        void ServerUpdateFriendPointDataDicUpdateAction(FriendPointData friendPointData)
        {
            GetFriendPopupListObj(friendPointData.CUID, FriendObjType.LIST)?.PointDataUpdate(friendPointData);
            GetFriendPopupListObj(friendPointData.CUID, FriendObjType.CARD)?.PointDataUpdate(friendPointData);
        }

        void LocalUpdateFriendDataAdd(FriendData friendData)
        {
            AddObjData(friendData);
            FriendCountSetting();
        }

        void LocalUpdateFriendDataRemove(long cuid)
        {
            RemoveFriendPopupListObj(cuid);
            FriendCountSetting();
        }

        void LocalUpdateFriendPointData(FriendPointData friendPointData)
        {
            GetFriendPopupListObj(friendPointData.CUID, FriendObjType.CARD)?.PointDataUpdate(friendPointData);
            GetFriendPopupListObj(friendPointData.CUID, FriendObjType.LIST)?.PointDataUpdate(friendPointData);
        }

        #endregion
    }
}
