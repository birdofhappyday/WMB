using System.Collections.Generic;
using AnotherWorld.Manager;
using AnotherWorld.UI.Quest.Item;
using AnotherWorld.Util;
using Cysharp.Threading.Tasks;
using GDT;
using SharedCode;
using UnityEngine;
using QuestInfo = AnotherWorld.Manager.QuestInfo;

namespace AnotherWorld.UI.Quest.Popup
{
    public class UI_QuestInfoView : MonoBehaviour
    {
        private TinyPool<UI_ItemBase> _rewardItemPool;

        [SerializeField]
        private Transform rewardItem;

        [SerializeField]
        private Transform rewardParentTr;

        [SerializeField]
        private GameObject questInfoView;

        public void Init()
        {
            ActiveInit();

            if (null == _rewardItemPool)
            {
                var comp = rewardItem.GetComponent<UI_ItemBase>();
                _rewardItemPool = new TinyPool<UI_ItemBase>(comp);
            }

            _rewardItemPool.Clear();

            QuestInfoViewActive(false);
        }

        public void QuestInfoViewActive(bool active)
        {
            questInfoView.SetActive(active);
        }

        [SerializeField]
        private List<UIButton> questInfoButtons;

        void QuestInfoButtonActive(bool active)
        {
            foreach (var button in questInfoButtons)
                button.SetActive(active);
        }

        public void SettingkQuestInfoButton()
        {
            QuestInfoButtonActive(false);
            if (null != _currentQuest)
            {
                switch (_currentQuest.GetQuestState)
                {
                    case QuestState.Wait:
                        questInfoButtons[QuestInfoButtonColor.Wait.ToInt()].SetActive(true);
                        questInfoButtons[QuestInfoButtonColor.Wait.ToInt()]
                            .ChangeInteractable(QuestAcceptType.QAT_Manual == _currentQuest.GetQuestAcceptType &&
                                                _currentQuest.GetQuestStartCheck);
                        break;
                    case QuestState.Complete:
                        questInfoButtons[QuestInfoButtonColor.Complete.ToInt()].SetActive(true);
                        break;
                    case QuestState.Reward:
                        questInfoButtons[QuestInfoButtonColor.Reward.ToInt()].SetActive(true);
                        break;

                    case QuestState.Progress:
                        if (QuestType.QT_Main == _currentQuest.GetQuestType)
                            questInfoButtons[QuestInfoButtonColor.Progress.ToInt()].SetActive(true);
                        else
                            questInfoButtons[QuestInfoButtonColor.QuestCancel.ToInt()].SetActive(true);
                        break;
                }
            }
        }

        public void OnClickQuestCancel()
        {
            Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Cancel(_currentQuest.GetQuestID);
        }

        public void OnClickRewardButton()
        {
            Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Reward(_currentQuest.GetQuestID);
        }

        public void OnClickQuestStartButton()
        {
            switch (_currentQuest.GetQuestStartType)
            {
                case QuestConditionType.QT_NPC:
                    Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_NPC(
                        Managers.ObjectMgr.GetNPCObjectIDTable(_currentQuest.GetQuestStartValue01),
                        _currentQuest.GetQuestID);
                    break;

                case QuestConditionType.QT_ContentMove:
                    Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_Content(_currentQuest.GetQuestID);
                    break;

                case QuestConditionType.QT_Region:
                    Managers.NetworkMgr.GameClient.Proxy.Req_Quest_Start_RegionEntry(_currentQuest.GetQuestID,
                        Managers.SceneMgr.CurGameScene.MapTbl.MapID);
                    break;
            }
        }

        private UI_Quest_Main_SubSlot _uiQuestMainSubSlot;
        private QuestInfo _currentQuest;

