using System;
using System.IO;
using System.Web.Mvc;
using Moq;

namespace CerebelloWebRole.Tests
{
    public static class MockExtensions
    {
        /// <summary>
        /// Mocks the contents of a view that will be rendered during the test.
        /// </summary>
        /// <param name="mock"></param>
        /// <param name="viewName">Name of the view that is going to be rendered.</param>
        /// <param name="viewContentGetter">Expression that takes a ViewContext and returns the content of the view.</param>
        public static Mock<IViewEngine> SetViewContent(this Mock<IViewEngine> mock, string viewName, Func<ViewContext, string> viewContentGetter)
        {
            var resultView = new Mock<IView>();

            resultView
                .Setup(x => x.Render(It.IsAny<ViewContext>(), It.IsAny<TextWriter>()))
                .Callback<ViewContext, TextWriter>((vc, tw) => { tw.Write(viewContentGetter(vc)); });

            var viewEngineResult = new ViewEngineResult(resultView.Object, mock.Object);

            mock
                .Setup(x => x.FindPartialView(It.IsAny<ControllerContext>(), viewName, It.IsAny<bool>()))
                .Returns(viewEngineResult);

            mock
                .Setup(x => x.FindView(It.IsAny<ControllerContext>(), viewName, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(viewEngineResult);

            return mock;
        }
    }
}
