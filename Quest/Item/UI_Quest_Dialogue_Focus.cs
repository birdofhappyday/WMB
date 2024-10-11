using System;
using System.Collections.Generic;
using AnotherWorld.UI;
using AnotherWorld.UI.Tween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using AnotherWorld.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;

namespace AnotherWorld
{
    public class UI_Quest_Dialogue_Focus : UI_Popup
    {
        public override bool IsFullScreenWindow { get; protected set; } = false;
        
        private const string BasicFindPath = "/@UI_Root/";

        [SerializeField]
        private Transform _focusUI_Parent;

        [SerializeField]
        private GameObject bg;

        private GameObject copyObj;
        private GameObject findObj;

        private GraphicRaycaster _graphicRaycaster;

        public override void Init()
        {
            base.Init();
            if (_graphicRaycaster == null)
                _graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 그냥 FocusUI 설정
        /// </summary>
        /// <param name="scenarioSceneT"></param>
        public void Setting(GDT.ScenarioSceneT scenarioSceneT)
        {
            ClearFocusObject();

            bg.SetActive(true);

            FindGameObject(scenarioSceneT);

            if (findObj == null)
            {
                Debug.LogError("Tablerror");
                return;
            }

            CopyObjectMake();

            MakeImage(scenarioSceneT.ScenarioDirectingValue02);

            _graphicRaycaster.enabled = false;
        }

        /// <summary>
        /// FocusUI 버튼 설정
        /// </summary>
        /// <param name="scenarioSceneT"></param>
        public void ButtonSetting(GDT.ScenarioSceneT scenarioSceneT)
        {
            ClearFocusObject();

            bg.SetActive(true);

            FindGameObject(scenarioSceneT);

            if (findObj == null)
            {
                Debug.LogError("Tablerror");
                return;
            }

            CopyObjectMake();

            var value02StrArray = scenarioSceneT.ScenarioDirectingValue02.Split(", ");

            if (value02StrArray.Length == 3)
            {
                MakeImage(value02StrArray[0]);

                MakeMarkDisplay(value02StrArray[1], value02StrArray[2]);
            }
            else
            {
                MakeImage(scenarioSceneT.ScenarioDirectingValue02);
            }

            var button = copyObj.GetComponent<ButtonBase>();
            if (null != button)
                button.onClick.AddListener(OnClick);
            else
            {
                var uiButton = copyObj.GetComponent<UIButton>();
                if (null != uiButton)
                {
                    uiButton.ChangeGlobalCoomTimeSet(false);
                    uiButton.AddButtonClick(OnClick);
                }
            }

            _graphicRaycaster.enabled = true;
        }

        public void FocusAreaSetting(GDT.ScenarioSceneT scenarioSceneT)
        {
            ClearFocusObject();

            MakeFocusImage(scenarioSceneT);
        }

        void FindGameObject(GDT.ScenarioSceneT scenarioSceneT)
        {
            var splitString = scenarioSceneT.ScenarioDirectingValue01.Split(", ");

            if (splitString.Length == 1)
                findObj = GameObject.Find(
                    $"{BasicFindPath}{scenarioSceneT.ScenarioDirectingValue01}");
            else
            {
                findObj = GameObject.Find(
                    $"{BasicFindPath}{splitString[0]}");
                
                if(null == findObj)
                    return;
                
                if (int.TryParse(splitString[1], out var value))
                    findObj = findObj.transform.GetChild(value).gameObject;
                else
                {
                    Debug.LogError("Tablerror");
                    Managers.ScenarioSceneMgr.ObjectClear();
                    return;
                }
            }
        }

        public Action ClickAction { get; set; }

        void OnClick()
        {
            if (copyObj != null)
            {
                Destroy(copyObj);
                copyObj = null;
            }

            if (findObj != null)
            {
                var buttonBase = findObj.GetComponent<ButtonBase>();
                if (buttonBase != null)
                {
                    buttonBase.onClick.Invoke();
                }
                else
                {
                    var uiButton = findObj.GetComponent<UIButton>();
                    if (uiButton != null)
                    {
                        uiButton.Button.onClick.Invoke();
                    }
                }                    
            }

            imgObj.SetActive(false);

            ClickAction?.Invoke();
            Managers.ScenarioSceneMgr.ProgressScenarioScene();
        }

        /// <summary>
        /// NOTE::HANS
        /// 이전 FocusUI 가 남아있는 이슈가 있어서 Clear 하는 함수 추가
        /// </summary>
        public void ClearFocusObject()
        {
            if (copyObj != null)
            {
                Destroy(copyObj);
                copyObj = null;
            }

            if (mark != null)
            {
                Destroy(mark);
                mark = null;
            }

            if (flickerObj != null)
            {
                Destroy(flickerObj);
                flickerObj = null;
            }

            if (imgObj.activeSelf)
                imgObj.SetActive(false);

            foreach (var img in _blackImageList)
                img.SetActive(false);

            flicker.SetActive(false);
            bg.SetActive(false);
        }

        private const string Bg_Point_01 = "Bg_Point_01";
        private const string Bg_Point_02 = "Bg_Point_02";

        [SerializeField]
        private GameObject imgObj;

        private GameObject flickerObj;

        void MakeImage(string image)
        {
            //OtherObjMake(img, imgObj);

            flickerObj = Instantiate(imgObj, copyObj.transform);
            flickerObj.transform.SetParent(_focusUI_Parent);
            flickerObj.transform.SetSiblingIndex(0);
            flickerObj.gameObject.SetActive(true);

            var img = flickerObj.GetOrAddComponent<Image>();
            img.sprite = Managers.ResourceMgr.LoadSprite(image);
            img.type = Image.Type.Sliced;

            var oriRectTr = copyObj.GetOrAddComponent<RectTransform>();
            var rectTr = flickerObj.GetOrAddComponent<RectTransform>();

            if (string.Equals(Bg_Point_02, image))
            {
                var size = oriRectTr.sizeDelta;
                size.x += 20;
                size.y += 20;
                rectTr.sizeDelta = size;
                //rectTr.sizeDelta = oriRectTr.sizeDelta;
            }
            else
            {
                rectTr.sizeDelta = oriRectTr.sizeDelta * 1.2f;
            }

            img.GetOrAddComponent<TweenFlicker>();
            flickerObj.SetActive(true);
        }

        void CopyObjectMake()
        {
            copyObj = Instantiate(findObj, _focusUI_Parent, true);

            var check = copyObj.GetComponent<ContentSizeFitter>();
            if (check != null)
                check.enabled = false;
        }

        /// <summary>
        /// copy오브젝트 중심으로 contain위치를 맞춤
        /// </summary>
        /// <param name="contain"></param>
        void OtherObjMake(GameObject contain, GameObject origin)
        {
            // var check = copyObj.GetComponent<LayoutElement>();
            // if (check != null)
            //     check.enabled = false;

            contain = Instantiate(origin, copyObj.transform);
            // contain.transform.SetParent(copyObj.transform);
            // contain.transform.localPosition = Vector3.zero;
            contain.transform.SetParent(_focusUI_Parent);

            contain.transform.SetSiblingIndex(0);
            contain.gameObject.SetActive(true);
            // if (check != null)
            //     check.enabled = true;
        }

        [SerializeField]
        private GameObject flicker;

        /// <summary>
        /// 왼쪽 아래점부터 시계방향으로 돌아가면서 점을 계산하고
        /// 영역을 계산한다. 
        /// </summary>
        void MakeFocusImage(GDT.ScenarioSceneT scenarioSceneT)
        {
            flicker.transform.localPosition = global::Util.GetVector2(scenarioSceneT.ScenarioDirectingValue02);

            var rectTr = flicker.GetComponent<RectTransform>();

            rectTr.sizeDelta = global::Util.GetVector2(scenarioSceneT.ScenarioDirectingValue01);

            //FindGameObject(scenarioSceneT);

            // var rectTr = findObj.GetComponent<RectTransform>();
            // Vector3[] cornersArray = new Vector3[4];
            // // rectTr.GetWorldCorners(cornersArray);
            // // Debug.Log("World Corners");
            // // for (var i = 0; i < 4; i++)
            // // {
            // //     Debug.Log("World Corner " + i + " : " + cornersArray[i]);
            // // }
            // rectTr.GetLocalCorners(cornersArray);
            // Debug.Log("Local Corners");
            // for (var i = 0; i < 4; i++)
            // {
            //     Debug.Log("Local Corner " + i + " : " + cornersArray[i]);
            //     _blackImageList[i].transform.SetParent(findObj.transform);
            //     _blackImageList[i].transform.localScale = Vector3.one;
            //     _blackImageList[i].transform.localPosition = cornersArray[i];
            //     Debug.Log("_blackImageList[0].transform.position " + i + " : " + _blackImageList[i].transform.position);
            //     Debug.Log("_blackImageList[0].GetComponent<RectTransform>().anchoredPosition " + i + " : " +
            //               _blackImageList[i].GetComponent<RectTransform>().anchoredPosition);
            // }
            //
            // YieldOneFrame().Forget();


            var objPos = rectTr.anchoredPosition;
            var objSizeDelta = rectTr.sizeDelta / 2;
            // flicker.transform.position = objPos;
            // flicker.GetComponent<RectTransform>().sizeDelta = findObj.GetComponent<RectTransform>().sizeDelta;

            var rectParTr = Managers.UIMgr.Root.GetComponent<RectTransform>().sizeDelta;
            rectParTr /= 2;

            var corner1 = Vector2.zero;
            corner1.x = objPos.x - objSizeDelta.x;
            corner1.y = objPos.y - objSizeDelta.y;

            var corners1Center = new Vector2(corner1.x + -rectParTr.x, corner1.y + rectParTr.y);

            corners1Center /= 2;

            BlackImageSetting(corners1Center,
                new Vector2(corner1.x + rectParTr.x, -corner1.y + rectParTr.y), 0);

            var corner2 = Vector2.zero;
            corner2.x = objPos.x - objSizeDelta.x;
            corner2.y = objPos.y + objSizeDelta.y;

            var corners2Center = new Vector2(corner2.x + rectParTr.x, corner2.y + rectParTr.y);

            corners2Center /= 2;

            BlackImageSetting(corners2Center, new Vector2(-corner2.x + rectParTr.x, -corner2.y + rectParTr.y), 1);

            var corner3 = Vector2.zero;
            corner3.x = objPos.x + objSizeDelta.x;
            corner3.y = objPos.y + objSizeDelta.y;

            var corners3Center = new Vector2(corner3.x + rectParTr.x, corner3.y + -rectParTr.y);

            corners3Center /= 2;

            BlackImageSetting(corners3Center, new Vector2(rectParTr.x - corner3.x, corner3.y + rectParTr.y), 2);

            var corner4 = Vector2.zero;
            corner4.x = objPos.x + objSizeDelta.x;
            corner4.y = objPos.y - objSizeDelta.y;

            var corner4Center = new Vector2(corner4.x + -rectParTr.x, corner4.y + -rectParTr.y);
            corner4Center /= 2;

            BlackImageSetting(corner4Center, new Vector2(corner4.x + rectParTr.x, corner4.y + rectParTr.y), 3);

            flicker.SetActive(true);
        }

        async UniTask YieldOneFrame()
        {
            await UniTask.WaitForSeconds(1f);

            _blackImageList[0].transform.SetParent(_focusUI_Parent);

            var anchorPos1 = _blackImageList[0].GetComponent<RectTransform>().anchoredPosition;

            var rectParTr = Managers.UIMgr.Root.GetComponent<RectTransform>().sizeDelta;
            rectParTr /= 2;

            var corners1Center = new Vector2(anchorPos1.x - rectParTr.x,
                anchorPos1.y + findObj.GetComponent<RectTransform>().sizeDelta.y - rectParTr.y);
            corners1Center /= 2;

            Vector2 size1 = new Vector2(anchorPos1.x + rectParTr.x,
                anchorPos1.y + rectParTr.y + findObj.GetComponent<RectTransform>().sizeDelta.y);
            //Vector2 size1 = new Vector2(100,  100);

            BlackImageSetting(corners1Center,
                size1, 0);


            _blackImageList[1].transform.SetParent(_focusUI_Parent);

            var anchorPos2 = _blackImageList[1].GetComponent<RectTransform>().anchoredPosition;

            var corners2Center = new Vector2(
                anchorPos2.x + findObj.GetComponent<RectTransform>().sizeDelta.x - rectParTr.x,
                anchorPos2.y + rectParTr.y);
            corners2Center /= 2;

            var size2 = new Vector2(anchorPos2.x + findObj.GetComponent<RectTransform>().sizeDelta.x + rectParTr.x,
                -anchorPos2.y + rectParTr.y);
            //Vector2 size = new Vector2(100,  100);

            BlackImageSetting(corners2Center,
                size2, 1);

            _blackImageList[2].transform.SetParent(_focusUI_Parent);

            var anchorPos3 = _blackImageList[2].GetComponent<RectTransform>().anchoredPosition;

            var corners3Center = new Vector2(anchorPos2.x + rectParTr.x,
                anchorPos2.y + rectParTr.y);
            corners3Center /= 2;

            var size3 = new Vector2(anchorPos2.x + rectParTr.x,
                -anchorPos2.y + rectParTr.y);
            //Vector2 size = new Vector2(100,  100);

            BlackImageSetting(corners3Center,
                size3, 2);
        }

        [SerializeField]
        private List<GameObject> _blackImageList;

        void BlackImageSetting(Vector2 pos, Vector2 sizeDelta, int index)
        {
            var vertexObj = _blackImageList[index];
            vertexObj.GetOrAddComponent<RectTransform>().sizeDelta = sizeDelta;
            vertexObj.GetComponent<RectTransform>().anchoredPosition = pos;
            vertexObj.SetActive(true);
        }

        private const string Icon_TutorialPoint_Arrow = "UI/Quest/Icon_TutorialPoint_Arrow";
        public float modifyPos = 5;

        [SerializeField]
        private GameObject markObj;

        private GameObject mark;

        void MakeMarkDisplay(string pos, string rot)
        {
            //OtherObjMake(mark, markObj);

            mark = Instantiate(markObj, copyObj.transform);
            mark.transform.SetParent(_focusUI_Parent);
            mark.transform.SetSiblingIndex(0);
            mark.gameObject.SetActive(true);

            var copyRectSizeDelta = copyObj.GetComponent<RectTransform>().sizeDelta;
            var markRectSizeDelta = mark.GetComponent<RectTransform>().sizeDelta;
            Vector3 position = mark.GetComponent<RectTransform>().anchoredPosition;
            switch (pos)
            {
                case "LT":
                    position.x -= copyRectSizeDelta.x / 2;
                    position.x -= markRectSizeDelta.x / 2;
                    position.y += copyRectSizeDelta.y / 2;
                    position.y += markRectSizeDelta.y / 2;
                    break;

                case "LB":
                    position.x -= copyRectSizeDelta.x / 2;
                    position.x -= markRectSizeDelta.x / 2;
                    position.y -= copyRectSizeDelta.y / 2;
                    position.y -= markRectSizeDelta.y / 2;
                    break;

                case "RT":
                    position.x += copyRectSizeDelta.x / 2;
                    position.x += markRectSizeDelta.x / 2;
                    position.y += copyRectSizeDelta.y / 2;
                    position.y += markRectSizeDelta.y / 2;
                    break;

                case "RB":
                    position.x += copyRectSizeDelta.x / 2;
                    position.x += markRectSizeDelta.x / 2;
                    position.y -= copyRectSizeDelta.y / 2;
                    position.y -= markRectSizeDelta.y / 2;
                    break;
            }

            position.x += modifyPos;
            position.y += modifyPos;

            mark.GetComponent<RectTransform>().anchoredPosition = position;

            var rotZ = Vector3.zero;
            rotZ.z = float.Parse(rot);
            mark.transform.localEulerAngles = rotZ;

            mark.SetActive(true);
        }
    }
}
