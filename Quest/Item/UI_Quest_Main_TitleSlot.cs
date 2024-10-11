using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using AnotherWorld.Manager;
using AnotherWorld.UI.Quest.Popup;
using AnotherWorld.Util;
using GDT;
using Gpm.Ui;
using SharedCode;
using UnityEngine;
using UnityEngine.UI;
using QuestInfo = AnotherWorld.Manager.QuestInfo;
using Vector2 = UnityEngine.Vector2;

namespace AnotherWorld.UI.Quest.Item
{
    public class QuestListData : InfiniteScrollData
    {
        public int QuestGroupID { get; }

        public List<QuestInfo> QuestList { get; private set; }

        public QuestTabType QuestTabType { get; private set; }

        public bool QuestUIClick { get; set; } = false;

        public QuestListData(int id, List<QuestInfo> questList)
        {
            QuestGroupID = id;
            QuestList = questList;
        }

        public QuestListData(int id)
        {
            QuestGroupID = id;
            QuestList = new List<QuestInfo>();
            var questGroup = Managers.TableMgr.CommonGDT.GetQuestGroupInfo(id);
            if (null != questGroup)
                QuestTabType = questGroup.QuestTabTypeEnum;
        }

        public void AddQuestInfo(QuestInfo questInfo)
        {
            QuestList.Add(questInfo);
        }

        public void Reset()
        {
            foreach (var questInfo in QuestList)
            {
                questInfo.Reset();
            }
        }
    }

    public class UI_Quest_Main_TitleSlot : InfiniteScrollItem
    {
        [SerializeField]
        UI_Localize questTitle;

        [SerializeField]
        List<GameObject> questStateObjList;

        [SerializeField]
        private GameObject questInfoSubItem;

        [SerializeField]
        private Transform questInfoSubitemParent;

        [SerializeField]
        private Toggle clickToggle;

        private TinyPool<Transform> _questInfoTinyPool;

        private QuestState _questState = QuestState.Wait;

        public QuestState QuestState
        {
            get { return _questState; }

            set
            {
                _questState = value;
                SetQuestStateMark();
            }
        }

        public QuestListData QuestListData { get; private set; }

        private RectTransform rect;

        public override void UpdateData(InfiniteScrollData scrollData)
        {
            base.UpdateData(scrollData);
            QuestListData = (QuestListData)scrollData;

            SetTitle(QuestListData.QuestGroupID);

            if (null == _questInfoTinyPool)
            {
                var comp = questInfoSubItem.GetComponent<Transform>();
                _questInfoTinyPool = new TinyPool<Transform>(comp);
            }

            _questInfoTinyPool.Clear();

            QuestInfoSubItemSetting();
            clickToggle.isOn = QuestListData.QuestUIClick;
            OnClickOpenQuestList(QuestListData.QuestUIClick);

            clickToggle.interactable = null != QuestListData && null != QuestListData.QuestList &&
                                       0 < QuestListData.QuestList.Count;

            SetQuestState();

            //레드닷 세팅.
            var reddot = GetComponentInChildren<NewRedDot>();
            if (null != reddot)
            {
                reddot.SetRedDot(ReddotMainType.RMT_Quest, ReddotSubType.RST_QuestTitle, QuestListData.QuestGroupID);
            }
        }

        public void SetTitle(int questTileID)
        {
            questTitle.OnTxt(UI_Localize.LanguageType.Quest,
                Managers.TableMgr.CommonGDT.GetQuestGroupInfo(questTileID).QuestGroupTitle);
        }

        /// <summary>
        /// 클릭해서 아래 퀘스트 목록 불러오기
        /// </summary>
        /// <param name="isOn"></param>
        private Vector2 _toggleOffSize = new Vector2(568, 100);

        private Vector2 _toggleOnSize = new Vector2(568, 100);
        
        /// <summary>
        /// 퀘스트를 클릭했을때 아래 오브젝트를 켜면서 계산을 해주고
        /// 데이터 사이즈 업데이트를 알려준다.
        /// </summary>
        /// <param name="isOn"></param>
        public void OnClickOpenQuestList(bool isOn)
        {
            questInfoSubitemParent.SetActive(isOn);
            QuestListData.QuestUIClick = isOn;

            if (isOn)
            {
                Managers.NewRedDotMgr.SetQuestRedDotAllOff(QuestListData.QuestGroupID);
            }

            if (isOn)
            {
                _toggleOnSize.y = 100 + (QuestListData.QuestList.Count * 80);
                SetSize(_toggleOnSize);
            }
            else
            {
                SetSize(_toggleOffSize);
            }

            OnUpdateItemSize();
        }

        void QuestInfoSubItemSetting()
        {
            var count = 0;
            foreach (var questData in QuestListData.QuestList)
            {
                if (!questData.GetDisplayUI)
                    continue;

                ++count;
                var questSubitem = _questInfoTinyPool.Rent().GetComponent<UI_Quest_Main_SubSlot>();
                questSubitem.transform.SetParent(questInfoSubitemParent.transform, false);
                questSubitem.Setting(questData);
            }

            questInfoSubitemParent.SetActive(false);
        }

        void SetQuestStateMark()
        {
            foreach (var obj in questStateObjList)
                obj.SetActive(false);

            QuestUIState questUIState = QuestUIState.Before;
            switch (QuestState)
            {
                case QuestState.Wait:
                    questUIState = QuestUIState.Before;
                    break;

                case QuestState.Progress:
                case QuestState.Complete:
                    questUIState = QuestUIState.Progress;
                    break;

                case QuestState.Reward:
                    questUIState = QuestUIState.Done;
                    break;
            }

            questStateObjList[questUIState.ToInt()].SetActive(true);
        }

        /// <summary>
        /// 아래 퀘스트 상태를 체크해 마크를 세팅한다
        /// 진행중일경우 아래 리스트도 켠다.
        /// 현재 체크로 진행중은 임의로 킨다
        /// </summary>
        /// <param name="questList"></param>
        public void SetQuestState()
        {
            var questState = QuestListData.QuestList[0].GetQuestState;

            if (QuestState.Wait == questState)
                QuestState = QuestState.Wait;
            else
            {
                if (1 != QuestListData.QuestList.Count)
                {
                    questState = QuestListData.QuestList[QuestListData.QuestList.Count - 1].GetQuestState;
                }

                if (QuestState.Reward != questState)
                {
                    QuestState = QuestState.Progress;

                    var questPopup = Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>();

                    if (null == questPopup)
                        return;

                    if (!questPopup.IsSetting)
                    {
                        clickToggle.isOn = true;
                        OnClickOpenQuestList(true);
                        Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.QuestScrollSetting(GetDataIndex());
                    }
                }
                else
                    QuestState = questState;
            }
        }
    }
}
