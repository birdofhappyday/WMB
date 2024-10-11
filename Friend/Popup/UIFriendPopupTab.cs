using System;
using System.Collections.Generic;
using System.Threading;
using AnotherWorld.UI.Friend.Obj;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnotherWorld.UI.Friend.Popup
{
    enum FriendPopupOrder
    {
        ASCENDING,
        DESCENDING,
    }

    enum FriendPopupStandard
    {
        //PLAYERNAME = 1 << 0,
        TIME,
        POINT,
    }

    public enum FriendObjType
    {
        LIST,
        CARD,
    }

    [Serializable]
    public class ListCardStruct
    {
        public UnityEngine.UI.ScrollRect list;
        public UnityEngine.UI.ScrollRect card;
    }

    /// <summary>
    /// ģ�� �˾����� �� ���� �θ� ��ũ��Ʈ
    /// </summary>
    public abstract class UIFriendPopupTab : PopupBase
    {
        public abstract void Refresh(params object[] valueArray);

        protected virtual void ListClear(int index = -1)
        {
            if (index == -1)
            {
                foreach (ListCardStruct group in contentGroup)
                {
                    if (!isObjInit)
                    {
                        foreach (Transform t in group.list.content.transform)
                        {
                            Destroy(t.gameObject);
                        }

                        foreach (Transform t in group.card.content.transform)
                        {
                            Destroy(t.gameObject);
                        }
                    }
                    else
                    {
                        foreach (Transform t in group.list.content.transform)
                        {
                            Managers.ResourceMgr.ReleaseInstance(t.gameObject);
                        }

                        foreach (Transform t in group.card.content.transform)
                        {
                            Managers.ResourceMgr.ReleaseInstance(t.gameObject);
                        }
                    }
                }

                isObjInit = true;
            }
            else
            {
                if (!isObjInit)
                {
                    foreach (Transform t in contentGroup[index].list.content.transform)
                    {
                        Destroy(t.gameObject);
                    }

                    foreach (Transform t in contentGroup[index].card.content.transform)
                    {
                        Destroy(t.gameObject);
                    }

                    isObjInit = true;
                }
                else
                {
                    foreach (Transform t in contentGroup[index].list.content.transform)
                    {
                        Managers.ResourceMgr.ReleaseInstance(t.gameObject);
                    }

                    foreach (Transform t in contentGroup[index].card.content.transform)
                    {
                        Managers.ResourceMgr.ReleaseInstance(t.gameObject);
                    }
                }
            }
        }

        protected override void Init()
        {
            base.Init();

            foreach (var setting in uiFriendPopupTopSettings)
                setting.Init(this);

            _refreshCoolTime = Managers.TableMgr.CommonGDT.CommonTable.FriendRefreshCoolTime;

            ListClear();
        }

        public virtual void GameToLobby()
        {
            _cancellationTokenSource?.Cancel();

            foreach (var setting in uiFriendPopupTopSettings)
            {
                setting.GameToLobby();
            }

            //InitClear();
        }

        protected override bool DestoryInit()
        {
            if (base.DestoryInit())
            {
                foreach (var setting in uiFriendPopupTopSettings)
                {
                    setting.DestoryInit();
                }

                _cancellationTokenSource?.Cancel();
            }

            return base.DestoryInit();
        }

        protected virtual void InitObjData() { }

        public override void Open(params object[] valueArray)
        {
            base.Open(valueArray);
            Managers.FriendMgr.DayOff();
            if (ObjType == FriendObjType.LIST)
            {
                foreach (ListCardStruct group in contentGroup)
                {
                    group.list.gameObject.SetActive(true);
                    group.card.gameObject.SetActive(false);
                }
            }
            else if (ObjType == FriendObjType.CARD)
            {
                foreach (ListCardStruct group in contentGroup)
                {
                    group.list.gameObject.SetActive(false);
                    group.card.gameObject.SetActive(true);
                }
            }

            foreach (var setting in uiFriendPopupTopSettings)
                setting.FriendSortTabInit();
        }

        [SerializeField]
        protected UIFriendPopupTopSetting[] uiFriendPopupTopSettings;

        public abstract List<UIFriendPopupListObj> GetListObj();

        [SerializeField]
        protected ListCardStruct[] contentGroup;

        protected FriendObjType ObjType;

        protected float _refreshCoolTime = 0;
        public CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        [SerializeField]
        private Image btnRefreshDimmedImg;

        [SerializeField]
        private UI_Localize btnRefreshDimmedTxt;

        public const string BUTTON_FRIENDLIST_REFRESHTIME_TEXT = "BUTTON_FRIENDLIST_REFRESHTIME_TEXT";

        protected async UniTask RefreshButtonUniTask(float time, Action<bool> afterAction = null)
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new();

            if (btnRefreshDimmedImg != null)
            {
                btnRefreshDimmedImg.DOKill();
                btnRefreshDimmedImg.fillAmount = 1;
                btnRefreshDimmedImg.DOFillAmount(0, time).SetEase(Ease.Linear).SetAutoKill(true);
            }

            while (time > 0)
            {
                if (btnRefreshDimmedTxt != null)
                {
                    btnRefreshDimmedTxt.OnTxt(UI_Localize.LanguageType.UI, BUTTON_FRIENDLIST_REFRESHTIME_TEXT,
                        time.ToString());
                }

                --time;
                var isCancelled = await UniTask.WaitForSeconds(1, cancellationToken: _cancellationTokenSource.Token)
                    .SuppressCancellationThrow();

                if (isCancelled == true)
                {
                    RefreshButtonInteractable(true);
                    afterAction?.Invoke(false);
                    break;
                }
            }


            RefreshButtonInteractable(true);
            afterAction?.Invoke(true);
        }

        [SerializeField]
        protected ButtonBase refreshButton;

        protected void RefreshButtonInteractable(bool active)
        {
            refreshButton.interactable = active;
        }
    }
}
