namespace ProjectUpdater
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.updateButton = new System.Windows.Forms.Button();
			this.filePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.browseButton = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.createBackups = new System.Windows.Forms.CheckBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.saveFolderTree = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// updateButton
			// 
			this.updateButton.Enabled = false;
			this.updateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.updateButton.Location = new System.Drawing.Point(183, 145);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(140, 85);
			this.updateButton.TabIndex = 0;
			this.updateButton.Text = "Update!";
			this.toolTip1.SetToolTip(this.updateButton, "Update the game project.");
			this.updateButton.UseVisualStyleBackColor = true;
			this.updateButton.Click += new System.EventHandler(this.updateProject);
			// 
			// filePath
			// 
			this.filePath.Location = new System.Drawing.Point(12, 41);
			this.filePath.Name = "filePath";
			this.filePath.Size = new System.Drawing.Size(433, 20);
			this.filePath.TabIndex = 1;
			this.toolTip1.SetToolTip(this.filePath, "The full path to an XNA Game Studio 2.0 game project.");
			this.filePath.TextChanged += new System.EventHandler(this.filePath_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(401, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Path To XNA Project File To Update (e.g. C:\\Users\\Joe\\MyGame\\MyGame.csproj):";
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(451, 39);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(58, 23);
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "Browse";
			this.toolTip1.SetToolTip(this.browseButton, "Browse for a game project.");
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// createBackups
			// 
			this.createBackups.AutoSize = true;
			this.createBackups.Checked = true;
			this.createBackups.CheckState = System.Windows.Forms.CheckState.Checked;
			this.createBackups.Location = new System.Drawing.Point(178, 78);
			this.createBackups.Name = "createBackups";
			this.createBackups.Size = new System.Drawing.Size(163, 17);
			this.createBackups.TabIndex = 4;
			this.createBackups.Text = "Create Backup Project Files?";
			this.toolTip1.SetToolTip(this.createBackups, "If checked, this option will cause the updater to create backup files of your gam" +
					"e and content projects.");
			this.createBackups.UseVisualStyleBackColor = true;
			// 
			// saveFolderTree
			// 
			this.saveFolderTree.AutoSize = true;
			this.saveFolderTree.Checked = true;
			this.saveFolderTree.CheckState = System.Windows.Forms.CheckState.Checked;
			this.saveFolderTree.Location = new System.Drawing.Point(178, 108);
			this.saveFolderTree.Name = "saveFolderTree";
			this.saveFolderTree.Size = new System.Drawing.Size(168, 17);
			this.saveFolderTree.TabIndex = 4;
			this.saveFolderTree.Text = "Save Folder Tree In Zip Files?";
			this.toolTip1.SetToolTip(this.saveFolderTree, "Sets whether or not folder structure is preserved in the ZIP file.");
			this.saveFolderTree.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(518, 242);
			this.Controls.Add(this.saveFolderTree);
			this.Controls.Add(this.createBackups);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.filePath);
			this.Controls.Add(this.updateButton);
			this.Name = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button updateButton;
		private System.Windows.Forms.TextBox filePath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.CheckBox createBackups;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.CheckBox saveFolderTree;
	}
}

