using System;
using System.Windows.Forms;
using Memento.Persistence.Commons.Keygen;

namespace Memento.Test.App
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Entities());    
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                KeyGeneration.Synchronize();
            }
        }
    }
}