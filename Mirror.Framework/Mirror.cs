using System;
using System.Linq.Expressions;

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


        public MethodArrange<TMirroredType> Arrange(Expression<Action<TMirroredType>> inputFunc)
        {
            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);

            MethodReturnValueInfo newMethodReturnValueInfo = null;
            if (!_proxy.ReturnValues.TryGetValue(method, out newMethodReturnValueInfo))
            {
                newMethodReturnValueInfo = new MethodReturnValueInfo();
                _proxy.ReturnValues.Add(method, newMethodReturnValueInfo);
            }

            return new MethodArrange<TMirroredType>() { MethodReturnValueInfo = newMethodReturnValueInfo, ParameterValues = parameterValues };
        }

        /// <summary>
        /// Sets up the given return value or action for the given function
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="inputFunc"></param>
        /// <param name="value"></param>
        public MethodArrange<TMirroredType> Arrange<TReturnType>(Expression<Func<TMirroredType, TReturnType>> inputFunc)
        {
            var methodCallExpression = inputFunc.Body as MethodCallExpression;
            var method = methodCallExpression.Method;
            var parameters = methodCallExpression.Arguments;

            object[] parameterValues = GetParameterValues(parameters);

            MethodReturnValueInfo newMethodReturnValueInfo = null;
            if(!_proxy.ReturnValues.TryGetValue(method, out newMethodReturnValueInfo))
            {
                newMethodReturnValueInfo = new MethodReturnValueInfo();
                _proxy.ReturnValues.Add(method, newMethodReturnValueInfo);
            }
          
            _proxy.ReturnValues[method] = newMethodReturnValueInfo;

            return new MethodArrange<TMirroredType>() { MethodReturnValueInfo = newMethodReturnValueInfo, ParameterValues = parameterValues };
        }


        private object[] GetParameterValues(System.Collections.ObjectModel.ReadOnlyCollection<Expression> parameters)
        {
            object[] parameterArray = new object[parameters.Count];

            for(int i = 0; i < parameters.Count; ++i)
            {
                var parameterExpression = parameters[i];
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
