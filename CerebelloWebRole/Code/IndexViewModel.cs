using System.Collections.Generic;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Basic view-model for index views
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class IndexViewModel<TModel> where TModel : class
    {
        /// <summary>
        /// The count of objects being displayed
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The objects being displayed
        /// </summary>
        public List<TModel> Objects { get; set; }
    }
}