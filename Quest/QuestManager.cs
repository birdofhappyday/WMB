using System.Collections.Generic;
using System.Linq;
using AnotherWorld.UI.Quest.Item;
using AnotherWorld.UI.Quest.Popup;
using Cysharp.Threading.Tasks;
using GDT;
using Gpm.Ui;
using SharedCode;
using Unity.Cinemachine;

namespace AnotherWorld.Manager
{
    public enum QuestUIState
    {
        Progress,
        Before,
        Done
    }

    public enum QuestInfoButtonColor
    {
        Wait,
        Progress,
        Complete,
        Reward,
        QuestCancel
    }

    public class QuestInfo : InfiniteScrollData
    {
        private QuestInfoT _questInfoT;
        private QuestState _questState;
        private List<QuestComplete> _questCompleteList;

        private bool _questHud;
        private bool _questStartCheck;

        public QuestInfo(QuestInfoT questInfoT)
        {
            _questInfoT = questInfoT;
            _questState = QuestState.Wait;
            _questCompleteList = new List<QuestComplete>();
        }

        public void Reset()
        {
            _questState = QuestState.Wait;
            foreach (var questComplete in _questCompleteList)
            {
                questComplete.Reset();
            }
        }

        public void SetQuestState(QuestData questData)
        {
            _questState = questData.State;
        }

        public void SetQuestState(QuestState questState)
        {
            _questState = questState;
        }

        public QuestState GetQuestState
        {
            get { return _questState; }
        }

        public int GetQuestID
        {
            get { return _questInfoT.QuestID; }
        }

        public QuestConditionType GetQuestStartType
        {
            get { return _questInfoT.QuestStartType; }
        }

        public int GetQuestStartValue01
        {
            get { return _questInfoT.QuestStartValue01; }
        }

        public int GetQuestStartValue02
        {
            get { return _questInfoT.QuestStartValue02; }
        }

        public QuestAcceptType GetQuestAcceptType
        {
            get { return _questInfoT.QusetAcceptType; }
        }

        public int GetStartScenarioIndex
        {
            get { return _questInfoT.StartScenarioID; }
        }

        public QuestType GetQuestType
        {
            get { return _questInfoT.QuestType; }
        }

        public string GetQuestTitle
        {
            get { return _questInfoT.QuestTitle; }
        }

        public List<QuestComplete> GetQuestCompleteList
        {
            get { return _questCompleteList; }
        }

        public void AddQuestComplete(QuestComplete questComplete)
        {
            _questCompleteList.Add(questComplete);
        }

        public void SetQuestComplete(SharedCode.QuestComplete questComplete)
        {
            foreach (var qce in _questCompleteList)
            {
                if (qce.GetQuestCompleteID == questComplete.QuestCompleteID)
                    qce.SetQuestComplete(questComplete);
            }
        }

        public QuestComplete GetQuestCompleteScenarioIndex(int scenarioIndex)
        {
            foreach (var qce in _questCompleteList)
            {
                if (qce.GetQuestCompleteID == scenarioIndex)
                    return qce;
            }

            return null;
        }

        public QuestComplete GetQuestCompleteIndex(int index)
        {
            if (index >= _questCompleteList.Count)
                return null;

            return _questCompleteList[index];
        }

        public bool QuestPossibleCheck()
        {
            return Managers.QuestMgr.QuestPossibleCheck(this);
        }

        public ContentsResetType GetQuestResetType
        {
            get { return _questInfoT.QuestReset; }
        }

        public int GetQuestLevelMin
        {
            get { return _questInfoT.QuestLevelMin; }
        }

        public int GetQuestLevelMax
        {
            get { return _questInfoT.QuestLevelMax; }
        }

        public int GetPreQuestID
        {
            get { return _questInfoT.PreQuestID; }
        }

        public int GetQuestGroupID
        {
            get { return _questInfoT.QuestGroupID; }
        }

        public int GetQuestRewardID
        {
            get { return _questInfoT.RewardDataID; }
        }

        public string GetQuestText
        {
            get { return _questInfoT.QuestText; }
        }

        public QuestAcceptType GetQusetAcceptType
        {
            get { return _questInfoT.QusetAcceptType; }
        }

        public bool GetQuestHud
        {
            get { return _questHud; }
        }

