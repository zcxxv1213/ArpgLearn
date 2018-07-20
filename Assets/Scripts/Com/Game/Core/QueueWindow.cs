using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Com.Game.Core
{
    public class QueueWindow:BaseWindow
    {
        protected override void InternalOnViewShow()
        {
            base.InternalOnViewShow();

            sUIManager.AddQueueWindow(this);
        }

        protected override void OnRepeatShow()
        {
            base.OnRepeatShow();

            sUIManager.AddQueueWindow(this);
        }

        protected override void OnClickHideView(UnityEngine.GameObject go)
        {
            base.OnClickHideView(go);

            sUIManager.RemoveQueueWindow(this);
        }
    }
}
