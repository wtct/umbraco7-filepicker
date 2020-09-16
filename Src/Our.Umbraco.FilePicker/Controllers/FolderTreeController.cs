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
	[Tree("dummy", "fileTree", "Folders")]
	[PluginController("FilePicker")]
	public class FolderTreeController : TreeController
	{
		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
		{
            string qsStartFolder = queryStrings.Get("startfolder");

            if (!string.IsNullOrWhiteSpace(qsStartFolder))
			{
				string folder = id == "-1" ? qsStartFolder : id;

                folder = folder.EnsureStartsWith("/");

                var treeNodes = GetFolderTreeNodes(folder, queryStrings);

                treeNodes.AddRange(GetFileTreeNodes(folder, queryStrings));

                return treeNodes;
			}

			return GetFolderTreeNodes(id == "-1" ? "" : id, queryStrings);
		}

		private TreeNodeCollection GetFileTreeNodes(string folder, FormDataCollection queryStrings)
		{
			if (string.IsNullOrWhiteSpace(folder))
				return null;

            var qsStartFolder = queryStrings.Get("startfolder");
            var filter = queryStrings.Get("filter").Split(',').Select(a=>a.Trim().EnsureStartsWith(".")).ToArray();

			var path = IOHelper.MapPath(folder);
			var rootPath = IOHelper.MapPath(queryStrings.Get("startfolder"));

            var treeNodes = new TreeNodeCollection();
            var pickerApiController = new FilePickerApiController();

            var files = pickerApiController.GetFiles(folder, filter);

            treeNodes.AddRange(files.Select(file => CreateFileTreeNode(rootPath, path, queryStrings, file)));

			return treeNodes;
		}

		private TreeNodeCollection GetFolderTreeNodes(string parent, FormDataCollection queryStrings)
		{
			var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

			var treeNodes = new TreeNodeCollection();
            var pickerApiController = new FilePickerApiController();

            var dirs = pickerApiController.GetDirectories(parent, filter);

            treeNodes.AddRange(dirs.Select(dir => CreateFolderTreeNode(parent, queryStrings, pickerApiController, dir, filter)));

			return treeNodes;
		}

        private TreeNode CreateFileTreeNode(string rootPath, string path, FormDataCollection queryStrings, FileInfo file)
        {
            string id = file.FullName.Replace(rootPath, "").Replace("\\", "/");

            return CreateTreeNode(id, path, queryStrings, file.Name, "icon-document", false);
        }

        private TreeNode CreateFolderTreeNode(string parent, FormDataCollection queryStrings, FilePickerApiController pickerApiController, DirectoryInfo dir, string[] filter)
        {
            string id = dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/");
            bool hasChildren = filter[0] == "." ? dir.EnumerateDirectories().Any() 
                || pickerApiController.GetFiles(id, filter).Any() : pickerApiController.GetFiles(id, filter).Any();

            return CreateTreeNode(id, "~/" + parent, queryStrings, dir.Name, "icon-folder", hasChildren);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			return null;
		}
	}
}
