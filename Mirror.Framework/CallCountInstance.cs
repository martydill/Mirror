
namespace Mirror.Framework
{
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
