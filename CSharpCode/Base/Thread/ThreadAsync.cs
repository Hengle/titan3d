﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Thread
{
    public class TtThreadAsync : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtThreadAsync), nameof(Tick));
        public override void Tick()
        {
            //把异步事件做完
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();
            }

            //if (UEngine.Instance.TextureManager.WaitStreamingCount == 0 &&
            //    UEngine.Instance.SkeletonActionManager.PendingActions.Count == 0 &&
            //    UEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count == 0 &&
            //    (AsyncEvents.Count + ContinueEvents.Count == 0))
            if (AsyncEvents.Count + ContinueEvents.Count == 0)
            {
                System.Threading.Thread.Sleep(50);

                lock (UEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                {
                    foreach (var i in UEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                    {
                        i.ExecutePostEvent();
                        i.ExecuteContinue();
                    }
                }
            }

            base.Tick();
        }
        protected override void OnThreadStart()
        {
            
        }
        protected override void OnThreadExited()
        {

        }
    }
}
