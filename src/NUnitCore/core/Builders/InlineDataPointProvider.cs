// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;
using System.Reflection;
using System.Collections;
using NUnit.Core.Extensibility;

namespace NUnit.Core.Builders
{
    public class InlineDataPointProvider : IDataPointProvider
    {
        private static readonly string ParameterDataAttribute = "NUnit.Framework.ParameterDataAttribute";
        private static readonly string NUnitLiteDataAttribute = "NUnit.Framework.DataAttribute";

        private static readonly string GetDataMethod = "GetData";

        #region IDataPointProvider Members

        public bool HasDataFor(ParameterInfo parameter)
        {
            return Reflect.HasAttribute(parameter, ParameterDataAttribute, false)
                || Reflect.HasAttribute(parameter, NUnitLiteDataAttribute, false);
        }

        public IEnumerable GetDataFor(ParameterInfo parameter)
        {
            Attribute attr = Reflect.GetAttribute(parameter, ParameterDataAttribute, false);
            if (attr == null) attr = Reflect.GetAttribute(parameter, NUnitLiteDataAttribute, false);
            if (attr == null) return null;

            MethodInfo getData = attr.GetType().GetMethod(
                GetDataMethod,
                new Type[] { typeof(ParameterInfo) });
            if ( getData == null) return null;
            
            return getData.Invoke(attr, new object[] { parameter }) as IEnumerable;
        }
        #endregion
    }
}
