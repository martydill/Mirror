
namespace Mirror.Framework
{
    /// <summary>
    /// Object that represents 'any parameter'. Used to allow for a mocking of, for example, any string value
    /// </summary>
    public class Any<TParameterType>
    {
        /// <summary>
        /// Returns an object of type T
        /// </summary>
        public static TParameterType Value
        {
            get
            {
            return default(TParameterType);
            }
         }
    }
}
