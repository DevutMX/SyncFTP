using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SyncFTP.Controllers;

namespace SyncFTP.Views
{
    public partial class MovementsForm : XtraForm
    {
        private static MovementsForm _singleton;

        public static MovementsForm GetInstance()
        {
            if (_singleton == null || _singleton.IsDisposed)
            {
                _singleton = new MovementsForm();
            }

            _singleton.BringToFront();

            return _singleton;
        }

        private MovementsForm()
        {
            InitializeComponent();

            GetList();
        }

        Bridge _bridge = new Bridge();

        private void GetList()
        {
            try
            {
                _bridge.MovementsToList(gdcMovements);

                gdvMovements.Columns["Fecha"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;

                gdvMovements.Columns["Fecha"].DisplayFormat.FormatString = "g";
            }
            catch (Exception)
            {
                
            }
        }

        private void MovementsForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar.ToString().ToUpper() == Convert.ToChar(Keys.R).ToString())
                {
                    GetList();
                }
            }
            catch (Exception)
            {
                
            }
        }

    }
}