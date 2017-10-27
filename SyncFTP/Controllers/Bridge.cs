using DevExpress.XtraGrid;
using SyncFTP.Models;

namespace SyncFTP.Controllers
{
    class Bridge
    {
        /// <summary>
        /// Objeto que permite llamar los metodos del repositorio
        /// </summary>
        SyncFTPRepository _repository = new SyncFTPRepository();

        /// <summary>
        /// Enlace que registra un movimiento en la base de datos
        /// </summary>
        /// <param name="movement">Objeto que contiene todos los datos a almacenar en la BD</param>
        /// <returns>True cuando el registro fue exitoso</returns>
        protected internal bool CreateMovement(Movements movement)
        {
            return _repository.CreateMovement(movement);
        }

        /// <summary>
        /// Enlace que llena un GridControl con informacion de la base de datos
        /// </summary>
        /// <param name="_toFill">GridControl donde se mostrarán los datos</param>
        protected internal void MovementsToList(GridControl _toFill)
        {
            _repository.MovementsToList(_toFill);
        }

    }
}
