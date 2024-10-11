using System;
using GDT;
using UnityEngine;
using UnityEngine.UI;

namespace AnotherWorld.UI.Quest.Popup
{
    public class UI_Dialogue_Tuto_Popup : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = false;

        // 순서는 ST_Dialog_Toto_TopL 부터 ST_Dialog_Toto_bottomR 순으로 넣는다.
        [SerializeField]
        private Transform[] trPosArray;

        [SerializeField]
        private UI_Quest_Dialogue_Tuto_Text uiQuestDialogTutoText;

        [SerializeField]
        private ButtonBase buttonBase;

        [SerializeField]
        private Image bg;

        public Action ClickAction { get; private set; }

        private GraphicRaycaster _graphicRaycaster;

        public override void Init()
        {
            base.Init();
            //uiQuestDialogTotoText.ButtonInit();
            buttonBase.onClick.AddListener(OnClickAction);
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        private const string Player = "Player";

        public void Setting(ScenarioSceneT scenarioSceneT, Action clickAction = null)
        {
            Transform tr = null;
            switch (scenarioSceneT.ScriptType)
            {
                case ScriptType.ST_Dialogue_Toto_TopL:
                    tr = trPosArray[0];
                    break;

                case ScriptType.ST_Dialogue_Toto_TopM:
                    tr = trPosArray[1];
                    break;

                case ScriptType.ST_Dialogue_Toto_TopR:
                    tr = trPosArray[2];
                    break;

                case ScriptType.ST_Dialogue_Toto_bottomL:
                    tr = trPosArray[3];
                    break;

                case ScriptType.ST_Dialogue_Toto_bottomM:
                    tr = trPosArray[4];
                    break;

                case ScriptType.ST_Dialogue_Toto_bottomR:
                    tr = trPosArray[5];
                    break;
            }

            ClickAction = clickAction;
            string characterName = string.Empty;
            string characterImage = string.Empty;

            var characterCountenanceTs =
                Managers.TableMgr.CommonGDT.CharacterCountenanceTable[scenarioSceneT.CharacterCountenanceID];
            foreach (var characterCountenance in characterCountenanceTs)
            {
                if (characterCountenance.CharacterID == scenarioSceneT.CharacterInfoID)
                {
                    var characterInfo = Managers.TableMgr.CommonGDT.GetCharacterInfo(scenarioSceneT.CharacterInfoID);

                    if (null != characterInfo && 0 != characterInfo.NpcExtention01)
                    {
                        characterName = Managers.TableMgr.CommonGDT
                            .GetCharacterNpcExtention(characterInfo.NpcExtention01).NpcName;
                    }

                    characterImage = characterCountenance.CountenanceValue01;

                    break;
                }
            }
            
            //TODO: 임시로 캐릭터 무조건 이름을 넣고 있다. 추후 반드시 수정 필요
            uiQuestDialogTutoText.SetText(UI_Localize.GetLanguage(UI_Localize.LanguageType.Quest, characterName),
                UI_Localize.GetLanguage(UI_Localize.LanguageType.Quest, scenarioSceneT.ScripTxt1, _Me.EnterChar.Name), tr);
            uiQuestDialogTutoText.SetImage(characterImage);


            var color = bg.color;
            Vector2 movePos = new Vector2(scenarioSceneT.ScenarioDirectingValue03,
                scenarioSceneT.ScenarioDirectingValue04);

            uiQuestDialogTutoText.Move(movePos);

            if (scenarioSceneT.ScenarioDirectingType != ScenarioDirectingType.None)
            {
                if (scenarioSceneT.ScenarioDirectingType == ScenarioDirectingType.SD_UI_Button_Focus)
                    _graphicRaycaster.enabled = false;
                else
                {
                    _graphicRaycaster.enabled = true;
                }

                color.a = 0;
                bg.color = color;
            }
            else
            {
                color.a = 0.7f;
                bg.color = color;
                _graphicRaycaster.enabled = true;
            }
        }

        public void Off()
        {
            uiQuestDialogTutoText.Off();
        }

        void OnClickAction()
        {
            ClickAction?.Invoke();
        }
    }
}
