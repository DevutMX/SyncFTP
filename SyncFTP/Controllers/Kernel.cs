using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;
using WinSCP;

namespace SyncFTP.Controllers
{
    /// <summary>
    /// Nucleo principal donde se guardan y leen los ajustes, ademas de verificar acceso a los servidores
    /// </summary>
    class Kernel
    {
        /// <summary>
        /// Variable que indica la direccion logica donde se debe alojar el archivo de las configuraciones
        /// </summary>
        private string _configPath = Application.StartupPath + @"\Settings\config.json";
        /// <summary>
        /// Variable que indica la direccion logica donde se debe alojar el archivo de log remoto
        /// </summary>
        private string _rLogPath = Application.StartupPath + @"\Settings\RemoteLog.txt";
        /// <summary>
        /// Variable que indica la direccion logica donde se debe alojar el archivo de log local
        /// </summary>
        private string _lLogPath = Application.StartupPath + @"\Settings\LocalLog.txt";

        /// <summary>
        /// Objeto a nivel global que invoca los metodos de la clase de cifrado
        /// </summary>
        Secret _secret = new Secret();

        /// <summary>
        /// Metodo que prueba las conexiones, las rechaza y/o aprueba, en caso de ser correctas, las almacena en un archivo JSON
        /// </summary>
        /// <param name="localServer"></param>
        /// <param name="remoteServer"></param>
        /// <returns>Si los ajustes son validos se retornara true</returns>
        public bool TestAndSaveSettings(Local localServer, Remote remoteServer)
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    using (File.Create(_configPath)) { }
                }

                string _textData = File.ReadAllText(_configPath);

                Servers _servers = new Servers {Local = localServer, Remote = remoteServer};

                if (_textData == null || string.IsNullOrWhiteSpace(_textData))
                {
                    string _data = JsonConvert.SerializeObject(_servers, Formatting.Indented);

                    File.WriteAllText(_configPath, _data);
                }

