using UnityEngine;
using UnityEngine.Events;

namespace AnotherWorld.UI.Friend.Popup
{
    public class UITabFriend : UI_TabBase
    {
        protected UIFriendPopup _FriendBase = null;

        public override void Init(GameObject parent, UnityAction<int> action = null)
        {
            _FriendBase = parent.GetComponent<UIFriendPopup>();

            if (dicMainToggle.Count > 0)
                return;

            var toggleGroups = parent.transform.GetComponentsInChildren<ButtonBaseGroup>();
            if (toggleGroups.Length == 0)
                return;

            MainToggleGroup = toggleGroups[0];
            var mainToggle = MainToggleGroup.gameObject.transform.GetComponentInChildren<ButtonBase>(true);
            if (mainToggle == null)
                return;

            mainToggle.ButtonGroup = null;
            mainToggle.isOnToggle = true;
            mainToggle.gameObject.SetActive(false);

            for (int i = 0; i < _FriendBase._Tab_Text.Length; i++)
            {
                if (dicMainToggle.ContainsKey(i))
                    continue;

                // 토글 추가
                var toggle = GameObject.Instantiate(mainToggle, MainToggleGroup.gameObject.transform);
                toggle.SetText(UI_Localize.LanguageType.UI, _FriendBase._Tab_Text[i]);
                toggle.ButtonGroup = MainToggleGroup;
                toggle.gameObject.SetActive(true);

                int j = i;
                toggle.onToggleValueChanged = (bool _isOn) =>
                {
                    //탭 설정 변경한 후 인벤토리에서 해줘야하는 것들 처리
                    if (_isOn)
                        _FriendBase.OnClickFriendTab(j);
                };

                dicMainToggle.Add(i, toggle);
            }

            MainToggleGroup.SetAllTogglesOff();
        }

        public void SetMainToggle(GDT.QuestType subType)
        {
            if (dicMainToggle.ContainsKey((int)subType) == false)
                return;

            dicMainToggle[(int)subType].isOn = true;
        }
    }
}
