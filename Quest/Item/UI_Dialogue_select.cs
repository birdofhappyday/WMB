using System;
using AnotherWorld.UI.Quest.Popup;
using UnityEngine;

public class UI_Dialogue_select : MonoBehaviour
{
    [SerializeField]
    UI_Localize[] SelectText;

    [SerializeField]
    ButtonBase ClickButton;

    private void Start()
    {
        Init();
    }

    void Init()
    {
        ClickButton.onClick.AddListener(
            () =>
            {
                _clickAction?.Invoke();
                Managers.UIMgr.GetOpenPopup<UI_Scenario_Basic_Popup>()?.ClosePopup();
            }
        );
    }

    private Action _clickAction;

    public void SetData(string text, Action clickAction)
    {
        foreach (var item in SelectText)
            item.OnTxt(text);

        _clickAction = clickAction;
        gameObject.SetActive(true);
    }
}
