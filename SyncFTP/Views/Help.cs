using DevExpress.XtraEditors;
using System;
using System.Windows.Forms;

namespace SyncFTP.Views
{
    public partial class Help : XtraForm
    {
        /// <summary>
        /// Variable estatica de tipo Help que mantiene solo una ventana de la misma a la vez
        /// </summary>
        private static Help _singleton;

        /// <summary>
        /// Metodo estatico que permite ser instanciado desde otros formularios
        /// </summary>
        /// <returns>Retorna una ventana, si ya existe una, retorna la misma</returns>
        public static Help GetInstance()
        {
            if (_singleton == null || _singleton.IsDisposed)
            {
                _singleton = new Help();
            }

            _singleton.BringToFront();

            return _singleton;
        }

        private Help()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Variable global que obtiene la direccion logica donde esta almacenado el Manual del usuario
        /// </summary>
        private static string _userManualPath = Application.StartupPath + @"\Manuals\UserManual.pdf";

        /// <summary>
        /// Metodo que carga el manual del usuario para hacerlo visible desde que se carga el formulario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help_Load(object sender, EventArgs e)
        {
            try
            {
                pdfViewer1.LoadDocument(_userManualPath);
            }
            catch (Exception)
            {
                
            }
        }
    }
}