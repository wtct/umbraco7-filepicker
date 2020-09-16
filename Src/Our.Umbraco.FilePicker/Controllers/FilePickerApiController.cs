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
		public IEnumerable<DirectoryInfo> GetDirectories(string virtualPath)
		{
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);

			return dir.GetDirectories();
		}

		public IEnumerable<FileInfo> GetFiles(string virtualPath, string[] filter )
		{
            var path = IOHelper.MapPath("~" + virtualPath);
            var dir = new DirectoryInfo(path);

            if (filter != null && filter.Any())
            {
                var files = dir.EnumerateFiles();

                return files.Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
            }

            return dir.GetFiles();
        }
	}
}