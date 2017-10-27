using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SyncFTP.Views
{
    public partial class Notify : XtraForm
    {
        /// <summary>
        /// Constructor que inicializa el formulario, recibe tres parametros obligatorios para designar el tipo de notificacion
        /// </summary>
        /// <param name="title">TItulo para la notificacion</param>
        /// <param name="message">Mensaje o cuerpo de la notificacion</param>
        /// <param name="icon">0 = OK, 1 = Info, 2 = Warning, 3 = Error</param>
        public Notify(string title, string message, int icon)
        {
            InitializeComponent();

            KindOfNotification(title, message, icon);

            NewPosition();

            Behavior();
        }

        /// <summary>
        /// Sobrecarga del metodo para evitar el foco en el formulario
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// Metodo que arma el cuadro de notificacion
        /// </summary>
        /// <param name="title">TItulo para la notificacion</param>
        /// <param name="message">Mensaje o cuerpo de la notificacion</param>
        /// <param name="icon">0 = OK, 1 = Info, 2 = Warning, 3 = Error</param>
        private void KindOfNotification(string title, string message, int icon)
        {
            switch (icon)
            {
                case 0:
                    petIcon.Image = Properties.Resources.OkIcon;
                    BackColor = Color.Green;
                    break;

                case 1:
                    petIcon.Image = Properties.Resources.InfoIcon;
                    BackColor = Color.CadetBlue;
                    break;

                case 2:
                    petIcon.Image = Properties.Resources.WarningIcon;
                    BackColor = Color.Gold;
                    break;

                case 3:
                    petIcon.Image = Properties.Resources.ErrorIcon;
                    BackColor = Color.OrangeRed;
                    break;

                default:
                    petIcon.Image = Properties.Resources.InfoIcon;
                    BackColor = Color.CadetBlue;
                    break;
            }

            lblTitle.Text = title;

            lblMessage.Text = message;
        }

        /// <summary>
        /// Indica la ubicacion en donde debe mostrarse la notificacion
        /// </summary>
        private void NewPosition()
        {
            Point _original = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Location = new Point(_original.X - 370, _original.Y - 125);
        }

        /// <summary>
        /// Habilita el tiempo que tendrá visible la notificacion
        /// </summary>
        private void Behavior()
        {
            tmpShowTime.Enabled = true;

            BringToFront();
        }

        /// <summary>
        /// Se cierra la notificacion transcurrido el tiempo indicado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmpShowTime_Tick(object sender, EventArgs e)
        {
            tmpShowTime.Enabled = false;

            Close();
        }

        /// <summary>
        /// Cierra la notificacion al hacer clic sobre la notificacion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Notify_Click(object sender, EventArgs e)
        {
            tmpShowTime.Enabled = false;

            Close();
        }
    }
}