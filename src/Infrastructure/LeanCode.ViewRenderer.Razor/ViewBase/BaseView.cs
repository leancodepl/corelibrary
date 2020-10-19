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
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private StreamWriter? output;

        [AllowNull]
        private StreamWriter Output
        {
            get => output ?? throw new NullReferenceException();
            set => output = value;
        }

        private string? attributeEnding;

        private List<string?>? attributeValues;

        [AllowNull]
        private List<string?> AttributeValues
        {
            get => attributeValues ??= new List<string?>();
            set => attributeValues = value;
        }

        internal BaseView? ChildView { get; set; }

        public dynamic? Model { get; set; }

        public abstract Task ExecuteAsync();

        internal async Task ExecuteAsync(Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream, UTF8NoBOM, 4096, leaveOpen: true))
            {
                await ExecuteAsync(writer);
            }
        }

        private async Task ExecuteAsync(StreamWriter writer)
        {
            Output = writer;
            await ExecuteAsync();
            Output = null;
        }

        private async Task<object?> RenderBodyAsyncInternalAsync()
        {
            if (ChildView != null)
            {
                ChildView.Model = Model;

                await ChildView.ExecuteAsync(Output);
            }

            return null; // what?
        }

        protected Task<object?> RenderBodyAsync() =>
            RenderBodyAsyncInternalAsync();

        protected void Write(object value) => WriteTo(Output, value);
        protected void Write(string value) => WriteTo(Output, value);
        protected void Write(HelperResult result) => WriteTo(Output, result);
        protected void WriteTo(TextWriter writer, string value) =>
            WriteLiteralTo(writer, HtmlEncoder.Default.Encode(value));

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
                    WriteTo(writer, Stringify(value));
                }
            }
        }

        protected void WriteLiteral(string value) => WriteLiteralTo(Output, value);
        protected void WriteLiteral(object value) => WriteLiteralTo(Output, value);
        protected void WriteLiteralTo(TextWriter writer, object value) => WriteLiteralTo(writer, Stringify(value));

        [SuppressMessage("?", "CA1822", Justification = "We need to expose certain interface.")]
        protected void WriteLiteralTo(TextWriter writer, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer?.Write(value);
            }
        }

        [SuppressMessage("?", "IDE0060", Justification = "We need to expose certain interface.")]
        protected void WriteAttributeValue(
            string thingy, int startPostion, object value, int endValue, int dealyo, bool yesno)
        {
            AttributeValues.Add(value.ToString());
        }

        [SuppressMessage("?", "IDE0060", Justification = "We need to expose certain interface.")]
        protected void BeginWriteAttribute(
            string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            Debug.Assert(string.IsNullOrEmpty(attributeEnding), $"{nameof(attributeEnding)} should not be null nor empty");

            Output.Write(begining);
            attributeEnding = ending;
        }

        protected void EndWriteAttribute()
        {
            Debug.Assert(!string.IsNullOrEmpty(attributeEnding), $"{nameof(attributeEnding)} should not be null nor empty");

            Output.Write(string.Join(" ", AttributeValues));
            AttributeValues = null;

            Output.Write(attributeEnding);
            attributeEnding = null;
        }

        protected void WriteAttributeTo(
            TextWriter writer,
            string name,
            string leader,
            string trailer,
            params AttributeValue[] values)
        {
            WriteLiteralTo(writer, leader);

            foreach (var value in values)
            {
                WriteLiteralTo(writer, value.Prefix);

                string? stringValue;

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

        [return: NotNullIfNotNull("o")]
        private static string? Stringify(object? o) =>
            Convert.ToString(o, CultureInfo.InvariantCulture);
    }
}
