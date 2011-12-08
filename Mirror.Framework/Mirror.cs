using System;
using System.Linq;  
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Mirror.Framework
{
    public class Mirror<TMirroredType> where TMirroredType : class
    {
        private readonly TMirroredType _proxyImpl;
        private readonly MirrorProxy _proxy;

        public Mirror()
        {
            var type = typeof(TMirroredType);
            if (!type.IsInterface && !type.IsSubclassOf(typeof(MarshalByRefObject)))
                throw new MirrorCreationException(String.Format("Type {0} is not an interface or a MarshalByRefObject and cannot be mocked", type.FullName));

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
            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            MemberCallInfo methodCallInfo = GetMethodCallInfo(method);
            return methodCallInfo.CallCount(parameters);
        }


        /// <summary>
        /// Configures the mirror to return the specified value for the specified method or property call
        /// </summary>
        public void Returns<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc, TReturnType returnValue )
        {
            if (inputFunc.Body is MethodCallExpression)
            {
               var methodCallInfo = AddMethod((MethodCallExpression)inputFunc.Body);
               var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc.Body);
               methodCallInfo.AddReturnValue(returnValue, parameterValues);
            }
            else if (inputFunc.Body is MemberExpression)
            {
                var memberInfo = AddMember((MemberExpression)inputFunc.Body);
                memberInfo.AddReturnValue(returnValue, null);
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

            if (inputFunc.Body is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc.Body);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc.Body);
                methodCallInfo.AddMethodExecution(methodToCall, parameterValues);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.Body.GetType().Name);
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

            if (inputFunc.Body is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc.Body);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc.Body);
                methodCallInfo.AddMethodExecution(methodToCall, parameterValues);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.Body.GetType().Name);
        }


        /// <summary>
        /// Throws the specified exception when the specified method or property is called with the specified parameters
        /// </summary>
        /// <param name="inputFunc">The function being mocked</param>
        /// <param name="exceptionToThrow">The exception to be thrown</param>
        public void Throws(Expression<Action<TMirroredType>> inputFunc, Exception exceptionToThrow)
        {
            if (exceptionToThrow == null)
                throw new MirrorArrangeException("exceptionToThrow cannot be null");

            if (inputFunc.Body is MethodCallExpression)
            {
                var methodCallInfo = AddMethod((MethodCallExpression)inputFunc.Body);
                var parameterValues = GetMethodParameters((MethodCallExpression)inputFunc.Body);
                methodCallInfo.AddMethodException(exceptionToThrow, parameterValues);
            }
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.Body.GetType().Name);
        }


        private MemberCallInfo GetMethodCallInfo(System.Reflection.MethodInfo method)
        {
            MemberCallInfo memberCallInfo = null;
            if (!_proxy.MemberCallInfoCollection.TryGetValue(method, out memberCallInfo))
            {
                memberCallInfo = new MemberCallInfo();
                _proxy.MemberCallInfoCollection.Add(method, memberCallInfo);
            }
            return memberCallInfo;
        }

        private MemberCallInfo GetMemberCallInfo(System.Reflection.MemberInfo member)
        {
            MemberCallInfo memberCallInfo = null;
            if (!_proxy.MemberCallInfoCollection.TryGetValue(member, out memberCallInfo))
            {
                memberCallInfo = new MemberCallInfo();
                _proxy.MemberCallInfoCollection.Add(member, memberCallInfo);
            }
            return memberCallInfo;
        }

        private MemberCallInfo AddMethod(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;

            MemberCallInfo methodCallInfo = GetMethodCallInfo(method);
            _proxy.MemberCallInfoCollection[method] = methodCallInfo;
            return methodCallInfo;
        }



        private MemberCallInfo AddMember(MemberExpression memberExpression)
        {
            var member = memberExpression.Member;
            //memberExpression.
            MemberCallInfo methodCallInfo = GetMemberCallInfo(member);
            _proxy.MemberCallInfoCollection[member] = methodCallInfo;
            return methodCallInfo;

            //var property = memberExpression.Member;
            //var value = GetParameterValues(new Expression[]{memberExpression})[0];

            //PropertyCallInfo propertyCallInfo = GetPropertyCallInfo(property);
            //_proxy.PropertyInfoCollection[property] = propertyCallInfo;
            //return new MemberArrangeResult<TMirroredType>() { };
        }


        private object[] GetMethodParameters(MethodCallExpression methodCallExpression)
        {
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);
            return parameterValues;
        }


        private object[] GetParameterValues(IEnumerable<Expression> parameters)
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
    }
}
