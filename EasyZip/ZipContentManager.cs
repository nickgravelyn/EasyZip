using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

using ionic.utils.zip;

namespace EasyZip
{
	/// <summary>
	/// A content manager used to read files stored inside of a .zip file.
	/// </summary>
	public class ZipContentManager : ContentManager
	{
		string zipFilePath;
		ZipFile zipFile;
		bool caseSensitive;
		List<string> extractedFiles = new List<string>();
		List<string> extractedDirectories = new List<string>();

		/// <summary>
		/// Gets the zip file's path.
		/// </summary>
		public string ZipFilePath
		{
			get { return zipFilePath; }
		}

		/// <summary>
		/// Creates a new ZipContentManager that loads files from the specified
		/// zip file.
		/// </summary>
		/// <param name="serviceProvider">Service provider</param>
		/// <param name="zipFile">Zip file to extract assets from</param>
		public ZipContentManager(IServiceProvider serviceProvider, string zipFile)
			: this(serviceProvider, zipFile, true)
		{
			zipFilePath = zipFile;
		}

		/// <summary>
		/// Creates a new ZipContentManager that loads files from the specified
		/// zip file.
		/// </summary>
		/// <param name="serviceProvider">Service provider</param>
		/// <param name="zipFile">Zip file to extract assets from</param>
		/// <param name="caseSensitive">Whether or not loading assets is case-sensitive</param>
		public ZipContentManager(IServiceProvider serviceProvider, string zipFile, bool caseSensitive)
			: base(serviceProvider)
		{
			this.zipFile = ZipFile.Read(zipFile);
			this.caseSensitive = caseSensitive;
			zipFilePath = zipFile;
		}

		protected override Stream OpenStream(string assetName)
		{
			assetName = assetName.Replace("/", "\\");

			string fullAssetName = assetName + ".xnb";

			if (!caseSensitive)
				fullAssetName = fullAssetName.ToLower();

			foreach (ZipEntry entry in zipFile)
			{
				string entryName = (caseSensitive) ? entry.FileName : entry.FileName.ToLower();

				if (entryName.Equals(fullAssetName))
					return entry.GetStream();
			}

			throw new Exception("Failed to find asset '" + assetName + "' in zip file.");
		}

		/// <summary>
		/// Gets an array of the asset names of the content inside the zip file.
		/// </summary>
		/// <returns>An array of strings with all the asset names.</returns>
		public string[] GetAssetNames()
		{
			List<string> filenames = new List<string>();

			foreach (ZipEntry entry in zipFile)
			{
				string name = entry.FileName;
				if (name.EndsWith(".xnb"))
					name = name.Remove(name.Length - 4, 3);
				filenames.Add(name);
			}

			return filenames.ToArray();
		}

		/// <summary>
		/// Gets an array of asset names from a specific directory within
		/// the zip file.
		/// </summary>
		/// <param name="directory">The directory to get asset names from</param>
		/// <returns>An array of strings with all the asset names</returns>
		public string[] GetAssetNamesFromDirectory(string directory)
		{
			List<string> filenames = new List<string>();

			foreach (ZipEntry entry in zipFile)
			{
				string name = entry.FileName;
				if (name.EndsWith(".xnb"))
					name = name.Remove(name.Length - 4, 3);

				string[] parts = name.Split('\\');
				string dir = "";
				for (int i = 0; i < parts.Length - 1; i++)
					dir += parts[i] + "/";
				if (dir == directory)
					filenames.Add(name);
			}

			return filenames.ToArray();
		}

		/// <summary>
		/// Gets a Stream for a file inside of the zip folder. For use with non-Content Pipeline
		/// files such as plain images or XML files.
		/// </summary>
		/// <param name="filename">The name of the file to find (with extension)</param>
		/// <returns>A Stream that can be used to load the file</returns>
		public Stream GetFileStream(string filename)
		{
			filename = filename.Replace("/", "\\");

			if (!caseSensitive)
				filename = filename.ToLower();

			foreach (ZipEntry entry in zipFile)
			{
				string entryName = (caseSensitive) ? entry.FileName : entry.FileName.ToLower();

				if (entryName.Equals(filename))
					return entry.GetStream();
			}

			throw new Exception("Failed to find file '" + filename + "' in zip file.");
		}
	}
}
