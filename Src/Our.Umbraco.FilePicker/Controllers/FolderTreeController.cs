using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.IO;
using Umbraco.Core;
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

            treeNodes.AddRange(pickerApiController.GetFiles(folder, filter)
				.Select(file => CreateTreeNode(file.FullName.Replace(rootPath, "").Replace("\\", "/"),
					path, queryStrings, file.Name, "icon-document", false)));

			return treeNodes;
		}

		private TreeNodeCollection GetFolderTreeNodes(string parent, FormDataCollection queryStrings)
		{
			var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

			var treeNodes = new TreeNodeCollection();
            var pickerApiController = new FilePickerApiController();

            treeNodes.AddRange(pickerApiController.GetFolders(parent,filter)
				.Select(dir => CreateTreeNode(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"),
					"~/" + parent, queryStrings, dir.Name,
					"icon-folder", filter[0]=="." ? dir.EnumerateDirectories().Any() || pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any() : pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any())));

			return treeNodes;
		}

		protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			return null;
		}
	}
}
