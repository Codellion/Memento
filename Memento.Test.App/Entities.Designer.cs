namespace Memento.Test.App
{
    partial class Entities
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgData = new System.Windows.Forms.DataGridView();
            this.cmbEntidades = new System.Windows.Forms.ComboBox();
            this.lblEntities = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).BeginInit();
            this.SuspendLayout();
            // 
            // dgData
            // 
            this.dgData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgData.Location = new System.Drawing.Point(12, 67);
            this.dgData.Name = "dgData";
            this.dgData.Size = new System.Drawing.Size(782, 439);
            this.dgData.TabIndex = 0;
            // 
            // cmbEntidades
            // 
            this.cmbEntidades.FormattingEnabled = true;
            this.cmbEntidades.Location = new System.Drawing.Point(82, 22);
            this.cmbEntidades.Name = "cmbEntidades";
            this.cmbEntidades.Size = new System.Drawing.Size(167, 21);
            this.cmbEntidades.TabIndex = 1;
            this.cmbEntidades.SelectedIndexChanged += new System.EventHandler(this.cmbEntidades_SelectedIndexChanged);
            // 
            // lblEntities
            // 
            this.lblEntities.AutoSize = true;
            this.lblEntities.Location = new System.Drawing.Point(22, 25);
            this.lblEntities.Name = "lblEntities";
            this.lblEntities.Size = new System.Drawing.Size(54, 13);
            this.lblEntities.TabIndex = 2;
            this.lblEntities.Text = "Entidades";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(573, 22);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(129, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Guardar Cambios";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(719, 22);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Eliminar";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Entities
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 518);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblEntities);
            this.Controls.Add(this.cmbEntidades);
            this.Controls.Add(this.dgData);
            this.Name = "Entities";
            this.Text = "Ejemplo de Memento";
            this.Load += new System.EventHandler(this.Entities_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgData;
        private System.Windows.Forms.ComboBox cmbEntidades;
        private System.Windows.Forms.Label lblEntities;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button button3;
    }
}

