using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Memento.Persistence.Interfaces;
using Memento.Test.Entities;
using Memento.Persistence;

namespace Memento.Test.App
{
    public partial class Entities : Form
    {
        public Entities()
        {
            InitializeComponent();
        }

        private void Entities_Load(object sender, EventArgs e)
        {
            NameValueCollection section = ConfigurationManager.GetSection("PersistenceEntities") as NameValueCollection;

            cmbEntidades.Items.Clear();

            foreach(string cls in section.AllKeys)
            {
                cmbEntidades.Items.Add(section[cls]);
            }

            cmbEntidades.Items.Insert(0, string.Empty);
        }

        private void cmbEntidades_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbEntidades.SelectedIndex != 0)
            {   
                if(cmbEntidades.SelectedItem.Equals("Producto"))
                {
                    IPersistence<Producto> servPers = new Persistence<Producto>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
                else if (cmbEntidades.SelectedItem.Equals("Cliente"))
                {
                    IPersistence<Cliente> servPers = new Persistence<Cliente>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
                else if (cmbEntidades.SelectedItem.Equals("Linea"))
                {
                    IPersistence<Linea> servPers = new Persistence<Linea>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
                else if (cmbEntidades.SelectedItem.Equals("Factura"))
                {
                    IPersistence<Factura> servPers = new Persistence<Factura>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
            }
            else
            {
                dgData.DataSource = null;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dgData.Rows)
                {
                    if(row.IsNewRow)
                    {
                        
                    }
                    else
                    {
                        DataRowView drv = row.DataBoundItem as DataRowView;

                        switch (drv.Row.RowState)
                        {
                            case DataRowState.Modified:
                                
                                break;
                            case DataRowState.Deleted:

                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IPersistence<Linea> pers = new Persistence<Linea>();

            Linea linea = pers.GetEntity(1);

            IList<Linea> testLineas = pers.GetEntities(new Linea());

            List<Linea> lineas = linea.Producto.Value.Lineas.Value;

            Producto prod1 = new Producto();
            prod1.Nombre = "Test1";

            Producto prod2 = new Producto();
            prod2.Nombre = "Test2";

            using(DataContext context = new DataContext())
            {
                try
                {
                    IPersistence<Producto> servProd = new Persistence<Producto>(context);

                    prod1 = servProd.InsertEntity(prod1);

                    prod2 = servProd.InsertEntity(prod2);

                    context.SaveChanges();    
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