                else
                {
                    var _tempServer = JsonConvert.DeserializeObject<Servers>(_textData);
                    
                    if(_tempServer.Local.Server == "")
                    {
                        if (localServer.Server != "")
                        {
                            _tempServer.Local = localServer;

                            string _data = JsonConvert.SerializeObject(_tempServer, Formatting.Indented);

                            File.WriteAllText(_configPath, _data);
                        }
                    }

                    else
                    {
                        if (localServer.Server != "")
                        {
                            _tempServer.Local = localServer;

                            string _data = JsonConvert.SerializeObject(_tempServer, Formatting.Indented);

                            File.WriteAllText(_configPath, _data);
                        }
                    }

                    if(_tempServer.Remote.Server == "")
                    {
                        if(remoteServer.Server != "")
                        {
                            _tempServer.Remote = remoteServer;

                            string _data = JsonConvert.SerializeObject(_tempServer, Formatting.Indented);

                            File.WriteAllText(_configPath, _data);
                        }
                    }

                    else
                    {
                        if (remoteServer.Server != "")
                        {
                            _tempServer.Remote = remoteServer;

                            string _data = JsonConvert.SerializeObject(_tempServer, Formatting.Indented);

                            File.WriteAllText(_configPath, _data);
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                //XtraMessageBox.Show(UserLookAndFeel.Default, "Ocurrió un error al generar el archivo de configuraciones.\n\nError:\n" + ex.ToString(), "Error inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// Metodo que permite ubicar el archivo JSON y obtener los ajustes de los servidores
        /// </summary>
        /// <returns>Devuelve una estructura de clase Servers que contiene informacion de los servidores</returns>
        public Servers ReadSettings()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    using (File.Create(_configPath)) { }

                    return null;
                }

                string _data = File.ReadAllText(_configPath);

                Servers _settings = JsonConvert.DeserializeObject<Servers>(_data);
                
                return _settings;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Verifica la conexion al servidor remoto, obteniendo el directorio indicado
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Devuelve un cliente de conexion FTP para realizar procesos</returns>
        public SessionOptions RemoteIsValid(Servers server)
        {
            try
            {
                SessionOptions _client = new SessionOptions();

                int _port = Convert.ToInt32(_secret.Decrypt(server.Remote.Port));

                if(_port <= 0)
                {
                    _port = 0;
                }

                string _encryption = _secret.Decrypt(server.Remote.FTPMode);

                _client.Protocol = Protocol.Ftp;
                _client.HostName = _secret.Decrypt(server.Remote.Server);
                _client.UserName = _secret.Decrypt(server.Remote.User);
                _client.Password = _secret.Decrypt(server.Remote.Password);
                _client.PortNumber = _port;
                _client.TimeoutInMilliseconds = 20000;

                //Encriptación Implicita
                if (_encryption == "1")
                {
                    _client.FtpSecure = FtpSecure.Implicit;
                }

                //Encriptacion Explicita
                if (_encryption == "2")
                {
                    _client.FtpSecure = FtpSecure.Explicit;
                }

                //Se tira la seguridad y se acepta cualquier certificado
                if (_secret.Decrypt(server.Remote.WithCert) == "True")
                {
                    _client.GiveUpSecurityAndAcceptAnyTlsHostCertificate = true;
                }

                //Modificar a servidor FTP Activo
                if (_secret.Decrypt(server.Remote.IsActive) == "True")
                {
                    _client.FtpMode = FtpMode.Active;
                }

                _client.AddRawSettings("ProxyPort", "0");
                
                if (!File.Exists(_rLogPath))
                {
                    using (File.Create(_rLogPath))
                    {

                    }
                }

                using (Session _session = new Session())
                {
                    _session.DebugLogLevel = 0;

                    _session.DebugLogPath = _rLogPath;

                    _session.Open(_client);

                    RemoteDirectoryInfo _directoryInfo = _session.ListDirectory(_secret.Decrypt(server.Remote.Find));

                    return _directoryInfo != null ? _client : null;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());

                return null;
            }
        }

        /// <summary>
        /// Verifica la conexion al servidor local, obteniendo el directorio indicado
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Devuelve un cliente de conexion FTP para realizar procesos</returns>
        public SessionOptions LocalIsValid(Servers server)
        {
            try
            {
                SessionOptions _client = new SessionOptions();

                int _port = Convert.ToInt32(_secret.Decrypt(server.Local.Port));

                if (_port <= 0)
                {
                    _port = 0;
                }

                string _encryption = _secret.Decrypt(server.Local.FTPMode);

                _client.Protocol = Protocol.Ftp;
                _client.HostName = _secret.Decrypt(server.Local.Server);
                _client.UserName = _secret.Decrypt(server.Local.User);
                _client.Password = _secret.Decrypt(server.Local.Password);
                _client.PortNumber = _port;
                _client.TimeoutInMilliseconds = 20000;

                if (_encryption == "1")
                {
                    _client.FtpSecure = FtpSecure.Implicit;
                }

                if (_encryption == "2")
                {
                    _client.FtpSecure = FtpSecure.Explicit;
                }

                if (_secret.Decrypt(server.Local.WithCert) == "True")
                {
                    _client.GiveUpSecurityAndAcceptAnyTlsHostCertificate = true;
                }

                if (_secret.Decrypt(server.Local.IsActive) == "True")
                {
                    _client.FtpMode = FtpMode.Active;
                }

                _client.AddRawSettings("ProxyPort", "0");

                if (!File.Exists(_lLogPath))
                {
                    using (File.Create(_lLogPath))
                    {

                    }
                }

                using (Session _session = new Session())
                {
                    _session.DebugLogLevel = 0;

                    _session.DebugLogPath = _lLogPath;

                    _session.Open(_client);

                    RemoteDirectoryInfo _directoryInfo = _session.ListDirectory(_secret.Decrypt(server.Local.Find));

                    return _directoryInfo != null ? _client : null;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());

                return null;
            }
        }
    }
}
