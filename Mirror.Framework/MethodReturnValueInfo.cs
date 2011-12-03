using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Framework
{
    /// <summary>
    /// Stores the return value info for a given method, based on its parameter values
    /// </summary>
    public class MethodReturnValueInfo
    {
        internal class ParameterInfo
        {
            internal object ReturnValue { get; set; }

            internal Action MethodToCall { get; set; }

            internal object[] ParameterValues { get; set; }
        }


        private List<ParameterInfo> _parameterValues = new List<ParameterInfo>();


        public MethodReturnValueInfo()
        {
        }


        public void AddReturnValue(object returnValue, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { ReturnValue = returnValue, ParameterValues = parameterValues });
        }

        internal object CalculateReturnValue(object[] inParameterValues)
        {
            object returnValue = null;
            foreach (var parameterInfo in _parameterValues)
            {
                if (inParameterValues.Count() != parameterInfo.ParameterValues.Count())
                    continue;

                bool doParametersMatch = true;
                for (int i = 0; i < inParameterValues.Count(); ++i)
                {
                    object result = parameterInfo.ParameterValues[i];

                    // If the result is an expression, evaluate it first
                    if (result is Expression)
                    {
                        var lambda = Expression.Lambda(parameterInfo.ParameterValues[i] as Expression);
                        var func = lambda.Compile();
                        result = func.DynamicInvoke();

                    }

                    // Then, check if it matches the given parameters
                    if (!Object.Equals(inParameterValues[i], result))
                    {
                        doParametersMatch = false;
                        break;
                    }
                }

                if (doParametersMatch)
                {
     
                        returnValue = parameterInfo.ReturnValue;


                    if (parameterInfo.MethodToCall != null)
                    {
                        parameterInfo.MethodToCall.DynamicInvoke(new object[]{});
                    }
                    break;
                }
            }

            return returnValue;
        }

        internal void AddMethodExecution(Action methodToCall, object[] parameterValues)
        {
            _parameterValues.Add(new ParameterInfo() { MethodToCall = methodToCall, ParameterValues = parameterValues });
        }
    }
}
