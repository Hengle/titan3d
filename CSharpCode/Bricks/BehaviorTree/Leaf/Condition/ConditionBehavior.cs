﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Leaf.Condition
{
    //[Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ConditionBehavior : LeafBehavior
    {
        public ConditionBehavior()
        {
        }
        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context)
        {            return BehaviorStatus.Failure;
        }
    }
}
