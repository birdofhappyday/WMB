using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GDT;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using QuestInfo = AnotherWorld.Manager.QuestInfo;

namespace AnotherWorld.UI.Quest.Popup
{
    public class UI_Scenario_Basic_Popup : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = false;

        [SerializeField]
        private Image[] _img_ch_a;

        [SerializeField]
        private Image[] _img_ch_b;

        [SerializeField]
        private Image[] _img_ch_c;

        [SerializeField]
        private Image[] _im_bg_a;

        [SerializeField]
        private Image[] _im_bg_b;

        [SerializeField]
        private UI_Localize questNameTxt;

        [SerializeField]
        private UI_Localize npcNameTxt;

        [SerializeField]
        private UIButton speedButton;

        [SerializeField]
        private TextMeshProUGUI[] speedTexts;

        [SerializeField]
        private Toggle autoToggle;

        [SerializeField]
        private UIButton skipToggle;

        [SerializeField]
        private UIButton clickButton;

        [SerializeField]
        private TextMeshProUGUI dialogueDisplay;

        [SerializeField]
        private List<UI_Dialogue_select> _uiDialogueSelectList;

        private int _speed = 1;

        private ScenarioSceneT _scenarioScene;

        private List<CharacterNpcFunctionT> _characterNpcFunctionTs;

        private TinyPool<Transform> _selectMessagePool;

        [SerializeField]
        private Transform selectMessageParent;

        [SerializeField]
        private GameObject selectGameObject;

        public override void Init()
        {
            base.Init();

            if (null == _selectMessagePool)
            {
                var comp = selectGameObject.GetComponent<Transform>();
                _selectMessagePool = new TinyPool<Transform>(comp);
            }
        }

        public override void GameToLobby()
        {
            base.GameToLobby();
            StartSetting();
        }

        public override void OnClosePopup()
        {
            _clickAction = null;
            _Input.IsMoveLock = false;
            base.OnClosePopup();
        }

        public void StartSetting()
        {
            return;

            autoToggle.isOn = false;
            SpeedButtonReset();
        }

        private Action _clickAction;

        public void Setting(ScenarioSceneT scenarioScene, int npcFunctionID, int npcID, Action clickAction = null)
        {
            Clear();

            _scenarioScene = scenarioScene;

            if (ScriptCharacterType.NPC == scenarioScene.ScriptCharacterTypeEnum)
            {
                if (Managers.TableMgr.CommonGDT.CharacterInfoTable.TryGetValue(scenarioScene.CharacterInfoID,
                        out var charaterInfo))
                {
                    if (0 != charaterInfo.NpcExtention01)
                    {
                        npcNameTxt.OnTxt(UI_Localize.LanguageType.Quest,
                            Managers.TableMgr.CommonGDT.GetCharacterNpcExtention(charaterInfo.NpcExtention01).NpcName);
                    }
                }
                else
                    npcNameTxt.txt.text = string.Empty;
            }
            else if (ScriptCharacterType.Player == scenarioScene.ScriptCharacterTypeEnum)
            {
                npcNameTxt.txt.text = _Me.EnterChar.Name;
            }
            else
            {
                npcNameTxt.txt.text = string.Empty;
            }


            ButtonEnable();
            _clickAction = clickAction;

            _characterNpcFunctionTs = Managers.TableMgr.CommonGDT.GetCharacterNpcFunctionList(npcFunctionID);

            ScriptStart();
            CharacterFunctionSetting(npcID);
        }

        public void SettingSelectMessage(ScenarioSceneT scenarioScene, Action clickAction = null)
        {
            Clear();

            _scenarioScene = scenarioScene;

            if (Managers.TableMgr.CommonGDT.CharacterInfoTable.TryGetValue(scenarioScene.CharacterInfoID,
                    out var charaterInfo))
            {
                npcNameTxt.OnTxt(charaterInfo.RaceTextID);
            }

            ButtonEnable();
            _clickAction = clickAction;

            SelectMessageSetting();
        }

        /// <summary>
        /// 스킵 버튼 작동 여부 판단
        /// </summary>
        void ButtonEnable()
        {
            if (null != skipToggle)
                skipToggle.gameObject.SetActive(_scenarioScene.ScriptSkip);
        }

        /// <summary>
        /// 사용한 오브젝트들 초기화
        /// </summary>
        void Clear()
        {
            if (null != questNameTxt)
                questNameTxt.gameObject.SetActive(false);
            foreach (var select in _uiDialogueSelectList)
                select.gameObject.SetActive(false);

            if (null != _selectMessagePool)
                _selectMessagePool.Clear();
        }

        /// <summary>
        /// 대화 속도 기능
        /// </summary>
        public void OnClickSpeedButton()
        {
            _speed <<= 1;

            if (_speed == 8)
                _speed = 1;

            foreach (var txt in speedTexts)
                txt.text = $"{_speed}x";
        }

        public void SpeedButtonReset()
        {
            _speed = 1;

            foreach (var txt in speedTexts)
                txt.text = $"{_speed}x";
        }

        /// <summary>
        /// 자동 페이지 넘기는 기능
        /// </summary>
        /// <param name="isOn"></param>
        public async void OnClickAutoButton(bool isOn)
        {
            Managers.SoundMgr.PlaySfx(1);
            
            if (isOn)
            {
                autoToggle.targetGraphic.color = autoToggle.colors.pressedColor;
            }
            else
            {
                autoToggle.targetGraphic.color = autoToggle.colors.normalColor;
                _autoCancellationTokenSource?.Cancel();
                return;
            }

            if (_scriptEnd)
            {
                _clickAction?.Invoke();
            }
        }

