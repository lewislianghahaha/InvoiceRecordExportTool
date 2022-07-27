//获取连接字符串,并创建SqlConnection

using System.Data.SqlClient;

namespace InvoiceRecordExportTool.DB
{
    public class ConDb
    {
        ConnString connString=new ConnString();

        /// <summary>
        /// 获取K3数据连接
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetK3CloudConn()
        {
            var sqlcon = new SqlConnection(connString.GetConnectionString(0));
            return sqlcon;
        }

        /// <summary>
        /// 获取Financial连接
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetFinancialConn()
        {
            var sqlcon = new SqlConnection(connString.GetConnectionString(1));
            return sqlcon;
        }
    }
}
