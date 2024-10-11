using Gpm.Ui;
using UnityEngine;

namespace AnotherWorld.UI.Quest.Popup
{
    public class QuestScroll : InfiniteScroll
    {
        public void Init()
        {
            Initialize();
            itemPrefab.SetActive(false);
        }

        public void SetContent(GameObject contentObj)
        {
            content = contentObj.GetOrAddComponent<RectTransform>();
        }
        
        public RectTransform GetContent()
        {
            return content;
        }
    }
}
