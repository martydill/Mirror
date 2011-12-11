// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;
using System.Globalization;

namespace Mirror.Framework
{
    /// <summary>
    /// The subclass of RealProxy that handles intercepting of method calls
    /// </summary>
    internal sealed class MirrorProxy : RealProxy
    {
        private readonly Dictionary<object, MockedMemberInfo> _memberCallInfoCollection = new Dictionary<object, MockedMemberInfo>();

        public MirrorProxy(Type classToProxy)
            : base(classToProxy)
        {

        }

        /// <summary>
        /// The collection of method/property mock info for this proxy
        /// </summary>
        public Dictionary<object, MockedMemberInfo> MemberCallInfoCollection
        {
            get
            {
                return _memberCallInfoCollection;
            }
        }


        /// <summary>
        /// Handles the messages that our proxy receives
        /// </summary>
        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage)
            {
                var methodCallMessage = msg as IMethodCallMessage;
                var methodCallMessageWrapper = new MethodCallMessageWrapper(methodCallMessage);

                object returnValue = null;
                MockedMemberInfo methodCallInfo = null;

                // First, try to find the PropertyInfo for this method (because it might be a backing
                // method for a property). If that fails, just use the method from the message.
                object key = GetPropertyInfoFromMethodInfo(methodCallMessageWrapper.MethodBase as MethodInfo);
                if (key == null)
                    key = methodCallMessageWrapper.MethodBase;

                if (MemberCallInfoCollection.TryGetValue(key, out methodCallInfo))
                {
                    // Method call has been arranged. Figure out the desired return value.
                    returnValue = methodCallInfo.ExecuteMockedMember(methodCallMessage.InArgs);
                }
                else
                { 
                    // Method call has not been arranged. Add an arrangement for it (for logging purposes)
                    // and return a default value
                    methodCallInfo = new MockedMemberInfo();
                    MemberCallInfoCollection.Add(methodCallMessageWrapper.MethodBase, methodCallInfo);
                    returnValue = CalculateDefaultReturnValue(methodCallMessageWrapper);
                }

                methodCallInfo.LogMethodCall(methodCallMessage.InArgs);

                var returnMessage = new ReturnMessage(returnValue, null, 0, methodCallMessageWrapper.LogicalCallContext, methodCallMessage);
                return returnMessage;
            }

            return null;
        }


        /// <summary>
        /// From http://stackoverflow.com/a/7819571/184630
        /// Returns a MethodInfo from the given PropertyInfo
        /// </summary>
        private static PropertyInfo GetPropertyInfoFromMethodInfo(MethodInfo method)
        {
            if (method.IsSpecialName && method.Name.StartsWith("set_", true, CultureInfo.InvariantCulture) || method.Name.StartsWith("get_", true, CultureInfo.InvariantCulture))
            {
                var prop = method.DeclaringType.GetProperty(method.Name.Substring(4), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return prop;
            }
            return null;
        }


        /// <summary>
        /// Calculates and returns the default return value for the given message based on its type
        /// </summary>
        private static object CalculateDefaultReturnValue(MethodCallMessageWrapper methodCallMessage)
        {
            var returnType = methodCallMessage.MethodSignature as Type[];

            Object returnValue = null;
            if(returnType.Length > 0)
                returnValue = Activator.CreateInstance(returnType[0]);

            return returnValue;
        }
    }
}
