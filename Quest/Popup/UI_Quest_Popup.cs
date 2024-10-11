using System.Collections.Generic;
using AnotherWorld.UI.Quest.Item;
using GDT;
using Gpm.Ui;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnotherWorld.UI.Quest.Popup
{
    public enum QuestPopupMode
    {
        ALL,
        PROGESS
    }

    public class UI_Quest_Popup : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = true;

        [SerializeField]
        private QuestScroll questScroll;

        [SerializeField]
        private UI_QuestInfoView questInfoView;

        [SerializeField]
        private UI_Localize hudCount;

        [SerializeField]
        private UITabGroup _uiTabGroup;

        [SerializeField]
        private UITab episodeTab;

        [SerializeField]
        private UITab subTab;

        private UI_Quest_Main_SubSlot _currentClickQuest;

        public QuestPopupMode QuestPopupMode { get; private set; } = QuestPopupMode.ALL;

        private UI_Quest_Main_SubSlot CurrentClickQuest
        {
            get => _currentClickQuest;

            set
            {
                _currentClickQuest = value;
                questInfoView.QuestSetting(_currentClickQuest);
            }
        }

        public override void Init()
        {
            base.Init();
            ButtonInit();
            questInfoView.Init();
            ButtonSetting();
            TabSetting();
            //_uiTabGroup.InitializeTabGroup();
            questScroll.Init();
            questScroll.dynamicItemSize = true;
        }

        protected override void OnOpenPopup()
        {
            base.OnOpenPopup();
            OnClickTab();
            HudCountSetting();
        }

        public override void BackButtonEvent()
        {
            base.BackButtonEvent();
            ClosePopup();
        }

        public override void OnClosePopup()
        {
            Managers.QuestMgr.QuestHudSeverSetting();
            IsSetting = false;
            base.OnClosePopup();
        }

        void ButtonSetting()
        {
            episodeTab.AddTabClick(OnClickEpisodeTap);
            subTab.AddTabClick(OnClickSubTap);

            //레드닷 셋팅.
            var episodeRedDot = episodeTab.GetComponentInChildren<NewRedDot>();
            if (null != episodeRedDot)
            {
                episodeRedDot.SetRedDot(ReddotMainType.RMT_Quest, ReddotSubType.RST_QuestMainTab,
                    QuestTabType.QT_Q_Episode.GetHashCode());
            }

            var subRedDot = subTab.GetComponentInChildren<NewRedDot>();
            if (null != subRedDot)
            {
                subRedDot.SetRedDot(ReddotMainType.RMT_Quest, ReddotSubType.RST_QuestMainTab,
                    QuestTabType.QT_Q_sub.GetHashCode());
            }
        }

        public void OnClickEpisodeTap()
        {
            OnClickTab(QuestTabType.QT_Q_Episode);
        }

        public void OnClickTribeTap()
        {
            OnClickTab(QuestTabType.QT_Q_Tribe);
        }

        public void OnClickSubTap()
        {
            OnClickTab(QuestTabType.QT_Q_sub);
        }

        public void QuestInfoSetting(UI_Quest_Main_SubSlot uiQuestMainSubSlot)
        {
            CurrentClickQuest = uiQuestMainSubSlot;
        }

        private const string Quest_Hud_Info_Check_TEXT = "Quest_Hud_Info_Check_TEXT";

        public void HudCountSetting()
        {
            hudCount.OnTxt(UI_Localize.LanguageType.UI, Quest_Hud_Info_Check_TEXT,
                Managers.QuestMgr.GetHudQuestList().Count.ToString(),
                Managers.TableMgr.CommonGDT.CommonTable.QuestHudInfoCheckCount.ToString());
        }

        public void Clear() { }

        void ButtonInit() { }

        private const int _episodeTabKey = 20000000;
        private const int _subTabKey = 22000000;

        void TabSetting()
        {
            episodeTab.gameObject.SetActive(Managers.TableMgr.CommonGDT.GetQuestTabInfo(_episodeTabKey).Show);
            subTab.gameObject.SetActive(Managers.TableMgr.CommonGDT.GetQuestTabInfo(_subTabKey).Show);
        }

        public void OnClickProgressQuestDisplay(bool isOn)
        {
            if (isOn)
                QuestPopupMode = QuestPopupMode.PROGESS;
            else
                QuestPopupMode = QuestPopupMode.ALL;

            OnClickTab();
        }

        private QuestTabType _questTabType = QuestTabType.QT_Q_Episode;

        public void OnClickTab(QuestTabType questTabType = QuestTabType.QT_None)
        {
            Managers.QuestMgr.QuestClickInit();

            questInfoView.QuestInfoViewActive(false);

            if (QuestTabType.QT_None != questTabType)
                _questTabType = questTabType;

            questInfoView.QuestSetting();

            questScroll.Clear();
            questScroll.InsertData(Managers.QuestMgr.GetQuestListDatas(_questTabType).ToArray());
            questScroll.UpdateAllData();
        }

        public void QuestRefresh()
        {
            questScroll.UpdateAllData();
        }

        /// <summary>
        /// UI_Quest_Main_TitleSlot에서 초기에 킬 오브젝트를 선택했는지 확인 할 때 쓰는 변수
        /// </summary>
        public bool IsSetting { get; private set; } = false;

        public void QuestScrollSetting(int index)
        {
            if (!IsSetting)
            {
                questScroll.MoveTo(questScroll.GetData(index), InfiniteScroll.MoveToType.MOVE_TO_CENTER, 0);
                IsSetting = true;
            }
        }
    }
}
