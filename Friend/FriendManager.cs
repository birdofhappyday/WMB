using SharedCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GDT;
using UnityEngine;
using UnityEngine.Events;

public class FriendManager
{
    public int FriendPointSendCount { get; private set; }

    public const string FriendPopupPath = "UI/UIFriendPopup";
    public const string FriendPopupName = "UIFriendPopup";
    
    // 값 직접 변경하지 마세요.
    private Dictionary<long, FriendData> _friendDataDic = new Dictionary<long, FriendData>();
    private Dictionary<long, FriendBaseData> _friendRequestWaitDataDic = new Dictionary<long, FriendBaseData>();
    private Dictionary<long, FriendBaseData> _friendAcceptWaitDataDic = new Dictionary<long, FriendBaseData>();
    private Dictionary<long, FriendBlockData> _friendBlockDataDic = new Dictionary<long, FriendBlockData>();
    
    
    // 서버로부터 받은 정보 초기화
    public void SetFriendDataFromServer(List<FriendData> friendDatas, List<FriendBaseData> requestWaitDatas, List<FriendBaseData> acceptWaitDatas, List<FriendBlockData> blockDatas, int pointSendCount)
    {
        _friendDataDic.Clear();
        _friendRequestWaitDataDic.Clear();
        _friendAcceptWaitDataDic.Clear();
        _friendBlockDataDic.Clear();
        FriendPointSendCount = 0;

        foreach (FriendData friendData in friendDatas)
            _friendDataDic.Add(friendData.BaseData.CUID, friendData);

        foreach (FriendBaseData friendBaseData in requestWaitDatas)
            _friendRequestWaitDataDic.Add(friendBaseData.CUID, friendBaseData);

        foreach (FriendBaseData friendBaseData in acceptWaitDatas)
            _friendAcceptWaitDataDic.Add(friendBaseData.CUID, friendBaseData);

        foreach (FriendBlockData friendBlockData in blockDatas)
            _friendBlockDataDic.Add(friendBlockData.CUID, friendBlockData);

        FriendPointSendCount = pointSendCount;
        FriendPointSendCountChange?.Invoke(FriendPointSendCount);
    }

    // 서버로부터 받은 정보 업데이트
    #region 서버 업데이트
    public void UpdateFriendDataFromServer(FriendState friendState, FriendBaseData friendBaseData)
    {
        switch (friendState)
        {
            case FriendState.Friend:

                if (_friendRequestWaitDataDic.ContainsKey(friendBaseData.CUID))
                {
                    ServerUpdateFriendRequestWaitDataDicRemove(friendBaseData.CUID);
                    FriendData friendData = new FriendData();
                    friendData.BaseData = friendBaseData;
                    ServerUpdateFriendDataDicAdd(friendData);
                }
                else if (_friendDataDic.ContainsKey(friendBaseData.CUID))
                {
                    ServerUpdateFriendDataDicUpdate(friendBaseData);
                }
                else
                {
                    Debug.Log($"예상치 못한 경우 : friendState : {friendState} friendBaseData.CUID : {friendBaseData.CUID}");
                }
                break;

            case FriendState.AcceptWait:

                ServerUpdateFriendAcceptWaitDataDicAdd(friendBaseData);

                break;

            case FriendState.None:

                if (_friendRequestWaitDataDic.ContainsKey(friendBaseData.CUID))
                {
                    ServerUpdateFriendRequestWaitDataDicRemove(friendBaseData.CUID);
                }
                else if (_friendDataDic.ContainsKey(friendBaseData.CUID))
                {
                    ServerUpdateFriendDataDicRemove(friendBaseData.CUID);
                }
                else if (_friendAcceptWaitDataDic.ContainsKey(friendBaseData.CUID))
                {
                    ServerUpdateFriendAcceptWaitDataDicRemove(friendBaseData.CUID);
                }
                else
                {
                    Debug.Log($"예상치 못한 경우 : friendState : {friendState} friendBaseData.CUID : {friendBaseData.CUID}");
                }
                break;
        }
    }

