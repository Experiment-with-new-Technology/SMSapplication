/*
 * Created by: Syeda Anila Nusrat. 
 * Date: 1st August 2009
 * Time: 2:54 PM 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using GsmComm.GsmCommunication;
namespace SMSapplication
{
    public partial class SMSapplication : Form
    {

        #region Constructor
        public SMSapplication()
        {
            string[] name = new string[] { "" };
            string[] template_header = new string[] { "" };
            int i;

            SQLiteDatabase db;
            try
            {
                db = new SQLiteDatabase();
                DataTable recipe;
                String query = "select name \"Name\", cell_no \"Cell Number\",";
                query += "group_name \"Group Name\"";
                query += "from cellnumber;";
                recipe = db.GetDataTable(query);
                i = 0;
                foreach (DataRow r in recipe.Rows)
                {


                    name[i] = r["Name"].ToString(); i++;

                }
                DataTable recipe1;
                String query1 = "select header \"Header Name\"";
                query1 += "from template;";
                recipe1 = db.GetDataTable(query1);
                i = 0;
                foreach (DataRow r1 in recipe1.Rows)
                {


                    template_header[i] = r1["Header Name"].ToString(); i++;

                }


            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
            }

            int numOfObjects = name.Length;
            object[] name_objects = new object[numOfObjects];
            for (int j = 0; j < numOfObjects; j++)
            {
                name_objects[j] = (object)name[j];
            }

            numOfObjects = template_header.Length;
            object[] template_header_objects = new object[numOfObjects];
            for (int j = 0; j < numOfObjects; j++)
            {
                template_header_objects[j] = (object)template_header[j];
            }


            InitializeComponent(name_objects, template_header_objects);
        }
        #endregion

        #region Private Variables
        SerialPort port = new SerialPort();
        clsSMS objclsSMS = new clsSMS();
        ShortMessageCollection objShortMessageCollection = new ShortMessageCollection();
        #endregion

        #region Private Methods

        #region Write StatusBar
        private void WriteStatusBar(string status)
        {
            try
            {
                statusBar1.Text = "Message: " + status;
            }
            catch (Exception ex)
            {
                
            }
        }
        #endregion
        
        #endregion

        #region Private Events

        private void SMSapplication_Load(object sender, EventArgs e)
        {
            try
            {
                #region Display all available COM Ports
                string[] ports = SerialPort.GetPortNames();

                // Add all port names to the combo box:
                foreach (string port in ports)
                {
                    this.cboPortName.Items.Add(port);
                }
                #endregion

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);

                this.btnDisconnect.Enabled = false;
            }
            catch(Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //Open communication port 
                this.port = objclsSMS.OpenPort(this.cboPortName.Text, Convert.ToInt32(this.cboBaudRate.Text), Convert.ToInt32(this.cboDataBits.Text), Convert.ToInt32(this.txtReadTimeOut.Text), Convert.ToInt32(this.txtWriteTimeOut.Text));

                if (this.port != null)
                {
                    this.gboPortSettings.Enabled = false;

                    //MessageBox.Show("Modem is connected at PORT " + this.cboPortName.Text);
                    this.statusBar1.Text = "Modem is connected at PORT " + this.cboPortName.Text;

                    //Add tab pages
                    this.tabSMSapplication.TabPages.Add(tbSendSMS);
                    this.tabSMSapplication.TabPages.Add(tbReadSMS);
                    this.tabSMSapplication.TabPages.Add(tbDeleteSMS);

                    this.lblConnectionStatus.Text = "Connected at " + this.cboPortName.Text;
                    this.btnDisconnect.Enabled = true;
                }

                else
                {
                    //MessageBox.Show("Invalid port settings");
                    this.statusBar1.Text = "Invalid port settings";
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }

        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.gboPortSettings.Enabled = true;
                objclsSMS.ClosePort(this.port);

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);

                this.lblConnectionStatus.Text = "Not Connected";
                this.btnDisconnect.Enabled = false;

            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnSendSMS_Click(object sender, EventArgs e)
        {

            //.............................................. Send SMS ....................................................
            try
            {

                if (objclsSMS.sendMsg(this.port, this.txtSIM.Text, this.txtMessage.Text))
                {
                    //MessageBox.Show("Message has sent successfully");
                    this.statusBar1.Text = "Message has sent successfully";
                }
                else
                {
                    //MessageBox.Show("Failed to send message");
                    this.statusBar1.Text = "Failed to send message";
                }
                
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }
       

        #endregion

        #region Error Log
        public void ErrorLog(string Message)
        {
            StreamWriter sw = null;

            try
            {
                WriteStatusBar(Message);

                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                //string sPathName = @"E:\";
                string sPathName = @"SMSapplicationErrorLog_";

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                sw = new StreamWriter(sPathName + sErrorTime + ".txt", true);

                sw.WriteLine(sLogFormat + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }
            
        }
        #endregion 

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                Cursor.Current = Cursors.WaitCursor;
                string[] portnumber = new string[100];

                for (int j = 0; j < ports.Length; j++)
                {

                    portnumber[j] = ports[j];

                    int a;
                    int.TryParse(portnumber[j].Substring(3, portnumber[j].Length - 3), out a);
                    if (a != 1)
                    {
                        GsmCommMain comm = new GsmCommMain(a, 9600, 300);

                        comm.Open();
                        if (!comm.IsConnected()) { comm.Close(); continue; }
                        else
                        {
                            MessageBox.Show(this, "Successfully connected to the phone.", "Connection setup", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            comm.Close();
                            this.port = objclsSMS.OpenPort("COM" + a, Convert.ToInt32("9600"), Convert.ToInt32("8"), Convert.ToInt32("300"), Convert.ToInt32("300"));

                            if (this.port != null)
                            {
                                this.gboPortSettings.Enabled = false;

                                //MessageBox.Show("Modem is connected at PORT " + this.cboPortName.Text);
                                this.statusBar1.Text = "Modem is connected at PORT " + "COM" + a;

                                //Add tab pages
                                this.tabSMSapplication.TabPages.Add(tbSendSMS);
                                this.tabSMSapplication.TabPages.Add(tbReadSMS);
                                this.tabSMSapplication.TabPages.Add(tbDeleteSMS);

                                this.lblConnectionStatus.Text = "Connected at " + "COM" + a;
                                this.btnDisconnect.Enabled = true;
                            }

                            else
                            {
                                //MessageBox.Show("Invalid port settings");
                                this.statusBar1.Text = "Invalid port settings";
                            }

                            j = ports.Length;
                        }



                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Connection error: " + ex.Message, "Connection setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
          

        }
    
    }
}