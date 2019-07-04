using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;

namespace LeanCode.ViewRenderer.Razor
{
    class ViewLocator : RazorProject
    {
        private readonly RazorViewRendererOptions options;

        public ViewLocator(RazorViewRendererOptions options)
        {
            this.options = options;
        }

        public string GetRootPath()
        {
            return options.ViewLocations[0];
        }

        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            throw new NotSupportedException();
        }

        public override RazorProjectItem GetItem(string path)
        {
            var (basePath, fullPath, fileName) = LocateFile(GetFileName(path));
            if (fullPath == null)
            {
                return new Item(path);
            }
            else
            {
                return new Item(basePath, fileName, fullPath);
            }
        }

        private string GetFileName(string viewName) => viewName + options.Extension;

        private (string BasePath, string FullPath, string FileName) LocateFile(string fileName)
        {
            foreach (var path in options.ViewLocations)
            {
                var fullPath = Path.GetFullPath(Path.Combine(path, fileName));
                if (File.Exists(fullPath))
                {
                    return (path, fullPath, fileName);
                }
            }
            return (null, null, null);
        }

        class Item : RazorProjectItem
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
                BasePath = string.Empty;
                FilePath = path;
                PhysicalPath = string.Empty;
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
