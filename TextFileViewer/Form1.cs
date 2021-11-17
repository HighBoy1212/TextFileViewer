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
        // Fields
        // Keep track of path to the file whose text is currently in display
        private string strCurrentPath = null;
        // Keep track of whether displayed file has BOM
        private bool bHasBOM = false;
        // Keep trak of whether file uses CRLF for the end of a line
        private bool bLineEndCRLF = false;

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
                // Opening a new file. Save its path.
                strCurrentPath = ofdOpenFile.FileName;
                DisplayText();
                // Display the file's path in the window title bar
                this.Text = strCurrentPath;
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
            string strText = "";
            Encoding encUTF8 = Encoding.UTF8;
            byte[] byBuffer = new byte[1024];
            int iBytesRead;
            // Loop through the characters in the file 1024 bytes at a time
            while ((iBytesRead = fsFile.Read(byBuffer, 0, 1024)) > 0)
            {
                // Convert to a string of characters using the UTF-8 encoding
                string strDecodedChar = encUTF8.GetString(byBuffer, 0, iBytesRead);
                // Now we have the next string of characters. Append it to the string holding the text we have read so far
                strText += strDecodedChar;
            }
            // Finished reading the file. Close it.
            fsFile.Close();
            // Check whether the file uses CRLF for the end of a line, by searching for
            // the pair of characters \r\n (\r is carriage return, \n is new line (line feed))
            if (strText.IndexOf("\r\n") >= 0)
            {
                // CRLF appears in the text read in from the file. Save that info and 
                // convert all instances of CRLF to LF only
                bLineEndCRLF = true;
                strText = strText.Replace("\r\n", "\n");
            }
            else
            {
                bLineEndCRLF = false;
            }
            // Display the text in the rich textbox
            rtbText.Text = strText;

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
                // Save the fact that BOM was present
                bHasBOM = true;
                return;
            }
            // There is no BOM, need to move file position pointer back to the first byte
            fsFile.Seek(0, SeekOrigin.Begin);
            // Save the fact that no BOM is present
            bHasBOM = false;
        }

        // Save
        private void miSave_Click(object sender, EventArgs e)
        {
            if (strCurrentPath != null)
            {
                // Save to the current path
                SaveText(strCurrentPath);
            }
            /*else
            {
                if (sfdSaveFile.ShowDialog() == DialogResult.OK)
                {
                    // Proceed to save to the selected path
                    SaveText(sfdSaveFile.FileName);
                }
            }*/
        }

        // Save as
        private void miSaveAs_Click(object sender, EventArgs e)
        {
            // Pop up the save file dialog so the user can choose the path to save to
            // Set the initial path equal to the current path if there a valid one
            if(strCurrentPath != null)
            {
                // Seperate the directory name from the file name
                string strDirName = Path.GetDirectoryName(strCurrentPath);
                string strFileName = Path.GetFileName(strCurrentPath);
                sfdSaveFile.InitialDirectory = strDirName;
                sfdSaveFile.FileName = strFileName;
            }
            // Display the dialog and only proceed if it returns "OK"
            if (sfdSaveFile.ShowDialog() == DialogResult.OK)
            {
                // Proceed to save to the selected path
                SaveText(sfdSaveFile.FileName);
                // Store the new path we saved the text to as the current path
                strCurrentPath = sfdSaveFile.FileName;
                // Display the file's path in the window title bar
                this.Text = strCurrentPath;
            }
        }
        // Save the text currently in the rich textbox to the given file
        private void SaveText(string strSavePath)
        {
            // Open the file to save to. Create a new file if it does not already exist
            // and write over the file if one already exists
            using (FileStream fsSaveFile = File.Open(strSavePath, FileMode.Create, FileAccess.Write))
            {
                // Write a BOM if the original file contained one
                if (bHasBOM)
                {
                    // Write bytes (0xEF, 0xBB, 0xBF)
                    fsSaveFile.WriteByte(0xEF);
                    fsSaveFile.WriteByte(0xBB);
                    fsSaveFile.WriteByte(0xBF);
                }
                // Get the text from the rich textbox, convert to byte[] (using UTF-8 encoding), and write to file
                string strText = rtbText.Text;
                // If the original file used CRLF for the end of a line, we must voncert from LF to CRLF
                if (bLineEndCRLF)
                {
                    strText = strText.Replace("\n", "\r\n");
                }
                byte[] byBuffer = Encoding.UTF8.GetBytes(strText);
                fsSaveFile.Write(byBuffer, 0, byBuffer.Length);
            }
        }    
    }
}
