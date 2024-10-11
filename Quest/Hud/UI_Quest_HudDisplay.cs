using AnotherWorld;
using AnotherWorld.UI;
using AnotherWorld.UI.Quest.Hud;
using AnotherWorld.Util;
using GDT;
using SharedCode;
using UnityEngine;
using UnityEngine.Serialization;
using QuestInfo = AnotherWorld.Manager.QuestInfo;

public class UI_Quest_HudDisplay : MonoBehaviour
{
    [SerializeField]
    Transform parent;

    [SerializeField]
    UIButton activeButton;

    private QuestInfo _questInfo;

    bool _isInit = false;

    private TinyPool<Transform> _mainQuestInfoTinyPool;
    private TinyPool<Transform> _subQuestInfoTinyPool;

    [SerializeField]
    private GameObject mainQuestHubItem;

    [SerializeField]
    private GameObject subQuestHubItem;

    [SerializeField]
    private GameObject markerIcon;

    [SerializeField]
    private GameObject listConetent;

    public void Init()
    {
        activeButton.AddButtonClick(() =>
        {
            var isOn = listConetent.activeSelf;
            listConetent.SetActive(!isOn);
            markerIcon.SetActive(!isOn);
        });

        _isInit = true;

        if (null == _mainQuestInfoTinyPool)
        {
            var comp = mainQuestHubItem.GetComponent<Transform>();
            _mainQuestInfoTinyPool = new TinyPool<Transform>(comp);
        }

        if (null == _subQuestInfoTinyPool)
        {
            var comp = subQuestHubItem.GetComponent<Transform>();
            _subQuestInfoTinyPool = new TinyPool<Transform>(comp);
        }

        _mainQuestInfoTinyPool.Clear();
        _subQuestInfoTinyPool.Clear();
    }

    public void RefreshQuest()
    {
        if (!_isInit)
            Init();

        _mainQuestInfoTinyPool.Clear();
        _subQuestInfoTinyPool.Clear();

        Setting();
        MainQuestDisplay();
    }

    public void Setting()
    {
        if (!_isInit)
            Init();
        //퀘스트 열렸을 경우 마커 아이콘 켜주기
        markerIcon.SetActive(false);

        if (0 != Managers.QuestMgr.GetHudQuestList().Count)
        {
            foreach (var questInfo in Managers.QuestMgr.GetHudQuestList())
            {
                if (QuestType.QT_Main == questInfo.GetQuestType)
                {
                    var obj = _mainQuestInfoTinyPool.Rent().GetComponent<UI_Quest_List>();
                    obj.SetQuest(questInfo);
                    obj.transform.SetParent(parent, false);
                    obj.SetActive(true);
                    markerIcon.SetActive(true);
                }
                else if (QuestType.QT_Sub == questInfo.GetQuestType)
                {
                    var obj = _subQuestInfoTinyPool.Rent().GetComponent<UI_Quest_List>();
                    obj.SetQuest(questInfo);
                    obj.transform.SetParent(parent, false);
                    obj.SetActive(true);
                    markerIcon.SetActive(true);
                }
            }

            listConetent.SetActive(true);
            activeButton.ChangeInteractable(true);
        }
        else
        {
            listConetent.SetActive(false);
            activeButton.ChangeInteractable(false);
        }
    }

    /// <summary>
    /// 임시로 메인 퀘스트를 무조건 띄우기 위한 체크
    /// 대기 중인 퀘스트를 띄운다.
    /// </summary>
    void MainQuestDisplay()
    {
        if (0 == Managers.QuestMgr.GetHudQuestList().Count)
        {
            foreach (var questInfo in Managers.QuestMgr.GetQuestInfos(QuestType.QT_Main))
            {
                if (QuestState.Reward == questInfo.GetQuestState || QuestState.Complete == questInfo.GetQuestState)
                    continue;
                
                Managers.QuestMgr.QuestHudSettingTemp(questInfo.GetQuestID);
                
                break;
            }
        }
    }
}
