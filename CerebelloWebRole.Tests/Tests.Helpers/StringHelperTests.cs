using CerebelloWebRole.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests.Helpers
{
    [TestClass]
    public class StringHelperTests
    {
        class TestClass
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // The getters will be accessed via reflection.
            public string Name { get; set; }
            public int Age { get; set; }
            public TestClass Child { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        [TestMethod]
        public void ReflectionReplace_SimpleProperty()
        {
            var testData = new TestClass
            {
                Name = "Miguel Angelo Santos Bicudo",
                Age = 28,
            };
            var result = StringHelper.ReflectionReplace("{ Name: <%Name%>; Age: <%Age%> }", testData);
            Assert.AreEqual("{ Name: Miguel Angelo Santos Bicudo; Age: 28 }", result);
        }

        [TestMethod]
        public void ReflectionReplace_Indirection()
        {
            var testData = new TestClass
            {
                Name = "Paulo Ricardo Dias Bicudo",
                Age = 75,
                Child = new TestClass
                {
                    Name = "Miguel Angelo Santos Bicudo",
                    Age = 28,
                }
            };
            var result = StringHelper.ReflectionReplace("{ Name: <%Name%>; Child Name: <%Child.Name%> }", testData);
            Assert.AreEqual("{ Name: Paulo Ricardo Dias Bicudo; Child Name: Miguel Angelo Santos Bicudo }", result);
        }

    }
}
