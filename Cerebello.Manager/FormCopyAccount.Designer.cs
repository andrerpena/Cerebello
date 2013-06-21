namespace Cerebello.Manager
{
    partial class FormCopyAccount
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
            this.buttonCopyAccountData = new System.Windows.Forms.Button();
            this.listBoxItems = new System.Windows.Forms.ListBox();
            this.labelCopied = new System.Windows.Forms.Label();
            this.labelFailed = new System.Windows.Forms.Label();
            this.buttonCopyDatabase = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCopyAccountData
            // 
            this.buttonCopyAccountData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyAccountData.AutoSize = true;
            this.buttonCopyAccountData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCopyAccountData.Location = new System.Drawing.Point(352, 227);
            this.buttonCopyAccountData.Name = "buttonCopyAccountData";
            this.buttonCopyAccountData.Size = new System.Drawing.Size(79, 23);
            this.buttonCopyAccountData.TabIndex = 0;
            this.buttonCopyAccountData.Text = "Copy storage";
            this.buttonCopyAccountData.UseVisualStyleBackColor = true;
            this.buttonCopyAccountData.Click += new System.EventHandler(this.buttonCopyStorage_Click);
            // 
            // listBoxItems
            // 
            this.listBoxItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxItems.FormattingEnabled = true;
            this.listBoxItems.IntegralHeight = false;
            this.listBoxItems.Location = new System.Drawing.Point(12, 12);
            this.listBoxItems.Name = "listBoxItems";
            this.listBoxItems.Size = new System.Drawing.Size(419, 209);
            this.listBoxItems.TabIndex = 1;
            // 
            // labelCopied
            // 
            this.labelCopied.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCopied.AutoSize = true;
            this.labelCopied.ForeColor = System.Drawing.Color.ForestGreen;
            this.labelCopied.Location = new System.Drawing.Point(12, 232);
            this.labelCopied.Name = "labelCopied";
            this.labelCopied.Size = new System.Drawing.Size(85, 13);
            this.labelCopied.TabIndex = 2;
            this.labelCopied.Text = "Copying: 0 blobs";
            // 
            // labelFailed
            // 
            this.labelFailed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFailed.AutoSize = true;
            this.labelFailed.ForeColor = System.Drawing.Color.Brown;
            this.labelFailed.Location = new System.Drawing.Point(130, 232);
            this.labelFailed.Name = "labelFailed";
            this.labelFailed.Size = new System.Drawing.Size(75, 13);
            this.labelFailed.TabIndex = 3;
            this.labelFailed.Text = "Failed: 0 blobs";
            // 
            // buttonCopyDatabase
            // 
            this.buttonCopyDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyDatabase.AutoSize = true;
            this.buttonCopyDatabase.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCopyDatabase.Location = new System.Drawing.Point(258, 227);
            this.buttonCopyDatabase.Name = "buttonCopyDatabase";
            this.buttonCopyDatabase.Size = new System.Drawing.Size(88, 23);
            this.buttonCopyDatabase.TabIndex = 0;
            this.buttonCopyDatabase.Text = "Copy database";
            this.buttonCopyDatabase.UseVisualStyleBackColor = true;
            this.buttonCopyDatabase.Click += new System.EventHandler(this.buttoCopyDatabase_Click);
            // 
            // FormCopyAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 262);
            this.Controls.Add(this.labelFailed);
            this.Controls.Add(this.labelCopied);
            this.Controls.Add(this.listBoxItems);
            this.Controls.Add(this.buttonCopyDatabase);
            this.Controls.Add(this.buttonCopyAccountData);
            this.Name = "FormCopyAccount";
            this.Text = "Copy account";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCopyAccountData;
        private System.Windows.Forms.ListBox listBoxItems;
        private System.Windows.Forms.Label labelCopied;
        private System.Windows.Forms.Label labelFailed;
        private System.Windows.Forms.Button buttonCopyDatabase;
    }
}

