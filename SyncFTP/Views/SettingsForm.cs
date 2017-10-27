using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using SyncFTP.Controllers;
using System;
using System.Windows.Forms;

namespace SyncFTP.Views
{
    public partial class SettingsForm : XtraForm
    {
        public SettingsForm()
        {
            InitializeComponent();

            ReadSettings();
        }

        /// <summary>
        /// Objeto a nivel global que permite interactuar con el nucleo para hacer lectura y validaciones de los ajustes de los servidores
        /// </summary>
        Kernel _kernel = new Kernel();

        /// <summary>
        /// Objeto a nivel global que permite interactuar con los metodos de cifrado
        /// </summary>
        Secret _secret = new Secret();

        /// <summary>
        /// Objeto a nivel global que permite almacenar los ajustes de ambos servidores
        /// </summary>
        Servers _server = new Servers();

        #region Events

        /// <summary>
        /// Cierra la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Permite quitar la proteccion de los caracteres de la contraseña mientras este presionado el boton del mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petSeeRemotePassword_MouseDown(object sender, MouseEventArgs e)
        {
            txtRemotePassword.Properties.PasswordChar = '\0';
            petSeeRemotePassword.Image = Properties.Resources.Unlock;
        }

        /// <summary>
        /// Restablece la proteccion de los caracteres de contraseña mientras no este presionado el boton del mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petSeeRemotePassword_MouseUp(object sender, MouseEventArgs e)
        {
            txtRemotePassword.Properties.PasswordChar = '•';
            petSeeRemotePassword.Image = Properties.Resources.Lock;
        }

        /// <summary>
        /// Permite quitar la proteccion de los caracteres de la contraseña mientras este presionado el boton del mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petSeeLocalPassword_MouseDown(object sender, MouseEventArgs e)
        {
            txtLocalPassword.Properties.PasswordChar = '\0';
            petSeeLocalPassword.Image = Properties.Resources.Unlock;
        }

        /// <summary>
        /// Restablece la proteccion de los caracteres de contraseña mientras no este presionado el boton del mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petSeeLocalPassword_MouseUp(object sender, MouseEventArgs e)
        {
            txtLocalPassword.Properties.PasswordChar = '•';
            petSeeLocalPassword.Image = Properties.Resources.Lock;
        }

        /// <summary>
        /// Pide verificar la conexion si se detecta el cambio del contenido de alguno de los campos del servidor remoto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRemoteServer_TextChanged(object sender, EventArgs e)
        {
            lblRemoteStatus.Text = "Realice de nuevo la conexión.";

            btnRemoteTest.Image = Properties.Resources.Disconnected16;
        }

        /// <summary>
        /// Pide verificar la conexion si se detecta el cambio del contenido de alguno de los campos del servidor local
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLocalServer_TextChanged(object sender, EventArgs e)
        {
            lblLocalStatus.Text = "Realice de nuevo la conexión.";

            btnLocalTest.Image = Properties.Resources.Disconnected16;
        }

