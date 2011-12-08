using System;
using System.Linq;  
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Mirror.Framework
{
    public class Mirror<TMirroredType> where TMirroredType : class
    {
        private TMirroredType _proxyImpl;
        private readonly MirrorProxy _proxy;

        public Mirror()
        {
            var type = typeof(TMirroredType);
            if (!type.IsInterface && !type.IsSubclassOf(typeof(MarshalByRefObject)))
                throw new MirrorCreationException(String.Format("Type {0} is not an interface or a MarshalByRefObject and cannot be mocked", type.FullName));

            _proxy = new MirrorProxy(type);
        }


        /// <summary>
        /// Returns the number of times the given method was called with the given parameters
        /// </summary>
        public int Count(Expression<Action<TMirroredType>> inputFunc)
        {
            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            MethodCallInfo methodCallInfo = GetMethodCallInfo(method);
            return methodCallInfo.CallCount(parameters);
        }

        public ArrangeResult<TMirroredType> Arrange(Expression<Action<TMirroredType>> inputFunc)
        {
            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);

            MethodCallInfo newMethodReturnValueInfo = GetMethodCallInfo(method);

            return new MethodArrangeResult<TMirroredType>() { MethodReturnValueInfo = newMethodReturnValueInfo, ParameterValues = parameterValues };
        }


        private MethodCallInfo GetMethodCallInfo(System.Reflection.MethodInfo method)
        {
            MethodCallInfo newMethodReturnValueInfo = null;
            if (!_proxy.MethodCallInfoCollection.TryGetValue(method, out newMethodReturnValueInfo))
            {
                newMethodReturnValueInfo = new MethodCallInfo();
                _proxy.MethodCallInfoCollection.Add(method, newMethodReturnValueInfo);
            }
            return newMethodReturnValueInfo;
        }


        private PropertyCallInfo GetPropertyCallInfo(System.Reflection.MemberInfo property)
        {
            PropertyCallInfo propertyCallInfo = null;
            if (!_proxy.PropertyInfoCollection.TryGetValue(property, out propertyCallInfo))
            {
                propertyCallInfo = new PropertyCallInfo();
                _proxy.PropertyInfoCollection.Add(property, propertyCallInfo);
            }
            return propertyCallInfo;
        }


        /// <summary>
        /// Sets up the given return value or action for the given function
        /// </summary>
        public ArrangeResult<TMirroredType> Arrange<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc)
        {
            if (inputFunc.Body is MethodCallExpression)
                return HandleMethodArrange((MethodCallExpression)inputFunc.Body);
            else if (inputFunc.Body is MemberExpression)
                return HandleMemberArrange((MemberExpression)inputFunc.Body);
            else
                throw new MirrorArrangeException("Unsupported expression type " + inputFunc.Body.GetType().Name);
        }


        private ArrangeResult<TMirroredType> HandleMethodArrange(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);

            MethodCallInfo methodCallInfo = GetMethodCallInfo(method);
            _proxy.MethodCallInfoCollection[method] = methodCallInfo;

            return new MethodArrangeResult<TMirroredType>() { MethodReturnValueInfo = methodCallInfo, ParameterValues = parameterValues };
        }


        private ArrangeResult<TMirroredType> HandleMemberArrange(MemberExpression memberExpression)
        {
            var property = memberExpression.Member;
            //var value = GetParameterValues(new Expression[]{memberExpression})[0];

            PropertyCallInfo propertyCallInfo = GetPropertyCallInfo(property);
            _proxy.PropertyInfoCollection[property] = propertyCallInfo;
            return new MemberArrangeResult<TMirroredType>() { };
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

        public TMirroredType It
        {
            get
            {
                if(_proxyImpl == null)
                    _proxyImpl = (TMirroredType)_proxy.GetTransparentProxy();

                return _proxyImpl;
            }
        }
    }
}
