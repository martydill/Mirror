// The subclass of RealProxy that handles method calls

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;

namespace Mirror.Framework
{
    internal sealed class MirrorProxy : RealProxy
    {
        private readonly Dictionary<object, MemberCallInfo> _methodCallInfoCollection = new Dictionary<object, MemberCallInfo>();

        public MirrorProxy(Type classToProxy)
            : base(classToProxy)
        {

        }

        public object get(MethodInfo method)
        {
            if (method.IsSpecialName && method.Name.StartsWith("set_")|| method.Name.StartsWith("get_"))
            {
                var prop = method.DeclaringType.GetProperty(method.Name.Substring(4),
                       BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return prop;
            }
            return null;

        }
        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage)
            {
                var methodCallMessage = msg as IMethodCallMessage;
                var methodCallMessageWrapper = new MethodCallMessageWrapper(methodCallMessage);

               


                object returnValue = null;
                MemberCallInfo methodCallInfo = null;
                object key = methodCallMessageWrapper.MethodBase;

                var property = get(methodCallMessageWrapper.MethodBase as MethodInfo);
                if (property != null)
                    key = property;


                if (MemberCallInfoCollection.TryGetValue(key, out methodCallInfo))
                {
                    // Method call has been arranged. Figure out the desired return value.
                    returnValue = methodCallInfo.CalculateReturnValue(methodCallMessage.InArgs);
                }
                else
                { 
                    // Method call has not been arranged. Add an arrangement for it (for logging purposes)
                    // and return a default value
                    methodCallInfo = new MemberCallInfo();
                    MemberCallInfoCollection.Add(methodCallMessageWrapper.MethodBase, methodCallInfo);
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

        public Dictionary<object, MemberCallInfo> MemberCallInfoCollection
        {
            get
            {
                return _methodCallInfoCollection;
            }
        }
    }
}
