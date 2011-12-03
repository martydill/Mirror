using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Mirror.Framework
{
    internal sealed class MirrorProxy : RealProxy
    {
        public Dictionary<object, MethodReturnValueInfo> _returnValues = new Dictionary<object, MethodReturnValueInfo>();

        public MirrorProxy(Type classToProxy)
            : base(classToProxy)
        {

        }

        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage)
            {
                var methodCallMessage = msg as IMethodCallMessage;
                var methodCallMessageWrapper = new MethodCallMessageWrapper(methodCallMessage);

                var returnValueInfo = ReturnValues[methodCallMessageWrapper.MethodBase];

                var returnValue = returnValueInfo.CalculateReturnValue(methodCallMessage.InArgs);

                var returnMessage = new ReturnMessage(returnValue, null, 0, methodCallMessageWrapper.LogicalCallContext, methodCallMessage);
                return returnMessage;
            }

            return null;
        }

        public Dictionary<object, MethodReturnValueInfo> ReturnValues
        {
            get
            {
                return _returnValues;
            }
        }
    }
}
