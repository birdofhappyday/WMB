using System.Collections.Generic;
using AnotherWorld.Manager;
using AnotherWorld.UI.Quest.Popup;
using SharedCode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnotherWorld
{
    public class UI_Quest_Main_SubSlot : MonoBehaviour
    {
        [SerializeField]
        private UI_Localize questNameText;

        [SerializeField]
        private List<GameObject> stateObjectList;

        public QuestInfo QuestInfo { get; private set; }
        
        /// <summary>
        /// 퀘스트 정보를 세팅한다.
        /// 퀘스트 이름 및 퀘스트 상태, 허드 토글 관련.
        /// </summary>
        /// <param name="questInfo"></param>
        public void Setting(QuestInfo questInfo)
        {
            QuestInfo = questInfo;
            questNameText.OnTxt(UI_Localize.LanguageType.Quest, QuestInfo.GetQuestTitle);

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            SetClick(false);

            hudCheckToggle.interactable = questInfo.GetQuestState == QuestState.Progress &&
                                          Managers.TableMgr.CommonGDT.CommonTable.QuestHudInfoCheckCount >=
                                          Managers.QuestMgr.GetHudQuestList().Count;

            hudCheckToggle.isOn = questInfo.GetQuestHud;

            var popUp = Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>();

            if (null == popUp)
                return;

            ProgressMode(QuestPopupMode.PROGESS == popUp.QuestPopupMode);

            SetState();
        }

        private const string _basicTextColor = "#FAD37E";
        private const string _dimiedTextColor = "#5D5D5D";
        /// <summary>
        /// 퀘스트 상태 표시 이미지 세팅
        /// </summary>
        void SetState()
        {
            foreach (var obj in stateObjectList)
                obj.SetActive(false);
            
            SetNameTextColor(true);
            switch (QuestInfo.GetQuestState)
            {
                case QuestState.Wait:
                    if (QuestInfo.QuestPossibleCheck())
                        stateObjectList[1].SetActive(true);
                    else
                    {
                        stateObjectList[0].SetActive(true);
                    }
                    break;
                case QuestState.Progress:
                    stateObjectList[2].SetActive(true);
                    OnClickQuest();
                    break;
                case QuestState.Complete:
                    stateObjectList[2].SetActive(true);
                    break;
                case QuestState.Reward:
                    stateObjectList[3].SetActive(true);
                    SetNameTextColor(false);
                    break;
            }
        }
        
        /// <summary>
        /// 타이틀 색깔 지정
        /// </summary>
        /// <param name="active"></param>
        void SetNameTextColor(bool active)
        {
            if (active)
            {
                questNameText.OnColor(_basicTextColor);
            }
            else
            {
                questNameText.OnColor(_dimiedTextColor);
            }
        }
        
        /// <summary>
        /// 퀘스트 진행을 눌렀을 때 나오는 이미지 세팅
        /// 현재 이미지가 없어서 딱히 세팅이 없는 상태이다.
        /// </summary>
        /// <param name="isOn"></param>
        void ProgressMode(bool isOn)
        {
            if (isOn) { }
            else { }
        }
        
        /// <summary>
        /// 퀘스트를 클릭했을때 옆에 정보를 띄운다.
        /// </summary>
        public void OnClickQuest()
        {
            SetClick(true);
            Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.QuestInfoSetting(this);
        }

        [SerializeField]
        private GameObject selectObj;
        
        /// <summary>
        /// 클릭해서 이미지를 켜고 끈다.
        /// </summary>
        /// <param name="active"></param>
        public void SetClick(bool active)
        {
            selectObj.SetActive(active);
        }

        [SerializeField]
        private Toggle hudCheckToggle;
        
        /// <summary>
        /// 허드 체크 클릭.
        /// </summary>
        /// <param name="check"></param>
        public void OnClickHudCheck(bool check)
        {
            if (check && !Managers.QuestMgr.QuestHudCheckPossible())
            {
                hudCheckToggle.SetIsOnWithoutNotify(false);
                return;
            }

            Managers.QuestMgr.SetHudQuestInfo(QuestInfo.GetQuestID, check);
        }
    }
}
