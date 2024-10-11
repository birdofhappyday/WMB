using System.Threading;
using Cysharp.Threading.Tasks;
using GDT;
using UnityEngine;
using QuestInfo = AnotherWorld.Manager.QuestInfo;

namespace AnotherWorld.UI.Quest.Popup
{
    public class UI_Quest_Dialogue_Popup : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = false;

        [SerializeField]
        private UI_Localize questNameTxt;

        [SerializeField]
        private UI_Localize nameTxt;

        [SerializeField]
        private ButtonBase[] speedButtons;

        [SerializeField]
        private ButtonBase autoButton;

        [SerializeField]
        private ButtonBase skipButton;

        [SerializeField]
        private TMPro.TextMeshProUGUI dialogueDisplay;

        [SerializeField]
        private Transform selectMessageParent;

        private float speed = 0.1f;

        public override void Init()
        {
            base.Init();

            autoButton.onClick.AddListener(OnClickAutoButton);
            skipButton.onClick.AddListener(OnClickSkipButton);
        }

        public void Setting(QuestInfo questInfo, ScenarioSceneT scenarioScene)
        {
            questNameTxt.OnTxt(questInfo.GetQuestTitle);

            var npcExtention = Managers.TableMgr.CommonGDT.CharacterInfoTable[scenarioScene.CharacterInfoID]
                .NpcExtention01;

            if (0 != npcExtention)
            {
                nameTxt.OnTxt(UI_Localize.LanguageType.Quest,
                    Managers.TableMgr.CommonGDT.GetCharacterNpcExtention(npcExtention).NpcName);
            }
        }

        public void InterActionSetting(ScenarioSceneT scenarioScene)
        {
            var npcExtention = Managers.TableMgr.CommonGDT.CharacterInfoTable[scenarioScene.CharacterInfoID]
                .NpcExtention01;

            if (0 != npcExtention)
            {
                nameTxt.OnTxt(UI_Localize.LanguageType.Quest,
                    Managers.TableMgr.CommonGDT.GetCharacterNpcExtention(npcExtention).NpcName);
            }
        }

        public void OnClickSpeedButton() { }

        public void OnClickAutoButton() { }

        public void OnClickSkipButton() { }

        public void ScriptSetting() { }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        async UniTask DisplayScript(string script)
        {
            int lenght = script.Length;

            if (lenght == 0)
            {
                Debug.LogError("대사 잘못");
            }
            else
            {
                int index = 1;
                while (index >= lenght)
                {
                    dialogueDisplay.text = script.Substring(0, index);

                    var isCancelled = await UniTask
                        .WaitForSeconds(speed, cancellationToken: _cancellationTokenSource.Token)
                        .SuppressCancellationThrow();

                    if (isCancelled)
                        break;

                    ++index;
                }
            }

            dialogueDisplay.text = script;
        }
    }
}
