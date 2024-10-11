using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnotherWorld
{
    public class UI_Hud_Quest_Goal : MonoBehaviour
    {
        [SerializeField]
        private UI_Localize text_Description;

        [SerializeField]
        private TMPro.TextMeshProUGUI number;

        public void SetData(string nameKey, string progress)
        {
            text_Description.OnTxt(UI_Localize.LanguageType.Quest, nameKey);
            number.text = progress;
        }
    }
}
