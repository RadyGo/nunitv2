﻿#if CLR_2_0 || CLR_4_0
using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.Framework
{
    /// <summary>
    /// Provide actions to execute before and after tests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class TestActionAttribute : Attribute, ITestAction
    {
        public virtual void BeforeTest(TestDetails testDetails) { }
        public virtual void AfterTest(TestDetails testDetails) { }
        
        public virtual ActionTargets Targets
        {
            get { return ActionTargets.Site; }
        }
    }
}
#endif