        public bool SetQuestHud
        {
            set { _questHud = value; }
        }

        public bool GetDisplayUI
        {
            get { return _questInfoT.Show; }
        }

        public int GetRewardDataID
        {
            get { return _questInfoT.RewardDataID; }
        }

        public bool GetQuestStartCheck
        {
            get { return _questStartCheck; }
        }

        public bool SetQuestStartCheck
        {
            set { _questStartCheck = value; }
        }
    }

    public class QuestComplete
    {
        private QuestCompleteT _questCompleteT;
        private SharedCode.QuestComplete _questComplete;

        public QuestComplete(QuestCompleteT questCompleteT)
        {
            _questCompleteT = questCompleteT;
            _questComplete = new SharedCode.QuestComplete();
            _questComplete.State = QuestState.Progress;
        }

        public void SetQuestComplete(SharedCode.QuestComplete questComplete)
        {
            _questComplete = questComplete;
        }

        public void Reset()
        {
            _questComplete.Clear();
        }

        public int GetQuestCompleteID
        {
            get { return _questCompleteT.QuestCompleteID; }
        }

        public int GetQuestID
        {
            get { return _questCompleteT.QuestID; }
        }

        public QuestConditionType GetConditionType
        {
            get { return _questCompleteT.QuestConditionType; }
        }

        public int GetQuestCompleteValue01
        {
            get { return _questCompleteT.QuestCompleteValue01; }
        }

        public int GetQuestCompleteValue02
        {
            get { return _questCompleteT.QuestCompleteValue02; }
        }

        public int GetQuestCompleteValue03
        {
            get { return _questCompleteT.QuestCompleteValue03; }
        }


        public int GetQuestCompleteValue04
        {
            get { return _questCompleteT.QuestCompleteValue04; }
        }


        public string GetQuestCompleteHudText
        {
            get { return _questCompleteT.QuestHudText; }
        }

        public SharedCode.QuestComplete GetQuestComplete
        {
            get { return _questComplete; }
        }

        public int GetProgress
        {
            get { return _questComplete.Progress; }
        }

        public QuestState GetCompleteState
        {
            get { return _questComplete.State; }
        }

        public bool QuestCompletePossibleCheck()
        {
            return Managers.QuestMgr.QuestCompletePossibleCheck(this);
        }

        public int GetQuestCompleteCount
        {
            get { return _questCompleteT.QuestCompleteCount; }
        }
    }

    /// <summary>
    /// 퀘스트 정보를 담기 위해 만든 클래스
    /// 기본 퀘스트 정보와 완료 정보, 스크립트를 담았다.
    /// 퀘스트가 연계될 경우 이전과 이후도 담아져 있다.
    /// Step은 해당 단계를 표현한다.(0부터 시작)
    /// </summary>
    public class QuestManager
    {
        private Dictionary<int, QuestInfo> AllQuestInfoDic { get; set; }

        public QuestInfo GetQuestInfo(int id)
        {
            QuestInfo questInfo = null;

            AllQuestInfoDic.TryGetValue(id, out questInfo);

            return questInfo;
        }

        private Dictionary<int, QuestComplete> QuestCompleteDic { get; set; }

        // QuestGroupID로 묶어서 퀘스트 관리.
        public Dictionary<int, QuestListData> QuestListDataDic { get; private set; }

        // Hud에 표시되는 퀘스트
        public Dictionary<int, QuestInfo> HudQuestInfoDic { get; private set; }

        // 퀘스트 자동이동 중인지
        public bool IsOnQuestMove = false;

        /// <summary>
        /// QuestListData 생성
        /// </summary>
        /// <param name="questInfo"></param>
        void SetQuestListData(QuestInfo questInfo)
        {
            if (QuestListDataDic.TryGetValue(questInfo.GetQuestGroupID, out var questListData))
            {
                questListData.AddQuestInfo(questInfo);
            }
            else
            {
                questListData = new QuestListData(questInfo.GetQuestGroupID);
                questListData.AddQuestInfo(questInfo);
                QuestListDataDic.Add(questListData.QuestGroupID, questListData);
            }
        }

