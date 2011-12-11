// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Mirror.Framework
{
    /// <summary>
    /// The class responsible for doing all of the work
    /// </summary>
    public class Mirror<TMirroredType> where TMirroredType : class
    {
        /// <summary>
        /// The backing store for the object we are mocking
        /// </summary>
        private readonly TMirroredType _proxyImpl;

        /// <summary>
        /// The proxy object that does the method interception
        /// </summary>
        private readonly MirrorProxy _proxy;

        public Mirror()
        {
            var type = typeof(TMirroredType);
            if (!type.IsInterface && !type.IsSubclassOf(typeof(MarshalByRefObject)))
                throw new MirrorCreationException(String.Format(CultureInfo.CurrentCulture, "Type {0} is not an interface or a MarshalByRefObject and cannot be mocked", type.FullName));

            _proxy = new MirrorProxy(type);
            _proxyImpl = (TMirroredType)_proxy.GetTransparentProxy();
        }


        /// <summary>
        /// Returns the object of type TMirroredType that this mirror represents
        /// </summary>
        public TMirroredType It
        {
            get
            {
                return _proxyImpl;
            }
        }

                
        /// <summary>
        /// Returns the number of times the given method was called with the given parameters
        /// </summary>
        public int Count(Expression<Action<TMirroredType>> inputFunc)
        {
            if (inputFunc == null)
                throw new ArgumentNullException("inputFunc", "inputFunc is null.");

            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            MockedMemberInfo methodCallInfo = GetMethodCallInfo(method);
            return methodCallInfo.CallCount(parameters);
        }


        /// <summary>
        /// Configures the mirror to return the specified value for the specified method or property call
        /// </summary>
        public void Returns<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc, TReturnType returnValue)
        {
            if (inputFunc == null)
                throw new ArgumentNullException("inputFunc", "inputFunc is null.");

            if (inputFunc.Body is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc.Body);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc.Body);
                methodCallInfo.AddReturns(returnValue, parameterValues);
            }
            else if (inputFunc.Body is MemberExpression)
            {
                var memberInfo = AddMember((MemberExpression)inputFunc.Body);
                memberInfo.AddReturns(returnValue, null);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.Body.GetType().Name);
        }


        /// <summary>
        /// Calls the specified function when the specified method or property is called with the given parameters
        /// </summary>
        /// <param name="inputFunc">The function being mocked</param>
        /// <param name="methodToCall">The method to call</param>
        public void Calls(Expression<Action<TMirroredType>> inputFunc, Action methodToCall)
        {
            if (methodToCall == null)
                throw new MirrorArrangeException("methodToCall cannot be null");

            CallsImpl(inputFunc.Body, methodToCall);
        }

        
        /// <summary>
        /// Calls the specified function when the specified method or property is called with the given parameters
        /// </summary>
        /// <param name="inputFunc">The function being mocked</param>
        /// <param name="methodToCall">The method to call</param>
        public void Calls<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc, Action methodToCall)
        {
            if (methodToCall == null)
                throw new MirrorArrangeException("methodToCall cannot be null");

            CallsImpl(inputFunc.Body, methodToCall);
        }


        /// <summary>
        /// Throws the specified exception when the specified method or property is called with the specified parameters
        /// </summary>
        /// <param name="inputFunc">The function being mocked</param>
        /// <param name="exceptionToThrow">The exception to be thrown</param>
        public void Throws(Expression<Action<TMirroredType>> inputFunc, Exception exceptionToThrow)
        {
            if (inputFunc == null)
                throw new ArgumentNullException("inputFunc", "inputFunc is null.");

            ThrowsImpl(inputFunc.Body, exceptionToThrow);
        }


        /// <summary>
        /// Throws the specified exception when the specified method or property is called with the specified parameters
        /// </summary>
        /// <param name="inputFunc">The function being mocked</param>
        /// <param name="exceptionToThrow">The exception to be thrown</param>
        public void Throws<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc, Exception exceptionToThrow)
        {
            if (inputFunc == null)
                throw new ArgumentNullException("inputFunc", "inputFunc is null.");

            ThrowsImpl(inputFunc.Body, exceptionToThrow);
        }


        #region Private Methods

        /// <summary>
        /// Does the work of setting up the Throws method
        /// </summary>
        /// <param name="inputFunc">The body of the function beign mocked</param>
        /// <param name="exceptionToThrow">The exception to throw</param>
        private void ThrowsImpl(Expression inputFunc, Exception exceptionToThrow)
        {
            if (exceptionToThrow == null)
                throw new ArgumentNullException("exceptionToThrow", "exceptionToThrow is null.");

            if (inputFunc is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc);
                methodCallInfo.AddThrows(exceptionToThrow, parameterValues);
            }
            else if (inputFunc is MemberExpression)
            {
                var memberInfo = AddMember((MemberExpression)inputFunc);
                memberInfo.AddThrows(exceptionToThrow, null);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.GetType().Name);
        }

        /// <summary>
        /// Does the work of setting up the Calls mock
        /// </summary>
        /// <param name="inputFunc">The body of the function being mocked</param>
        /// <param name="methodToCall">The method to call</param>
        private void CallsImpl(Expression inputFunc, Action methodToCall)
        {

            if (inputFunc is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc);
                methodCallInfo.AddCalls(methodToCall, parameterValues);
            }
            else if (inputFunc is MemberExpression)
            {
                var memberInfo = AddMember((MemberExpression)inputFunc);
                memberInfo.AddCalls(methodToCall, null);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.GetType().Name);
        }


        private MockedMemberInfo GetMethodCallInfo(System.Reflection.MethodInfo method)
        {
            MockedMemberInfo memberCallInfo = null;
            if (!_proxy.MemberCallInfoCollection.TryGetValue(method, out memberCallInfo))
            {
                memberCallInfo = new MockedMemberInfo();
                _proxy.MemberCallInfoCollection.Add(method, memberCallInfo);
            }
            return memberCallInfo;
        }


        private MockedMemberInfo GetMemberCallInfo(System.Reflection.MemberInfo member)
        {
            MockedMemberInfo memberCallInfo = null;
            if (!_proxy.MemberCallInfoCollection.TryGetValue(member, out memberCallInfo))
            {
                memberCallInfo = new MockedMemberInfo();
                _proxy.MemberCallInfoCollection.Add(member, memberCallInfo);
            }
            return memberCallInfo;
        }


        private MockedMemberInfo AddMethod(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;

            MockedMemberInfo methodCallInfo = GetMethodCallInfo(method);
            _proxy.MemberCallInfoCollection[method] = methodCallInfo;
            return methodCallInfo;
        }


        private MockedMemberInfo AddMember(MemberExpression memberExpression)
        {
            var member = memberExpression.Member;

            MockedMemberInfo methodCallInfo = GetMemberCallInfo(member);
            _proxy.MemberCallInfoCollection[member] = methodCallInfo;
            return methodCallInfo;
        }


        private static object[] GetMethodParameters(MethodCallExpression methodCallExpression)
        {
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);
            return parameterValues;
        }


        private static object[] GetParameterValues(IEnumerable<Expression> parameters)
        {
            object[] parameterArray = new object[parameters.Count()];

            for(int i = 0; i < parameters.Count(); ++i) // fixme
            {
                var parameterExpression = parameters.ElementAt(i); 
                if (parameterExpression is ConstantExpression)
                {
                    var constantExpression = parameterExpression as ConstantExpression;
                    parameterArray[i] = constantExpression.Value;
                }
                else
                {
                    parameterArray[i] = parameterExpression;
                }
            }

            return parameterArray;
        }

        #endregion
    }
}
