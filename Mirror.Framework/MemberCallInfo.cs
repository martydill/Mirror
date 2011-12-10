// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Framework
{
    /// <summary>
    /// Stores the mock info for a given method/property
    /// </summary>
    public class MemberCallInfo
    {
        internal class ParameterInfo
        {
            public Exception ExceptionToThrow { get; set; }

            internal object ReturnValue { get; set; }

            internal Action MethodToCall { get; set; }

            internal object[] ParameterValues { get; set; }
        }

        private readonly List<CallCountInstance> _methodCallCounts = new List<CallCountInstance>();
        private readonly List<ParameterInfo> _parameterValues = new List<ParameterInfo>();

        internal void AddReturns(object returnValue, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ReturnValue = returnValue, ParameterValues = parameterValues });
        }

        internal object CalculateReturnValue(object[] methodArguments)
        {
            object returnValue = null;


            foreach (var parameterInfo in _parameterValues)
            {
              

                if (parameterInfo.ParameterValues != null && methodArguments.Count() != parameterInfo.ParameterValues.Count())
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

            if (arrangedParameterValues != null && methodArguments.Count() > 0)
            {
                for (int i = 0; i < methodArguments.Count(); ++i)
                {
                    object arrangedMethodArgument = GetValueForParameterValue(arrangedParameterValues[i]);
                    object actualMethodArgument = GetValueForParameterValue(methodArguments[i]);

                    if(!IsAnyParameter(arrangedParameterValues[i]))
                    {
                        // Then, check if it matches the given parameters
                        if (!Object.Equals(arrangedMethodArgument, actualMethodArgument))
                        {
                            doParametersMatch = false;
                            break;
                        }
                    }
                }
            }
            return doParametersMatch;
        }

        /// <summary>
        /// Returns whether or not the given arranged parameter is an Any of T "/>
        /// </summary>
        private static bool IsAnyParameter(object arrangedParameterValue)
        {
            bool isAnyParameter = false;
            var expr = arrangedParameterValue as MemberExpression;
            if (expr != null)
            {
                var type = expr.Member.DeclaringType;
                if(type.IsGenericType)
                    isAnyParameter = type.GetGenericTypeDefinition() == typeof(Any<>);
            }

            return isAnyParameter;
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

        internal void AddCalls(Action methodToCall, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { MethodToCall = methodToCall, ParameterValues = parameterValues });
        }

        internal void AddThrows(Exception exception, object[] ParameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ExceptionToThrow = exception, ParameterValues = ParameterValues });
        }

        /// <summary>
        /// Increments the method call counter for the given set of parametesr
        /// </summary>
        internal void LogMethodCall(object[] parameters)
        {
            _methodCallCounts.Add(new CallCountInstance() { Parameters = parameters });
        }

        /// <summary>
        /// Returns the number of times the method was called with the given set of parameters
        /// </summary>
        internal int CallCount(IEnumerable<Expression> parameters)
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
    /// Keeps track of individual instances of method/property calls with the specified parameters
    /// </summary>
    internal class CallCountInstance
    {
        /// <summary>
        /// The list of parameters that the method was called with
        /// </summary>
        public object[] Parameters { get; set; }
    }
}
