using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using Microsoft.Win32;
using Newtonsoft.Json;
using SyncFTP.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSCP;

namespace SyncFTP.Views
{
    public partial class MainForm : XtraForm
    {
        public MainForm()
        {
            InitializeComponent();

            ToStartup();
        }

        /// <summary>
        /// Objeto a nivel global que permite interactuar con los metodos de cifrado
        /// </summary>
        Secret _secret = new Secret();

        /// <summary>
        /// Objeto a nivel global que permite interactuar con el nucleo para hacer lectura y validaciones de los ajustes de los servidores
        /// </summary>
        Kernel _kernel = new Kernel();

        /// <summary>
        /// Objeto a nivel global que permite interactuar con los metodos de la base de datos
        /// </summary>
        Bridge _bridge = new Bridge();

        /// <summary>
        /// Objeto a nivel global que permite llamar notificaciones dentro de este formulario
        /// </summary>
        Notify _notify;

        /// <summary>
        /// Variable de nivel global que indica la direccion logica donde esta la carpeta con los archivos remotos
        /// </summary>
        private string _remoteFilesDirectory = Application.StartupPath + @"\RemoteFiles";

        /// <summary>
        /// Variable privada de nivel global que reinicia el contador de segundos por cada operacion
        /// </summary>
        private int _seconds = 0;

        #region Events

        /// <summary>
        /// Evento que se dispara justo despues de cargar la ventana del formulario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            PromptSettings();

            WindowState = FormWindowState.Minimized;
            
            if (CheckInternet())
            {
                BeginToSyncRemote();
            }

            else
            {
                BeginToSyncLocal();
            }
        }

        /// <summary>
        /// Metodo que al ser llamado verifica por medio de un ping si hay conexion a internet
        /// </summary>
        /// <returns>Retorna true cuando hay acceso internet</returns>
        private bool CheckInternet()
        {
            try
            {
                Ping _pinger = new Ping();

                PingReply _reply = _pinger.Send("www.google.com.mx");
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Evento que se dispara cuando se hace clic en la imagen de ajustes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petSettings_Click(object sender, EventArgs e)//Revisar integridad
        {
            SettingsForm _settingsForm = new SettingsForm();

            Hide();

            _settingsForm.ShowDialog();

            Show();

            PromptSettings();
        }
        
        /// <summary>
        /// Evento que envia el formulario al icono de sistema en la barra de tareas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                _notify = new Notify("Minimizado", "SyncFTP está en segundo plano", 1);

                _notify.Show();

                nicSystemTray.Visible = true;
                Hide();
                ShowInTaskbar = false;
            }

            else if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
            {
                nicSystemTray.Visible = false;
            }
        }

