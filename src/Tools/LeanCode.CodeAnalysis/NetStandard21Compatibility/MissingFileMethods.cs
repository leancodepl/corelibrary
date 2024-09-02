// Based on https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/File.cs
// as linked by docs on 02.09.2024

using System.Text;

namespace System.IO;

public static class MissingFileMethods
{
    private static readonly Encoding UTF8NoBOM = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true
    );

    public static Task WriteAllTextAsync(
        string path,
        string? contents,
        CancellationToken cancellationToken = default
    ) => WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

    public static async Task WriteAllTextAsync(
        string path,
        string? contents,
        Encoding encoding,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is invalid");
        }

        using var stream = File.OpenWrite(path);
        var preamble = encoding.GetPreamble();
        await stream.WriteAsync(preamble, 0, preamble.Length, cancellationToken);
        if (contents is not null)
        {
            var encoded = Encoding.Default.GetBytes(contents);
            await stream.WriteAsync(encoded, 0, encoded.Length, cancellationToken);
        }
    }
}