        /// <summary>
        /// 스킵 버튼 기능
        /// </summary>
        public void OnClickSkipButton()
        {
            autoToggle.isOn = false;
            _scriptSkip = true;

            if (!_scriptEnd)
            {
                _cancellationTokenSource?.Cancel();
            }

            if (Managers.ScenarioSceneMgr.CurrentScriptEndJudge() && _scriptEnd)
            {
                _clickAction?.Invoke();
            }
            else
            {
                Managers.ScenarioSceneMgr.ScriptSkip();
                dialogueDisplay.text = UI_Localize.GetLanguage(UI_Localize.LanguageType.Quest,
                    Managers.ScenarioSceneMgr.GetCurrentLastScript(), _Me.EnterChar.Name);
                _scriptEnd = true;
            }
        }

        /// <summary>
        /// Next 버튼 기능
        /// </summary>
        public void OnClickNextButton()
        {
            autoToggle.isOn = false;

            if (!_scriptEnd)
            {
                _cancellationTokenSource?.Cancel();
                _autoCancellationTokenSource?.Cancel();
            }
            else
                _clickAction?.Invoke();
        }

        /// <summary>
        /// 스크립트 한글자씩 출력
        /// </summary>
        public void ScriptStart()
        {
            _scriptSkip = false;

            //TODO: 임시로 현재 플레이어 이름을 무조건 넣는 중이다. 추후 반드시 수정 필요
            DisplayScript(UI_Localize.GetLanguage(UI_Localize.LanguageType.Quest, _scenarioScene.ScripTxt1,
                    _Me.EnterChar.Name))
                .Forget();
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _autoCancellationTokenSource = new CancellationTokenSource();

        private bool _scriptSkip;
        private bool _scriptEnd;

        private const char _compareCharStart = '<';
        private const char _compareCharEnd = '>';

        /// <summary>
        /// 스크립트를 차례대로 출력하는 함수.
        /// </summary>
        /// <param name="script"></param>
        async UniTask DisplayScript(string script)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new();

            int lenght = script.Length;
            _scriptEnd = false;
            if (lenght == 0)
            {
                Debug.LogError("대사 잘못");
            }
            else
            {
                int index = 0;
                while (index < lenght)
                {
                    // 대사중에 html문구 체크
                    if (script[index].Equals(_compareCharStart))
                    {
                        while (!script[index].Equals(_compareCharEnd))
                            ++index;

                        ++index;
                        while (!script[index].Equals(_compareCharEnd))
                            ++index;

                        ++index;
                    }

                    dialogueDisplay.text = script.Substring(0, index);

                    float time = 0.1f / _speed;

                    var isCancelled = await UniTask
                        .WaitForSeconds(time, cancellationToken: _cancellationTokenSource.Token)
                        .SuppressCancellationThrow();

                    if (isCancelled)
                        break;

                    ++index;
                }
            }

            if (_scriptSkip)
                return;

            dialogueDisplay.text = script;

            if (null != autoToggle)
            {
                if (autoToggle.isOn)
                {
                    _autoCancellationTokenSource.Dispose();
                    _autoCancellationTokenSource = new();

                    var isCancelled = await UniTask
                        .WaitForSeconds(_scenarioScene.SceneTime, cancellationToken: _autoCancellationTokenSource.Token)
                        .SuppressCancellationThrow();

                    if (!isCancelled)
                    {
                        _scriptEnd = true;
                        _clickAction?.Invoke();
                        return;
                    }
                }
            }

            _scriptEnd = true;
        }

        /// <summary>
        /// 캐릭터 기능 함수 실행
        /// </summary>
        void CharacterFunctionSetting(int npcID)
        {
            if (null == _characterNpcFunctionTs)
                return;

            foreach (var functionT in _characterNpcFunctionTs)
            {
                switch (functionT.NpcFunctionEnum)
                {
                    case NpcFunction.NP_Warp:
                        var warp = _selectMessagePool.Rent();
                        warp.GetComponent<UI_Dialogue_select>().SetData(functionT.LanguageID,
                            () =>
                            {
                                Managers.NetworkMgr.GameClient.Proxy.Req_Warp_Npc_Use(npcID,
                                    functionT.NpcFunctionParam1);
                                Managers.ScenarioSceneMgr.ObjectClear();
                            });
                        warp.SetParent(selectMessageParent, false);
                        break;

                    case NpcFunction.NP_UIOpen:
                        var ui = _selectMessagePool.Rent();
                        ui.GetComponent<UI_Dialogue_select>().SetData(functionT.LanguageID,
                            () =>
                            {
                                Managers.UIMgr.OpenPopup(
                                    Managers.TableMgr.CommonGDT.GetContentManagerInfo(functionT.NpcFunctionParam1));
                                Managers.ScenarioSceneMgr.ObjectClear();
                            });

                        ui.SetParent(selectMessageParent, false);
                        break;
                }
            }
        }

        void SelectMessageSetting()
        {
            var scripTxt1 = _selectMessagePool.Rent();
            scripTxt1.GetComponent<UI_Dialogue_select>().SetData(
                _scenarioScene.ScripTxt1,
                () =>
                {
                    Managers.ScenarioSceneMgr.SettingScriptIndex(_scenarioScene.ScriptNextStep);
                    _clickAction?.Invoke();
                });

            scripTxt1.SetParent(selectMessageParent, false);

            if (!string.IsNullOrEmpty(_scenarioScene.ScripTxt2))
            {
                var scripTxt2 = _selectMessagePool.Rent();
                scripTxt2.GetComponent<UI_Dialogue_select>().SetData(
                    _scenarioScene.ScripTxt2,
                    () =>
                    {
                        Managers.ScenarioSceneMgr.SettingScriptIndex(_scenarioScene.ScriptNextStep);
                        _clickAction?.Invoke();
                    });

                scripTxt2.SetParent(selectMessageParent, false);
            }
        }
    }
}
