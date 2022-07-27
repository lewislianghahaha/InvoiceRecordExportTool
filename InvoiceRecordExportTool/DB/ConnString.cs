using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceRecordExportTool.DB
{
    /// <summary>
    /// 获取连接字符串
    /// </summary>
    /// 0:读取K3-Cloud正式库,当为1:读取FinancialRecords库
    /// <returns></returns>
    /// 
    public class ConnString
    {
        public string GetConnectionString(int connid)
        {
            var strcon = string.Empty;

            if (connid == 0)
            {
                //读取App.Config配置文件中的Connstring节点    
                var pubs = ConfigurationManager.ConnectionStrings["Connstring"];
                strcon = pubs.ConnectionString;
            }
            else
            {
                //读取App.Config配置文件中的Financial节点    
                var pubs = ConfigurationManager.ConnectionStrings["Financial"];
                strcon = pubs.ConnectionString;
            }

            return strcon;
        }
    }
}
