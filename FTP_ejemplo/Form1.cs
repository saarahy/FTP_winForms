using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace FTP_ejemplo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Crear estructura de la configuracion FTP
        struct FTPConfig { 
            public string Servidor { get; set; }
            public string Usuario { get; set; }
            public string Contrasena { get; set; }
            public string nombreArchivo { get; set; }
            public string rutaArchivo { get; set; }

        }
        FTPConfig ftp;
        private void btnSubir_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialogo = new OpenFileDialog() 
            { Multiselect = false, ValidateNames = true, Filter = "All Files | *.*" }) 
            {
                try {
                    lblProgreso.Text = "Subiendo archivo..!";
                    if (dialogo.ShowDialog() == DialogResult.OK) {
                        FileInfo archivo = new FileInfo(dialogo.FileName);
                        ftp.Servidor = txtServidor.Text;
                        ftp.Usuario = txtUsuario.Text;
                        ftp.Contrasena = txtContrasena.Text;
                        ftp.nombreArchivo = archivo.Name;
                        ftp.rutaArchivo = archivo.FullName;
                        //Ejecutar nuestro subproceso en el background worker.
                        backgroundWorker1.RunWorkerAsync(ftp);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string nombreArchivo = ((FTPConfig)e.Argument).nombreArchivo;
            string rutaArchivo = ((FTPConfig)e.Argument).rutaArchivo;
            string usuario = ((FTPConfig)e.Argument).Usuario;
            string contrasena = ((FTPConfig)e.Argument).Contrasena;
            string servidor = ((FTPConfig)e.Argument).Servidor;

            FtpWebRequest peticion = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", servidor, nombreArchivo)));
            peticion.Method = WebRequestMethods.Ftp.UploadFile;
            peticion.Credentials = new NetworkCredential(usuario, contrasena);

            Stream ftpStream = peticion.GetRequestStream();
            FileStream envioArchivo = File.OpenRead(rutaArchivo);
            byte[] buffer = new byte[1024];
            double totalArchivo = envioArchivo.Length;
            int byteLectura = 0;
            double Lectura = 0;

            do
            {
                byteLectura = envioArchivo.Read(buffer, 0, 1024);
                ftpStream.Write(buffer, 0, byteLectura);
                Lectura += (double)byteLectura;
                double porcentaje = Lectura / totalArchivo * 100;
                backgroundWorker1.ReportProgress((int)porcentaje);

            } while (byteLectura != 0);
            MessageBox.Show("envio exitoso!");
            envioArchivo.Close();
            ftpStream.Close();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblProgreso.Text = $"Subiendo { e.ProgressPercentage} % ";
            pbProgreso.Value = e.ProgressPercentage;
            pbProgreso.Update();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblProgreso.Text = "Envio exitoso ..! ";
        }
    }
}