        /// <summary>
        /// Realiza la conexion y validacion del servidor local
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLocalTest_Click(object sender, EventArgs e)
        {
            if (txtLocalServer.Text == "" || txtLocalUser.Text == "" || txtLocalPassword.Text == "" || txtLocalPort.Text == "" || Convert.ToInt32(txtLocalPort.Text) < 0 || cbxLocalEncryption.SelectedIndex < 0)
            {
                XtraMessageBox.Show(UserLookAndFeel.Default, "Son necesarios todos los datos para poder probar la conexión local", "SyncFTP - Faltan datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtLocalServer.Focus();
            }

            else
            {
                lblLocalStatus.Text = "Conectando... espere por favor.";

                string _ftpLocalServer = "";

                if (txtLocalServer.Text.Contains("localhost") || txtLocalServer.Text.Contains("127.0.0.1"))
                {
                    _ftpLocalServer = "localhost";
                }

                else
                {
                    _ftpLocalServer = txtLocalServer.Text.Contains("ftp.") ? txtLocalServer.Text : "ftp." + txtLocalServer.Text;
                }

                _server = new Servers { Remote = new Remote { Server = "", IsAnonymous = "", User = "", Password = "", Port = "", Find = "", FTPMode = "", IsActive = "", WithCert = "" }, Local = new Local { Server = _secret.Encrypt(_ftpLocalServer), IsAnonymous = _secret.Encrypt(chkLocalAnonymous.Checked.ToString()), User = _secret.Encrypt(txtLocalUser.Text), Password = _secret.Encrypt(txtLocalPassword.Text), Port = _secret.Encrypt(txtLocalPort.Text), Find = _secret.Encrypt(txtLocalSaveFolder.Text == "" ? "/" : txtLocalSaveFolder.Text), FTPMode = _secret.Encrypt(cbxLocalEncryption.SelectedIndex.ToString()), IsActive = _secret.Encrypt(chkLocalActive.Checked.ToString()), WithCert = _secret.Encrypt(chkLocalCertificates.Checked.ToString())  } };

                if (_kernel.LocalIsValid(_server) != null)
                {
                    if (_kernel.TestAndSaveSettings(_server.Local, _server.Remote))
                    {
                        btnLocalTest.Image = Properties.Resources.Connected16;

                        lblLocalStatus.Text = "Ajustes válidos y guardados";
                    }
                }

                else
                {
                    btnLocalTest.Image = Properties.Resources.Disconnected16;

                    lblLocalStatus.Text = "Error al conectar.";
                }
            }
        }

        /// <summary>
        /// Realiza la conexion y validacion del servidor remoto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoteTest_Click(object sender, EventArgs e)
        {
            if (txtRemoteServer.Text == "" || txtRemoteUser.Text == "" || txtRemotePassword.Text == "" || txtRemotePort.Text == "" || Convert.ToInt32(txtRemotePort.Text) < 0 || cbxRemoteEncryption.SelectedIndex < 0)
            {
                XtraMessageBox.Show(UserLookAndFeel.Default, "Son necesarios todos los datos para poder probar la conexión remota", "SyncFTP - Faltan datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtRemoteServer.Focus();
            }

            else
            {
                lblRemoteStatus.Text = "Conectando... espere por favor.";

                string _ftpRemoteServer = "";

                if (txtRemoteServer.Text.Contains("localhost") || txtRemoteServer.Text.Contains("127.0.0.1"))
                {
                    _ftpRemoteServer = "localhost";
                }

                else
                {
                    _ftpRemoteServer = txtRemoteServer.Text.Contains("ftp.") ? txtRemoteServer.Text : "ftp." + txtRemoteServer.Text;
                }
                
                _server = new Servers { Remote = new Remote { Server = _secret.Encrypt(_ftpRemoteServer), IsAnonymous =_secret.Encrypt(chkRemoteAnonymous.Checked.ToString()), User = _secret.Encrypt(txtRemoteUser.Text), Password = _secret.Encrypt(txtRemotePassword.Text), Port = _secret.Encrypt(txtRemotePort.Text), Find = _secret.Encrypt(txtRemoteSaveFolder.Text == "" ? "/" : txtRemoteSaveFolder.Text), FTPMode = _secret.Encrypt(cbxRemoteEncryption.SelectedIndex.ToString()), IsActive = _secret.Encrypt(chkRemoteActive.Checked.ToString()), WithCert = _secret.Encrypt(chkRemoteCertificates.Checked.ToString()) }, Local = new Local { Server = "", IsAnonymous = "" , User = "", Password = "", Port = "", Find = "", FTPMode = "", IsActive = "", WithCert = "" } };

                if (_kernel.RemoteIsValid(_server) != null)
                {
                    if (_kernel.TestAndSaveSettings(_server.Local, _server.Remote))
                    {
                        btnRemoteTest.Image = Properties.Resources.Connected16;

                        lblRemoteStatus.Text = "Ajustes válidos y guardados";
                    }
                }

                else
                {
                    btnRemoteTest.Image = Properties.Resources.Disconnected16;

                    lblRemoteStatus.Text = "Error al conectar.";
                }
            }
        }

        /// <summary>
        /// Verifica si se activo la opcion de usuario "Anónimo" para el servidor remoto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkRemoteAnonymous_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRemoteAnonymous.Checked)
            {
                txtRemoteUser.Text = "anonymous";
                txtRemoteUser.ReadOnly = true;
                txtRemotePassword.Text = "anonymous@example.com";
                txtRemotePassword.ReadOnly = true;
            }

            else
            {
                txtRemoteUser.Text = "";
                txtRemoteUser.ReadOnly = false;
                txtRemotePassword.Text = "";
                txtRemotePassword.ReadOnly = false;
            }
        }

        /// <summary>
        /// Verifica si se activo la opcion de usuario "Anónimo" para el servidor local
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkLocalAnonymous_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLocalAnonymous.Checked)
            {
                txtLocalUser.Text = "anonymous";
                txtLocalUser.ReadOnly = true;
                txtLocalPassword.Text = "anonymous@example.com";
                txtLocalPassword.ReadOnly = true;
            }