    public UnityAction<long> ServerUpdateFriendRequestWaitDataDicRemoveAction = null;
    public void ServerUpdateFriendRequestWaitDataDicRemove(long cuid)
    {
        if (!_friendRequestWaitDataDic.ContainsKey(cuid))
            return;

        _friendRequestWaitDataDic.Remove(cuid);
        ServerUpdateFriendRequestWaitDataDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<FriendBaseData> ServerUpdateFriendAcceptWaitDataDicAddAction = null;
    public void ServerUpdateFriendAcceptWaitDataDicAdd(FriendBaseData friendBaseData)
    {
        if (_friendAcceptWaitDataDic.ContainsKey(friendBaseData.CUID))
            return;

        //레드닷 정보 갱신
        Managers.NewRedDotMgr.MakeFriendRedDot(ReddotSubType.RST_AcceptTab, friendBaseData.CUID);
        
        _friendAcceptWaitDataDic.Add(friendBaseData.CUID, friendBaseData);
        ServerUpdateFriendAcceptWaitDataDicAddAction?.Invoke(friendBaseData);
    }

    public UnityAction<long> ServerUpdateFriendAcceptWaitDataDicRemoveAction = null;
    public void ServerUpdateFriendAcceptWaitDataDicRemove(long cuid)
    {
        if (!_friendAcceptWaitDataDic.ContainsKey(cuid))
            return;

        //레드닷 제거.
        Managers.NewRedDotMgr.RemoveAcceptWaitFriendData(cuid);
        
        _friendAcceptWaitDataDic.Remove(cuid);
        ServerUpdateFriendAcceptWaitDataDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<long> ServerUpdateFriendDataDicRemoveAction = null;
    public void ServerUpdateFriendDataDicRemove(long cuid)
    {
        if (!_friendDataDic.ContainsKey(cuid))
            return;

        //레드닷 제거.
        Managers.NewRedDotMgr.RemoveFriendData(cuid);
        
        _friendDataDic.Remove(cuid);
        ServerUpdateFriendDataDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<FriendBaseData> ServerUpdateFriendDataDicUpdateAction = null;
    public void ServerUpdateFriendDataDicUpdate(FriendBaseData friendBaseData)
    {
        if (!_friendDataDic.ContainsKey(friendBaseData.CUID))
            return;

        _friendDataDic[friendBaseData.CUID].BaseData = friendBaseData;
        ServerUpdateFriendDataDicUpdateAction?.Invoke(friendBaseData);
    }

    public UnityAction<FriendPointData> ServerUpdateFriendPointDataDicUpdateAction = null;
    public void ServerUpdateFriendDataDicPointUpdate(FriendPointData friendPointData)
    {
        if (!_friendDataDic.ContainsKey(friendPointData.CUID))
            return;

        _friendDataDic[friendPointData.CUID].LikabilityData = friendPointData.LikabilityData;
        ServerUpdateFriendPointDataDicUpdateAction?.Invoke(friendPointData);
    }

    public UnityAction<FriendData> ServerUpdateFriendDataDicAddAction = null;
    public void ServerUpdateFriendDataDicAdd(FriendData friendData)
    {
        if (_friendDataDic.ContainsKey(friendData.BaseData.CUID))
            return;

        //레드닷 정보 갱신
        Managers.NewRedDotMgr.MakeFriendRedDot(ReddotSubType.RST_FriendTab, friendData.BaseData.CUID);
        
        _friendDataDic.Add(friendData.BaseData.CUID, friendData);
        ServerUpdateFriendDataDicAddAction?.Invoke(friendData);
    }

    #endregion

    #region 로컬 업데이트

    public UnityAction<FriendData> LocalUpdateFriendDataDicAddAction = null;
    public void LocalUpdateFriendDataDicAdd(FriendData friendData)
    {
        if (_friendDataDic.ContainsKey(friendData.BaseData.CUID))
            return;

        //레드닷 정보 갱신
        Managers.NewRedDotMgr.MakeFriendRedDot(ReddotSubType.RST_FriendTab, friendData.BaseData.CUID);
        
        _friendDataDic.Add(friendData.BaseData.CUID, friendData);
        LocalUpdateFriendDataDicAddAction?.Invoke(friendData);
    }

    public UnityAction<long> LocalUpdateFriendDataDicRemoveAction = null;
    public void LocalUpdateFriendDataDicRemove(long cuid)
    {
        if (!_friendDataDic.ContainsKey(cuid))
            return;

        //레드닷 정보 갱신
        Managers.NewRedDotMgr.RemoveFriendData(cuid);
        
        _friendDataDic.Remove(cuid);
        LocalUpdateFriendDataDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<FriendPointData> LocalUpdateFriendPointDataAction = null;
    public void LocalUpdateFriendPointData(FriendPointData friendPointData)
    {
        if (!_friendDataDic.ContainsKey(friendPointData.CUID))
            return;

        _friendDataDic[friendPointData.CUID].LikabilityData = friendPointData.LikabilityData;
        LocalUpdateFriendPointDataAction?.Invoke(friendPointData);
    }

    public UnityAction<FriendBaseData> LocalUpdateFriendRequestWaitDicAddAction = null;
    public void LocalUpdateFriendRequestWaitDicAdd(FriendBaseData friendBaseData)
    {
        if (_friendRequestWaitDataDic.ContainsKey(friendBaseData.CUID))
            return;

        _friendRequestWaitDataDic.Add(friendBaseData.CUID, friendBaseData);
        LocalUpdateFriendRequestWaitDicAddAction?.Invoke(friendBaseData);
    }

    public UnityAction<long> LocalUpdateFriendRequestWaitDicRemoveAction = null;
    public void LocalUpdateFriendRequestWaitDicRemove(long cuid)
    {
        if (!_friendRequestWaitDataDic.ContainsKey(cuid))
            return;

        _friendRequestWaitDataDic.Remove(cuid);
        LocalUpdateFriendRequestWaitDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<long> LocalUpdateFriendAcceptWaitDicRemoveAction = null;
    public void LocalUpdateFriendAcceptWaitDicRemove(long cuid)
    {
        if (!_friendAcceptWaitDataDic.ContainsKey(cuid))
            return;

        //레드닷 제거.
        Managers.NewRedDotMgr.RemoveAcceptWaitFriendData(cuid);
        
        _friendAcceptWaitDataDic.Remove(cuid);
        LocalUpdateFriendAcceptWaitDicRemoveAction?.Invoke(cuid);
    }

    public UnityAction<FriendBlockData> LocalUpdateFriendBlockDicAddAction = null;
    public void LocalUpdateFriendBlockDicAdd(FriendBlockData friendBlockData)
    {
        if (_friendBlockDataDic.ContainsKey(friendBlockData.CUID))
            return;

        _friendBlockDataDic.Add(friendBlockData.CUID, friendBlockData);
        LocalUpdateFriendBlockDicAddAction?.Invoke(friendBlockData);
    }

    public UnityAction<long> LocalUpdateFriendBlockDicRemoveAction = null;
    public void LocalUpdateFriendBlockDicRemove(long cuid)
    {
        if (!_friendBlockDataDic.ContainsKey(cuid))
            return;

        _friendBlockDataDic.Remove(cuid);
        LocalUpdateFriendBlockDicRemoveAction?.Invoke(cuid);
    }

    #endregion

    #region TableKey

    public const string FRIEND_PLAYERDATA_TAKELIKABILITY_MAX = "FRIEND_PLAYERDATA_TAKELIKABILITY_MAX";
    public const string FRIEND_PLAYERDATA_LIKABILITY = "FRIEND_PLAYERDATA_LIKABILITY";
    public const string FRIEND_PLAYERDATA_TAKELIKABILITY = "FRIEND_PLAYERDATA_TAKELIKABILITY";
    
    public const string POPUP_FRIEND_DELETE_FRIENDLIST_TEXT = "POPUP_FRIEND_DELETE_FRIENDLIST_TEXT";
    public const string POPUP_FRIEND_DELETE_FRIENDBLOCKLIST_TEXT = "POPUP_FRIEND_DELETE_FRIENDBLOCKLIST_TEXT"; 
    public const string POPUP_FRIEND_FRIENDSHIPPOINT_TEXT = "POPUP_FRIEND_FRIENDSHIPPOINT_TEXT";
    public const string POPUP_CHECK_BUTTON = "POPUP_CHECK_BUTTON";

    public const string POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_REFUSAL = "POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_REFUSAL";
    public const string POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_GET_ERROR = "POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_GET_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_OTHERPLAYERLIST_ERROR = "POPUP_FRIEND_TOASTMESSAGE_OTHERPLAYERLIST_ERROR";
    public const string POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_AGREE = "POPUP_FRIEND_RECEPTIONLIST_TOASTMESSAGE_REQUEST_AGREE";
    public const string POPUP_FRIEND_TOASTMESSAGE_BLOCKRELEASE = "POPUP_FRIEND_TOASTMESSAGE_BLOCKRELEASE";
    public const string POPUP_FRIEND_TOASTMESSAGE_ALREADY_REQUEST_ERROR = "POPUP_FRIEND_TOASTMESSAGE_ALREADY_REQUEST_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_SEND_ERROR = "POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_SEND_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_FRIENDREQUEST = "POPUP_FRIEND_TOASTMESSAGE_FRIENDREQUEST";
    public const string POPUP_FRIEND_TOASTMESSAGE_ALREADY_FRIEND_ERROR = "POPUP_FRIEND_TOASTMESSAGE_ALREADY_FRIEND_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_FRIENDREQUEST_CANCEL = "POPUP_FRIEND_TOASTMESSAGE_FRIENDREQUEST_CANCEL";
    public const string POPUP_FRIEND_TOASTMESSAGE_BLOCKLIST_MAX_ERROR = "POPUP_FRIEND_TOASTMESSAGE_BLOCKLIST_MAX_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_ALREADY_BLOCK_ERROR = "POPUP_FRIEND_TOASTMESSAGE_ALREADY_BLOCK_ERROR";
    public const string POPUP_FRIEND_TOASTMESSAGE_BLOCK = "POPUP_FRIEND_TOASTMESSAGE_BLOCK";
    public const string FRIEND_RECEPTIONLIST_MAX = "FRIEND_RECEPTIONLIST_MAX";
    public const string FRIEND_BLOCKLIST_MAX = "FRIEND_BLOCKLIST_MAX";
    public const string FRIEND_SORT_RECENTLYACCESS = "FRIEND_SORT_RECENTLYACCESS";
    public const string FRIEND_SORT_LIKABILITY = "FRIEND_SORT_LIKABILITY";
    public const string FRIEND_BUTTON_DELETE = "FRIEND_BUTTON_DELETE";
    public const string FRIEND_BUTTON_DELETECANCEL = "FRIEND_BUTTON_DELETECANCEL";
    public const string FRIEND_BUTTON_RECEIVE_ALL_POINT = "FRIEND_BUTTON_RECEIVE_ALL_POINT";
    public const string FRIEND_FRIENDSHIPPOINT_DATA = "FRIEND_FRIENDSHIPPOINT_DATA";
    public const string FRIEND_FRIENDLIST_COUNT = "FRIEND_FRIENDLIST_COUNT";
    public const string FRIEND_FRIENDREQUESTLIST_COUNT = "FRIEND_FRIENDREQUESTLIST_COUNT";
    public const string FRIEND_SEARCH_RESULT = "FRIEND_SEARCH_RESULT";
    public const string FRIEND_SEARCH_TEXT = "FRIEND_SEARCH_TEXT";
    public const string FRIEND_SEARCH_BUTTON = "FRIEND_SEARCH_BUTTON";
    public const string SYSTEM_MESSAGE_UNKNOWN_ERROR = "SYSTEM_MESSAGE_UNKNOWN_ERROR";
    public const string FRIEND_PLAYERDATA_RECENTACCESS_CONNECT = "FRIEND_PLAYERDATA_RECENTACCESS_CONNECT";
    public const string FRIEND_PLAYERDATA_RECENTACCESS_MINUTE = "FRIEND_PLAYERDATA_RECENTACCESS_MINUTE";
    public const string FRIEND_PLAYERDATA_RECENTACCESS_HOUR = "FRIEND_PLAYERDATA_RECENTACCESS_HOUR";
    public const string FRIEND_PLAYERDATA_RECENTACCESS_DAY = "FRIEND_PLAYERDATA_RECENTACCESS_DAY";
    public const string FRIEND_PLAYERDATA_RECENTACCESS_YEAR = "FRIEND_PLAYERDATA_RECENTACCESS_YEAR";
    public const string REFRESH_BUTTON = "BUTTON_MAILBOX_REFRESH_TEXT";
    public const string FRIEND_FRIENDSHIPPOINT_DAILY_TEXT = "FRIEND_FRIENDSHIPPOINT_DAILY_TEXT";
    public const string FRIEND_FRIENDSHIPPOINT_DAILY_COUNT = "FRIEND_FRIENDSHIPPOINT_DAILY_COUNT";
    public const long Min = 60;
    public const long Hour = 3600;
    public const long Day = 86400;
    public const long Year = 31536000;
    public const string FRIEND_PLAYERDATA_LEVEL = "FRIEND_PLAYERDATA_LEVEL";
    public const string FRIEND_PLAYERDATA_TIER = "FRIEND_PLAYERDATA_TIER";
    public const string FRIEND_FRIENDSHIPPOINT_DAILY_MAX_TEXT = "FRIEND_FRIENDSHIPPOINT_DAILY_MAX_TEXT";
    public const string CHATTING_MESSAGE_NOTCONNECTED_PLAYER = "CHATTING_MESSAGE_NOTCONNECTED_PLAYER";
    public const string POPUP_FRIEND_TOASTMESSAGE_SEARCH_MYSELF = "POPUP_FRIEND_TOASTMESSAGE_SEARCH_MYSELF";
    
    #endregion

    #region ObjPath

    public const string FriendPopup = "UI/UI_FriendPopup";

    #endregion

    #region Get Set

    public Dictionary<long, FriendData> GetFriendDataDic()
    {
        return _friendDataDic;
    }

    public Dictionary<long, FriendBaseData> GetRequestWaitDataDic()
    {
        return _friendRequestWaitDataDic;
    }

    public Dictionary<long, FriendBaseData> GetAcceptWaitDataDic()
    {
        return _friendAcceptWaitDataDic;
    }

    public Dictionary<long, FriendBlockData> GetFriendBlockDataDic()
    {
        return _friendBlockDataDic;
    }

    public int CountFriendDataDic()
    {
        return _friendDataDic.Count;
    }

    public int CountRequestWaitDataDic()
    {
        return _friendRequestWaitDataDic.Count;
    }

    public int CountAcceptWaitDataDic()
    {
        return _friendAcceptWaitDataDic.Count;
    }

    public int CountFriendBlockDataDic()
    {
        return _friendBlockDataDic.Count;
    }

    public int GetFriendPointSendCount()
    {
        return FriendPointSendCount;
    }

    public UnityAction<int> FriendPointSendCountChange = null;
    public void SetFriendPointCountDelta(int delta)
    {
        FriendPointSendCount += delta;
        FriendPointSendCountChange?.Invoke(FriendPointSendCount);
    }

    public FriendBaseData GetFriendAcceptWaitDataDic(long cuid)
    {
        return _friendAcceptWaitDataDic[cuid];
    }

    public bool ContainFriendDataDic(long cuid)
    {
        return _friendDataDic.ContainsKey(cuid);
    }

    public bool ContainFriendRequestWaitDataDic(long cuid)
    {
        return _friendRequestWaitDataDic.ContainsKey(cuid);
    }

    public bool ContainFriendBlockDataDic(long cuid)
    {
        return _friendBlockDataDic.ContainsKey(cuid);
    }

    public bool ContainAcceptWaitDataDic(long cuid)
    {
        return _friendAcceptWaitDataDic.ContainsKey(cuid);
    }

    public bool CurrentFriendRequestPossible(long cuid)
    {
        if (ContainFriendDataDic(cuid))
            return false;

        if (ContainFriendRequestWaitDataDic(cuid))
            return false;

        if (ContainAcceptWaitDataDic(cuid))
            return false;

        if (ContainFriendBlockDataDic(cuid))
            return false;

        return true;
    }

    #endregion

    #region 그외

    public UnityAction friendIsDayOff = null;

    DateTimeOffset tomorrow = new DateTimeOffset(DateTime.Today.AddDays(1));
    //날이 바꼈는지 체크 함수.
    public void DayOff()
    {
        if (Util.CurrentUnixTime() >= tomorrow.ToUnixTimeSeconds())
        {
            SetFriendPointCountDelta(-FriendPointSendCount);
            friendIsDayOff?.Invoke();
            tomorrow = new DateTimeOffset(DateTime.Today.AddDays(1));
        }
    }

    public void RequestFriend(long cuid)
    {
            if (Managers.FriendMgr.CountRequestWaitDataDic() >=
                Managers.TableMgr.CommonGDT.CommonTable.FriendRequestListMaxCount)
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_SEND_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }
            else if (Managers.FriendMgr.CountFriendDataDic() >=
                     Managers.TableMgr.CommonGDT.CommonTable.FriendRequestListMaxCount)
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_PLAYERLIST_SEND_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }
            else if (Managers.FriendMgr.ContainFriendDataDic(cuid))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_FRIEND_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }
            else if (Managers.FriendMgr.ContainAcceptWaitDataDic(cuid))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_REQUEST_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }
            else if (Managers.FriendMgr.ContainFriendRequestWaitDataDic(cuid))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_REQUEST_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }
            else if (Managers.FriendMgr.ContainFriendBlockDataDic(cuid))
            {
                var messageData =
                    Managers.TableMgr.CommonGDT.GetPopupMessage(FriendManager
                        .POPUP_FRIEND_TOASTMESSAGE_ALREADY_FRIEND_ERROR);
                Managers.UIMgr.OpenPopupMessage(messageData.MessagePositionEnum, UI_Localize.LanguageType.UI, 1,
                    messageData.ShowChat,
                    messageData.LanguageID);

                return;
            }

            Managers.NetworkMgr.GameClient.Proxy.Req_FriendRequest(cuid);
    }

    #endregion

    #region 추천 친구 UnitTaskCheck

    #endregion

    public void GameToLobby()
    {
        _friendDataDic.Clear();
        _friendRequestWaitDataDic.Clear();
        _friendAcceptWaitDataDic.Clear();
        _friendBlockDataDic.Clear();
        FriendPointSendCount = 0;
    }
}