        /// <summary>
        /// QuestTabType분류로 인한 QuestListData 반환
        /// </summary>
        /// <param name="questTabType"></param>
        /// <returns></returns>
        public List<QuestListData> GetQuestListDatas(QuestTabType questTabType)
        {
            var result = new List<QuestListData>();

            foreach (var data in QuestListDataDic.Values)
            {
                if (questTabType == data.QuestTabType)
                    result.Add(data);
            }

            return result;
        }

        /// <summary>
        /// 쿠스트 종류에 따른 데이터 얻어오기
        /// </summary>
        /// <param name="questType"></param>
        /// <returns></returns>
        public List<QuestInfo> GetQuestInfos(QuestType questType)
        {
            var result = new List<QuestInfo>();

            foreach (var data in AllQuestInfoDic.Values)
            {
                if (questType == data.GetQuestType)
                    result.Add(data);
            }

            return result;
        }

        public QuestComplete GetQuestComplete(int key)
        {
            QuestCompleteDic.TryGetValue(key, out var result);

            return result;
        }

        /// <summary>
        /// 퀘스트 완료조건과 같은 목록의 완료 반환
        /// </summary>
        /// <param name="questConditionType"></param>
        /// <returns></returns>
        public List<QuestComplete> GetQuestComplete(QuestConditionType questConditionType)
        {
            List<QuestComplete> result = new List<QuestComplete>();

            foreach (var questcomplete in QuestCompleteDic.Values)
            {
                if (questConditionType == questcomplete.GetConditionType)
                    result.Add(questcomplete);
            }

            return result;
        }

        public void DailyMissionReset()
        {
            var missions = Managers.QuestMgr.GetQuestListDatas(QuestTabType.QT_M_Daily);
            foreach (var mission in missions)
            {
                mission.Reset();
            }

            Managers.UIMgr.GetOpenPopup<UI_Mission_Popup>()?.MissionRefresh();
        }

        /// <summary>
        /// 치트키로 인한 퀘스트 리셋
        /// </summary>
        /// <param name="id"></param>
        public void ResetCheatQuest(int id)
        {
            if (AllQuestInfoDic.TryGetValue(id, out var value))
            {
                value.SetQuestState(QuestState.Wait);
            }
        }

        /// <summary>
        /// 퀘스트 상태 변경
        /// </summary>
        /// <param name="questData"></param>
        public void SetQuestState(QuestData questData)
        {
            GetQuestInfo(questData.QuestID).SetQuestState(questData.State);
        }

        /// <summary>
        /// 퀘스트 상태 변경
        /// </summary>
        /// <param name="questData"></param>
        public void SetQuestState(int questID, QuestState questState)
        {
            GetQuestInfo(questID).SetQuestState(questState);

            QuestRefreshNPC();
        }

        /// <summary>
        /// 가능한 퀘스트가 있는지 체크한다.
        /// </summary>
        /// <returns></returns>
        public List<QuestInfo> CheckPossibleQuestFind()
        {
            List<QuestInfo> result = new List<QuestInfo>();

            foreach (var questInfo in AllQuestInfoDic.Values)
            {
                if (QuestPossibleCheck(questInfo))
                {
                    result.Add(questInfo);
                }
            }

            return result;
        }

        /// <summary>
        /// 서버에서 받은 퀘스트 허드 설정
        /// </summary>
        /// <param name="questHUDs"></param>
        public void SettingHudQuest(List<QuestHud> questHUDs)
        {
            foreach (var questHud in questHUDs)
            {
                SetHudQuestInfo(questHud.QuestID, true);
            }
        }

        /// <summary>
        /// 퀘스트 허드 체크가 가능한지 체크하기
        /// </summary>
        /// <returns></returns>
        public bool QuestHudCheckPossible()
        {
            if (Managers.TableMgr.CommonGDT.CommonTable.QuestHudInfoCheckCount <
                Managers.QuestMgr.GetHudQuestList().Count)
                return false;

            return true;
        }

        /// <summary>
        /// 퀘스트 허드 체크
        /// </summary>
        /// <param name="id"></param>
        /// <param name="check"></param>
        public void SetHudQuestInfo(int id, bool check)
        {
            if (AllQuestInfoDic.TryGetValue(id, out var questInfo))
            {
                questInfo.SetQuestHud = check;
                if (check)
                    HudQuestInfoDic.TryAdd(id, questInfo);
                else
                {
                    if (HudQuestInfoDic.TryGetValue(id, out var value))
                        HudQuestInfoDic.Remove(id);
                }

                Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.HudCountSetting();

                UI_Scene_Game uiSceneGame = Managers.UIMgr.CurUIScene as UI_Scene_Game;
                if (null != uiSceneGame)
                    uiSceneGame.UIQuestHudDisplay.RefreshQuest();
            }
        }

