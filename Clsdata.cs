using Focus.DatabaseFactory;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace PrjNMC_Inbound
{
    public class Clsdata
    {
        public static DataSet GetData(string strSQLQry, int compid)
        {
            DataSet ds = null;

            try
            {
                Database db = DatabaseWrapper.GetDatabase(compid);
                ds = db.ExecuteDataSet(CommandType.Text, strSQLQry);

            }
            catch (Exception e)
            {
                if (ds != null)
                {
                   // if (ds.Tables.Count <= 0)
                        //PostFocus.SetLog(e.Message);
                        //LogFile("ExtQueryLog.txt", e.Message);
                }
            }
            finally
            {
            }
            return ds;
        }
        public static int GetExecute(string strSelQry, int CompId)
        {
            try
            {
                try
                {
                    Database obj = Focus.DatabaseFactory.DatabaseWrapper.GetDatabase(CompId);
                    return (obj.ExecuteNonQuery(CommandType.Text, strSelQry));
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public int GetCompanyId(string strCompanyCode)
        {
            try
            {
                string strCode = strCompanyCode;
                int iCompId = ((strCode[0] >= 'A' ? strCode[0] - 55 : strCode[0] - 48) * 36 * 36) + ((strCode[1] >= 'A' ? strCode[1] - 55 : strCode[1] - 48) * 36) + (strCode[2] - 48);
                return iCompId;
            }
            catch (Exception ex)
            {
                //LogFile("ExtQueryLog.txt", "Bad request In GetCompanyId" + ex.Message);
                return 0;
            }

        }
        public Int64 GetDateTimetoInt(DateTime dt)
        {
            Int64 val;
            val = Convert.ToInt64(dt.Year) * 8589934592 + Convert.ToInt64(dt.Month) * 33554432 + Convert.ToInt64(dt.Day) * 131072 + Convert.ToInt64(dt.Hour) * 4096 + Convert.ToInt64(dt.Minute) * 64 + Convert.ToInt64(dt.Second);
            return val;
        }
        public int GetDateToInt(DateTime dt)
        {
            int val;
            val = Convert.ToInt16(dt.Year) * 65536 + Convert.ToInt16(dt.Month) * 256 + Convert.ToInt16(dt.Day);
            return val;
        }

        public static void LogFile(string LogName, string content,int log)
        {
            if (log == 1)
            {

                string str = "Logs/" + LogName + ".txt";
                FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory.ToString() + str, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                writer.BaseStream.Seek(0L, SeekOrigin.End);
                writer.WriteLine(DateTime.Now.ToString() + " - " + content);
                writer.Flush();
                writer.Close();
            }
        }

        

    }
}