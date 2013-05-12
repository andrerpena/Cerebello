using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Cerebello.SolutionSetup.Parser
{
    /// <summary>
    /// Represents the 
    /// </summary>
    public class SimpleParser
    {
        private readonly string code;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParser"/> class.
        /// </summary>
        /// <param name="code"> The code to be parsed by the parser. </param>
        public SimpleParser(string code)
        {
            this.code = code;
        }

        /// <summary>
        /// Gets or sets the position of the next character to be parsed.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the global type, used as container of all globally available elements.
        /// </summary>
        public Type GlobalType { get; set; }

        /// <summary>
        /// Abstract representation of something that can translate code into a real entity,
        /// that is an object that represents the code.
        /// </summary>
        public abstract class EntityBuilder
        {
            /// <summary>
            /// Reads the code using a SimpleParser object, and using a previously parsed result,
            /// and then returns a new object representing the result from parsing the current code.
            /// </summary>
            /// <param name="parser">The parser object used to read the code.</param>
            /// <param name="prevData">Previously parsed result. This is used to chain results together.</param>
            /// <returns>Returns an object that results from parsing the code.</returns>
            public abstract object Read(SimpleParser parser, object prevData);
        }

        /// <summary>
        /// Reads a string until it finds the given character or the end of file.
        /// </summary>
        /// <param name="ch">Character that ends the string.</param>
        /// <returns>Returns the string read until finding either the specified character, or the end of file.</returns>
        public string ReadUntilCharOrEof(char ch)
        {
            var pos = this.Position;
            while (pos < this.code.Length && this.code[pos] != ch)
                pos++;
            var result = this.code.Substring(this.Position, pos - this.Position);
            this.Position = pos;
            return result;
        }

        /// <summary>
        /// Reads a string when the predicate indicates (return false for a character) or the end of file.
        /// </summary>
        /// <param name="predicate">Predicate that is used to know what characters should be used or not.</param>
        /// <returns>Returns the string read while allowed by the predicate, or the end of file.</returns>
        public string ReadWhileCharOrEof(Predicate<char> predicate)
        {
            var pos = this.Position;
            while (pos < this.code.Length && predicate(this.code[pos]))
                pos++;
            var result = this.code.Substring(this.Position, pos - this.Position);
            this.Position = pos;
            return result;
        }

        /// <summary>
        /// Reads only the specified character.
        /// </summary>
        /// <param name="ch">Character to be read.</param>
        /// <returns>True if the character was read; otherwise false.</returns>
        public bool ReadChar(char ch)
        {
            if (this.Position < this.code.Length)
                if (this.code[this.Position] == ch)
                {
                    this.Position++;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Reads a character, given an unicode category.
        /// </summary>
        /// <param name="category">The category of the unicode character to read.</param>
        /// <returns>True if a character in the passed unicode category was read; otherwise false.</returns>
        public bool ReadChar(UnicodeCategory category)
        {
            if (this.Position < this.code.Length)
                if (char.GetUnicodeCategory(this.code[this.Position]) == category)
                {
                    this.Position++;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Skips spaces, carriage returns, line feeds and all sorts of empty or invisible characters.
        /// </summary>
        /// <returns>Returns the number of skipped characters.</returns>
        public int SkipSpaces()
        {
            var pos = this.Position;
            int count = 0;
            while (pos < this.code.Length && char.GetUnicodeCategory(this.code[pos]) == UnicodeCategory.SpaceSeparator)
            {
                count++;
                pos++;
            }

            this.Position = pos;
            return count;
        }

        /// <summary>
        /// Creates a checkpoint, that allows some parsing to be done, and if it fails, to be reverted.
        /// </summary>
        /// <returns>
        /// Returns a disposable object,
        /// that can be used to indicate success,
        /// add undoing delegates in case of failure,
        /// and get information about the parsing state.
        /// </returns>
        public LookAroundDisposer LookAround()
        {
            return new LookAroundDisposer(this);
        }

        /// <summary>
        /// Disposable object used to control a reversible parsing block.
        /// </summary>
        public class LookAroundDisposer : IDisposable
        {
            private readonly SimpleParser parser;
            private bool ok;
            private readonly int oldPosition;

            /// <summary>
            /// Initializes a new instance of the <see cref="LookAroundDisposer"/> class.
            /// </summary>
            /// <param name="parser">
            /// The SimpleParser object that should be reverted to the current state, if the parsing block fails.
            /// </param>
            public LookAroundDisposer(SimpleParser parser)
            {
                this.parser = parser;
                this.oldPosition = parser.Position;
            }

            /// <summary>
            /// Indicates that the parsing block has succeeded.
            /// </summary>
            public void Success()
            {
                this.ok = true;
            }

            /// <summary>
            /// Disposes the current parsing code, reverting the changes or not depending on the success state.
            /// </summary>
            public void Dispose()
            {
                if (!this.ok)
                    this.parser.Position = this.oldPosition;
            }

            /// <summary>
            /// Gets the text that has been parsed since the beginning of this parsing block.
            /// </summary>
            public string Text
            {
                get { return this.parser.code.Substring(this.oldPosition, this.parser.Position - this.oldPosition); }
            }
        }

        /// <summary>
        /// Reads an entity the will be decoded by an EntityBuilder type, passed as a generic parameter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the EntityBuilder to be used to parse the code.</typeparam>
        /// <typeparam name="TResult">The type of the resulting object.</typeparam>
        /// <returns>The object resulted from parsing the code using the specified EntityBuilder type.</returns>
        public TResult Read<TEntity, TResult>() where TEntity : EntityBuilder, new()
        {
            var b = new TEntity();
            object data = null;
            while (true)
            {
                var newData = b.Read(this, data);
                if (newData == null)
                    break;
                data = newData;
            }

            return (TResult)data;
        }

        /// <summary>
        /// Reads a list of entities decoded by an EntityBuilder type, and separated by the specified SimpleParser predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the EntityBuilder to be used to parse the code.</typeparam>
        /// <typeparam name="TResult">The type of the resulting list elements.</typeparam>
        /// <param name="separatorReader">Predicate that reads the separator code, and returns true, if the separator is found.</param>
        /// <returns>Returns a list of objects resulted from parsing the code as a list.</returns>
        public List<TResult> ReadList<TEntity, TResult>(Predicate<SimpleParser> separatorReader) where TEntity : EntityBuilder, new()
        {
            var result = new List<TResult>();
            while (true)
            {
                var item = this.Read<TEntity, TResult>();
                result.Add(item);

                using (var la = this.LookAround())
                    if (separatorReader(this)) la.Success();
                    else return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the code has ended, that is there is no more code to read.
        /// </summary>
        public bool Eof
        {
            get { return this.Position >= this.code.Length; }
        }

        /// <summary>
        /// Reads any of the string alternatives in the code.
        /// </summary>
        /// <param name="strs"> Array of strings containing all the alternatives. </param>
        /// <returns> Returns the string that was read from the code. </returns>
        public string ReadAnyStringAlternative(params string[] strs)
        {
            strs = strs.OrderByDescending(s => s.Length).ToArray();
            int pos = this.Position;
            foreach (var str in strs)
            {
                int itCh;
                for (itCh = 0; itCh < str.Length; itCh++)
                {
                    if (itCh + pos >= this.code.Length || str[itCh] != this.code[itCh + pos])
                        break;
                }

                if (itCh == str.Length)
                {
                    this.Position = pos + itCh;
                    return str;
                }
            }

            return null;
        }
    }
}