        /// <summary>
        /// 허드 퀘스트 리스트 정렬해서 반환
        /// </summary>
        /// <returns></returns>
        public List<QuestInfo> GetHudQuestList()
        {
            var result = HudQuestInfoDic.Values.ToList();
            result.Sort((l, r) =>
            {
                if (l.GetQuestID > r.GetQuestID)
                {
                    return 1;
                }

                if (l.GetQuestID < r.GetQuestID)
                {
                    return -1;
                }

                return 0;
            });

            return result;
        }

        /// <summary>
        /// 퀘스트 초기화
        /// </summary>
        public void Init()
        {
            Debug.Log("[ManagerInit] QuestManager Init - Start");
            
#if UNITY_EDITOR
            if (Managers.IsDevMode)
            {
                return;
            }
#endif
            DicInit();

            DataSetting();

            Debug.Log("[ManagerInit] QuestManager Init - End");
        }

        public void Clear() { }

        /// <summary>
        /// 로비로 돌아갔을때 초기화
        /// </summary>
        public void GameToLobby()
        {
            DicInit();

            DataSetting();
        }

        /// <summary>
        /// 서버에서 보내온 정보 초기화
        /// </summary>
        /// <param name="questDatas"></param>
        /// <param name="questCompletes"></param>
        public void ServerInit(List<QuestData> questDatas, List<SharedCode.QuestComplete> questCompletes)
        {
            foreach (var q in questDatas)
            {
                if (AllQuestInfoDic.TryGetValue(q.QuestID, out var questInfo))
                {
                    questInfo.SetQuestState(q);
                    Managers.NewRedDotMgr.SetMissionRedDot(q);
                }
            }

            foreach (var q in questCompletes)
            {
                if (QuestCompleteDic.TryGetValue(q.QuestCompleteID, out var questComplete))
                    questComplete.SetQuestComplete(q);
            }

            QuestInfoDataSetting();
        }

        /// <summary>
        /// 퀘스트 정보 서버에 보내서 세팅하기
        /// </summary>
        public void QuestHudSeverSetting()
        {
            var datas = new List<QuestHud>();

            var hudQuestDatas = Managers.QuestMgr.GetHudQuestList();

            byte i = 0;
            for (i = 0; i < 5; ++i)
            {
                var data = new QuestHud();
                data.SlotID = i;
                datas.Add(data);
            }

            i = 0;
            foreach (var questInfo in hudQuestDatas)
            {
                datas[i].QuestID = questInfo.GetQuestID;
                ++i;
            }

            Managers.NetworkMgr.GameClient.Proxy.Send_QuestHud_Save(datas);
        }

        public void QuestInfoDataSetting()
        {
            foreach (var questComplete in QuestCompleteDic)
            {
                if (AllQuestInfoDic.TryGetValue(questComplete.Value.GetQuestID, out var questInfo))
                {
                    questInfo.AddQuestComplete(questComplete.Value);
                }
            }

            foreach (var questInfo in AllQuestInfoDic)
            {
                SetQuestListData(questInfo.Value);
            }

            // 시작 대기 퀘스트가 존재.
            foreach (var questInfo in Managers.QuestMgr.CheckPossibleQuestFind())
            {
                Managers.NewRedDotMgr.SetQuestRedDot(questInfo);
            }
        }

        /// <summary>
        /// 시작 대기인 퀘스트 진행 가능한지 체크
        /// </summary>
        public bool QuestPossibleCheck(QuestInfo questInfo)
        {
            if (questInfo.GetQuestLevelMin != 0 && questInfo.GetQuestLevelMin > _Me.EnterChar.Level)
                return false;

            if (questInfo.GetQuestLevelMax != 0 && questInfo.GetQuestLevelMax < _Me.EnterChar.Level)
                return false;

            if (questInfo.GetPreQuestID != 0 &&
                AllQuestInfoDic[questInfo.GetPreQuestID].GetQuestState != QuestState.Reward)
                return false;

            if (QuestState.Wait != questInfo.GetQuestState)
                return false;

            if (QuestAcceptType.QAT_None == questInfo.GetQuestAcceptType)
                return false;

            return true;
        }

