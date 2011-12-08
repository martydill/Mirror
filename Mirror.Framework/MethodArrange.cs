using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Framework
{
    public abstract class ArrangeResult<T> where T : class
    {
        public Mirror<T> Mirror { get; set; }

        public abstract void Returns<TReturnType>(TReturnType value);

        public abstract void Calls(Action methodToCall);

        public abstract void Throws(Exception exception);

    }

    public class MethodArrangeResult<T> : ArrangeResult<T> where T : class
    {
        public MethodCallInfo MethodReturnValueInfo { get; set; }

        public object[] ParameterValues { get; set; }

        public override void Returns<TReturnType>(TReturnType value)
        {
            MethodReturnValueInfo.AddReturnValue(value, ParameterValues);
        }

        public override void Calls(Action methodToCall)
        {
            MethodReturnValueInfo.AddMethodExecution(methodToCall, ParameterValues);
        }

        public override void Throws(Exception exception)
        {
            if (exception == null)
                throw new MirrorArrangeException("Cannot arrange to throw a null Exception");

            MethodReturnValueInfo.AddMethodException(exception, ParameterValues);
        }
    }

    public class MemberArrangeResult<T> : ArrangeResult<T> where T : class
    {
        public override void Returns<TReturnType>(TReturnType value)
        {
           // MethodReturnValueInfo.AddReturnValue(value, ParameterValues);
        }

        public override void Calls(Action methodToCall)
        {
           // MethodReturnValueInfo.AddMethodExecution(methodToCall, ParameterValues);
        }

        public override void Throws(Exception exception)
        {
            if (exception == null)
                throw new MirrorArrangeException("Cannot arrange to throw a null Exception");

            //MethodReturnValueInfo.AddMethodException(exception, ParameterValues);
        }
    }
}
