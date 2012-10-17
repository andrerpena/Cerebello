using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Tests
{
    [TestClass]
    public class StringHelperTests
    {
        class TestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public TestClass Child { get; set; }
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
