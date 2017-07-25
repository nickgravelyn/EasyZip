using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ProjectUpdater
{
	public partial class Form1 : Form
	{
		#region Version Fields

		/// <summary>
		/// the current version
		/// </summary>
		string folderVersion = "v1.3";

		/// <summary>
		/// the library version for the reference
		/// </summary>
		string libraryVersion = "1.3.0.0";

		/// <summary>
		/// all the old values that can be used when updating
		/// </summary>
		string[] olderVersion = new string[]
		{
			"v1.2",
			"v1.0.0",
		};

		#endregion

		#region Defined Strings

		/// <summary>
		/// the installation folder for finding EasyZip
		/// </summary>
		string installationDirectory = "$(ProgramFiles)\\EasyZip";

		/// <summary>
		/// the default project .targets file path
		/// </summary>
		string defaultTargetsFile = "$(MSBuildExtensionsPath)\\Microsoft\\XNA Game Studio\\v2.0\\" +
			"Microsoft.Xna.GameStudio.ContentPipeline.targets";

		/// <summary>
		/// the format for the EasyZip .targets file path
		/// </summary>
		string easyZipTargetsFileFormat = "{0}\\EasyZip.targets";

		/// <summary>
		/// the format for the EasyZip reference name
		/// </summary>
		string easyZipRefFormat = "EasyZip, Version={0}, Culture=neutral, processorArchitecture=x86";

		/// <summary>
		/// the format for the EasyZip reference hint path
		/// </summary>
		string easyZipRefHintFormat = "{0}\\{1}\\EasyZip.dll";

		/// <summary>
		/// the reference include for the System.IO.Compression library
		/// </summary>
		string sysIORefInclude = "System.IO.Compression, Version=1.0.2631.7054, Culture=neutral, processorArchitecture=MSIL";

		/// <summary>
		/// the format for the System.IO.Compression hint path
		/// </summary>
		string sysIORefHintFormat = "{0}\\Xbox\\System.IO.Compression.dll";

		/// <summary>
		/// the name of the task for the Using tag
		/// </summary>
		string usingTaskName = "ZipWriter";

		/// <summary>
		/// the format of the path for the Using tag's AssemblyFile attribute
		/// </summary>
		string usingTaskFileFormat = "{0}\\Windows\\EasyZip.dll";

		#endregion

		public enum Platform
		{
			Windows,
			Xbox
		}

		public Form1()
		{
			InitializeComponent();

			//get the execution path as the installation directory
			installationDirectory = Path.GetDirectoryName(Application.ExecutablePath);

			//change the utility window's title
			Text = "EasyZip Project Utility (" + folderVersion + ")";

			//set up the open file dialog properties
			openFileDialog1.DefaultExt = ".csproj";
			openFileDialog1.Filter = "XNA Project Files (*.csproj)|*.csproj";
			openFileDialog1.Multiselect = false;
			openFileDialog1.Title = "Select the XNA project to update...";
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			openFileDialog1.FileName = filePath.Text;
			DialogResult result = openFileDialog1.ShowDialog();

			if (result == DialogResult.OK)
				filePath.Text = openFileDialog1.FileName;
		}

		private void filePath_TextChanged(object sender, EventArgs e)
		{
			updateButton.Enabled = true;
		}

		private void updateProject(object sender, EventArgs e)
		{
			XmlDocument document;
			string documentNS = string.Empty;
			Platform gamePlatform = Platform.Windows;
			string contentProjectPath = string.Empty;
			XmlNode importNode = null;
			XmlElement usingNode = null;
			XmlElement afterBuildTask = null;

			try
			{
				//convert the path to the full path
				filePath.Text = Path.GetFullPath(filePath.Text);

				#region Validate File

				//make sure we have a valid .csproj path
				if (!filePath.Text.EndsWith(".csproj"))
				{
					MessageBox.Show("Invalid file specified. Please enter the path to an " +
						"XNA Game Studio 2.0 project file (*.csproj).");
					return;
				}

				//get some information about the file
				FileInfo projectFileInfo = new FileInfo(filePath.Text);

				//make sure the file actually exists
				if (!projectFileInfo.Exists)
				{
					MessageBox.Show("XNA Game Studio 2.0 project file does not exist at path given.");
					return;
				}

				//make sure the file isn't read-only
				if (projectFileInfo.IsReadOnly)
				{
					MessageBox.Show("The specified project file is marked as read-only. The update cannot continue.");
					return;
				} 

				#endregion

				//load in the XML file
				document = new XmlDocument();
				document.Load(filePath.Text);

				//get the document level namespace
				documentNS = document.DocumentElement.NamespaceURI;

				#region Get General Information About Project

				//go through all of the nodes in the document
				foreach (XmlNode node in document.DocumentElement.ChildNodes)
				{
					//look for the property groups to find some information out about the project
					if (node.Name == "PropertyGroup")
					{
						#region Find Platform and XNA version

						//go through all the children nodes of the property group
						foreach (XmlNode subNode in node.ChildNodes)
						{
							//look for the platform node
							if (subNode.Name == "XnaPlatform")
							{
								//check the InnerText of the node and save our platform for later
								if (subNode.InnerText == "Windows")
									gamePlatform = Platform.Windows;
								else if (subNode.InnerText == "Xbox 360")
									gamePlatform = Platform.Xbox;
							}

							//also look for the version node to verify this is a 2.0 project
							if (subNode.Name == "XnaFrameworkVersion")
							{
								//if this isn't a v2.0 project, we can't continue
								if (subNode.InnerText != "v2.0")
								{
									MessageBox.Show(
										"Cannot update game project. Game project is for XNA Framework " +
											subNode.InnerText +
											". EasyZip is only compatible with XNA Framework 2.0 projects",
										"Update Failed");

									//reset the UI
									filePath.Text = string.Empty;
									updateButton.Enabled = false;
									return;
								}
							}
						}

						#endregion
					}
				}

				#endregion

				//find or add the EasyZip and System.IO references
				FindOrAddReferences(document, documentNS, gamePlatform);

				//find the content subproject's filepath
				contentProjectPath = GetContentSubprojectFilePath(document, contentProjectPath);

				//create the backup for the game project
				CreateBackupProject();

				//we're done with the game project. we need to save out the changes.
				document.Save(filePath.Text);

				//now to figure out the full path of the content project
				contentProjectPath = Path.GetDirectoryName(filePath.Text) + @"\" + contentProjectPath;

				//open the content project up
				document = new XmlDocument();
				document.Load(contentProjectPath);

				//locate and update the content pipeline import tag
				importNode = FindContentPipelineImportTag(document, importNode);

				//try to find the UsingTask tag
				usingNode = LocateUsingTaskTag(document, usingNode);

				//try to find the AfterBuild task tag
				afterBuildTask = LocateAfterBuildTaskNode(document, afterBuildTask);

				//create the UsingTaskTag if needed
				usingNode = CreateUsingTaskTag(document, documentNS, importNode, usingNode);

				//create the AfterBuild task tag and make sure the ZipWriter task is in there
				afterBuildTask = CreateAfterBuildTaskTag(document, documentNS, usingNode, afterBuildTask);

				//create the content project backup file
				CreateContentProjectBackup(contentProjectPath);

				//save the content project
				document.Save(contentProjectPath);

				//reset the UI
				filePath.Text = string.Empty;
				updateButton.Enabled = false;
				MessageBox.Show("Project updated for EasyZip version " + folderVersion);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Update failed: " + ex.Message + "\n\n" + ex.StackTrace);
			}
		}

		private void CreateContentProjectBackup(string contentProjectPath)
		{

			//if the user wants a backup, make one of the content project
			if (createBackups.Checked)
			{
				//create the backup path
				string backupContentProjectPath =
					contentProjectPath.Substring(0, contentProjectPath.Length - ".contentproj".Length) +
					"_backup.contentproj";

				//if the file exists, delete it
				if (File.Exists(backupContentProjectPath))
					File.Delete(backupContentProjectPath);

				//copy the real project to the backup path
				File.Copy(contentProjectPath, backupContentProjectPath);
			}
		}

		private XmlElement CreateAfterBuildTaskTag(
			XmlDocument document, 
			string documentNS, 
			XmlElement usingNode, 
			XmlElement afterBuildTask)
		{

			//if we didn't find an AfterBuild tag, we need to make one of those
			if (afterBuildTask == null)
			{
				//create the element
				afterBuildTask = document.CreateElement("Target", documentNS);

				//add the Name attribute
				afterBuildTask.SetAttribute("Name", "AfterBuild");

				//append the element AFTER the using node
				document.DocumentElement.InsertAfter(afterBuildTask, usingNode);
			}

			//next test to see whether the AfterBuild task contains the ZipWriter instructions.
			//we do this manually so that we can remove the task if we find it. we want to totally
			//recreate the tag each time in case the structure changes
			for (int i = 0; i < afterBuildTask.ChildNodes.Count; i++)
			{
				//if we have a ZipWriter task, delete it
				if (afterBuildTask.ChildNodes[i].Name == "ZipWriter")
					afterBuildTask.RemoveChild(afterBuildTask.ChildNodes[i]);
			}

			//create the ZipWriter task
			XmlElement zipWriterTask = document.CreateElement("ZipWriter", documentNS);

			//add the attributes onto the task
			zipWriterTask.SetAttribute("OutDir", "$(ParentOutputDir)");
			zipWriterTask.SetAttribute("ContentDir", "$(ProjectDir)$(OutputPath)");
			zipWriterTask.SetAttribute("ZipName", "$(ContentRootDirectory)");
			zipWriterTask.SetAttribute("SaveFolderTree", saveFolderTree.Checked.ToString());

			//add the zipWriterTask onto the AfterBuild task
			afterBuildTask.AppendChild(zipWriterTask);
			return afterBuildTask;
		}

		private XmlElement CreateUsingTaskTag(
			XmlDocument document, 
			string documentNS, 
			XmlNode importNode, 
			XmlElement usingNode)
		{
			//if we need to create a using tag, let's do that
			if (usingNode == null)
			{
				//create the element
				usingNode = document.CreateElement("UsingTask", documentNS);

				//set the Task attribute
				usingNode.SetAttribute("TaskName", usingTaskName);

				//set the AssemblyFile attribute
				usingNode.SetAttribute("AssemblyFile", string.Format(usingTaskFileFormat, installationDirectory));

				//append the element AFTER the import tag we found earlier
				document.DocumentElement.InsertAfter(usingNode, importNode);
			}

			return usingNode;
		}

		private static XmlElement LocateAfterBuildTaskNode(
			XmlDocument document, 
			XmlElement afterBuildTask)
		{

			//try to find the AfterBuild task node
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//look for a node named Target with a Name attribute of "AfterBuild"
				if (node.Name == "Target" && node.Attributes["Name"].Value == "AfterBuild")
				{
					//save this node
					afterBuildTask = (XmlElement)node;

					//end the loop;
					break;
				}
			}

			//if we didn't find the AfterBuild node, we have to see if the default comment
			//is still in the file, remove the text for the AfterBuild node, and create a new node.
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//see if the node is a comment
				XmlComment comment = node as XmlComment;
				if (comment != null)
				{
					//build the regex to look for the target tags with anything between them
					string regex = "<Target Name=\"AfterBuild\">[\\s\\S]*</Target>";

					//replace any occurance of the regex pattern with an empty string
					comment.Value = Regex.Replace(comment.Value, regex, string.Empty);
				}
			}
			return afterBuildTask;
		}

		private XmlElement LocateUsingTaskTag(
			XmlDocument document, 
			XmlElement usingNode)
		{

			//see if we have an older UsingTask tag in the document
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//find any UsingTask tags
				if (node.Name == "UsingTask")
				{
					//see if we have a refernce
					if (node.Attributes["TaskName"].Value == usingTaskName)
					{
						//we have a using tag already (so we won't create one later)
						usingNode = (XmlElement)node;

						//make sure we have the latest path in there
						node.Attributes["AssemblyFile"].Value = string.Format(usingTaskFileFormat, installationDirectory);

						//no need to keep looping
						break;
					}
				}
			}
			return usingNode;
		}

		private XmlNode FindContentPipelineImportTag(
			XmlDocument document, 
			XmlNode importNode)
		{
			//scan the document for the import tags for the content pipeline .targets file
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//find any import tags
				if (node.Name == "Import")
				{
					//if we have the default targets file, update it to the new EasyZip one
					if (node.Attributes["Project"].Value == defaultTargetsFile)
					{
						node.Attributes["Project"].Value = string.Format(easyZipTargetsFileFormat, installationDirectory);

						//save for later
						importNode = node;

						//done with the loop
						break;
					}

					//if it's not the default, see if it's the newest EasyZip one
					else if (node.Attributes["Project"].Value ==
						string.Format(easyZipTargetsFileFormat, installationDirectory))
					{
						//save for later
						importNode = node;

						//done with the loop
						break;
					}

					//otherwise see if we have an older EasyZip targets file
					else
					{
						foreach (string s in olderVersion)
						{
							if (node.Attributes["Project"].Value.Contains("EasyZip"))
							{
								//update the text to the newest targets file
								node.Attributes["Project"].Value =
									string.Format(easyZipTargetsFileFormat, installationDirectory);

								//save for later
								importNode = node;

								//done with the loop
								break;
							}
						}

						//if we found the node, we can be done with the loop
						if (importNode != null)
							break;
					}
				}
			}
			return importNode;
		}

		private void CreateBackupProject()
		{
			//create the backup if the user wants it
			if (createBackups.Checked)
			{
				//construct the backup project
				string backupProjectFileName =
					filePath.Text.Substring(0, filePath.Text.Length - ".csproj".Length) + "_backup.csproj";

				//if there's already a backup project, we delete it
				if (File.Exists(backupProjectFileName))
					File.Delete(backupProjectFileName);

				//copy the main project over to the backup file path
				File.Copy(filePath.Text, backupProjectFileName);
			}
		}

		private static string GetContentSubprojectFilePath(
			XmlDocument document, 
			string contentProjectPath)
		{
			//now we do the final pass of the game project to find the location of the content
			//subproject.
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//again looking for an item group
				if (node.Name == "ItemGroup")
				{
					//the node should have just one item in it with the name of NestedContentProject
					if (node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "NestedContentProject")
					{
						//grab the path
						contentProjectPath = node.ChildNodes[0].Attributes["Include"].Value;
					}
				}
			}
			return contentProjectPath;
		}

		private void FindOrAddReferences(
			XmlDocument document, 
			string documentNS, 
			Platform gamePlatform)
		{

			//we do another pass for the references. this ensures that modified projects
			//where the user may have moved the references above the property groups will
			//work out
			foreach (XmlNode node in document.DocumentElement.ChildNodes)
			{
				//look for item groups to find the references collection
				if (node.Name == "ItemGroup")
				{
					//see if the node contains references
					if (node.ChildNodes[0].Name == "Reference")
					{
						#region Find References

						//we're going to see if these references already exist and
						//if not add them to the collection.
						bool hasEasyZipReference = false;
						bool hasSystemIOReference = false;

						//loop the child nodes
						foreach (XmlNode subNode in node.ChildNodes)
						{
							//just to be safe, make sure we're looking at a reference node
							if (subNode.Name == "Reference")
							{
								//look for the EasyZip include
								if (subNode.Attributes["Include"].Value.Contains("EasyZip"))
								{
									//we found it
									hasEasyZipReference = true;

									//figure out which version EasyZip reference this is
									string curVersion = subNode.Attributes["Include"].Value;

									//see if we don't have the current version of the library
									if (curVersion != string.Format(easyZipRefFormat, libraryVersion))
									{
										//we need to update this node to the latest version
										subNode.Attributes["Include"].Value = string.Format(easyZipRefFormat, libraryVersion);

										//now we need to update the hint path to the correct place
										foreach (XmlNode subSubNode in subNode.ChildNodes)
										{
											//find the hint path and update it. we use the version and
											//game platform to form the correct path
											if (subSubNode.Name == "HintPath")
												subSubNode.InnerText = string.Format(
													easyZipRefHintFormat,
													folderVersion,
													gamePlatform.ToString());
										}
									}
								}

								//if we're on the Xbox platform, check for the System.IO.Compression reference
								else if (gamePlatform == Platform.Xbox &&
									subNode.Attributes["Include"].Value.Contains("System.IO.Compression"))
								{
									//we found it
									hasSystemIOReference = true;
								}
							}
						}

						#endregion

						#region Add References

						//if there's no EasyZip reference, add it.
						if (!hasEasyZipReference)
						{
							//create the element
							XmlElement ezRefElement = document.CreateElement("Reference", documentNS);

							//append it to the main node (the "ItemGroup" node)
							node.AppendChild(ezRefElement);

							//set the "Include" attribute
							ezRefElement.SetAttribute("Include", string.Format(easyZipRefFormat, libraryVersion));

							//create the SpecificVersion subnode
							XmlNode svNode = document.CreateElement("SpecificVersion", documentNS);

							//set it to False
							svNode.InnerText = "False";

							//add it to the reference element
							ezRefElement.AppendChild(svNode);

							//create the HintPath subnode
							XmlNode hpNode = document.CreateElement("HintPath", documentNS);

							//set the value
							hpNode.InnerText = string.Format(
								easyZipRefHintFormat,
								installationDirectory,
								gamePlatform.ToString());

							//add it to the reference element
							ezRefElement.AppendChild(hpNode);
						}

						//if we have an Xbox game with no System.IO.Compression reference, add it.
						if (gamePlatform == Platform.Xbox && !hasSystemIOReference)
						{
							//create the element
							XmlElement sysIORefElement = document.CreateElement("Reference", documentNS);

							//append it to the main node (the "ItemGroup" node)
							node.AppendChild(sysIORefElement);

							//set the "Include" attribute
							sysIORefElement.SetAttribute("Include", sysIORefInclude);

							//create the SpecificVersion subnode
							XmlNode svNode = document.CreateElement("SpecificVersion", documentNS);

							//set it to False
							svNode.InnerText = "False";

							//add it to the reference element
							sysIORefElement.AppendChild(svNode);

							//create the HintPath subnode
							XmlNode hpNode = document.CreateElement("HintPath", documentNS);

							//set the value
							hpNode.InnerText = string.Format(sysIORefHintFormat, installationDirectory);

							//add it to the reference element
							sysIORefElement.AppendChild(hpNode);
						}

						#endregion
					}
				}
			}
		}
	}
}