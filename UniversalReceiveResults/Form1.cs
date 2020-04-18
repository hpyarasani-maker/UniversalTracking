using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace UniversalReceiveResults
{
    public partial class Form1 : Form
    {

        HTMLParserNewTask WOWS = new HTMLParserNewTask();

        Timer timer = new Timer();
        public Form1()
        {
            InitializeComponent();
            WOWS.OnKeywordDone += WOWS_OnKeywordDone;
        }
        void timerExit()
        {
            timer.Interval = 110 * 60000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Environment.Exit(Environment.ExitCode);
        }
        int cntr = 1;
        int errors = 1;
        private void WOWS_OnKeywordDone(string value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (value.StartsWith("Error:"))
                {
                    txtErrors.Text += value + "\r\n\r\n";
                    lblErrors.Text = errors++.ToString();
                }
                else
                    lblCompletedKw.Text = value;

                lblCount.Text = cntr++.ToString();
            });
        }
        

        public string strConn()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\Callback_TrackingTrending.xml";
                // You'll need to put the correct path to your xml file here
                xml.Load(fileName);

                // Select a specific node
                XmlNode node = xml.SelectSingleNode("ConnectionString/con");
                // Get its value
                string name = node.InnerText;

                return name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Universal_Receiveing";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }

    public delegate void KeywordDone(string value);
}
