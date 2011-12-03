using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Framework
{
    public class MethodArrange<T> where T : class
    {
        public MethodReturnValueInfo MethodReturnValueInfo { get; set; }

        public object[] ParameterValues { get; set; }

        public Mirror<T> Mirror { get; set; }

        public void Returns<TReturnType>(TReturnType value)
        {
            MethodReturnValueInfo.AddReturnValue(value, ParameterValues);
        }

        public void Calls(Action methodToCall)
        {
            MethodReturnValueInfo.AddMethodExecution(methodToCall, ParameterValues);
        }

        public void Throws(Exception exception)
        {
            if (exception == null)
                throw new MirrorArrangeException("Cannot arrange to throw a null Exception");

            MethodReturnValueInfo.AddMethodException(exception, ParameterValues);
        }
    }
}
