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
using Memento.Persistence.Commons;
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
                else if (cmbEntidades.SelectedItem.Equals("DetalleLinea"))
                {
                    IPersistence<DetalleLinea> servPers = new Persistence<DetalleLinea>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
                else if (cmbEntidades.SelectedItem.Equals("Proveedor"))
                {
                    IPersistence<Proveedor> servPers = new Persistence<Proveedor>();
                    dgData.DataSource = servPers.GetEntitiesDs().Tables[0];
                }
                else if (cmbEntidades.SelectedItem.Equals("ProductoProveedor"))
                {
                    IPersistence<ProductoProveedor> servPers = new Persistence<ProductoProveedor>();
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
            //IPersistence<Linea> pers = new Persistence<Linea>();

            //Linea linea = pers.GetEntity(1);

            //IList<Linea> testLineas = pers.GetEntities(new Linea());

            //BindingList<Linea> lineas = linea.Producto.Value.Lineas.Value;

            //Producto prod1 = new Producto();
            //prod1.Nombre = "Test1";

            //Producto prod2 = new Producto();
            //prod2.Nombre = "Test2";

            //using(DataContext context = new DataContext())
            //{
            //    try
            //    {
            //        IPersistence<Producto> servProd = new Persistence<Producto>(context);

            //        prod1 = servProd.InsertEntity(prod1);

            //        prod2 = servProd.InsertEntity(prod2);

            //        context.SaveChanges();    
            //    }
            //    catch(Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //}


            try
            {
                IPersistence<Factura> servFactura = new Persistence<Factura>();
                //Factura factura = servFactura.GetEntity(1);

                //factura.Lineas.Value.RemoveAt(1);

                //servFactura.PersistEntity(factura);
                


                //Linea newLinea = new Linea();
                //newLinea.Cantidad = 2;
                //newLinea.Descripcion = "Test de dependencias";
                //newLinea.Producto = new Reference<Producto>(1);
                //newLinea.Factura = new Reference<Factura>(1);

                //DetalleLinea detalleL = new DetalleLinea();
                //detalleL.Detalle = "Detalle de test de dependencias";

                //newLinea.DetalleLinea = new Dependence<DetalleLinea>(detalleL);

                //newLinea.DetalleLinea.Value.Detalle = "test";

                //IPersistence<Linea> servLinea = new Persistence<Linea>();
                //servLinea.PersistEntity(newLinea);


                //newLinea.DetalleLinea.Value.Detalle = "nuevo test";
                //servLinea.PersistEntity(newLinea);

                //servLinea.DeleteEntity(newLinea);
                
                //Factura newFactura = new Factura();
                //newFactura.Importe = 12.4f;
                //newFactura.Cliente = new Reference<Cliente>(1);

                //newFactura.Lineas = new Dependences<Linea>(newLinea);

                
                //newFactura = servFactura.PersistEntity(newFactura);


                IPersistence<Proveedor> servProv = new Persistence<Proveedor>();
                Proveedor proveedor2 = servProv.GetEntity(2);

                IList<ProductoProveedor> prodsProv2 = proveedor2.Productos.Value;


                IPersistence<Producto> servProd = new Persistence<Producto>();
                Producto lapiz = servProd.GetEntity(1);

                IList<ProductoProveedor> provsLapiz = lapiz.Proveedores.Value;

                Proveedor newProv4 = new Proveedor();
                newProv4.Nombre = "Nuevo Proveedor 5";
                newProv4.Telefono = "5";
                newProv4.Email = "e@x.com";

                ProductoProveedor newProdProv = lapiz.Proveedores.CreateDependence();
                newProdProv.Proveedor = new Reference<Proveedor>(newProv4);
                newProdProv.Precio = 33;

                provsLapiz.Add(newProdProv);

                servProd.PersistEntity(lapiz);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            string dummy;
        }
    }
}
