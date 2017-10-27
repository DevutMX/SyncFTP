using System;

namespace SyncFTP.Controllers
{
    #region Servers Structure

    /// <summary>
    /// Estructura de los datos necesarios para almacenar temporalmente los ajustes del servidor local en archivo JSON
    /// </summary>
    public class Local
    {
        public string Server { get; set; }
        public string IsAnonymous { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Find { get; set; }
        public string FTPMode { get; set; }
        public string IsActive { get; set; }
        public string WithCert { get; set; }
    }

    /// <summary>
    /// Estructura de los datos necesarios para almacenar temporalmente los ajustes del servidor remoto en archivo JSON
    /// </summary>
    public class Remote
    {
        public string Server { get; set; }
        public string IsAnonymous { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Find { get; set; }
        public string FTPMode { get; set; }
        public string IsActive { get; set; }
        public string WithCert { get; set; }
    }

    /// <summary>
    /// Estructura que permite la consistencia de los ajustes de los servidores tanto local como remoto para almacenar en archivo JSON
    /// </summary>
    public class Servers
    {
        public Local Local { get; set; }
        public Remote Remote { get; set; }
    }

    #endregion

    #region DirectoryInfo

    /// <summary>
    /// Estructura que contiene datos de los archivos en los servidores FTP
    /// </summary>
    class DirectoryStructure
    {
        public string FilePermissions { get; set; }
        public char FileType { get; set; }
        public string FullName { get; set; }
        public string Group { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsParentDirectory { get; set; }
        public bool IsThisDirectory { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }
        public int Length32 { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
    }

    /// <summary>
    /// Estructura de consistencia de la informacion de los archivos alojados en los servidores FTP para poder almacenarla en archivos JSON
    /// </summary>
    class FullPath
    {
        public DirectoryStructure PathStructure { get; set; }
    }

    #endregion

    #region Movements

    class Movements
    {
        public int MovementId { get; private set; }
        public string Machine { get; set; }
        public string SO { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    #endregion
}
