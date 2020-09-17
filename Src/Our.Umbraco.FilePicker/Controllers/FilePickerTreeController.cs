using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Our.Umbraco.FilePicker.Controllers
{
    [Tree("filePickerDialog", "filePickerTree", "Folders and files")]
    [PluginController("FilePicker")]
    public class FilePickerTreeController : TreeController
    {
        bool _isFolderPicker;

        string _rootVirtualPath;
        string _rootPath;

        string _currentVirtualPath;
        string _currentPath;

        string[] _fileExtensionFilters;

        FormDataCollection _queryStrings;

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            Inicjalize(id, queryStrings);

            var treeNodes = GetFolderTreeNodes();

            if (!_isFolderPicker)
            {
                var fileTreeNodes = GetFileTreeNodes();

                treeNodes.AddRange(fileTreeNodes);
            }

            return treeNodes;
        }

        private void Inicjalize(string id, FormDataCollection queryStrings)
        {
            _queryStrings = queryStrings;

            string qsStartFolder = _queryStrings.Get("startfolder");
            string qsFilter = _queryStrings.Get("filter");

            if (string.IsNullOrWhiteSpace(qsStartFolder))
            {
                _isFolderPicker = true;
                _rootVirtualPath = id == "-1" ? "/" : id;
                _currentVirtualPath = id == "-1" ? "/" : id;
            }
            else
            {
                _rootVirtualPath = qsStartFolder;
                _currentVirtualPath = id == "-1" ? qsStartFolder : id;
            }

            _rootPath = IOHelper.MapPath("~" + _rootVirtualPath);
            _currentPath = IOHelper.MapPath("~" + _currentVirtualPath);

            _fileExtensionFilters = qsFilter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim().EnsureStartsWith(".")).ToArray();
        }

        private TreeNodeCollection GetFolderTreeNodes()
        {
            var treeNodes = new TreeNodeCollection();
            var dirs = GetAllDirectories(_currentPath);

            treeNodes.AddRange(dirs.Select(dir => CreateFolderTreeNode(dir)));

            return treeNodes;
        }

        private TreeNodeCollection GetFileTreeNodes()
        {
            var treeNodes = new TreeNodeCollection();
            var files = GetFiles(_currentPath, _fileExtensionFilters).OrderBy(f => f.Name);

            treeNodes.AddRange(files.Select(file => CreateFileTreeNode(file)));

            return treeNodes;
        }

        private TreeNode CreateFolderTreeNode(DirectoryInfo dir)
        {
            string virtualPath = GetVirtualPath(dir.FullName);
            bool hasChildren = dir.EnumerateDirectories().Any() || !_isFolderPicker && GetFiles(dir, _fileExtensionFilters).Any();

            return CreateTreeNode(virtualPath, _currentVirtualPath, _queryStrings, dir.Name, "icon-folder", hasChildren);
        }

        private TreeNode CreateFileTreeNode(FileInfo file)
        {
            string virtualPath = GetVirtualPath(file.FullName);

            return CreateTreeNode(virtualPath, _currentVirtualPath, _queryStrings, file.Name, "icon-document", false);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }

        private string GetVirtualPath(string physicalPath)
        {
            return physicalPath.Replace(_rootPath, _rootVirtualPath).Replace("\\", "/");
        }

        public IEnumerable<DirectoryInfo> GetAllDirectories(string path)
        {
            var dir = new DirectoryInfo(path);

            return dir.GetDirectories().OrderBy(d => d.Name);
        }

        public IEnumerable<FileInfo> GetFiles(string path, string[] filter)
        {
            var dir = new DirectoryInfo(path);

            return GetFiles(dir, filter);
        }

        public IEnumerable<FileInfo> GetFiles(DirectoryInfo dir, string[] filter)
        {
            var files = dir.EnumerateFiles();

            if (filter != null && filter.Any())
                return files.Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

            return files;
        }
    }
}
