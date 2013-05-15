using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cerebello.SolutionSetup.Parser
{
    /// <summary>
    /// Parses code inside a template text, using an object that provides values to the template.
    /// </summary>
    public static class TemplateParser
    {
        /// <summary>
        /// Represents a node of parsed code, composed of two processing methods: Compile and Execute.
        /// </summary>
        internal interface INode
        {
            /// <summary>
            /// Executes an expression using data gathered in the Compile method.
            /// If will run the code, and return the resulting data.
            /// </summary>
            /// <param name="root">Root object, used to provide all global context data.</param>
            /// <returns>Returns an object that resulted from running the code.</returns>
            object Execute(object root);

            /// <summary>
            /// Compiling is called always before Execute,
            /// to gather information and store in a way that can later be used by Execute.
            /// It also does some static analysis of the code, such as determining the expression Type.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            Type Compile(Type rootType);
        }

        /// <summary>
        /// Represents a node parsed from a property getting expression.
        /// </summary>
        internal class PropertyGet : INode
        {
            private PropertyInfo pinfo;

            /// <summary>
            /// Gets or sets the name of the property.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the element for witch the named property is a member.
            /// </summary>
            public INode Element { get; set; }

            /// <summary>
            /// Executes the call to the property using the PropertyInfo collected at compile time.
            /// </summary>
            /// <param name="root">Root object, used to provide all global context data.</param>
            /// <returns>Returns an object that resulted from getting the property.</returns>
            public object Execute(object root)
            {
                var elementExec = this.Element;
                var elementValue = elementExec != null ? elementExec.Execute(root) : root;
                var result = this.pinfo.GetValue(elementValue, null);
                return result;
            }

            /// <summary>
            /// Compiles the property get operation.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                var elementExec = this.Element;
                var elementType = elementExec != null ? elementExec.Compile(rootType) : rootType;
                this.pinfo = elementType.GetProperty(this.Name);
                return this.pinfo.PropertyType;
            }
        }

        /// <summary>
        /// Represents a node parsed from an indexer getting expression.
        /// </summary>
        internal class IndexerGet : INode
        {
            private PropertyInfo pinfo;

            /// <summary>
            /// Gets or sets the nodes used as indexes of the indexer.
            /// </summary>
            public List<INode> Indexes { get; set; }

            /// <summary>
            /// Gets or sets the element for witch the named property is a member.
            /// </summary>
            public INode Element { get; set; }

            /// <summary>
            /// Executes the call to the indexer.
            /// </summary>
            /// <param name="root">Root object, used to provide all global context data.</param>
            /// <returns>Returns an object that resulted from calling the indexer.</returns>
            public object Execute(object root)
            {
                var elementValue = this.Element.Execute(root);
                var indexValues = this.Indexes.Select(exec => exec.Execute(root)).ToArray();

                if (this.pinfo == null)
                    return ((Array)elementValue).GetValue(indexValues.Select(Convert.ToInt64).ToArray());

                var result = this.pinfo.GetValue(elementValue, indexValues);
                return result;
            }

            /// <summary>
            /// Compiles the indexer get operation.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                var elementType = this.Element.Compile(rootType);
                var indexTypes = this.Indexes.Select(exec => exec.Compile(rootType)).ToArray();

                this.pinfo = elementType.GetProperty("Item", indexTypes);
                if (this.pinfo == null)
                {
                    var pis = elementType.GetProperties()
                        .Where(pi => pi.Name == "Item")
                        .ToArray();

                    var piParams = pis
                        .Select(pi => pi.GetIndexParameters().Select(par => par.ParameterType).ToArray())
                        .ToArray();

                    var idxCall = Call.FindParamsMatch(piParams, indexTypes);
                    if (idxCall >= 0)
                        this.pinfo = pis[idxCall];
                }

                if (this.pinfo == null)
                {
                    if (!elementType.IsArray)
                        throw new Exception("Cannot use indexer on this element.");

                    return elementType.GetElementType();
                }

                return this.pinfo.PropertyType;
            }
        }

        /// <summary>
        /// Represents a node parsed from a method call expression.
        /// </summary>
        internal class Call : INode
        {
            private MethodInfo minfo;

            /// <summary>
            /// Gets or sets the name of the method to call.
            /// </summary>
            public string Name { get; set; }

            public List<INode> Parameters { get; set; }

            public INode Element { get; set; }

            public object Execute(object root)
            {
                var elementValue = this.Element != null ? this.Element.Execute(root) : root;
                var paramValues = this.Parameters.Select(exec => exec.Execute(root)).ToArray();
                var result = this.minfo.Invoke(elementValue, paramValues);
                return result;
            }

            /// <summary>
            /// Compiles the method call operation.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                var elementType = this.Element != null ? this.Element.Compile(rootType) : rootType;
                var paramTypes = this.Parameters.Select(exec => exec.Compile(rootType)).ToArray();

                this.minfo = elementType.GetMethod(this.Name, paramTypes);
                if (this.minfo == null)
                {
                    var mis = elementType.GetMethods()
                        .Where(pi => pi.Name == this.Name)
                        .ToArray();

                    var miParams = mis
                        .Select(pi => pi.GetParameters().Select(par => par.ParameterType).ToArray())
                        .ToArray();

                    var idxCall = Call.FindParamsMatch(miParams, paramTypes);
                    if (idxCall >= 0)
                        this.minfo = mis[idxCall];
                }

                if (this.minfo == null)
                    throw new Exception("Invalid method call.");

                return this.minfo.ReturnType;
            }

            public static int FindParamsMatch(Type[][] availableCalls, Type[] operandTypes)
            {
                for (int itAvailable = 0; itAvailable < availableCalls.Length; itAvailable++)
                {
                    var currentCall = availableCalls[itAvailable];
                    int it;
                    for (it = 0; it < operandTypes.Length; it++)
                    {
                        var castIs = operandTypes[it].IsCastableTo(currentCall[it]);
                        if (castIs != TypeCastIs.Covariant && castIs != TypeCastIs.BuiltInImplicit && castIs != TypeCastIs.NotNeeded)
                            break;
                    }

                    if (it == operandTypes.Length)
                        return itAvailable;
                }

                return -1;
            }

            public static object[] ConvertParams(Type[] types, object[] operands)
            {
                var newOperands = new object[types.Length];
                for (int it = 0; it < types.Length; it++)
                    newOperands[it] = Convert.ChangeType(operands[it], types[it]);

                return newOperands;
            }
        }

        /// <summary>
        /// Represents a node parsed from an array construct expression.
        /// </summary>
        internal class ArrayConstruct : INode
        {
            private Type type;

            public List<INode> Values { get; set; }

            public object Execute(object root)
            {
                var values = this.Values.Select(exec => exec.Execute(root)).ToArray();
                var array = Array.CreateInstance(this.type.GetElementType(), values.Length);
                Array.Copy(values, array, values.LongLength);
                return array;
            }

            /// <summary>
            /// In fact this just does the static type analysis.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                var types = this.Values.Select(exec => exec.Compile(rootType)).ToArray();
                if (types.Length == 1 || types.Length > 1 && types.Skip(1).All(t => t == types[0]))
                    this.type = types[0].MakeArrayType();
                else
                    this.type = typeof(object).MakeArrayType();
                return this.type;
            }
        }

        /// <summary>
        /// Represents a node parsed from a numeric literal expression.
        /// </summary>
        internal class NumberLiteral : INode
        {
            /// <summary>
            /// Gets or sets the text representation of the number.
            /// </summary>
            public string Data { get; set; }

            /// <summary>
            /// Gets the numeric literal value.
            /// </summary>
            /// <param name="root">The parameter is not used.</param>
            /// <returns>Returns the parsed number.</returns>
            public object Execute(object root)
            {
                return int.Parse(this.Data);
            }

            /// <summary>
            /// In fact this just does the static type analysis, returning the type of the literal.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                return typeof(int);
            }
        }

        /// <summary>
        /// Represents a node parsed from a string literal expression.
        /// </summary>
        internal class StringLiteral : INode
        {
            /// <summary>
            /// The string that was parsed from the literal representation.
            /// </summary>
            public string Data { get; set; }

            /// <summary>
            /// Gets the string literal value.
            /// </summary>
            /// <param name="root">The parameter is not used.</param>
            /// <returns>Returns the parsed string.</returns>
            public object Execute(object root)
            {
                return this.Data;
            }

            /// <summary>
            /// In fact this just does the static type analysis, returning typeof(string).
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                return typeof(string);
            }
        }

        /// <summary>
        /// Represents a node parsed from a ternary conditional expression.
        /// </summary>
        internal class TernaryInlineIf : INode
        {
            public INode Condition { get; set; }

            public INode TrueValue { get; set; }

            public INode FalseValue { get; set; }

            public object Execute(object root)
            {
                return (bool)this.Condition.Execute(root) ? this.TrueValue.Execute(root) : this.FalseValue.Execute(root);
            }

            /// <summary>
            /// In fact this just does the static type analysis, returning the type of the ternary expression.
            /// </summary>
            /// <param name="rootType">The type of the root object.</param>
            /// <returns>Returns the type of the parsed expression, by doing a static analysis.</returns>
            public Type Compile(Type rootType)
            {
                var typeCondition = this.Condition.Compile(rootType);
                if (typeCondition != typeof(bool))
                    throw new Exception("First operand of ternary conditional operator must be boolean.");
                var typeTrue = this.TrueValue.Compile(rootType);
                var typeFalse = this.FalseValue.Compile(rootType);
                var trueToFalse = typeTrue.IsCastableTo(typeFalse);
                var falseToTrue = typeFalse.IsCastableTo(typeTrue);

                if (trueToFalse == TypeCastIs.BuiltInImplicit || trueToFalse == TypeCastIs.Covariant || trueToFalse == TypeCastIs.NotNeeded)
                    // can cast to typeTrue to typeFalse without problems
                    return typeFalse;

                if (falseToTrue == TypeCastIs.BuiltInImplicit || falseToTrue == TypeCastIs.Covariant || falseToTrue == TypeCastIs.NotNeeded)
                    // can cast to typeFalse to typeTrue without problems
                    return typeTrue;

                return typeof(object);
            }
        }

        /// <summary>
        /// Represents a node parsed from a binary boolean operation expression.
        /// </summary>
        internal class BinaryBoolOps : INode
        {
            private Func<object, object, bool> func;
            private bool aToB;
            private bool bToA;
            private Type typeA;
            private Type typeB;

            public INode ValueA { get; set; }

            public INode ValueB { get; set; }

            public string Operator { get; set; }

            public object Execute(object root)
            {
                var valA = this.ValueA.Execute(root);
                var valB = this.ValueB.Execute(root);
                if (this.bToA) valB = Convert.ChangeType(valB, this.typeA);
                else if (this.aToB) valA = Convert.ChangeType(valA, this.typeB);
                return this.func(valA, valB);
            }

            public Type Compile(Type rootType)
            {
                this.typeA = this.ValueA.Compile(rootType);
                this.typeB = this.ValueB.Compile(rootType);
                var castAb = this.typeA.IsCastableTo(this.typeB);
                this.aToB = castAb == TypeCastIs.BuiltInImplicit || castAb == TypeCastIs.Covariant || castAb == TypeCastIs.NotNeeded;
                var castBa = this.typeB.IsCastableTo(this.typeA);
                this.bToA = castBa == TypeCastIs.BuiltInImplicit || castBa == TypeCastIs.Covariant || castBa == TypeCastIs.NotNeeded;
                this.func = this.GetOperator();
                return typeof(bool);
            }

            private Func<object, object, bool> GetOperator()
            {
                switch (this.Operator)
                {
                    case "==": return (a, b) => a.Equals(b);
                    case "!=": return (a, b) => !a.Equals(b);
                    case ">": return (a, b) => Comparer.Default.Compare(a, b) > 0;
                    case "<": return (a, b) => Comparer.Default.Compare(a, b) < 0;
                    case ">=": return (a, b) => Comparer.Default.Compare(a, b) >= 0;
                    case "<=": return (a, b) => Comparer.Default.Compare(a, b) <= 0;
                    case "||": return (a, b) => (bool)a || (bool)b;
                    case "&&": return (a, b) => (bool)a && (bool)b;
                    default:
                        throw new Exception("Invalid binary operator.");
                }
            }
        }

        /// <summary>
        /// EntityBuilder that builds objects that represents values.
        /// </summary>
        internal class ValueBuilder : SimpleParser.EntityBuilder
        {
            /// <summary>
            /// Reads the code trying to extract the AST from it.
            /// </summary>
            /// <param name="parser">The parser object used to read the code.</param>
            /// <param name="prevData">Previously parsed result. This is used to chain results together.</param>
            /// <returns>Returns an object that results from parsing the code.</returns>
            public override object Read(SimpleParser parser, object prevData)
            {
                var prevExecutor = (INode)prevData;

                parser.SkipSpaces();

                if (prevExecutor == null)
                {
                    var name = this.ReadName(parser);

                    // this is a direct method call in the root
                    var call = ReadMethodCall(parser, null, name);
                    if (call != null)
                        return call;

                    // this is a global property
                    if (!string.IsNullOrWhiteSpace(name))
                        return new PropertyGet { Name = name, Element = null };

                    // this is an array construct
                    if (parser.ReadChar('{'))
                    {
                        var values = parser.ReadList<ValueBuilder, INode>(ReadListSeparator);
                        if (values.Count > 0)
                        {
                            parser.SkipSpaces();
                            if (!parser.ReadChar('}'))
                                return new Exception("Unterminated array construct.");

                            return new ArrayConstruct { Values = values };
                        }
                    }

                    // this is a precedence operator
                    if (parser.ReadChar('('))
                    {
                        var value = parser.Read<ValueBuilder, INode>();
                        if (value == null)
                            return new Exception("Syntax error.");

                        parser.SkipSpaces();
                        if (!parser.ReadChar(')'))
                            return new Exception("Unterminated precedence construct.");

                        return value;
                    }

                    // this is a number literal
                    var number = parser.ReadWhileCharOrEof(char.IsNumber);
                    if (number.Length > 0)
                        return new NumberLiteral { Data = number };

                    // this is a string literal
                    if (parser.ReadChar('"'))
                    {
                        var strBuilder = new StringBuilder();
                        while (true)
                        {
                            strBuilder.Append(parser.ReadWhileCharOrEof(ch => ch != '\\' && ch != '"'));

                            if (parser.Eof)
                                return new InvalidOperationException("String literal not terminated.");

                            if (parser.ReadChar('\\'))
                            {
                                if (parser.ReadChar('0')) strBuilder.Append('\0');
                                else if (parser.ReadChar('n')) strBuilder.Append('\n');
                                else if (parser.ReadChar('r')) strBuilder.Append('\r');
                                else if (parser.ReadChar('\\')) strBuilder.Append('\\');
                                else if (parser.ReadChar('"')) strBuilder.Append('"');
                                else
                                    return new InvalidOperationException("Escape sequence not recognized.");
                            }

                            if (parser.ReadChar('"'))
                                break;
                        }

                        return new StringLiteral { Data = strBuilder.ToString() };
                    }
                }
                else
                {
                    if (parser.ReadChar('.'))
                    {
                        var name = this.ReadName(parser);

                        // this is a direct method call in the previous element
                        var call = ReadMethodCall(parser, prevExecutor, name);
                        if (call != null)
                            return call;

                        if (!string.IsNullOrWhiteSpace(name))
                            return new PropertyGet { Name = name, Element = prevExecutor };
                    }

                    // indexer property
                    if (parser.ReadChar('['))
                    {
                        var indexValues = parser.ReadList<ValueBuilder, INode>(ReadListSeparator);

                        if (indexValues.Count == 0)
                            return new Exception("Index is missing.");

                        parser.SkipSpaces();
                        if (!parser.ReadChar(']'))
                            return new Exception("Unterminated indexer.");

                        return new IndexerGet { Element = prevExecutor, Indexes = indexValues };
                    }

                    // this only happens when calling a delegate
                    // direct method calls never fall here
                    if (parser.ReadChar('('))
                    {
                        var paramValues = parser.ReadList<ValueBuilder, INode>(ReadListSeparator);

                        parser.SkipSpaces();
                        if (parser.ReadChar(')'))
                            return new Call { Element = prevExecutor, Name = "Invoke", Parameters = paramValues };
                    }

                    if (parser.ReadChar('?'))
                    {
                        var trueValue = parser.Read<ValueBuilder, INode>();
                        parser.SkipSpaces();
                        if (!parser.ReadChar(':'))
                            throw new Exception("Missing ':' for ternary operator a?b:c.");
                        parser.SkipSpaces();
                        var falseValue = parser.Read<ValueBuilder, INode>();

                        return new TernaryInlineIf { Condition = prevExecutor, TrueValue = trueValue, FalseValue = falseValue };
                    }

                    var binOp = parser.ReadAnyStringAlternative("==", "!=", ">", "<", ">=", "<=", "&&", "||");
                    if (binOp != null)
                    {
                        var valueB = parser.Read<ValueBuilder, INode>();

                        return new BinaryBoolOps { ValueA = prevExecutor, ValueB = valueB, Operator = binOp };
                    }
                }

                return null;
            }

            private static bool ReadListSeparator(SimpleParser parser)
            {
                parser.SkipSpaces();
                return parser.ReadChar(',');
            }

            private static object ReadMethodCall(SimpleParser parser, object prevData, string name)
            {
                using (var la = parser.LookAround())
                {
                    parser.SkipSpaces();
                    if (parser.ReadChar('('))
                    {
                        var prevExecutor = (INode)prevData;
                        var type = prevData == null ? parser.GlobalType : prevExecutor.Compile(parser.GlobalType);
                        var isMethodGroup = type.GetMethods().Any(m => m.Name == name);
                        if (isMethodGroup)
                        {
                            var paramValues = parser.ReadList<ValueBuilder, INode>(ReadListSeparator);
                            if (paramValues.Count == 1 && paramValues[0] == null)
                                paramValues.Clear();

                            parser.SkipSpaces();
                            if (parser.ReadChar(')'))
                            {
                                la.Success();
                                return new Call { Element = prevExecutor, Name = name, Parameters = paramValues };
                            }
                        }
                    }

                    return null;
                }
            }

            private string ReadName(SimpleParser parser)
            {
                using (var la = parser.LookAround())
                {
                    if (!parser.ReadChar('_')
                        && !parser.ReadChar(UnicodeCategory.LowercaseLetter)
                        && !parser.ReadChar(UnicodeCategory.UppercaseLetter))
                        return null;

                    parser.ReadWhileCharOrEof(IsIdentifierChar);

                    la.Success();

                    return la.Text;
                }
            }

            private static bool IsIdentifierChar(char ch)
            {
                var uc = char.GetUnicodeCategory(ch);
                return ch == '_' || uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter
                    || uc == UnicodeCategory.DecimalDigitNumber;
            }
        }
    }
}
