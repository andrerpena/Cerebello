using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Cerebello view page.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class CerebelloViewPage<TModel> : WebViewPage<TModel>
    {
        private ViewDataDictionary originalViewData;

        /// <summary>
        /// Sets the view data.
        /// </summary>
        /// <param name="viewData">The view data.</param>
        protected override void SetViewData(ViewDataDictionary viewData)
        {
            this.originalViewData = viewData;
            base.SetViewData(viewData);
        }

        /// <summary>
        /// Runs the page hierarchy for the ASP.NET Razor execution pipeline.
        /// </summary>
        public override void ExecutePageHierarchy()
        {
            base.ExecutePageHierarchy();

            // after executing the page, we need to copy value from the cloned ViewData
            // to the original ViewData... so that the caller can see elements inserted
            // or changed by the view
            foreach (var eachViewDataItem in this.ViewData)
                this.originalViewData[eachViewDataItem.Key] = eachViewDataItem.Value;
        }
    }

    /// <summary>
    /// Cerebello view page.
    /// </summary>
    public abstract class CerebelloViewPage : WebViewPage
    {
        private ViewDataDictionary originalViewData;

        /// <summary>
        /// Sets the view data.
        /// </summary>
        /// <param name="viewData">The view data.</param>
        protected override void SetViewData(ViewDataDictionary viewData)
        {
            this.originalViewData = viewData;
            base.SetViewData(viewData);
        }

        /// <summary>
        /// Runs the page hierarchy for the ASP.NET Razor execution pipeline.
        /// </summary>
        public override void ExecutePageHierarchy()
        {
            base.ExecutePageHierarchy();

            // after executing the page, we need to copy value from the cloned ViewData
            // to the original ViewData... so that the caller can see elements inserted
            // or changed by the view
            foreach (var eachViewDataItem in this.ViewData)
                this.originalViewData[eachViewDataItem.Key] = eachViewDataItem.Value;
        }
    }
}