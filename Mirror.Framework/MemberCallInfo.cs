// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mirror.Framework
{
    /// <summary>
    /// Stores the mock info for a given method/property
    /// </summary>
    public class MockedMemberInfo
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


        /// <summary>
        /// Adds a return value to this mocked member
        /// </summary>
        /// <param name="returnValue">The value to return</param>
        /// <param name="parameterValues">The parameters that the member is being mocked with</param>
        internal void AddReturns(object returnValue, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ReturnValue = returnValue, ParameterValues = parameterValues });
        }


        /// <summary>
        /// Adds a 'calls' method execution to this mocked member
        /// </summary>
        /// <param name="methodToCall">The method that will be called</param>
        /// <param name="parameterValues">The parameters that the member is being mocked with</param>
        internal void AddCalls(Action methodToCall, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { MethodToCall = methodToCall, ParameterValues = parameterValues });
        }



        /// <summary>
        /// Adds throwing of an exception to this mocked member
        /// </summary>
        /// <param name="methodToCall">The exception that will be thrown</param>
        /// <param name="parameterValues">The parameters that the member is being mocked with</param>
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
       
        
        /// <summary>
        /// Calculates and returns the appropriate return value for this member, based on the passed-in arguments
        /// Also throws exceptions or calls methods, as appropriate
        /// </summary>
        internal object ExecuteMockedMember(object[] methodArguments)
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


        /// <summary>
        /// Returns whether or not the given collection of arguments matches the given collection of arranged parameter values
        /// </summary>
        private static bool DoParametersMatch(object[] methodArguments, object[] arrangedParameterValues)
        {
            bool doParametersMatch = true;

            if (arrangedParameterValues != null && methodArguments.Count() > 0)
            {
                for (int i = 0; i < methodArguments.Count(); ++i)
                {
                    // Figure out the value for each of the parameters
                    // (Since they could be lambdas/method calls/etc.)
                    object arrangedMethodArgument = GetValueForParameterValue(arrangedParameterValues[i]);
                    object actualMethodArgument = GetValueForParameterValue(methodArguments[i]);

                    // We have to check both the arranged parameter values and the actual method arguments 
                    // for Any() to handle the case where we are checking for Count(...)
                    if(!IsAnyParameter(arrangedParameterValues[i]) && !IsAnyParameter(methodArguments[i]))
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
        /// Returns whether or not the given arranged parameter is an 'Any of T' object, and thus should match everything of that type 
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


        /// <summary>
        /// Returns an actual value for the given parameter
        /// Executes it if it is an expression to get the result
        /// </summary>
        private static object GetValueForParameterValue(object value)
        {
            // If the result is an expression, evaluate it first
            if (value is Expression)
            {
                var lambda = Expression.Lambda(value as Expression);
                var func = lambda.Compile();
                value = func.DynamicInvoke();
            }
            return value;
        }

    }
}
