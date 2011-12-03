using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Mirror.Framework
{
    internal sealed class MirrorProxy : RealProxy
    {
        public Dictionary<object, MethodCallInfo> _returnValues = new Dictionary<object, MethodCallInfo>();

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

                object returnValue = null;
                MethodCallInfo methodCallInfo = null;
                if (MethodCallInfoCollection.TryGetValue(methodCallMessageWrapper.MethodBase, out methodCallInfo))
                {
                    methodCallInfo.LogMethodCall(methodCallMessage.InArgs);
                    returnValue = methodCallInfo.CalculateReturnValue(methodCallMessage.InArgs);
                }
                else
                { 
                    // If this method has not been arranged, return a default value
                    returnValue = CalculateDefaultReturnValue(methodCallMessageWrapper);
                }

                var returnMessage = new ReturnMessage(returnValue, null, 0, methodCallMessageWrapper.LogicalCallContext, methodCallMessage);
                return returnMessage;
            }

            return null;
        }

        private object CalculateDefaultReturnValue(MethodCallMessageWrapper methodCallMessage)
        {
            var returnType = methodCallMessage.MethodSignature as Type[];

            Object returnValue = null;
            if(returnType.Length > 0)
                returnValue = Activator.CreateInstance(returnType[0]);

            return returnValue;
        }

        public Dictionary<object, MethodCallInfo> MethodCallInfoCollection
        {
            get
            {
                return _returnValues;
            }
        }
    }
}
