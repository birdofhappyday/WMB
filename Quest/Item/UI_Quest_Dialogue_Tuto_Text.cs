using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnotherWorld
{
    public class UI_Quest_Dialogue_Tuto_Text : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI textName;

        [SerializeField]
        private TMPro.TextMeshProUGUI textContent;

        [SerializeField]
        private Image imgCharcter;

        [SerializeField]
        private ButtonBase clickButton;

        private Action _onClickAction = null;

        public void ButtonInit()
        {
            clickButton.onClick.AddListener(OnClick);
        }

        public void SetText(string scriptTxt1, string scriptTxt2, Transform parent, Action clickAction = null)
        {
            textName.text = scriptTxt1;
            textContent.text = scriptTxt2;

            _onClickAction = clickAction;

            transform.SetParent(parent, false);

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
                imgCharcter.sprite = Managers.ResourceMgr.LoadSprite(imageName);
        }

        public void Move(Vector2 pos)
        {
            Vector3 movePos = new Vector3(pos.x, pos.y, 0);
            transform.localPosition += movePos;
        }

        public void OnClick()
        {
            _onClickAction?.Invoke();
        }

        public void Off()
        {
            gameObject.SetActive(false);
        }
    }
}
