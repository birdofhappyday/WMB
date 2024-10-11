using GDT;
using SharedCode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AnotherWorld.UI.Friend.Obj
{
    public enum Uifriendpopuplistobjmode
    {
        NORMAL,
        CARD,
    }

    /// <summary>
    /// 친구목록에서 사용되는 ListObject다.
    /// </summary>
    public class UIFriendPopupListObj : MonoBehaviour
    {
        public const string FRIEND_FRIENDSHIPPOINT_SEND_BUTTON = "FRIEND_FRIENDSHIPPOINT_SEND_BUTTON";
        public const string FRIEND_FRIENDSHIPPOINT_RECEPTION = "FRIEND_FRIENDSHIPPOINT_RECEPTION";
        public const string FRIEND_RECEPTIONLIST_ACCEPT_BUTTON = "FRIEND_RECEPTIONLIST_ACCEPT_BUTTON";
        public const string FRIEND_RECOMMENDATION_BUTTON = "FRIEND_RECOMMENDATION_BUTTON";
        public const string FRIEND_RECEPTIONLIST_REFUSAL_BUTTON = "FRIEND_RECEPTIONLIST_REFUSAL_BUTTON";
        public const string FRIEND_BLOCKLIST_BUTTON = "FRIEND_BLOCKLIST_BUTTON";
        public const string FRIEND_BLOCKLIST_UNLOCK_BUTTON = "FRIEND_BLOCKLIST_UNLOCK_BUTTON";
        

        [SerializeField]
        TMPro.TextMeshProUGUI m_nickNameText;

        [SerializeField]
        TMPro.TextMeshProUGUI m_levelText;

        string playerName;

        public string PlayerName
        {
            get => playerName;

            set
            {
                playerName = value;
                m_nickNameText.text = string.Format($"{UI_Localize.GetLanguage(FRIEND_PLAYERDATA_NAME)}", playerName);
            }
        }

        short _playerTierLevel;

        public short PlayerTierLevel
        {
            get => _playerTierLevel;
            set
            {
                _playerTierLevel = value;
                m_levelText.text = string.Format($"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_TIER)}",
                    _playerTierLevel);
            }
        }

        [SerializeField]
        private UI_CharacterPortrait portrait;

        CharacterSettingScriptableObject _characterSettingScriptableObject = null;

        const string FRIEND_PLAYERDATA_NAME = "FRIEND_PLAYERDATA_NAME";

        protected long Cuid = 0;

        public FriendData FriendData { get; protected set; } = null;
        protected FriendBaseData FriendBaseData = null;
        protected FriendBlockData FriendBlockData = null;

        const string ScriptableObj = "CharacterSetting";

        public virtual void Init(FriendBaseData friendBaseData)
        {
            friendBaseData.TierLevel = (short)Managers.TierMgr.GetTierMaxGrade(TierType.TT_AWM, friendBaseData.TierLevel);
            
            this.FriendBaseData = friendBaseData;
            Cuid = friendBaseData.CUID;
            PlayerName = friendBaseData.Name;
            PlayerTierLevel = friendBaseData.TierLevel;

            if (_characterSettingScriptableObject == null)
                _characterSettingScriptableObject =
                    (CharacterSettingScriptableObject)(Managers.ResourceMgr.LoadScriptableObject(ScriptableObj));

            portrait.SetData(friendBaseData.Name, (int)friendBaseData.TierLevel);
            portrait.SetThumbnail(Cuid, friendBaseData.LookInfo);
            
            init = true;
        }

        public virtual void Init(FriendData friendData)
        {
            friendData.BaseData.TierLevel = (short)Managers.TierMgr.GetTierMaxGrade(TierType.TT_AWM, friendData.BaseData.TierLevel);
            
            this.FriendData = friendData;
            this.FriendBaseData = friendData.BaseData;
            Cuid = friendData.BaseData.CUID;
            PlayerName = friendData.BaseData.Name;
            PlayerTierLevel = friendData.BaseData.TierLevel;

            if (_characterSettingScriptableObject == null)
                _characterSettingScriptableObject =
                    (CharacterSettingScriptableObject)(Managers.ResourceMgr.LoadScriptableObject(ScriptableObj));

            portrait.SetData(friendData.BaseData.Name, (int)friendData.BaseData.TierLevel);
            portrait.SetThumbnail(Cuid, friendData.BaseData.LookInfo);
            
            init = true;
        }

        public virtual void Init(FriendBlockData friendBlockData)
        {
            friendBlockData.TierLevel = (short)Managers.TierMgr.GetTierMaxGrade(TierType.TT_AWM, friendBlockData.TierLevel);
            
            this.FriendBlockData = friendBlockData;
            Cuid = friendBlockData.CUID;
            PlayerName = friendBlockData.Name;
            PlayerTierLevel = friendBlockData.TierLevel;

            if (_characterSettingScriptableObject == null)
                _characterSettingScriptableObject =
                    (CharacterSettingScriptableObject)(Managers.ResourceMgr.LoadScriptableObject(ScriptableObj));

            portrait.SetData(friendBlockData.Name, (int)friendBlockData.TierLevel);
            portrait.SetThumbnail(Cuid, friendBlockData.LookInfo);

            init = true;
        }
        
        public virtual void Refresh()
        {
            if (null != this.FriendBaseData)
            {
                portrait.SetData(this.FriendBaseData.Name, (int)this.FriendBaseData.TierLevel);
                portrait.SetThumbnail(Cuid, this.FriendBaseData.LookInfo);
            }
            else if (null != this.FriendBlockData)
            {
                portrait.SetData(this.FriendBlockData.Name, (int)this.FriendBlockData.TierLevel);
                portrait.SetThumbnail(Cuid, this.FriendBlockData.LookInfo);
            }
        }

        private long _loginTime;
        private long _logoutTime;

        public long LoginTime
        {
            get { return _loginTime; }
            private set { _loginTime = value; }
        }

        public long LogoutTime
        {
            get { return _logoutTime; }
            private set { _logoutTime = value; }
        }

        // 현재 접속상태인지 아닌지 파악한다.
        protected string CurrentConnectJudge(long login, long logout)
        {
            LoginTime = login;
            LogoutTime = logout;

            string result;
            long delta = logout - login;

            // 현재 접속상태
            if (delta < 0)
                result = UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_RECENTACCESS_CONNECT);
            else
            {
                long now = global::Util.CurrentUnixTime();
                delta = now - logout;

                if (delta < FriendManager.Hour)
                    result = string.Format(
                        $"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_RECENTACCESS_MINUTE)}",
                        delta / FriendManager.Min);
                else if (delta < FriendManager.Day)
                    result = string.Format(
                        $"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_RECENTACCESS_HOUR)}",
                        delta / FriendManager.Hour);
                else if (delta < FriendManager.Year)
                    result = string.Format(
                        $"{UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_RECENTACCESS_DAY)}",
                        delta / FriendManager.Day);
                else
                    result = UI_Localize.GetLanguage(FriendManager.FRIEND_PLAYERDATA_RECENTACCESS_YEAR);
            }

            return result;
        }

        public bool CurrentConnect()
        {
            long delta = LogoutTime - LoginTime;

            // 현재 접속상태
            if (delta < 0)
                return true;
            else
            {
                return false;
            }
        }

        public string CurrentConnectSetting(long login, long logout)
        {
            return CurrentConnectJudge(login, logout);
        }

        bool init = false;

        private void InitJudge()
        {
            if (init)
                CallBackInit();
        }

        private void OnDestroy()
        {
            InitJudge();
        }

        protected virtual void CallBackInit()
        {
            init = false;
        }
    }
}