            else
            {
                txtLocalUser.Text = "";
                txtLocalUser.ReadOnly = false;
                txtLocalPassword.Text = "";
                txtLocalPassword.ReadOnly = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verifica la existencia de los ajustes del servidor local y remoto
        /// </summary>
        private void ReadSettings()
        {
            try
            {
                Servers _settings = _kernel.ReadSettings();

                if (_settings != null)
                {
                    if (_settings.Remote.Server != null)
                    {
                        txtRemoteServer.Text = _secret.Decrypt(_settings.Remote.Server);
                        if (_secret.Decrypt(_settings.Remote.IsAnonymous) == "True")
                        {
                            chkRemoteAnonymous.Checked = true;
                        }
                        txtRemoteUser.Text = _secret.Decrypt(_settings.Remote.User);
                        txtRemotePassword.Text = _secret.Decrypt(_settings.Remote.Password);
                        txtRemotePort.Text = _secret.Decrypt(_settings.Remote.Port);
                        txtRemoteSaveFolder.Text = _secret.Decrypt(_settings.Remote.Find);
                        cbxRemoteEncryption.SelectedIndex = Convert.ToInt32(_secret.Decrypt(_settings.Remote.FTPMode));
                        if (_secret.Decrypt(_settings.Remote.IsActive) == "True")
                        {
                            chkRemoteActive.Checked = true;
                        }
                        if (_secret.Decrypt(_settings.Remote.WithCert) == "True")
                        {
                            chkRemoteCertificates.Checked = true;
                        }
                    }

                    if (_settings.Local.Server != null)
                    {
                        txtLocalServer.Text = _secret.Decrypt(_settings.Local.Server);
                        if (_secret.Decrypt(_settings.Local.IsAnonymous) == "True")
                        {
                            chkLocalAnonymous.Checked = true;
                        }
                        txtLocalUser.Text = _secret.Decrypt(_settings.Local.User);
                        txtLocalPassword.Text = _secret.Decrypt(_settings.Local.Password);
                        txtLocalPort.Text = _secret.Decrypt(_settings.Local.Port);
                        txtLocalSaveFolder.Text = _secret.Decrypt(_settings.Local.Find);
                        cbxLocalEncryption.SelectedIndex = Convert.ToInt32(_secret.Decrypt(_settings.Local.FTPMode));
                        if (_secret.Decrypt(_settings.Local.IsActive) == "True")
                        {
                            chkLocalActive.Checked = true;
                        }
                        if (_secret.Decrypt(_settings.Local.WithCert) == "True")
                        {
                            chkLocalCertificates.Checked = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        #endregion
        
    }
}