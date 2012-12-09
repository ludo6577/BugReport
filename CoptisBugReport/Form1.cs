using AviFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace CoptisBugReport
{
    public partial class FormBugReport : Form
    {
        #region Variables

        private static String VIDEO_FILENAME = @"../videoTest.avi";

        private AviManager aviManager;
        private VideoStream aviStream;
        private Bitmap bitmap;

        private Thread threadRecorder;
        private bool threadRecording;
        private bool recorded;

        String currentScreen;

        #endregion

        #region Initialisation

        public FormBugReport()
        {
            InitializeComponent();
            ScreenChanged("Coptis");
        }

        #endregion

        #region TODO

        /*
         *  TODO : Appeller cette méthode depuis Coptis et charger les données depuis l'intranet!
         */ 
        public void ScreenChanged(String screenName)
        {
            /*
             *  TODO : Charger les données depuis l'intranet !
             *  (bugs correspondant a l'écran)
             */ 
            dataGridView.Rows.Clear();
            for (int i = 0; i < 10; i++)
            {
                dataGridView.Rows.Add(new Object[] { "Ref", "Titre", "Description", "Gravity", "Priority"});
            }
        }


        /*  
         *  TODO: Envoyer les information a l'intranet (créer un bug)
         */ 
        private void buttonSend_Click(object sender, EventArgs e)
        {
            /*  TODO: send to intranet
             *  _The video:         ../videoTest.avi
             *  _Titre:             textBoxTitre.Text
             *  _Description:       richTextBoxDescription.Text
             *  _Priority:          textBoxPriority.Text
             *  _Gravity:           comboBoxGravity.Text
             *  _Screen:            (la je sais pas)
             */
            String title = textBoxBugName.Text;
            String description = richTextBoxDescription.Text;
            String priority = textBoxPriority.Text;
            String gravity = comboBoxGravity.Text;
            //FileStream video = File.OpenRead(VIDEO_FILENAME);
            //String Screen = Variables.Screen;
            //... (autre variables utiles)

            //TODO : SEND THESE INFORMATIONS TO THE INTRANET !!!!!!!!!!!!

            //A voir on pourrais ouvrir la page correspondant au bug directement dans le navigateur par défaut
            this.Close();
        }

        #endregion

        # region Recording methods

        /*
         *  Initialise the recording attributes
         *  (Automatic call to the record method)
         */ 
        private bool startRecording(String fileName)
        {
            try
            {
                //Set the first image of the video
                int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                bitmap = new Bitmap(ScreenWidth, ScreenHeight);
                Graphics.FromImage(bitmap).DrawImage(Image.FromFile(@"../Coptis.png"), 0, 0, ScreenWidth, ScreenHeight);

                aviManager = new AviManager(fileName, false);
                aviStream = aviManager.AddVideoStream(false, 4, bitmap);
                aviStream.GetFrameOpen();

                Graphics.FromImage(bitmap).CopyFromScreen(Point.Empty, Point.Empty, Screen.PrimaryScreen.Bounds.Size);
                aviStream.AddFrame(bitmap);

                //The Recording Thread
                threadRecorder = new Thread(new ThreadStart(Record));
                threadRecording = true;
                recorded = true;
                threadRecorder.Start();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occured. Please reload the program:" + Environment.NewLine + e.Message, "Coptis bug reporter");
                return false;
            }
        }

        /*
         *  Recording method
         *  Must be called in a Thread
         */ 
        private void Record()
        {
            while (threadRecording)
            {
                try{
                    Graphics.FromImage(bitmap).CopyFromScreen(Point.Empty, Point.Empty, Screen.PrimaryScreen.Bounds.Size);
                    aviStream.AddFrame(bitmap);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error occured. Please reload the program:" + Environment.NewLine + e.Message, "Coptis bug reporter");
                }
            }
        }

        /*
         *  Stop the recording
         *  Bug if you try to record again
         */ 
        private void stopRecording()
        {
            try{
                threadRecording = false;

                aviStream.GetFrameClose();
               // aviStream.Close();
                aviManager.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occured. Please reload the program:" + Environment.NewLine + e.Message, "Coptis bug reporter");
            }
        }

        #endregion

        #region Events

        /*
         *  Check if we are allowed to send the report
         *  Check if all the textBox/comboBox are filled and we aren't recording
         */ 
        private bool CheckAllowedToSend()
        {
            if (textBoxBugName.Text != "" && richTextBoxDescription.Text != "" && textBoxPriority.Text != "" && comboBoxGravity.Text != "" && threadRecording==false && recorded==true)
            {
                buttonSend.Enabled = true;
                return true;
            }
            else
            {
                buttonSend.Enabled = false;
                return false;
            }
        }

        /*
         *  Click event on the record button
         */ 
        private void buttonRecord_Click(object sender, EventArgs e)
        {
            if(buttonRecord.Text=="Record")
            {
                if (startRecording(VIDEO_FILENAME) == true)
                    buttonRecord.Text = "Stop recording";
                else
                    buttonRecord.Text = "Record";
            }
            else if (buttonRecord.Text == "Stop recording")
            {
                buttonRecord.Text = "Record";
                stopRecording();
            }

            CheckAllowedToSend();
        }

        /*
         *  When something change (text in textBox or index in comboBox)
         *  Test if we can send the report
         */ 
        private void Changed(object sender, EventArgs e)
        {
            CheckAllowedToSend();
        }

        private void TextChanged_NumericOnly(object sender, EventArgs e)
        {
            TextBox textBox = ((TextBox)sender);

            for (int i = 0; i < textBox.Text.Length; i++)
            {
                char lastChar = textBox.Text[i];
                if (lastChar < '0' || lastChar > '9')
                {
                    textBox.Text = textBox.Text.Remove(i, 1);
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }

            Changed(sender, e);
        }

        private void FormBugReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadRecording == true)
            {
                e.Cancel = true;
                MessageBox.Show("Stop recording first", "Coptis bug reporter");
            }
            else
            {
                File.Delete(VIDEO_FILENAME);
            }
        }

        #endregion
    }
}
