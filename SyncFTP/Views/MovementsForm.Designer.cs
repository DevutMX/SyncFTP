namespace SyncFTP.Views
{
    partial class MovementsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovementsForm));
            this.gdcMovements = new DevExpress.XtraGrid.GridControl();
            this.gdvMovements = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.gdcMovements)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdvMovements)).BeginInit();
            this.SuspendLayout();
            // 
            // gdcMovements
            // 
            this.gdcMovements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gdcMovements.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gdcMovements.Location = new System.Drawing.Point(0, 0);
            this.gdcMovements.MainView = this.gdvMovements;
            this.gdcMovements.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gdcMovements.Name = "gdcMovements";
            this.gdcMovements.Size = new System.Drawing.Size(584, 361);
            this.gdcMovements.TabIndex = 0;
            this.gdcMovements.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gdvMovements});
            this.gdcMovements.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MovementsForm_KeyPress);
            // 
            // gdvMovements
            // 
            this.gdvMovements.GridControl = this.gdcMovements;
            this.gdvMovements.Name = "gdvMovements";
            this.gdvMovements.OptionsBehavior.Editable = false;
            this.gdvMovements.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MovementsForm_KeyPress);
            // 
            // MovementsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.gdcMovements);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MovementsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Registro de transferencias";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MovementsForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.gdcMovements)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdvMovements)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gdcMovements;
        private DevExpress.XtraGrid.Views.Grid.GridView gdvMovements;
    }
}