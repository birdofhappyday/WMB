using AnotherWorld.UI.Quest.Popup;
using GDT;
using SharedCode;
using UnityEngine;
using QuestComplete = AnotherWorld.Manager.QuestComplete;

namespace AnotherWorld.UI.Quest.Item
{
    public class UI_Quest_CompleteListItem : MonoBehaviour
    {
        [SerializeField]
        private UI_Localize questCompleteText;

        [SerializeField]
        private TMPro.TextMeshProUGUI progressText;

        private QuestComplete _questComplete;

        public void Clear()
        {
            gameObject.SetActive(false);
        }

        public void Setting(int questCompleteID)
        {
            _questComplete = Managers.QuestMgr.GetQuestComplete(questCompleteID);
            Setting();
            gameObject.SetActive(true);
            ButtonSetting();
        }

        [SerializeField]
        private UIButton clickButton;

        void ButtonSetting()
        {
            if (!_questComplete.QuestCompletePossibleCheck())
                clickButton.ChangeInteractable(false);
            else
            {
                switch (_questComplete.GetConditionType)
                {
                    case QuestConditionType.QT_NPC:
                        clickButton.ChangeInteractable(true);
                        break;

                    default:
                        clickButton.ChangeInteractable(false);
                        break;
                }
            }
        }

        public void OnClickButton()
        {
            Managers.QuestMgr.QuestNPCCompleteNavi(_questComplete);
            Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.ClosePopup();
        }

        void Setting()
        {
            string key = _questComplete.GetQuestCompleteHudText;
            string value1 = string.Empty;
            string value2 = string.Empty;
            switch (_questComplete.GetConditionType)
            {
                case QuestConditionType.QT_NPC:
                case QuestConditionType.QT_Region:
                    if (_questComplete.GetCompleteState == QuestState.Progress)
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
                    value1 = Managers.QuestMgr.GetQuestComplete(_questComplete.GetQuestCompleteID).GetProgress
                        .ToString();
                    value2 = _questComplete.GetQuestCompleteValue02.ToString();
                    break;
            }

            questCompleteText.OnTxt(UI_Localize.LanguageType.Quest, key, value1, value2);


            // var size = questCompleteText.txt.preferredWidth;
            // var rect = questCompleteText.GetComponent<RectTransform>();
            // rect.sizeDelta = new Vector2(size, rect.sizeDelta.y);
        }
    }
}
