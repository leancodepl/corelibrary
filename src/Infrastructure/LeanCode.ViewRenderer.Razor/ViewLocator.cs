using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;

namespace LeanCode.ViewRenderer.Razor
{
    internal class ViewLocator : RazorProject
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ViewLocator>();

        private readonly RazorViewRendererOptions options;

        public ViewLocator(RazorViewRendererOptions options)
        {
            this.options = options;
        }

        public string GetRootPath() => options.ViewLocations[0];

        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath) =>
            throw new NotSupportedException();

        [Obsolete("Use GetItem(string path, string fileKind) instead.")]
        public override RazorProjectItem GetItem(string path) => GetItem(path, null);

        public override RazorProjectItem GetItem(string path, string? fileKind)
        {
            if (fileKind != null)
            {
                logger.Warning("GetItem: `fileKind` parameter ignored: {fileKind}", fileKind);
            }

            if (LocateFile(GetFileName(path)) is var (basePath, fullPath, fileName))
            {
                return new Item(basePath, fileName, fullPath);
            }

            return new Item(path);
        }

        private string GetFileName(string viewName) => viewName + options.Extension;

        private (string BasePath, string FullPath, string FileName)? LocateFile(string fileName)
        {
            foreach (var path in options.ViewLocations)
            {
                var fullPath = Path.GetFullPath(Path.Combine(path, fileName));

                if (File.Exists(fullPath))
                {
                    return (path, fullPath, fileName);
                }
            }

            return null;
        }

        private class Item : RazorProjectItem
        {
            public override string BasePath { get; }
            public override string FilePath { get; }
            public override string PhysicalPath { get; }
            public override bool Exists { get; }

            public Item(string basePath, string filename, string fullPath)
            {
                BasePath = basePath;
                FilePath = filename;
                PhysicalPath = fullPath;
                Exists = true;
            }

            public Item(string path)
            {
                BasePath = "";
                FilePath = path;
                PhysicalPath = "";
                Exists = false;
            }

            public override Stream Read()
            {
                if (!Exists)
                {
                    throw new InvalidOperationException("File does not exist.");
                }

                return File.OpenRead(PhysicalPath);
            }
        }
    }
}
