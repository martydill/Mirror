using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Framework
{
    /// <summary>
    /// Stores the return value info for a given method, based on its parameter values
    /// </summary>
    public class MethodCallInfo
    {
        internal class ParameterInfo
        {
            public Exception ExceptionToThrow { get; set; }

            internal object ReturnValue { get; set; }

            internal Action MethodToCall { get; set; }

            internal object[] ParameterValues { get; set; }
        }

        private readonly List<MethodCallCountInstance> _methodCallCounts = new List<MethodCallCountInstance>();
        private readonly List<ParameterInfo> _parameterValues = new List<ParameterInfo>();

        internal void AddReturnValue(object returnValue, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ReturnValue = returnValue, ParameterValues = parameterValues });
        }

        internal object CalculateReturnValue(object[] methodArguments)
        {
            object returnValue = null;

            foreach (var parameterInfo in _parameterValues)
            {
                if (methodArguments.Count() != parameterInfo.ParameterValues.Count())
                    continue;

                bool doParametersMatch = DoParametersMatch(methodArguments, parameterInfo.ParameterValues);

                if (doParametersMatch)
                {
                    returnValue = parameterInfo.ReturnValue;

                    if (parameterInfo.ExceptionToThrow != null)
                    {
                        throw parameterInfo.ExceptionToThrow;
                    }
                    else if (parameterInfo.MethodToCall != null)
                    {
                        parameterInfo.MethodToCall.DynamicInvoke(new object[]{});
                    }
                    break;
                }
            }

            return returnValue;
        }

        private static bool DoParametersMatch(object[] methodArguments, object[] arrangedParameterValues)
        {
            bool doParametersMatch = true;
            for (int i = 0; i < methodArguments.Count(); ++i)
            {
                object result1 = GetValueForParameterValue(arrangedParameterValues[i]);
                object result2 = GetValueForParameterValue(methodArguments[i]);

                // Then, check if it matches the given parameters
                if (!Object.Equals(result1, result2))
                {
                    doParametersMatch = false;
                    break;
                }
            }
            return doParametersMatch;
        }

        private static object GetValueForParameterValue(object result)
        {
            // If the result is an expression, evaluate it first
            if (result is Expression)
            {
                var lambda = Expression.Lambda(result as Expression);
                var func = lambda.Compile();
                result = func.DynamicInvoke();
            }
            return result;
        }

        internal void AddMethodExecution(Action methodToCall, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { MethodToCall = methodToCall, ParameterValues = parameterValues });
        }

        internal void AddMethodException(Exception exception, object[] ParameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ExceptionToThrow = exception, ParameterValues = ParameterValues });
        }

        /// <summary>
        /// Increments the method call counter for the given set of parametesr
        /// </summary>
        internal void LogMethodCall(object[] parameters)
        {
            _methodCallCounts.Add(new MethodCallCountInstance() { Parameters = parameters });
        }

        /// <summary>
        /// Returns the number of times the method was called with the given set of parameters
        /// </summary>
        internal int CallCount(System.Collections.ObjectModel.ReadOnlyCollection<Expression> parameters)
        {
            int callCount = 0;

            foreach (var methodCallCountInstance in _methodCallCounts)
            {
                bool isMatchingMethodCall = DoParametersMatch(parameters.ToArray(), methodCallCountInstance.Parameters);

                if (isMatchingMethodCall)
                    ++callCount;
            }

            return callCount;
        }
    }

    /// <summary>
    /// Keeps track of individual instances of method calls with the specified parameters
    /// </summary>
    internal class MethodCallCountInstance
    {
        public object[] Parameters;
    }
}
