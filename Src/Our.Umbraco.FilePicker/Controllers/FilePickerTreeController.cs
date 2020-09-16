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
		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
		{
            string qsStartFolder = queryStrings.Get("startfolder");
            string qsFilter = queryStrings.Get("filter");

            string rootVirtualPath = !string.IsNullOrEmpty(qsStartFolder) ? qsStartFolder : "/";
            var rootPath = IOHelper.MapPath("~" + rootVirtualPath);
            string virtualPath = id == "-1" ? "/" : id;
            var filter = qsFilter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

            if (!string.IsNullOrWhiteSpace(qsStartFolder))
			{
                virtualPath = id == "-1" ? qsStartFolder : id;

                var treeNodes = GetFolderTreeNodes(rootPath, rootVirtualPath, virtualPath, filter, queryStrings);
                var fileTreeNodes = GetFileTreeNodes(rootPath, rootVirtualPath, virtualPath, filter, queryStrings);

                treeNodes.AddRange(fileTreeNodes);

                return treeNodes;
			}

            return GetFolderTreeNodes(rootPath, rootVirtualPath, virtualPath, filter, queryStrings);
        }

		private TreeNodeCollection GetFileTreeNodes(string rootPath, string rootVirtualPath, string virtualPath, string[] filter, FormDataCollection queryStrings)
        {
            var treeNodes = new TreeNodeCollection();
            var files = GetFiles(virtualPath, filter).OrderBy(f => f.Name);

            treeNodes.AddRange(files.Select(file => CreateFileTreeNode(rootPath, rootVirtualPath, virtualPath, queryStrings, file)));

            return treeNodes;
		}

		private TreeNodeCollection GetFolderTreeNodes(string rootPath, string rootVirtualPath, string virtualPath, string[] filter, FormDataCollection queryStrings)
        {
			var treeNodes = new TreeNodeCollection();
            var dirs = GetAllDirectories(virtualPath);

            treeNodes.AddRange(dirs.Select(dir => CreateFolderTreeNode(rootPath, rootVirtualPath, virtualPath, queryStrings, dir, filter)));

            return treeNodes;
		}

        private TreeNode CreateFileTreeNode(string rootPath, string rootVirtualPath, string virtualPath, FormDataCollection queryStrings, FileInfo file)
        {
            string id = file.FullName.Replace(rootPath, rootVirtualPath).Replace("\\", "/");

            return CreateTreeNode(id, virtualPath, queryStrings, file.Name, "icon-document", false);
        }

        private TreeNode CreateFolderTreeNode(string rootPath, string rootVirtualPath, string virtualPath, FormDataCollection queryStrings, DirectoryInfo dir, string[] filter)
        {
            string id = dir.FullName.Replace(rootPath, rootVirtualPath).Replace("\\", "/");
            bool hasChildren = HasDirectoryChildren(id, filter);

            return CreateTreeNode(id, virtualPath, queryStrings, dir.Name, "icon-folder", hasChildren);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			return null;
		}

        private bool HasDirectoryChildren(string virtualPath, string[] filter)
        {
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);

            return dir.EnumerateDirectories().Any() || GetFiles(virtualPath, filter).Any();
        }

        public IEnumerable<DirectoryInfo> GetAllDirectories(string virtualPath)
        {
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);

            return dir.GetDirectories().OrderBy(d => d.Name);
        }

        public IEnumerable<FileInfo> GetFiles(string virtualPath, string[] filter)
        {
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);
            var files = dir.EnumerateFiles();

            if (filter != null && filter.Any())
                return files.Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

            return files;
        }
    }
}