        public void QuestSetting(UI_Quest_Main_SubSlot uiQuestMainSubSlot = null)
        {
            if (null != _uiQuestMainSubSlot && uiQuestMainSubSlot != _uiQuestMainSubSlot)
                _uiQuestMainSubSlot.SetClick(false);

            _uiQuestMainSubSlot = uiQuestMainSubSlot;

            if (null != _uiQuestMainSubSlot)
                _currentQuest = _uiQuestMainSubSlot.QuestInfo;
            else
                _currentQuest = null;

            if (null != _currentQuest)
            {
                ClearQuestCompleteListItem();
                SettingkQuestInfoButton();

                questInfoNameText.OnTxt(UI_Localize.LanguageType.Quest, _currentQuest.GetQuestTitle);

                questInfoText.OnTxt(UI_Localize.LanguageType.Quest, _currentQuest.GetQuestText);

                for (int i = 0; i < _currentQuest.GetQuestCompleteList.Count; ++i)
                    questCompleteListItems[i].Setting(_currentQuest.GetQuestCompleteList[i].GetQuestCompleteID);

                QuestInfoViewActive(true);

                questStartButton.ChangeInteractable(QuestState.Wait == _currentQuest.GetQuestState &&
                                                    QuestConditionType.QT_NPC == _currentQuest.GetQuestStartType &&
                                                    _currentQuest.QuestPossibleCheck());
                
                questStartText.OnColor(questStartButton.Button.interactable ? _basicTextColor : _textDimedColor);

                RewardSetting();
            }
        }
        
        private const string _basicTextColor = "#FAD37E";
        private const string _textDimedColor = "#5D5D5D";

        void RewardSetting()
        {
            _rewardItemPool.Clear();

            if (0 == _currentQuest.GetRewardDataID)
                return;

            var rewardDataList = Managers.TableMgr.CommonGDT.GetRewardDataList(_currentQuest.GetRewardDataID);
            int i = 0;
            foreach (var rewardData in rewardDataList)
            {
                if (CurrencyMainType.CMT_Item == rewardData.CurrencyMainTypeID)
                {
                    var itemEquipT = Managers.TableMgr.CommonGDT.GetItemEquip(rewardData.CurrencySubTypeID);

                    bool isPossible = true;

                    if (itemEquipT != null)
                    {
                        isPossible = (Managers.LocalPlayer.CharTbl.CharacterType == Managers.TableMgr.CommonGDT
                                          .GetItemInfo(itemEquipT.ItemID).CharacterTypeID
                                      || itemEquipT.EquipSlot == EquipSlot.ES_Device);
                        // && Managers.TierMgr.GetTierData(TierType.TT_AWM).TierGrade >=
                        // itemEquipT.ReqTier;
                    }

                    if (!isPossible)
                        continue;
                }

                var reward = _rewardItemPool.Rent();
                reward.transform.SetParent(rewardParentTr, false);
                reward.transform.SetSiblingIndex(i++);
                reward.SetItemData(rewardData.CurrencyMainTypeID, rewardData.CurrencySubTypeID, rewardData.MaxCount);
                reward.gameObject.SetActive(true);
            }
        }

        [SerializeField]
        private UI_Localize questInfoNameText;

        [SerializeField]
        private UI_Localize questInfoText;

        [SerializeField]
        private List<UI_Quest_CompleteListItem> questCompleteListItems;

        void ClearQuestCompleteListItem()
        {
            foreach (var complete in questCompleteListItems)
                complete.Clear();
        }

        [SerializeField]
        private Transform deadLine;

        void ActiveInit()
        {
            deadLine.SetActive(false);
        }

        [SerializeField]
        private UIButton questStartButton;

        [SerializeField]
        private UI_Localize questStartText;

        public void OnClickQuestStart()
        {
            if (QuestState.Wait != _currentQuest.GetQuestState)
                return;

            Managers.QuestMgr.QuestNPCStartNavi(_currentQuest.GetQuestStartValue01);

            Managers.UIMgr.GetOpenPopup<UI_Quest_Popup>()?.ClosePopup();
        }
    }
}