        public bool QuestCompletePossibleCheck(QuestComplete questComplete)
        {
            if (QuestState.Progress != AllQuestInfoDic[questComplete.GetQuestID].GetQuestState)
                return false;

            if (QuestState.Progress != questComplete.GetCompleteState)
                return false;

            return true;
        }

        /// <summary>
        /// 퀘스트 허드로 보내기. 임시코드일 수 있음
        /// </summary>
        /// <param name="questID"></param>
        public void QuestHudSettingTemp(int questID)
        {
            if (QuestType.QT_Main == Managers.QuestMgr.GetQuestInfo(questID).GetQuestType)
            {
                if (Managers.QuestMgr.QuestHudCheckPossible())
                {
                    Managers.QuestMgr.SetHudQuestInfo(questID, true);
                    Managers.QuestMgr.QuestHudSeverSetting();
                }
            }
        }

        /// <summary>
        /// 퀘스트가 시작했을 때 시작상태로 설정하고
        /// 시작 시나리오를 재생한다.
        /// </summary>
        /// <param name="questID"></param>
        public void QuestStartCheck(int questID)
        {
            var questInfo = Managers.QuestMgr.GetQuestInfo(questID);

            if (QuestConditionType.QT_NPC == questInfo.GetQuestStartType)
                return;

            if (0 != questInfo.GetStartScenarioIndex)
            {
                var qce = questInfo.GetQuestCompleteScenarioIndex(questInfo
                    .GetStartScenarioIndex);

                if (qce != null)
                    Managers.ScenarioSceneMgr.ScenarioQuestSetting(questInfo.GetStartScenarioIndex,
                        questInfo, 0);
            }
        }

