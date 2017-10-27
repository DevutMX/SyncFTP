using DevExpress.XtraGrid;
using SyncFTP.Controllers;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace SyncFTP.Models
{
    class SyncFTPRepository
    {
        private readonly string _connectionString = "Data Source = " + Application.StartupPath + @"\Registry.db; Version = 3;";

        SQLiteConnection _connection;
        SQLiteCommand _command;
        SQLiteDataReader _dataReader;
        SQLiteDataAdapter _dataAdapter;
        DataTable _table;

        /// <summary>
        /// Registra un movimiento en la base de datos
        /// </summary>
        /// <param name="movement">Objeto que contiene todos los datos a almacenar en la BD</param>
        /// <returns>True cuando el registro fue exitoso</returns>
        protected internal bool CreateMovement(Movements movement)
        {
            try
            {
                using (_connection = new SQLiteConnection(_connectionString))
                {
                    string _query = "INSERT INTO Registros VALUES( null, @a, @b, @c, @d, @e, @f);";

                    _connection.Open();

                    _command = new SQLiteCommand(_query, _connection);

                    _command.Parameters.AddWithValue("@a", movement.Machine);
                    _command.Parameters.AddWithValue("@b", movement.SO);
                    _command.Parameters.AddWithValue("@c", movement.Date);
                    _command.Parameters.AddWithValue("@d", movement.Type);
                    _command.Parameters.AddWithValue("@e", movement.From);
                    _command.Parameters.AddWithValue("@f", movement.To);

                    int _response = _command.ExecuteNonQuery();

                    return _response > 0 ? true : false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Llena un GridControl con informacion de la base de datos
        /// </summary>
        /// <param name="_toFill">GridControl donde se mostrarán los datos</param>
        protected internal void MovementsToList(GridControl _toFill)
        {
            try
            {
                using (_connection = new SQLiteConnection(_connectionString))
                {
                    string _query = "SELECT * FROM Registros;";

                    _connection.Open();

                    _command = new SQLiteCommand(_query, _connection);

                    _table = new DataTable();

                    _dataAdapter = new SQLiteDataAdapter(_command);

                    _dataAdapter.Fill(_table);

                    _toFill.DataSource = _table;
                }
            }
            catch (Exception)
            {

            }
        }

    }
}
