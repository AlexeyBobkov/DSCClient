namespace ScopeDSCClient
{
    partial class SkyObjectForm
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
            this.buttonFromList = new System.Windows.Forms.Button();
            this.buttonByCoordinates = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonLastObj = new System.Windows.Forms.Button();
            this.buttonStellarium = new System.Windows.Forms.Button();
            this.buttonUnselect = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonFromList
            // 
            this.buttonFromList.Location = new System.Drawing.Point(62, 30);
            this.buttonFromList.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFromList.Name = "buttonFromList";
            this.buttonFromList.Size = new System.Drawing.Size(277, 105);
            this.buttonFromList.TabIndex = 0;
            this.buttonFromList.Text = "From List";
            this.buttonFromList.UseVisualStyleBackColor = true;
            this.buttonFromList.Click += new System.EventHandler(this.buttonFromList_Click);
            // 
            // buttonByCoordinates
            // 
            this.buttonByCoordinates.Location = new System.Drawing.Point(62, 169);
            this.buttonByCoordinates.Margin = new System.Windows.Forms.Padding(4);
            this.buttonByCoordinates.Name = "buttonByCoordinates";
            this.buttonByCoordinates.Size = new System.Drawing.Size(277, 105);
            this.buttonByCoordinates.TabIndex = 2;
            this.buttonByCoordinates.Text = "By Coordinates";
            this.buttonByCoordinates.UseVisualStyleBackColor = true;
            this.buttonByCoordinates.Click += new System.EventHandler(this.buttonByCoordinates_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(364, 473);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(277, 105);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonLastObj
            // 
            this.buttonLastObj.Location = new System.Drawing.Point(62, 309);
            this.buttonLastObj.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLastObj.Name = "buttonLastObj";
            this.buttonLastObj.Size = new System.Drawing.Size(277, 105);
            this.buttonLastObj.TabIndex = 4;
            this.buttonLastObj.Text = "Recent Objects";
            this.buttonLastObj.UseVisualStyleBackColor = true;
            this.buttonLastObj.Click += new System.EventHandler(this.buttonLastObj_Click);
            // 
            // buttonStellarium
            // 
            this.buttonStellarium.Location = new System.Drawing.Point(364, 169);
            this.buttonStellarium.Margin = new System.Windows.Forms.Padding(4);
            this.buttonStellarium.Name = "buttonStellarium";
            this.buttonStellarium.Size = new System.Drawing.Size(277, 105);
            this.buttonStellarium.TabIndex = 3;
            this.buttonStellarium.Text = "Stellarium";
            this.buttonStellarium.UseVisualStyleBackColor = true;
            this.buttonStellarium.Click += new System.EventHandler(this.buttonStellarium_Click);
            // 
            // buttonUnselect
            // 
            this.buttonUnselect.Location = new System.Drawing.Point(62, 473);
            this.buttonUnselect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonUnselect.Name = "buttonUnselect";
            this.buttonUnselect.Size = new System.Drawing.Size(277, 105);
            this.buttonUnselect.TabIndex = 5;
            this.buttonUnselect.Text = "Unselect Object";
            this.buttonUnselect.UseVisualStyleBackColor = true;
            this.buttonUnselect.Click += new System.EventHandler(this.buttonUnselect_Click);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(364, 30);
            this.buttonSearch.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(277, 105);
            this.buttonSearch.TabIndex = 1;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // SkyObjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 612);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.buttonUnselect);
            this.Controls.Add(this.buttonStellarium);
            this.Controls.Add(this.buttonLastObj);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonByCoordinates);
            this.Controls.Add(this.buttonFromList);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "SkyObjectForm";
            this.Text = "Select Object";
            this.Load += new System.EventHandler(this.SkyObjectForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SkyObjectForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonFromList;
        private System.Windows.Forms.Button buttonByCoordinates;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonLastObj;
        private System.Windows.Forms.Button buttonStellarium;
        private System.Windows.Forms.Button buttonUnselect;
        private System.Windows.Forms.Button buttonSearch;
    }
}