        /// <summary>
        /// 이 콘텐츠 클릭으로 시작하는 퀘스트가 있는지 체크한다.
        /// </summary>
        /// <param name="contentMainType"></param>
        public void QuestOnClickUI(ContentMainType contentMainType)
        {
            KeyValuePair<int, QuestInfo> questInfoPair = new KeyValuePair<int, QuestInfo>(0, null);

            foreach (var infoPair in AllQuestInfoDic)
            {
                if (infoPair.Value.GetQuestResetType == ContentsResetType.CRT_None)

                    if (infoPair.Value.GetQuestState == QuestState.Complete ||
                        infoPair.Value.GetQuestState == QuestState.Reward)
                        continue;

                if (0 != infoPair.Value.GetPreQuestID)
                {
                    if (AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Wait ||
                        AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Progress ||
                        AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Complete)
                        continue;
                }

                if (infoPair.Value.GetQuestStartType != QuestConditionType.QT_ContentMove)
                    continue;

                if ((ContentMainType)infoPair.Value.GetQuestStartValue01 == contentMainType)
                {
                    questInfoPair = infoPair;
                    break;
                }
            }

            if (questInfoPair.Value == null)
                return;

            if (questInfoPair.Value.GetQuestAcceptType == QuestAcceptType.QAT_Manual &&
                questInfoPair.Value.GetQuestState == QuestState.Wait)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE, Popup_Notice.POPUP_TITLE,
                    () =>
                    {
                        questInfoPair.Value.SetQuestStartCheck = true;
                        Managers.UIMgr.OpenPopup<UI_Quest_Popup>();
                    });
            }
            else
            {
                if (questInfoPair.Value.GetQuestState == QuestState.Wait)
                {
                    SettingQuestStartUI(questInfoPair);
                }
                else if (questInfoPair.Value.GetQuestState == QuestState.Progress)
                {
                    var qce = questInfoPair.Value.GetQuestCompleteScenarioIndex(questInfoPair.Value
                        .GetStartScenarioIndex);

                    if (qce != null)
                        Managers.ScenarioSceneMgr.ScenarioQuestSetting(questInfoPair.Value.GetStartScenarioIndex,
                            questInfoPair.Value, 0);
                }
            }
        }

        /// <summary>
        /// UI 클릭했을 때 퀘스트 시작 패킷
        /// </summary>
        /// <param name="questInfoPair"></param>
        void SettingQuestStartUI(KeyValuePair<int, QuestInfo> questInfoPair)
        {
            Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_Content(questInfoPair.Key);
        }

        /// <summary>
        /// 존에 입장함으로 시작할 수 있는 퀘스트가 있는지 체크한다.
        /// </summary>
        /// <param name="zoneID"></param>
        public void QuestEnterZone(int zoneID)
        {
            List<KeyValuePair<int, QuestInfo>> questInfoPairList = new List<KeyValuePair<int, QuestInfo>>();

            foreach (var infoPair in AllQuestInfoDic)
            {
                if (QuestState.Complete == infoPair.Value.GetQuestState ||
                    QuestState.Reward == infoPair.Value.GetQuestState)
                    continue;

                if (0 != infoPair.Value.GetPreQuestID)
                {
                    if (AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Wait ||
                        AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Progress ||
                        AllQuestInfoDic[infoPair.Value.GetPreQuestID].GetQuestState == QuestState.Complete)
                        continue;
                }

                if (QuestConditionType.QT_Region != infoPair.Value.GetQuestStartType)
                    continue;

                if (zoneID != infoPair.Value.GetQuestStartValue01)
                    continue;

                questInfoPairList.Add(infoPair);
            }

            if (0 == questInfoPairList.Count)
                return;

            foreach (var questInfoPair in questInfoPairList)
            {
                if (questInfoPair.Value.GetQuestAcceptType == QuestAcceptType.QAT_Manual &&
                    questInfoPair.Value.GetQuestState == QuestState.Wait)
                {
                    Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE, Popup_Notice.POPUP_TITLE,
                        () =>
                        {
                            questInfoPair.Value.SetQuestStartCheck = true;
                            Managers.UIMgr.OpenPopup<UI_Quest_Popup>();
                        });
                }
                else
                {
                    if (questInfoPair.Value.GetQuestState == QuestState.Wait)
                    {
                        SettingQuestStartZone(questInfoPair, zoneID);
                    }
                    else if (questInfoPair.Value.GetQuestState == QuestState.Progress)
                    {
                        if (0 == questInfoPair.Value.GetStartScenarioIndex)
                            continue;

                        var qce = questInfoPair.Value.GetQuestCompleteScenarioIndex(questInfoPair.Value
                            .GetStartScenarioIndex);

                        if (qce != null)
                            Managers.ScenarioSceneMgr.ScenarioQuestSetting(questInfoPair.Value.GetStartScenarioIndex,
                                questInfoPair.Value, 0);
                    }
                }
            }
        }

        /// <summary>
        /// 이 존 입장으로 끝낼 수 있는 퀘스트가 있는지 체크한다.
        /// </summary>
        /// <param name="zoneID"></param>
        public void QuestEndZone(int zoneID)
        {
            List<KeyValuePair<int, QuestComplete>> questCompletList = new List<KeyValuePair<int, QuestComplete>>();

            foreach (var questCompletePair in QuestCompleteDic)
            {
                if (QuestState.Progress != AllQuestInfoDic[questCompletePair.Value.GetQuestID].GetQuestState)
                    continue;

                if (QuestConditionType.QT_Region != questCompletePair.Value.GetConditionType)
                    continue;

                if (zoneID != questCompletePair.Value.GetQuestCompleteValue01)
                    continue;

                questCompletList.Add(questCompletePair);
            }

            foreach (var questComPletePair in questCompletList)
            {
                Managers.NetworkMgr.GameClient.Proxy.Send_Quest_Complete_RegionEntry(questComPletePair.Key);
            }
        }

        /// <summary>
        /// 존 입장 퀘스트 서버 패킷
        /// </summary>
        /// <param name="questInfoPair"></param>
        /// <param name="zoneID"></param>
        void SettingQuestStartZone(KeyValuePair<int, QuestInfo> questInfoPair, int zoneID)
        {
            Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_RegionEntry(questInfoPair.Key, zoneID);
        }

        /// <summary>
        /// 이 시나리오로 퀘스트를 끝낼 수 있는지 체크한다.
        /// </summary>
        /// <param name="questInfo"></param>
        /// <param name="scenarioSceneTIndex"></param>
        public void QuestCompleteScenarioSceneCheck(QuestInfo questInfo, int scenarioSceneTIndex)
        {
            foreach (var questComplete in questInfo.GetQuestCompleteList)
            {
                switch (questComplete.GetConditionType)
                {
                    case QuestConditionType.QT_ScenarioScene:
                        if (questComplete.GetQuestCompleteValue01 == scenarioSceneTIndex)
                        {
                            Managers.NetworkMgr.GameClient.Proxy.Send_Quest_Complete_ScenarioScene(
                                questComplete.GetQuestCompleteID);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// 이 npc로 퀘스트를 끝낼 수 있는지 체크한다.
        /// </summary>
        /// <param name="questComplete"></param>
        /// <param name="npcID"></param>
        public void QuestCompleteNPCCheck(QuestComplete questComplete, int npcID)
        {
            if (npcID == 0)
                return;

            if (QuestConditionType.QT_NPC != questComplete.GetConditionType)
                return;

            if (QuestState.Progress != questComplete.GetCompleteState)
                return;

            Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Complete_NPC(npcID, questComplete.GetQuestCompleteID);
        }

        /// <summary>
        /// 이 npc로 퀘스트를 시작할 수 있는지 체크한다.
        /// </summary>
        /// <param name="questInfo"></param>
        /// <param name="npcID"></param>
        public void QuestNPCStartCheck(QuestInfo questInfo, int npcID)
        {
            if (npcID == 0)
                return;

            if (QuestConditionType.QT_NPC != questInfo.GetQuestStartType)
                return;

            if (QuestState.Wait != questInfo.GetQuestState)
                return;

            if (QuestAcceptType.QAT_None == questInfo.GetQuestAcceptType)
                return;

            if (QuestAcceptType.QAT_Manual == questInfo.GetQuestAcceptType)
            {
                Managers.UIMgr.OpenPopup<Popup_Notice>().OnPopup(Popup_Notice.POPUP_TITLE, Popup_Notice.POPUP_TITLE,
                    () =>
                    {
                        questInfo.SetQuestStartCheck = true;
                        Managers.UIMgr.OpenPopup<UI_Quest_Popup>();
                    });
            }
            else
            {
                Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_NPC(npcID, questInfo.GetQuestID);
            }
        }

        /// <summary>
        /// 아이디로 NPC가 퀘스트를 가지고 있는 체크한다,
        /// </summary>
        public List<QuestInfo> GetQuestInfoNPCSetting(int npcID)
        {
            List<QuestInfo> result = new List<QuestInfo>();

            foreach (var questInfo in AllQuestInfoDic.Values)
            {
                if (QuestConditionType.QT_NPC != questInfo.GetQuestStartType)
                    continue;

                if (npcID != questInfo.GetQuestStartValue01)
                    continue;

                result.Add(questInfo);
            }

            return result;
        }

        /// <summary>
        /// npc에게 세팅된 컴플리트를 찾아서 던져준다.
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public List<QuestComplete> GetQuestCompleteNpcSetting(int npcID)
        {
            List<QuestComplete> result = new List<QuestComplete>();

            foreach (var questComplete in QuestCompleteDic.Values)
            {
                if (QuestConditionType.QT_NPC != questComplete.GetConditionType)
                    continue;

                if (npcID != questComplete.GetQuestCompleteValue01)
                    continue;

                result.Add(questComplete);
            }

            return result;
        }

        /// <summary>
        /// 퀘스트 상태 변경시 팝업들 새로고침해주기
        /// </summary>
        /// <param name="questID"></param>
        /// <param name="questState"></param>
        public void QuestPopupRefresh(int questID, QuestState questState)
        {
            Managers.QuestMgr.SetQuestState(questID, questState);

            Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.QuestRefresh();
            Managers.UIMgr.GetOpenPopup<UI_Mission_Popup>()?.MissionRefresh();
        }

        public void QuestClickInit()
        {
            foreach (var questListData in QuestListDataDic.Values)
                questListData.QuestUIClick = false;
        }

        /// <summary>
        /// 퀘스트 완료 조건이 NPC일 경우 네비 기능.
        /// </summary>
        /// <param name="questComplete"></param>
        public void QuestNPCCompleteNavi(QuestComplete questComplete)
        {
            // 메인 카메라 셋팅이 덜 됐을 때 UI 클릭이 되는 경우가 생겨서 카메라가 없는 경우 리턴 처리.
            if (null == Managers.CameraMgr.CurCamera)
                return;

            // 점프 중일 땐 이용 불가
            if (!Managers.MapMoveMgr.GroundCheck("POPUP_AUTOMOVE_JUMP_PROHIBIT_TEXT", false))
                return;

            var id = Managers.ObjectMgr.GetNPCObjectIDTable(questComplete.GetQuestCompleteValue01);
            var npcSpawner = Managers.TableMgr.CommonGDT.GetNpcSpawner(id);

            // 퀘스트 자동이동 시 카메라 변경. (다른 맵인 경우 이동하지 않고 확인 팝업 출력)
            if (npcSpawner.MapID == _Me.EnterChar.MapId)
            {
                Managers.CameraMgr.StartQuestMoveCamera(false);
            }
            else
            {
                Managers.CameraMgr.StartQuestMoveCamera(true);
            }

            Managers.MapMoveMgr.OnNpc(npcSpawner).ToCoroutine();
        }

        /// <summary>
        /// 퀘스트 시작 조건이 NPC일 경우 네비 기능.
        /// </summary>
        /// <param name="questStartValue"></param>
        public void QuestNPCStartNavi(int questStartValue)
        {
            // 메인 카메라 셋팅이 덜 됐을 때 UI 클릭이 되는 경우가 생겨서 카메라가 없는 경우 리턴 처리.
            if (null == Managers.CameraMgr.CurCamera)
                return;

            // 점프 중일 땐 이용 불가
            if (!Managers.MapMoveMgr.GroundCheck("POPUP_AUTOMOVE_JUMP_PROHIBIT_TEXT", false))
                return;

            var npcId = Managers.ObjectMgr.GetNPCObjectIDTable(questStartValue);
            var npcSpawner = Managers.TableMgr.CommonGDT.GetNpcSpawner(npcId);

            // 퀘스트 자동이동 시 카메라 변경. (다른 맵인 경우 이동하지 않고 확인 팝업 출력)
            if (npcSpawner.MapID == _Me.EnterChar.MapId)
            {
                Managers.CameraMgr.StartQuestMoveCamera(false);
            }
            else
            {
                Managers.CameraMgr.StartQuestMoveCamera(true);
            }

            Managers.MapMoveMgr.OnNpc(npcSpawner).ToCoroutine();
        }

        /// <summary>
        /// NPC위의 마크 퀘스트 상황에 따라 리프레쉬해주기
        /// </summary>
        public void QuestRefreshNPC() { }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        void DicInit()
        {
            if (null == AllQuestInfoDic)
            {
                AllQuestInfoDic = new Dictionary<int, QuestInfo>();
                QuestCompleteDic = new Dictionary<int, QuestComplete>();
                QuestListDataDic = new Dictionary<int, QuestListData>();
                HudQuestInfoDic = new Dictionary<int, QuestInfo>();
            }
            else
            {
                AllQuestInfoDic.Clear();
                QuestCompleteDic.Clear();
                QuestListDataDic.Clear();
                HudQuestInfoDic.Clear();
            }
        }

        /// <summary>
        /// 기본 퀘스트 데이터 세팅
        /// </summary>
        void DataSetting()
        {
            foreach (var questInfoT in Managers.TableMgr.CommonGDT.QuestInfoTable)
            {
                var questInfo = new QuestInfo(questInfoT.Value);
                AllQuestInfoDic.Add(questInfo.GetQuestID, questInfo);
            }

            foreach (var questCompleteT in Managers.TableMgr.CommonGDT.QuestCompleteTable)
            {
                var questComplete = new QuestComplete(questCompleteT.Value);
                QuestCompleteDic.Add(questComplete.GetQuestCompleteID, questComplete);
            }

            var allQuestList = AllQuestInfoDic.Values.ToList();
            allQuestList.Sort((value1, value2) => value1.GetQuestID.CompareTo(value2.GetQuestID));

            var questConpleteList = QuestCompleteDic.Values.ToList();
            questConpleteList.Sort((value1, value2) => value1.GetQuestCompleteID.CompareTo(value2.GetQuestCompleteID));
        }
    }
}
