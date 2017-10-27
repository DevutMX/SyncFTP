using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SyncFTP.Controllers
{
    /// <summary>
    /// Clase que permite metodos para cifrar o descifrar cadenas de texto
    /// </summary>
    class Secret
    {
        /// <summary>
        /// Metodo que recibe un string para proceder a encriptarlo
        /// </summary>
        /// <param name="_normalText"></param>
        /// <returns>Devuelve un string ya cifrado</returns>
        protected internal string Encrypt(string _normalText)
        {
            try
            {
                string _encryptionKey = "SolucionesDigitales";
                byte[] clearBytes = Encoding.Unicode.GetBytes(_normalText);
                using (Aes _encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes _pdb = new Rfc2898DeriveBytes(_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    _encryptor.Key = _pdb.GetBytes(32);
                    _encryptor.IV = _pdb.GetBytes(16);
                    using (MemoryStream _ms = new MemoryStream())
                    {
                        using (CryptoStream _cs = new CryptoStream(_ms, _encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            _cs.Write(clearBytes, 0, clearBytes.Length);
                            _cs.Close();
                        }
                        _normalText = Convert.ToBase64String(_ms.ToArray());
                    }
                }
                return _normalText;
            }
            catch (Exception)
            {
                return "Error al cifrar";
            }
        }

        /// <summary>
        /// Metodo que recibe un string encriptado y lo regresa a su estado original
        /// </summary>
        /// <param name="_encryptedText"></param>
        /// <returns>Devuelve un string cifrado a su estado original</returns>
        protected internal string Decrypt(string _encryptedText)
        {
            try
            {
                string _encryptionKey = "SolucionesDigitales";
                _encryptedText = _encryptedText.Replace(" ", "+");
                byte[] _cipherBytes = Convert.FromBase64String(_encryptedText);
                using (Aes _encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes _pdb = new Rfc2898DeriveBytes(_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    _encryptor.Key = _pdb.GetBytes(32);
                    _encryptor.IV = _pdb.GetBytes(16);
                    using (MemoryStream _ms = new MemoryStream())
                    {
                        using (CryptoStream _cs = new CryptoStream(_ms, _encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            _cs.Write(_cipherBytes, 0, _cipherBytes.Length);
                            _cs.Close();
                        }
                        _encryptedText = Encoding.Unicode.GetString(_ms.ToArray());
                    }
                }
                return _encryptedText;
            }
            catch (Exception)
            {
                return "Error al decifrar";
            }
        }
    }
}
