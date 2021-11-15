using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Need to access IO classes
using System.IO;

namespace TextFileViewer {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void miQuit_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void miOpen_Click(object sender, EventArgs e)
        {
            // Show the open file dialog and store the result it returns
            DialogResult drResult = ofdOpenFile.ShowDialog();
            // Only open and display the file if the user clicked open which returns a dialog result "OK"\
            if (drResult == DialogResult.OK)
            {
                DisplayText();
            }
        }

        // Function to display the text in the chosen file
        private void DisplayText()
        {
            // Clear the rich textbox
            rtbText.Clear();
            // Open the file for reading. Returns an instance of filestream which we use to read data from a file
            string strPath = ofdOpenFile.FileName;
            FileStream fsFile = File.Open(strPath, FileMode.Open, FileAccess.Read);
            // Check for BOM at the beginning of file
            checkBOM(fsFile);
            // Read data from the file one byte at a time. Translate each byte into the corresponding character
            // and display it in the rich textbox
            // Note that the read byte method returns the next byte as an integer
            // The special return value -1 indicates that we have reached the end of the file and there is no more data to read
            Encoding encUTF8 = Encoding.UTF8;
            byte[] byBuffer = new byte[1];
            int iNextByte = 0;
            // Loop through the characters in the file
            while ((iNextByte = fsFile.ReadByte()) != -1)
            {
                // Convert to a character using the UTF-8 encoding. Must pass the bytes to the encoder as an array
                byBuffer[0] = (byte)iNextByte;
                string strDecodedChar = encUTF8.GetString(byBuffer);
                // Now we have the next character. Append it to the text in the rich textbox
                rtbText.AppendText(strDecodedChar);
            }
            // Finished reading the file. Close it.-
            fsFile.Close();
        }

        // A function to check for a BOM (0xEF, 0xBB, 0xBF) at the beginning of the file and skip over it if present
        private void checkBOM(FileStream fsFile) 
        {
            // Read 3 bytes in a single read of data from the file
            // Read method returns the number of bytes actually read from the file
            byte[] byBuffer = new byte[3];
            int iBytesRead = fsFile.Read(byBuffer, 0, 3);
            // Check whether we read 3 bytes and they match the BOM bytes
            if(iBytesRead == 3 && byBuffer[0] == 0xEF && byBuffer[1] == 0xBB && byBuffer[2] == 0xBF)
            {
                // The BOM is present and we skipped over it by reading it
                return;
            }
            // There is no BOM, need to move file position pointer back to the first byte
            fsFile.Seek(0, SeekOrigin.Begin);
        }
    }
}
