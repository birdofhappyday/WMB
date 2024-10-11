using AnotherWorld.Util;
using GDT;
using SharedCode;
using UnityEngine;
using UnityEngine.UI;
using QuestInfo = AnotherWorld.Manager.QuestInfo;

namespace AnotherWorld.UI.Quest.Hud
{
    public class UI_Quest_List : MonoBehaviour
    {
        [SerializeField]
        UI_Localize title;

        [SerializeField]
        UI_Hud_Quest_Goal completeText;

        [SerializeField]
        private Button clickButton;

        private QuestInfo _questInfo;

        private TinyPool<UI_Hud_Quest_Goal> _completeItemPool;

        [SerializeField]
        private Transform completeTextParent;

        public void SetQuest(QuestInfo questInfo)
        {
            if (null == _completeItemPool)
            {
                completeText.SetActive(false);
                var comp = completeText.GetComponent<UI_Hud_Quest_Goal>();
                _completeItemPool = new TinyPool<UI_Hud_Quest_Goal>(comp);
            }

            _completeItemPool.Clear();

            _questInfo = questInfo;

            SetQuestType();
            title.OnTxt(UI_Localize.LanguageType.Quest, _questInfo.GetQuestTitle);

            SetQuestCompleteList();

            NPCFindInteractable();
        }

        public string SetQuestType()
        {
            string result = string.Empty;
            switch (_questInfo.GetQuestType)
            {
                case QuestType.QT_Main:
                    result = "Main";
                    break;

                case QuestType.QT_Sub:
                    result = "Sub";
                    break;
            }

            return result;
        }

        public void SetQuestCompleteList()
        {
            foreach (var complete in _questInfo.GetQuestCompleteList)
            {
                string key = complete.GetQuestCompleteHudText;
                string value1 = string.Empty;
                string value2 = string.Empty;
                switch (complete.GetConditionType)
                {
                    case QuestConditionType.QT_NPC:
                    case QuestConditionType.QT_Region:
                    case QuestConditionType.QT_TierLevel:
                        if (QuestState.Progress == complete.GetCompleteState)
                        {
                            value1 = 0.ToString();
                        }
                        else
                        {
                            value1 = 1.ToString();
                        }

                        value2 = 1.ToString();
                        break;

                    case QuestConditionType.QT_JoinMode:
                    case QuestConditionType.QT_ItemGet:
                    case QuestConditionType.QT_ItemUse:
                    case QuestConditionType.QT_ModeVictory:
                    case QuestConditionType.QT_ModeFailure:
                        value1 = Managers.QuestMgr.GetQuestComplete(complete.GetQuestCompleteID).GetProgress.ToString();
                        value2 = complete.GetQuestCompleteValue02.ToString();
                        break;
                }

                var text = _completeItemPool.Rent();
                text.transform.SetParent(completeTextParent, false);
                text.SetData(key, $"{value1}/{value2}");
            }
        }

        /// <summary>
        /// 버튼 클릭시 동작 결정
        /// </summary>
        public void OnClick()
        {
            //NOTE(JUDY) : 아직 로딩 팝업이 띄워져 있는 상태인데 UI가 눌리는 경우가 있어서 그럴 경우 리턴 처리.
            if (Managers.UIMgr.IsLoadingState)
            {
                return;
            }

            if (QuestState.Progress == _questInfo.GetQuestState)
                Managers.QuestMgr.QuestNPCCompleteNavi(_questInfo.GetQuestCompleteList[0]);
            else if (QuestState.Wait == _questInfo.GetQuestState)
                Managers.QuestMgr.QuestNPCStartNavi(_questInfo.GetQuestStartValue01);
        }

        void NPCFindInteractable()
        {
            if (QuestState.Progress == _questInfo.GetQuestState)
                clickButton.interactable =
                    _questInfo.GetQuestCompleteList[0].GetConditionType == QuestConditionType.QT_NPC;
            else if (QuestState.Wait == _questInfo.GetQuestState)
            {
                clickButton.interactable = _questInfo.GetQuestStartType == QuestConditionType.QT_NPC;
            }
        }
    }
}
