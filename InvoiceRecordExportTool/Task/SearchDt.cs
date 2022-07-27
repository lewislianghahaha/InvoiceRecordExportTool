using System;
using System.Data;
using System.Data.SqlClient;
using InvoiceRecordExportTool.DB;

namespace InvoiceRecordExportTool.Task
{
    //查询
    public class SearchDt
    {
        ConDb conDb=new ConDb();
        SqlList sqlList=new SqlList();

        /// <summary>
        /// 根据SQL语句查询得出对应的DT
        /// </summary>
        /// <param name="conid">0:读取K3-Cloud正式库,1:读取FinancialRecords库</param>
        /// <param name="sqlscript">sql语句</param>
        /// <returns></returns>
        private DataTable UseSqlSearchIntoDt(int conid, string sqlscript)
        {
            var resultdt = new DataTable();

            try
            {
                var sqlcon = conid == 0 ? conDb.GetK3CloudConn() : conDb.GetFinancialConn();

                var sqlDataAdapter = new SqlDataAdapter(sqlscript,sqlcon);
                sqlDataAdapter.Fill(resultdt);
            }
            catch (Exception)
            {
                resultdt.Rows.Clear();
                resultdt.Columns.Clear();
            }
            return resultdt;
        }

        /// <summary>
        /// 获取FinancialRecords内的T_BD_CustomerList记录
        /// </summary>
        /// <returns></returns>
        public DataTable SearchCustomerBaseRecord()
        {
            var dt = UseSqlSearchIntoDt(1,sqlList.Get_SearchCustomerList()).Copy();
            return dt;
        }

        /// <summary>
        /// 获取FinancialRecords内的T_BD_MaterialBarcode记录
        /// </summary>
        /// <returns></returns>
        public DataTable SearchMaterialBaseRecord()
        {
            var dt = UseSqlSearchIntoDt(1, sqlList.Get_SearchMaterialBarcode()).Copy();
            return dt;
        }

        /// <summary>
        /// 根据‘开始’ ‘结束’日期 查询K3数据源
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <returns></returns>
        public DataTable SearchK3Record(string sdt,string edt)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.GetSourceRecord(sdt,edt)).Copy();
            return dt;
        }

    }
}
