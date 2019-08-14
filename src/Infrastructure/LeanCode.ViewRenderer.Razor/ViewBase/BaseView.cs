using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LeanCode.ViewRenderer.Razor.ViewBase
{
    // Based on https://github.com/aspnet/Common/blob/ffb7c20fb22a31ac31d3a836a8455655867e8e16/shared/Microsoft.Extensions.RazorViews.Sources/BaseView.cs
    public abstract class BaseView
    {
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private readonly HtmlEncoder htmlEncoder = HtmlEncoder.Default;

        private StreamWriter output;

        private string attributeEnding;
        private List<string> attributeValues;

        internal BaseView ChildView { get; set; }

        public dynamic Model { get; set; }

        public abstract Task ExecuteAsync();

        internal async Task ExecuteAsync(Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream, UTF8NoBOM, 4096, leaveOpen: true))
            {
                await ExecuteAsync(writer).ConfigureAwait(false);
            }
        }

        private async Task ExecuteAsync(StreamWriter writer)
        {
            output = writer;
            await ExecuteAsync().ConfigureAwait(false);
            output = null;
        }

        private async Task<object> RenderBodyAsyncInternal()
        {
            if (ChildView != null)
            {
                ChildView.Model = Model;
                await ChildView.ExecuteAsync(output).ConfigureAwait(false);
            }

            return null;
        }

        protected ConfiguredTaskAwaitable<object> RenderBodyAsync()
        {
            return RenderBodyAsyncInternal().ConfigureAwait(false);
        }

        [Obsolete("Use `await RenderBodyAsync()`")]
        protected object RenderBody()
        {
            return RenderBodyAsyncInternal().Result;
        }

        protected void Write(object value) => WriteTo(output, value);
        protected void Write(string value) => WriteTo(output, value);
        protected void Write(HelperResult result) => WriteTo(output, result);
        protected void WriteTo(TextWriter writer, string value) => WriteLiteralTo(writer, htmlEncoder.Encode(value));

        protected void WriteTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                if (value is HelperResult helperResult)
                {
                    helperResult.WriteTo(writer);
                }
                else
                {
                    WriteTo(writer, Convert.ToString(value, CultureInfo.InvariantCulture));
                }
            }
        }

        protected void WriteLiteral(string value) => WriteLiteralTo(output, value);
        protected void WriteLiteral(object value) => WriteLiteralTo(output, value);
        protected void WriteLiteralTo(TextWriter writer, object value) => WriteLiteralTo(writer, Stringify(value));

        protected void WriteLiteralTo(TextWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        [SuppressMessage("?", "IDE0060", Justification = "We need to expose certain interface.")]
        protected void WriteAttributeValue(string thingy, int startPostion, object value, int endValue, int dealyo, bool yesno)
        {
            if (attributeValues == null)
            {
                attributeValues = new List<string>();
            }

            attributeValues.Add(value.ToString());
        }

        [SuppressMessage("?", "IDE0060", Justification = "We need to expose certain interface.")]
        protected void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            Debug.Assert(string.IsNullOrEmpty(attributeEnding), $"{nameof(attributeEnding)} should be null nor empty");

            output.Write(begining);
            attributeEnding = ending;
        }

        protected void EndWriteAttribute()
        {
            Debug.Assert(!string.IsNullOrEmpty(attributeEnding), $"{nameof(attributeEnding)} should not be null nor empty");

            var attributes = string.Join(" ", attributeValues);
            output.Write(attributes);
            attributeValues = null;

            output.Write(attributeEnding);
            attributeEnding = null;
        }

        protected void WriteAttributeTo(
            TextWriter writer,
            string name,
            string leader,
            string trailer,
            params AttributeValue[] values)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            else if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            else if (leader == null)
            {
                throw new ArgumentNullException(nameof(leader));
            }
            else if (trailer == null)
            {
                throw new ArgumentNullException(nameof(trailer));
            }

            WriteLiteralTo(writer, leader);
            foreach (var value in values)
            {
                WriteLiteralTo(writer, value.Prefix);

                string stringValue;
                if (value.Value is bool v)
                {
                    if (v)
                    {
                        stringValue = name;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    stringValue = value.Value as string;
                }

                if (value.Literal && stringValue != null)
                {
                    WriteLiteralTo(writer, stringValue);
                }
                else if (value.Literal)
                {
                    WriteLiteralTo(writer, value.Value);
                }
                else if (stringValue != null)
                {
                    WriteTo(writer, stringValue);
                }
                else
                {
                    WriteTo(writer, value.Value);
                }
            }

            WriteLiteralTo(writer, trailer);
        }

        private static string Stringify(object v)
        {
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }
    }
}
