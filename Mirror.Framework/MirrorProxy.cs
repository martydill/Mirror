// The subclass of RealProxy that handles method calls

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Mirror.Framework
{
    internal sealed class MirrorProxy : RealProxy
    {
        private readonly Dictionary<object, MethodCallInfo> _methodCallInfoCollection = new Dictionary<object, MethodCallInfo>();

        private readonly Dictionary<object, PropertyCallInfo> _propertyInfoCollection = new Dictionary<object, PropertyCallInfo>();

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
                    // Method call has been arranged. Figure out the desired return value.
                    returnValue = methodCallInfo.CalculateReturnValue(methodCallMessage.InArgs);
                }
                else
                { 
                    // Method call has not been arranged. Add an arrangement for it (for logging purposes)
                    // and return a default value
                    methodCallInfo = new MethodCallInfo();
                    MethodCallInfoCollection.Add(methodCallMessageWrapper.MethodBase, methodCallInfo);
                    returnValue = CalculateDefaultReturnValue(methodCallMessageWrapper);
                }

                methodCallInfo.LogMethodCall(methodCallMessage.InArgs);

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
                return _methodCallInfoCollection;
            }
        }

        public Dictionary<object, PropertyCallInfo> PropertyInfoCollection
        {
            get
            {
                return _propertyInfoCollection;
            }
        }
    }
}
