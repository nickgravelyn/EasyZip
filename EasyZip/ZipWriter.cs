using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using ionic.utils.zip;

namespace EasyZip
{
	public class ZipWriter : Task
	{
		static string[] MandatoryFolders = { ".fbm" };
		static string[] ExcludedExtensions = { ".xgs", ".xwb", ".xsb" };

		string contentDirectory;
		string outputDirectory;
		string zipName;
		bool saveFolders = true;

		[Required]
		public string OutDir
		{
			get { return outputDirectory; }
			set { outputDirectory = value; }
		}

		[Required]
		public string ContentDir
		{
			get { return contentDirectory; }
			set { contentDirectory = value; }
		}

		[Required]
		public string ZipName
		{
			get { return zipName; }
			set { zipName = value; }
		}

		public string SaveFolderTree
		{
			get { return saveFolders.ToString(); }
			set { saveFolders = (value.ToLower().Contains("true") || value.ToLower().Contains("yes")); }
		}

		public override bool Execute()
		{
#if DEBUG
			System.Diagnostics.Debugger.Launch();
#endif

			string zipFile = OutDir + ZipName + ".zip";

			//get all the files in our content output directory
			string[] files = Directory.GetFiles(Path.GetFullPath(ContentDir), "*", SearchOption.AllDirectories);

			//make sure the output directory actually exists
			if (!Directory.Exists(OutDir))
				Directory.CreateDirectory(OutDir);

			//if the zip file exists (from a previous build for example), see if any content has changed
			if (File.Exists(zipFile))
			{
				//get the zip file's info
				FileInfo zipInfo = new FileInfo(zipFile);

				//we're figuring out whether or not to make the zip file
				bool continueWithBuild = false;

				//loop all the content files
				foreach (string f in files)
				{
					//get the content info
					FileInfo info = new FileInfo(f);

					//if the content has a newer write time than the zip, we need to continue with the build
					if (info.LastWriteTimeUtc.CompareTo(zipInfo.LastWriteTimeUtc) == 1)
					{
						continueWithBuild = true;
						break;
					}
				}

				//if we are not continuing, leave the method
				if (!continueWithBuild)
					return true;

				//delete the zip file
				File.Delete(zipFile);
			}

			//create the zip file
			ZipFile zip = new ZipFile(zipFile);

			//move to the content directory to add the files
			Directory.SetCurrentDirectory(ContentDir);

			//add all of our files to the zip
			foreach (string file in files)
			{
				//make sure we have a full path
				string fullPathFile = Path.GetFullPath(file);
				bool keepFolder = saveFolders;

				//check to see if we a mandatory folder
				foreach (string folders in MandatoryFolders)
				{
					if (Path.GetDirectoryName(fullPathFile).Contains(folders))
					{
						keepFolder = true;
						break;
					}
				}

				//if we're not keeping folders
				if (!keepFolder)
				{
					//move to the file's directory
					Directory.SetCurrentDirectory(Path.GetDirectoryName(fullPathFile));

					//add the file
					AddFileToZip(zip, Path.GetFileName(fullPathFile));

					//move back to the content directory
					Directory.SetCurrentDirectory(ContentDir);
				}

				//otherwise just add the file
				else
					AddFileToZip(zip, fullPathFile.Remove(0, ContentDir.Length));
			}

			//save it up
			zip.Save();

			//done
			return true;
		}

		void AddFileToZip(ZipFile file, string path)
		{
			//if a file ends with an excluded extension, simply
			//ignore the file. XACT content is written out by
			//MSBuild so if it winds up in here, we don't need
			//to do anything with it.
			foreach (string ext in ExcludedExtensions)
				if (path.EndsWith(ext))
					return;

			file.AddFile(path);
		}
	}
}
