using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.FilePicker.Controllers
{
	[PluginController("FilePicker")]
	public class FilePickerApiController : UmbracoAuthorizedJsonController
	{
		public IEnumerable<DirectoryInfo> GetDirectories(string virtualPath, string[] filter)
		{
            var path = IOHelper.MapPath("~" + virtualPath);

            if (filter != null && filter[0] != ".")
			{
				var dirs = new DirectoryInfo(path).EnumerateDirectories();

				return dirs.Where(d => d.EnumerateFiles().Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase)).Any());
			}

			return new DirectoryInfo(path).GetDirectories("*");
		}

		public IEnumerable<FileInfo> GetFiles(string virtualPath, string[] filter )
		{
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);
			var files = dir.EnumerateFiles();

			if (filter != null && filter[0] != ".")
				return files.Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

			return new DirectoryInfo(path).GetFiles();
		}
	}
}