        /// <summary>
        /// Evento de emergencia, donde al presionar F12 en el teclado, permite que reaparezcan forzosamente los botones play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.F12))
            {
                VisibleButtons(true);
            }
        }

        /// <summary>
        /// Evento que restaura el formulario cuando se hace doble click en la barra de tareas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nicSystemTray_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        /// <summary>
        /// Cuando esta en la barra de tareas y se presiona salir, se pregunta para confirmar la acción
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExitSysTray_Click(object sender, EventArgs e)//Revisar inutilidad --
        {
            if (XtraMessageBox.Show(UserLookAndFeel.Default, "¿En verdad desea cerrar la aplicación?\nEsto detendrá todas las transferencias si no se han terminado y pueden quedar archivos corruptos", "Cerrar SyncFTP", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Evento que inicia la sincronizacion remota desde la barra de tareas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartRemote_Click(object sender, EventArgs e)//Preguntar si desea continuar --
        {
            if(CheckInternet())
            {
                BeginToSyncRemote();
            }

            else
            {
                DialogResult f = XtraMessageBox.Show(UserLookAndFeel.Default, "No cuenta con conexión a internet, pero puede intentar en caso de que\n el servidor \"Remoto\" este conectado de forma local.\n\n¿Desea continuar?", "SyncFTP - Sin conexión a internet", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (f == DialogResult.Yes)
                {
                    BeginToSyncRemote();
                }
            }
        }

        /// <summary>
        /// Evento que inicia la sincronizacion local desde la barra de tareas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartLocal_Click(object sender, EventArgs e)//Preguntar si desea continuar --
        {
            if (!CheckInternet())
            {
                BeginToSyncLocal();
            }

            else
            {
                DialogResult f = XtraMessageBox.Show(UserLookAndFeel.Default, "Al estar conectado a internet SyncFTP da preferencia a conexiones con el servidor \"Remoto\",\ncancelando acciones con el servidor \"Local\", pero aún así puede acceder a él, si este está accesible.\n\n¿Desea continuar?", "SyncFTP - Confirme acción", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (f == DialogResult.Yes)
                {
                    BeginToSyncLocal();
                }
            }
        }

        /// <summary>
        /// Obtiene el listado de directorios desde el servidor remoto y almacena la informacion en unn JSON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoteTreeStrip_Click(object sender, EventArgs e)//Convertir progress a marquee //
        {
            if(XtraMessageBox.Show(UserLookAndFeel.Default, "Este proceso puede tardar mucho tiempo. ¿Deseas continuar?", "SyncFTP - Confirme petición", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                GetRemoteLists();
            }
        }

        /// <summary>
        /// Obtiene el listado de directorios desde el servidor local y almacena la informacion en unn JSON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLocalTreeStrip_Click(object sender, EventArgs e)//Convertir progress a marquee //
        {
            if (XtraMessageBox.Show(UserLookAndFeel.Default, "Este proceso puede tardar mucho tiempo. ¿Deseas continuar?", "SyncFTP - Confirme petición", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                GetLocalLists();
            }
        }

        /// <summary>
        /// Evento que permite visualizar el formulario con la lista de trasnferencias realizadas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petTransactions_Click(object sender, EventArgs e)
        {
            MovementsForm _movementsForm = MovementsForm.GetInstance();

            _movementsForm.Show();
        }

        /// <summary>
        /// Evento que permite visualizar el formulario con el manual del usuario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petInfo_Click(object sender, EventArgs e)
        {
            Help _show = Help.GetInstance();

            _show.Show();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Guarda el registro de la aplicacion en el registro de windows para iniciar junto a el
        /// </summary>
        private void ToStartup()
        {
            try
            {
                RegistryKey _key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                //_key.SetValue("SyncFTP", Application.ExecutablePath);

                if (_key != null)
                {
                    _key.DeleteValue("SyncFTP", false);
                    _key.SetValue("SyncFTP", Application.ExecutablePath);
                }

                else
                {
                    _key.SetValue("SyncFTP", Application.ExecutablePath);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Quita el registro de la aplicacion en el registro de windows para evitar iniciar junto a el
        /// </summary>
        private void RemoveFromStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                key.DeleteValue("SyncFTP", false);
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Evalua si los ajustes de los servidores no estan vacios
        /// </summary>
        private void PromptSettings()
        {
            try
            {
                Servers _settings = _kernel.ReadSettings();

                if (_settings != null)
                {
                    if (_settings.Remote.Server == "")
                    {
                        XtraMessageBox.Show(UserLookAndFeel.Default, "Parece que no configuró el servidor \"Remoto\"...\nSolo se revisarán ajustes del servidor local.\nRecuerde que para configurar el servidor \"Remoto\" necesitará contar\ncon conexión a internet.", "SyncFTP - Servidor remoto desconocido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (_settings.Local.Server == "")
                    {
                        XtraMessageBox.Show(UserLookAndFeel.Default, "Parece que no configuró el servidor \"Local\"...\nSolo se revisarán ajustes del servidor remoto.", "SyncFTP - Servidor local desconocido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                else
                {
                    if (XtraMessageBox.Show(UserLookAndFeel.Default, "Parece que no ha configurado las conexiones a los servidores...\n\nPor favor, ingrese los datos del servidor que tenga disponible,\nrecuerde que para configurar el servidor \"Remoto\" necesitará\ncontar con conexión a internet.\n\n¿Desea proseguir con las configuraciones?", "SyncFTP - Servidores sin definir", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        petSettings_Click(null, null);
                    }

                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Permite restaurar u ocultar los botones de sincronizacion
        /// </summary>
        /// <param name="visible">True para habilitar los botones</param>
        private void VisibleButtons(bool visible)//Revisar emergencia --
        {
            if (visible)
            {
                _seconds = 0;
                _tmpElapsedTime.Enabled = !visible;
                lblElapsedTime.Visible = !visible;
                lblElapsedTime.Text = "Tiempo en ejecución:";
                pbcProgress.Position = 0;
                gbcLocal.Visible = visible;
                gbcRemote.Visible = visible;
                btnStartRemoteStrip.Visible = visible;
                btnStartLocalStrip.Visible = visible;
            }

            else
            {
                pbcProgress.Position = 0;
                _tmpElapsedTime.Enabled = !visible;
                lblElapsedTime.Visible = !visible;
                gbcLocal.Visible = visible;
                gbcRemote.Visible = visible;
                btnStartRemoteStrip.Visible = visible;
                btnStartLocalStrip.Visible = visible;
            }
        }

        /// <summary>
        /// Se procesa el directorio remoto completamente, pudiendo tardar demasiado - No recomendado su uso
        /// </summary>
        /// <param name="server">Datos de los servidores donde evalua el remoto</param>
        /// <returns>Devuelve una cadena de texto segun se halla completado el directorio o no</returns>
        public string ProcessingRemote(Servers server)
        {
            try
            {
                SessionOptions _client = _kernel.RemoteIsValid(server);

                if (_client != null)
                {
                    List<FullPath> _pathList = new List<FullPath>();

                    string _remotePathList = Application.StartupPath + @"\PathList\remote.json";

                    if (!File.Exists(_remotePathList))
                    {
                        using (File.Create(_remotePathList)) { }
                    }

                    using (Session _session = new Session())
                    {
                        _session.FileTransferProgress += FileTransferProgress;

                        _session.Open(_client);

                        petStatus.Invoke(new Action(() => petStatus.Image = Properties.Resources.SyncBusy));

                        IEnumerable<RemoteFileInfo> _fileInfos = _session.EnumerateRemoteFiles(_secret.Decrypt(server.Remote.Find), null, EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories);

                        foreach (RemoteFileInfo _fileInfo in _fileInfos)
                        {
                            DirectoryStructure _dirStructure = new DirectoryStructure
                            {
                                FilePermissions = _fileInfo.FilePermissions.ToString(),
                                FileType = _fileInfo.FileType,
                                FullName = _fileInfo.FullName,
                                Group = _fileInfo.Group,
                                IsDirectory = _fileInfo.IsDirectory,
                                IsParentDirectory = _fileInfo.IsParentDirectory,
                                IsThisDirectory = _fileInfo.IsThisDirectory,
                                LastWriteTime = _fileInfo.LastWriteTime,
                                Length = _fileInfo.Length,
                                Length32 = _fileInfo.Length32,
                                Name = _fileInfo.Name,
                                Owner = _fileInfo.Owner
                            };

                            lblNotifications.Invoke(new Action(() => lblNotifications.Text = "R - Obteniendo... " + _fileInfo.Name));

                            _pathList.Add(new FullPath { PathStructure = _dirStructure });
                        }

                        string _data = JsonConvert.SerializeObject(_pathList, Formatting.Indented);

                        File.WriteAllText(_remotePathList, _data);
                    }

                    return "Se creo el directorio remoto exitosamente";
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x001)", "Error al obtener directorios", 3);

                _notify.Show();

                return null;
            }
        }

        /// <summary>
        /// Se procesa el directorio local completamente, pudiendo tardar demasiado - No recomendado su uso
        /// </summary>
        /// <param name="server">Datos de los servidores donde evalua el local</param>
        /// <returns>Devuelve una cadena de texto segun se halla completado el directorio o no</returns>
        public string ProcessingLocal(Servers server)
        {
            try
            {
                SessionOptions _client = _kernel.LocalIsValid(server);

                if (_client != null)
                {
                    List<FullPath> _pathList = new List<FullPath>();

                    string _localPathList = Application.StartupPath + @"\PathList\local.json";

                    if (!File.Exists(_localPathList))
                    {
                        using (File.Create(_localPathList)) { }
                    }

                    using (Session _session = new Session())
                    {
                        _session.Open(_client);

                        petStatus.Invoke(new Action(() => petStatus.Image = Properties.Resources.SyncBusy));

                        IEnumerable<RemoteFileInfo> _fileInfos = _session.EnumerateRemoteFiles(_secret.Decrypt(server.Local.Find), null, EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories);

                        foreach (RemoteFileInfo _fileInfo in _fileInfos)
                        {
                            DirectoryStructure _dirStructure = new DirectoryStructure
                            {
                                FilePermissions = _fileInfo.FilePermissions.ToString(),
                                FileType = _fileInfo.FileType,
                                FullName = _fileInfo.FullName,
                                Group = _fileInfo.Group,
                                IsDirectory = _fileInfo.IsDirectory,
                                IsParentDirectory = _fileInfo.IsParentDirectory,
                                IsThisDirectory = _fileInfo.IsThisDirectory,
                                LastWriteTime = _fileInfo.LastWriteTime,
                                Length = _fileInfo.Length,
                                Length32 = _fileInfo.Length32,
                                Name = _fileInfo.Name,
                                Owner = _fileInfo.Owner
                            };

                            _pathList.Add(new FullPath { PathStructure = _dirStructure });

                            lblNotifications.Invoke(new Action(() => lblNotifications.Text = "L - Obteniendo... " + _fileInfo.Name));
                        }

                        string _data = JsonConvert.SerializeObject(_pathList, Formatting.Indented);

                        File.WriteAllText(_localPathList, _data);
                    }

                    return "Se creo el directorio local exitosamente";
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x002)", "Error al obtener directorios", 3);

                _notify.Show();

                return null;
            }
        }

        /// <summary>
        /// Metodo asincrono que permite obtener el directorio remoto
        /// </summary>
        private async void GetRemoteLists()//Verificar no halla ajustes vacios --
        {
            try
            {
                Servers _servers = _kernel.ReadSettings();
                
                if(_servers.Remote.Server != "")
                {
                    if (CheckInternet())
                    {
                        _notify = new Notify("Espere por favor...", "Obteniendo directorio del servidor remoto", 1);

                        _notify.Show();

                        lblNotifications.Text = "Obteniendo directorio remoto... espere por favor";

                        lblNotifications.Refresh();
                        Task<string> _remote = new Task<string>(() => ProcessingRemote(_servers));
                        _remote.Start();

                        if (await _remote == "Se creo el directorio remoto exitosamente")
                        {
                            lblNotifications.Text = "Se creo el directorio remoto exitosamente";
                            petStatus.Image = Properties.Resources.SyncOK;
                            _bridge.CreateMovement(new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Directorio", From = _secret.Decrypt(_servers.Remote.Server), To = "Equipo local" });
                        }

                        else
                        {
                            VisibleButtons(true);

                            _notify = new Notify("Error (0x003)", "Error al generar directorios", 3);

                            _notify.Show();
                        }
                    }

                    else
                    {
                        DialogResult f = XtraMessageBox.Show(UserLookAndFeel.Default, "No cuenta con conexión a internet, pero puede intentar en caso de que\n el servidor \"Remoto\" este conectado de forma local.\n\n¿Desea continuar?", "SyncFTP - Sin conexión a internet", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (f == DialogResult.Yes)
                        {
                            _notify = new Notify("Espere por favor...", "Obteniendo directorio del servidor remoto", 1);

                            _notify.Show();

                            lblNotifications.Text = "Obteniendo directorio remoto... espere por favor";

                            lblNotifications.Refresh();
                            Task<string> _remote = new Task<string>(() => ProcessingRemote(_servers));
                            _remote.Start();

                            if (await _remote == "Se creo el directorio remoto exitosamente")
                            {
                                lblNotifications.Text = "Se creo el directorio remoto exitosamente";
                                petStatus.Image = Properties.Resources.SyncOK;

                                _bridge.CreateMovement(new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Directorio", From = _secret.Decrypt(_servers.Remote.Server), To = "Equipo local" });
                            }

                            else
                            {
                                VisibleButtons(true);

                                _notify = new Notify("Error (0x003)", "Error al generar directorios", 3);

                                _notify.Show();
                            }
                        }
                    }
                }

                else
                {
                    _notify = new Notify("Sin configuración", "Faltan datos de conexión remota", 2);

                    _notify.Show();
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x003)", "Error al generar directorios", 3);

                _notify.Show();
            }
        }

        /// <summary>
        /// Metodo asincrono que permite obtener el directorio local
        /// </summary>
        private async void GetLocalLists()//Verificar no halla ajustes vacios --
        {
            try
            {
                Servers _servers = _kernel.ReadSettings();

                if (_servers.Local.Server != "")
                {
                    if (!CheckInternet())
                    {
                        _notify = new Notify("Espere por favor...", "Obteniendo directorio del servidor local", 1);

                        _notify.Show();

                        lblNotifications.Text = "Obteniendo directorio local... espere por favor";

                        lblNotifications.Refresh();
                        Task<string> _local = new Task<string>(() => ProcessingLocal(_servers));
                        _local.Start();

                        if (await _local == "Se creo el directorio local exitosamente")
                        {
                            lblNotifications.Text = "Se creo el directorio local exitosamente";

                            _notify = new Notify("Éxito", "Directorio del servidor local generado", 1);

                            _notify.Show();

                            petStatus.Image = Properties.Resources.SyncOK;

                            _bridge.CreateMovement(new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Directorio", From = _secret.Decrypt(_servers.Local.Server), To = "Equipo local" });
                        }

                        else
                        {
                            VisibleButtons(true);

                            _notify = new Notify("Error (0x004)", "Error al generar directorios", 3);

                            _notify.Show();
                        }
                    }

                    else
                    {
                        DialogResult f = XtraMessageBox.Show(UserLookAndFeel.Default, "Al estar conectado a internet SyncFTP da preferencia a conexiones con el servidor \"Remoto\",\ncancelando acciones con el servidor \"Local\", pero aún así puede acceder a él, si este está accesible.\n\n¿Desea continuar?", "SyncFTP - Confirme acción", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (f == DialogResult.Yes)
                        {
                            _notify = new Notify("Espere por favor...", "Obteniendo directorio del servidor local", 1);

                            _notify.Show();

                            lblNotifications.Text = "Obteniendo directorio local... espere por favor";

                            lblNotifications.Refresh();
                            Task<string> _local = new Task<string>(() => ProcessingLocal(_servers));
                            _local.Start();

                            if (await _local == "Se creo el directorio local exitosamente")
                            {
                                lblNotifications.Text = "Se creo el directorio local exitosamente";

                                _notify = new Notify("Éxito", "Directorio del servidor local generado", 1);

                                _notify.Show();

                                petStatus.Image = Properties.Resources.SyncOK;

                                _bridge.CreateMovement(new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Directorio", From = _secret.Decrypt(_servers.Local.Server), To = "Equipo local" });
                            }

                            else
                            {
                                VisibleButtons(true);

                                _notify = new Notify("Error (0x004)", "Error al generar directorios", 3);

                                _notify.Show();
                            }
                        }
                    }
                }

                else
                {
                    _notify = new Notify("Sin configuración", "Faltan datos de conexión local", 2);

                    _notify.Show();
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x004)", "Error al generar directorios", 3);

                _notify.Show();
            }
        }

        /// <summary>
        /// Comienza a sincronizar los archivos del servidor remoto al equipo
        /// </summary>
        /// <param name="server">Datos de los servidores donde evalua el remoto</param>
        /// <returns>Devuelve una cadena de texto segun se halla completado la sincronizacion o no</returns>
        private string SynchronizeRemoteData(Servers server)
        {
            try
            {
                SessionOptions _client = _kernel.RemoteIsValid(server);

                if (_client != null)
                {
                    if (!Directory.Exists(_remoteFilesDirectory))
                    {
                        Directory.CreateDirectory(_remoteFilesDirectory);
                    }

                    using (Session _session = new Session())
                    {
                        _session.FileTransferred += FilesTransferred;

                        _session.FileTransferProgress += FileTransferProgress;

                        petStatus.Invoke(new Action(() => petStatus.Image = Properties.Resources.SyncBusy));
                        
                        _session.Open(_client);
                        
                        SynchronizationResult _syncResult;
                        
                        _syncResult = _session.SynchronizeDirectories(SynchronizationMode.Local, _remoteFilesDirectory, _secret.Decrypt(server.Remote.Find), false, false, SynchronizationCriteria.Either);

                        _syncResult.Check();
                    }

                    return "Sincronización con el servidor remoto completada";
                }
                else
                {
                    return "Servidor remoto no válido";
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x005)", "Error al sincronizar", 3);

                _notify.Show();

                return null;
            }
        }

        /// <summary>
        /// Inicializa el proceso de sincronizacion desde el servidor remoto al equipo local
        /// </summary>
        private async void BeginToSyncRemote()//Verificar no halla ajustes vacios --
        {
            try
            {
                Servers _servers = _kernel.ReadSettings();

                if (_servers.Remote.Server != "")
                {
                    if (string.IsNullOrEmpty(_servers.Remote.Server))
                    {
                        lblNotifications.Text = "Aún no hay datos del servidor remoto";

                        _notify = new Notify("Sin datos...", "Aún no hay datos del servidor remoto", 2);

                        _notify.Show();
                    }

                    else
                    {
                        VisibleButtons(false);

                        _notify = new Notify("Iniciando conexión", "Sincronizando con servidor remoto", 1);

                        _notify.Show();

                        lblNotifications.Text = "Iniciando sincronización remota...";

                        lblNotifications.Refresh();
                        petStatus.Image = Properties.Resources.SyncBusy;
                        Task<string> _beginSync = new Task<string>(() => SynchronizeRemoteData(_servers));
                        _beginSync.Start();

                        if (await _beginSync == "Sincronización con el servidor remoto completada")
                        {
                            _notify = new Notify("Éxito", "Sincronización con servidor remoto completada", 0);

                            _notify.Show();

                            petStatus.Image = Properties.Resources.SyncOK;

                            VisibleButtons(true);

                            lblNotifications.Text = "Los archivos se sincronizaron con el servidor remoto. (Acción finalizada)";

                            string _copyTo = "";

                            _bridge.CreateMovement( new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Descarga", From = _secret.Decrypt(_servers.Remote.Server), To = "Equipo local" });

                            DialogResult _answer = XtraMessageBox.Show(UserLookAndFeel.Default, "¿Desea pasar los archivos sincronizados a un dispositivo extraíble?", "SyncFTP - ¿Copiar archivos?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                            if (_answer == DialogResult.Yes)
                            {
                                if (fbdFolderSelect.ShowDialog() == DialogResult.OK)
                                {
                                    _copyTo = fbdFolderSelect.SelectedPath + @"\RemoteFiles";

                                    if (!string.IsNullOrEmpty(_copyTo) && Directory.Exists(_remoteFilesDirectory))
                                    {
                                        Task<bool> _beginToCopy = new Task<bool>(() => CopyDirectory(_remoteFilesDirectory, _copyTo, true));
                                        _beginToCopy.Start();

                                        if (await _beginToCopy)
                                        {
                                            if (XtraMessageBox.Show(UserLookAndFeel.Default, "¡Listo!, ahora los archivos están donde indicaste.\n¿Desea cerrar el programa?", "SyncFTP - Copia de archivos terminada", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                            {
                                                Application.Exit();
                                            }
                                        }

                                        else
                                        {
                                            if (XtraMessageBox.Show(UserLookAndFeel.Default, "Ocurrió un error al copiar los archivos automáticamente.\nPor favor copie todos los archivos manualmente.\n¿Desea abrir la ventana con los archivos?", "SyncFTP - Error al copiar archivos", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                            {
                                                Process.Start("explorer", _remoteFilesDirectory);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        else
                        {
                            VisibleButtons(true);

                            _notify = new Notify("Verficando ajustes remotos", "Reintente sincronización", 2);

                            _notify.Show();
                        }
                    }
                }

                else
                {
                    _notify = new Notify("Sin configuración", "Faltan datos de conexión remota", 2);

                    _notify.Show();
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x006)", "Error al sincronizar", 3);

                _notify.Show();
            }
        }

        /// <summary>
        /// Comienza a sincronizar los archivos del equipo local al servidor local
        /// </summary>
        /// <param name="server">Datos de los servidores donde evalua el local</param>
        /// <returns>Devuelve una cadena de texto segun se halla completado la sincronizacion o no</returns>
        private string SynchronizeLocalData(Servers server)
        {
            try
            {
                SessionOptions _client = _kernel.LocalIsValid(server);

                if (_client != null)
                {
                    if (!Directory.Exists(_remoteFilesDirectory))
                    {
                        Directory.CreateDirectory(_remoteFilesDirectory);
                    }

                    using (Session _session = new Session())
                    {
                        _session.FileTransferred += FilesTransferred;

                        _session.FileTransferProgress += FileTransferProgress;

                        _session.Open(_client);

                        petStatus.Invoke(new Action(() => petStatus.Image = Properties.Resources.SyncBusy));
                        
                        SynchronizationResult _syncResult;
                        _syncResult = _session.SynchronizeDirectories(SynchronizationMode.Remote, _remoteFilesDirectory, _secret.Decrypt(server.Local.Find), false, false, SynchronizationCriteria.Either);
                        
                        _syncResult.Check();
                    }

                    return "Sincronización con servidor local completada";
                }
                else
                {
                    return "Servidor local no válido";
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x007)", "Error al sincronizar", 3);

                _notify.Show();

                return null;
            }
        }

        /// <summary>
        /// Inicializa el proceso de sincronizacion desde el equipo local al servidor local
        /// </summary>
        private async void BeginToSyncLocal()//Verificar no halla ajustes vacios --
        {
            try
            {
                Servers _servers = _kernel.ReadSettings();

                if (_servers.Local.Server != "")
                {

                    if (string.IsNullOrEmpty(_servers.Local.Server))
                    {
                        lblNotifications.Text = "Aún no hay datos del servidor local";

                        _notify = new Notify("Sin datos...", "Aún no hay datos del servidor local", 2);

                        _notify.Show();
                    }

                    else
                    {
                        VisibleButtons(false);

                        _notify = new Notify("Iniciando conexión...", "Sincronizando con servidor local", 1);

                        _notify.Show();

                        lblNotifications.Text = "Iniciando sincronización local...";

                        lblNotifications.Refresh();
                        Task<string> _beginSync = new Task<string>(() => SynchronizeLocalData(_servers));
                        _beginSync.Start();

                        if (await _beginSync == "Sincronización con servidor local completada")
                        {
                            VisibleButtons(true);

                            petStatus.Image = Properties.Resources.SyncOK;

                            lblNotifications.Text = "El servidor local a sido sincronizado";

                            _notify = new Notify("Éxito", "Sincronización con servidor local completada", 0);

                            _notify.Show();

                            _bridge.CreateMovement(new Movements { Machine = Environment.UserName, SO = Environment.OSVersion.ToString(), Date = DateTime.Now, Type = "Carga", From = "Equipo local", To = _secret.Decrypt(_servers.Local.Server) });
                        }

                        else
                        {
                            VisibleButtons(true);

                            _notify = new Notify("Verficando ajustes locales", "Reintente sincronización", 2);

                            _notify.Show();
                        }
                    }
                }

                else
                {
                    _notify = new Notify("Sin configuración", "Faltan datos de conexión local", 2);

                    _notify.Show();
                }
            }
            catch (Exception)
            {
                VisibleButtons(true);

                _notify = new Notify("Error (0x008)", "Error al sincronizar", 3);

                _notify.Show();
            }
        }

        /// <summary>
        /// Indica el tipo de movimiento o ajuste que se realiza al archivo con el que actualmente se esta trabajando
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesTransferred(object sender, TransferEventArgs e)
        {
            if (e.Error == null)
            {
                lblNotifications.Invoke(new Action(() => lblNotifications.Text = e.FileName + " cargado exitosamente"));
            }
            else
            {
                lblNotifications.Invoke(new Action(() => lblNotifications.Text = "Archivo: " + e.FileName + " error al cargar. Error: " + e.Error.ToString()));
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    lblNotifications.Invoke(new Action(() => lblNotifications.Text = "Archivo: " + e.Chmod.FileName + ", permisos modificados: " + e.Chmod.FilePermissions));
                }
                else
                {
                    lblNotifications.Invoke(new Action(() => lblNotifications.Text = "Falló el ajuste de permisos: " + e.Chmod.FilePermissions + ", Archivo: " + e.Chmod.FileName));
                }
            }
            else
            {
                lblNotifications.Invoke(new Action(() => lblNotifications.Text = "No se modificaron permisos de: " + e.Destination));
            }

            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {
                    lblNotifications.Invoke(new Action(() => lblNotifications.Text = "La fecha de: " + e.Touch.FileName + ", pasó a: " + e.Touch.LastWriteTime.ToString()));
                }
                else
                {
                    lblNotifications.Invoke(new Action(() => lblNotifications.Text = "Error: " + e.Touch.Error.ToString() + ", al ajustar fecha de: " + e.Touch.FileName.ToString()));
                }
            }
            else
            {
                lblNotifications.Invoke(new Action(() => lblNotifications.Text = "La fecha de: " + e.FileName.ToString() + ", no cambiará"));
            }
        }

        /// <summary>
        /// Muestra el progreso de las transferencias compatibles el la barra de progreso
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
            if (pbcProgress.Position > 0)
            {
                pbcProgress.Invoke(new Action(() => pbcProgress.Position = 0));
            }

            pbcProgress.Invoke(new Action(() => pbcProgress.Increment(Convert.ToInt32(e.FileProgress * 100))));
        }

        /// <summary>
        /// Permite copiar archivos dentro del mismo equipo local, con todo y subdirectorios
        /// </summary>
        /// <param name="sourceDirName">Direccion logica desde donde se copiaran los archivos</param>
        /// <param name="destDirName">Direccion logica a donde se copiaran los archivos</param>
        /// <param name="copySubDirs">True para copiar tambien todo y subdirectorios</param>
        /// <returns></returns>
        private bool CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "No se encontró la carpeta de origen de datos: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                FileInfo[] files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, true);
                }

                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        CopyDirectory(subdir.FullName, temppath, copySubDirs);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Metodo asincrono que permite la subida de archivos desde un pendrive al servidor local
        /// </summary>
        private async void UploadFromExternal()
        {
            try
            {
                if (XtraMessageBox.Show(UserLookAndFeel.Default, "¿Desea sincronizar los archivos desde un medio extraíble?\n\nSi es así, asegurese de conectar ahora el dispositivo", "SyncFTP - Medios extraíbles", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (fbdFolderSelect.ShowDialog() == DialogResult.OK)
                    {
                        Task<bool> _beginToCopy = new Task<bool>(() => CopyDirectory(fbdFolderSelect.SelectedPath, _remoteFilesDirectory, true));
                        _beginToCopy.Start();

                        if (await _beginToCopy)
                        {
                            XtraMessageBox.Show(UserLookAndFeel.Default, "Si lo desea, ya puede extraer el dispositivo, ahora empezaremos a subir los archivos.", "SyncFTP - Transferencia asegurada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            if (!CheckInternet())
                            {
                                BeginToSyncLocal();
                            }

                            else
                            {
                                DialogResult f = XtraMessageBox.Show(UserLookAndFeel.Default, "Parece que esta conectado a internet, se intentará acceder al servidor \"Local\".\n\n¿Desea continuar y transferir archivos?", "SyncFTP - Confirme acción", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                                if (f == DialogResult.Yes)
                                {
                                    BeginToSyncLocal();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _notify = new Notify("Error (0x009)", "Error al cargar datos externos", 3);

                _notify.Show();
            }
        }
        
        /// <summary>
        /// Inicia el metodo asincrono de copia de archivos desde un dispositivo extraible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petFromDrive_Click(object sender, EventArgs e)
        {
            try
            {
                UploadFromExternal();
            }
            catch (Exception)
            {
                _notify = new Notify("Error (0x010)", "Error al iniciar carga", 3);

                _notify.Show();
            }
        }
        
        /// <summary>
        /// Funcion sorpresa al usuario que permite cambiar de forma aleatoria la visualizacion de SyncFTP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    Random _random = new Random();

                    switch (_random.Next(0, 5))
                    {
                        case 1:
                            dlafSkins.LookAndFeel.SkinName = "Springtime";
                            break;

                        case 2:
                            dlafSkins.LookAndFeel.SkinName = "Summer 2008";
                            break;

                        case 3:
                            dlafSkins.LookAndFeel.SkinName = "Xmas 2008 Blue";
                            break;

                        case 4:
                            dlafSkins.LookAndFeel.SkinName = "Darkroom";
                            break;

                        case 5:
                            dlafSkins.LookAndFeel.SkinName = "Foggy";
                            break;

                        default:
                            dlafSkins.LookAndFeel.SkinName = "Office 2016 Colorful";
                            break;
                    }
                }
            }
            catch (Exception)
            {
                _notify = new Notify("Error (0x00T)", "Error al ajustar tema", 3);

                _notify.Show();
            }
        }
        
        /// <summary>
        /// Abre la carpeta donde se almacenan todos los archivos del servidor remoto en el explorador de windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void petRemoteFiles_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer", _remoteFilesDirectory);
            }
            catch (Exception)
            {
                _notify = new Notify("Error (0x010)", "Error al abrir ubicación", 3);

                _notify.Show();
            }
        }

        /// <summary>
        /// Temporizador que mide el tiempo que lleva ejecutandose una accion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _tmpElapsedTime_Tick(object sender, EventArgs e)
        {
            lblElapsedTime.Refresh();
            lblElapsedTime.Text = "Tiempo en ejecución: " + _seconds.ToString() + " segundos.";
            _seconds++;
        }

        #endregion

        #region Visual Effects
        
        private void petSettings_MouseEnter(object sender, EventArgs e)
        {
            petSettings.Image = Properties.Resources.SetttingHover;
        }

        private void petSettings_MouseLeave(object sender, EventArgs e)
        {
            petSettings.Image = Properties.Resources.Settting;
        }

        private void petRemoteFiles_MouseEnter(object sender, EventArgs e)
        {
            petRemoteFiles.Image = Properties.Resources.FolderHover;
        }

        private void petRemoteFiles_MouseLeave(object sender, EventArgs e)
        {
            petRemoteFiles.Image = Properties.Resources.Folder;
        }

        private void petInfo_MouseEnter(object sender, EventArgs e)
        {
            petInfo.Image = Properties.Resources.ManualHover;
        }

        private void petInfo_MouseLeave(object sender, EventArgs e)
        {
            petInfo.Image = Properties.Resources.Manual;
        }

        private void petFromDrive_MouseEnter(object sender, EventArgs e)
        {
            petFromDrive.Image = Properties.Resources.USBHover;
        }

        private void petFromDrive_MouseLeave(object sender, EventArgs e)
        {
            petFromDrive.Image = Properties.Resources.USB;
        }

        private void petTransactions_MouseEnter(object sender, EventArgs e)
        {
            petTransactions.Image = Properties.Resources.TransactionsHover;
        }

        private void petTransactions_MouseLeave(object sender, EventArgs e)
        {
            petTransactions.Image = Properties.Resources.Transaction;
        }

        #endregion
    }
}