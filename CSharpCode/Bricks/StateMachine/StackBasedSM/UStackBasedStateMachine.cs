﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class UStackBasedStateMachine : UStateMachine
    {
        protected Stack<StackBasedState> StatesStack = new Stack<StackBasedState>();
        public UStackBasedStateMachine(string name) : base(name)
        {

        }
    }
}
