using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Receiving
{
    class Common
    {
        internal static string ReadConnection()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\ServerIP1.xml";
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

        internal static string readAPI()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\ServerIP1.xml";
                //string fileName = @"C:\Inetpub\wwwroot\ServerIP_Callback.xml";
                // You'll need to put the correct path to your xml file here
                xml.Load(fileName);
                // Select a specific node
                XmlNode node = xml.SelectSingleNode("ConnectionString/apiNew");
                // Get its value
                string name = node.InnerText;
                return name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string strConn()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\ServerIP1.xml";
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

        //internal static string ReadConnection()
        //{
        //    XmlDocument xml = new XmlDocument();
        //    string fileName = @"C:\Inetpub\wwwroot\TrendingLiveAPI.xml";
        //    // You'll need to put the correct path to your xml file here
        //    xml.Load(fileName);

        //    // Select a specific node
        //    XmlNode node = xml.SelectSingleNode("TrendingAPI/con");
        //    // Get its value
        //    string name = node.InnerText.Trim();

        //    return name;
        //}

        internal static int GetOxylabsTime()
        {
            int time = 0;
            string strQuery = "exec [dbo].[GetOxylabsTime]";
            SqlConnection objCon = new SqlConnection(ReadConnection());
            try
            {
                objCon.Open();
                SqlCommand objCmd = new SqlCommand(strQuery, objCon);
                objCmd.CommandTimeout = 0;
                SqlDataReader objData = null;
                objData = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (objData.Read())
                {
                    time = Convert.ToInt32(objData[0]);
                }
                objData.Close();
            }
            catch (SqlException e)
            {
                string errMsg = "Database Connection is temporarily not working\n" + e.ToString();
            }
            finally
            {
                if (objCon.State == ConnectionState.Open)
                {
                    objCon.Close();
                }
            }
            return time;
        }

        internal static int GetOxylabsCount()
        {
            int count = 0;
            string strQuery = "exec [dbo].[GetOxylabsCount]";
            SqlConnection objCon = new SqlConnection(ReadConnection());
            try
            {
                objCon.Open();
                SqlCommand objCmd = new SqlCommand(strQuery, objCon);
                objCmd.CommandTimeout = 0;
                SqlDataReader objData = null;
                objData = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (objData.Read())
                {
                    count = Convert.ToInt16(objData[0]);
                }
                objData.Close();
            }
            catch (SqlException e)
            {
                string errMsg = "Database Connection is temporarily not working\n" + e.ToString();
            }
            finally
            {
                if (objCon.State == ConnectionState.Open)
                {
                    objCon.Close();
                }
            }
            return count;
        }
    }
}

