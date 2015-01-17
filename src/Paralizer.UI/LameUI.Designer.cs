 /*************************************************************************
	Author: Robert A. Olliff
	Date  : 1/16/2015 12:00:00 AM  

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
namespace Paralizer.UI
{
    partial class LameUI
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
            this.treeView = new System.Windows.Forms.TreeView();
            this.textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.typeLabel = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.textRaw = new System.Windows.Forms.RichTextBox();
            this.btnJSON = new System.Windows.Forms.Button();
            this.btnWeird = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.stuffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadParadoxFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spendTimeWithKidsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnNodeUpdate = new System.Windows.Forms.Button();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(2, 31);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(624, 587);
            this.treeView.TabIndex = 0;
            this.treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(633, 57);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(272, 20);
            this.textBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(634, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Supposed Type:";
            // 
            // typeLabel
            // 
            this.typeLabel.Location = new System.Drawing.Point(726, 34);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(179, 23);
            this.typeLabel.TabIndex = 3;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(920, 54);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(89, 23);
            this.buttonUpdate.TabIndex = 4;
            this.buttonUpdate.Text = "Update Value";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // textRaw
            // 
            this.textRaw.Location = new System.Drawing.Point(633, 120);
            this.textRaw.Name = "textRaw";
            this.textRaw.Size = new System.Drawing.Size(570, 498);
            this.textRaw.TabIndex = 5;
            this.textRaw.Text = "";
            // 
            // btnJSON
            // 
            this.btnJSON.Location = new System.Drawing.Point(637, 87);
            this.btnJSON.Name = "btnJSON";
            this.btnJSON.Size = new System.Drawing.Size(99, 23);
            this.btnJSON.TabIndex = 6;
            this.btnJSON.Text = "View as JSON";
            this.btnJSON.UseVisualStyleBackColor = true;
            this.btnJSON.Click += new System.EventHandler(this.btnJSON_Click);
            // 
            // btnWeird
            // 
            this.btnWeird.Location = new System.Drawing.Point(742, 87);
            this.btnWeird.Name = "btnWeird";
            this.btnWeird.Size = new System.Drawing.Size(132, 23);
            this.btnWeird.TabIndex = 7;
            this.btnWeird.Text = "View as WeirdFormat";
            this.btnWeird.UseVisualStyleBackColor = true;
            this.btnWeird.Click += new System.EventHandler(this.btnWeird_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stuffToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1205, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // stuffToolStripMenuItem
            // 
            this.stuffToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadParadoxFileToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.spendTimeWithKidsToolStripMenuItem});
            this.stuffToolStripMenuItem.Name = "stuffToolStripMenuItem";
            this.stuffToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.stuffToolStripMenuItem.Text = "Stuff";
            // 
            // loadParadoxFileToolStripMenuItem
            // 
            this.loadParadoxFileToolStripMenuItem.Name = "loadParadoxFileToolStripMenuItem";
            this.loadParadoxFileToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.loadParadoxFileToolStripMenuItem.Text = "Load Paradox File";
            this.loadParadoxFileToolStripMenuItem.Click += new System.EventHandler(this.loadParadoxFileToolStripMenuItem_Click);
            // 
            // spendTimeWithKidsToolStripMenuItem
            // 
            this.spendTimeWithKidsToolStripMenuItem.Name = "spendTimeWithKidsToolStripMenuItem";
            this.spendTimeWithKidsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.spendTimeWithKidsToolStripMenuItem.Text = "Spend time with kids";
            this.spendTimeWithKidsToolStripMenuItem.Click += new System.EventHandler(this.spendTimeWithKidsToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 623);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1205, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(35, 17);
            this.statusLabel.Text = "ready";
            // 
            // btnNodeUpdate
            // 
            this.btnNodeUpdate.Location = new System.Drawing.Point(877, 87);
            this.btnNodeUpdate.Name = "btnNodeUpdate";
            this.btnNodeUpdate.Size = new System.Drawing.Size(132, 23);
            this.btnNodeUpdate.TabIndex = 10;
            this.btnNodeUpdate.Text = "Update Node with Text";
            this.btnNodeUpdate.UseVisualStyleBackColor = true;
            this.btnNodeUpdate.Click += new System.EventHandler(this.btnNodeUpdate_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // LameUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1205, 645);
            this.Controls.Add(this.btnNodeUpdate);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnWeird);
            this.Controls.Add(this.btnJSON);
            this.Controls.Add(this.textRaw);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.typeLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LameUI";
            this.Text = "LameUI";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.RichTextBox textRaw;
        private System.Windows.Forms.Button btnJSON;
        private System.Windows.Forms.Button btnWeird;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem stuffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadParadoxFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spendTimeWithKidsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button btnNodeUpdate;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    }
}