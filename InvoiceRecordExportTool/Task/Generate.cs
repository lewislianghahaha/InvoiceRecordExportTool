using System;
using System.Data;
using System.Data.SqlClient;
using InvoiceRecordExportTool.DB;

namespace InvoiceRecordExportTool.Task
{
    //生成
    public class Generate
    {
        Import import=new Import();
        SearchDt searchDt=new SearchDt();
        ConDb conDb = new ConDb();
        SqlList sqlList=new SqlList();
        TempDtList tempDtList=new TempDtList();

        /// <summary>
        /// 导入并运算基础信息
        /// 根据typeid,导入相关EXCEL至DT,并按情况执行‘更新’及‘插入’数据
        /// </summary>
        /// <param name="typeid">0:导入客户基础资料 1:导入物料基础资料</param>
        /// <param name="fileaddress"></param>
        /// <returns></returns>
        public bool MakeBasicInfo(int typeid,string fileaddress)
        {
            var result = true;
            var resourcedt=new DataTable();

            var inserttemp=new DataTable();
            var uptemp=new DataTable();

            try
            {
                //通过fileaddress获取EXCEL数据;若返回DT为空,返回result为false
                var exceldt = import.ImportExcelToDt(fileaddress, typeid);

                if (exceldt.Rows.Count > 0)
                {
                    //todo:从数据库内获取对应表格内的数据(用于数据源比较)
                    resourcedt = typeid == 0 ? searchDt.SearchCustomerBaseRecord().Copy() : searchDt.SearchMaterialBaseRecord().Copy();

                    //todo:循环比较得出;若存在,就更新;反之,插入


                    //todo:将得出的结果进行插入或更新

                }
                else
                {
                    result = false;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }



        /// <summary>
        /// 针对指定表进行数据插入至条码表内
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        public void ImportDtToDb(string tableName, DataTable dt)
        {
            var sqlcon = conDb.GetFinancialConn();   //GetConnectionString(1);
             sqlcon.Open(); //若返回一个SqlConnection的话,必须要显式打开 
            //注:1)要插入的DataTable内的字段数据类型必须要与数据库内的一致;并且要按数据表内的字段顺序 2)SqlBulkCopy类只提供将数据写入到数据库内
            using (var sqlBulkCopy = new SqlBulkCopy(sqlcon))
            {
                sqlBulkCopy.BatchSize = 1000;                    //表示以1000行 为一个批次进行插入
                sqlBulkCopy.DestinationTableName = tableName;  //数据库中对应的表名
                sqlBulkCopy.NotifyAfter = dt.Rows.Count;      //赋值DataTable的行数
                sqlBulkCopy.WriteToServer(dt);               //数据导入数据库
                sqlBulkCopy.Close();                        //关闭连接 
            }
            sqlcon.Close();
        }

        /// <summary>
        /// 根据指定条件对数据表进行批量更新
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="dt"></param>
        /// <param name="typeid">0:更新记录 1:更新FRemarkid=1</param>
        public void UpdateDbFromDt(string tablename, DataTable dt, int typeid)
        {
            var sqladpter = new SqlDataAdapter();
            var ds = new DataSet();

            //根据typeid获取对应的模板表记录
            var searList = sqlList.SearchUpdateTable(typeid);

            using (sqladpter.SelectCommand = new SqlCommand(searList, conDb.GetFinancialConn()))
            {
                //将查询的记录填充至ds(查询表记录;后面的更新作赋值使用)
                sqladpter.Fill(ds);
                //建立更新模板相关信息(包括更新语句 以及 变量参数)
                sqladpter = GetUpdateAdapter(typeid, conDb.GetFinancialConn(), sqladpter);
                //开始更新(注:通过对DataSet中存在的表进行循环赋值;并进行更新)
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        ds.Tables[0].Rows[0].BeginEdit();
                        ds.Tables[0].Rows[0][j] = dt.Rows[i][j];
                        ds.Tables[0].Rows[0].EndEdit();
                    }
                    sqladpter.Update(ds.Tables[0]);
                }
                //完成更新后将相关内容清空
                ds.Tables[0].Clear();
                sqladpter.Dispose();
                ds.Dispose();
            }
        }

        /// <summary>
        /// 建立更新模板相关信息
        /// </summary>
        /// <param name="typeid">0:更新T_BD_CustomerList 1:更新T_BD_MaterialBarcode</param>
        /// <param name="conn"></param>
        /// <param name="da"></param>
        /// <returns></returns>
        private SqlDataAdapter GetUpdateAdapter(int typeid, SqlConnection conn, SqlDataAdapter da)
        {
            //根据tablename获取对应的更新语句
            var sqlscript = sqlList.UpdateEntry(typeid);
            da.UpdateCommand = new SqlCommand(sqlscript, conn);

            //定义所需的变量参数
            switch (typeid)
            {
                case 0:
                    da.UpdateCommand.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 500, "CustomerCode");
                    da.UpdateCommand.Parameters.Add("@CustomerSuCode", SqlDbType.NVarChar, 200, "CustomerSuCode");
                    da.UpdateCommand.Parameters.Add("@CustomerAdd", SqlDbType.NVarChar, 3000, "CustomerAdd");
                    da.UpdateCommand.Parameters.Add("@CustomerBrank",SqlDbType.NVarChar,1000, "CustomerBrank");
                    da.UpdateCommand.Parameters.Add("@Flastop_time",SqlDbType.DateTime,10, "Flastop_time");
                    break;
                case 1:
                    da.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 200, "Name");
                    da.UpdateCommand.Parameters.Add("@Code", SqlDbType.NVarChar, 100, "Code");
                    da.UpdateCommand.Parameters.Add("@CodeVersion", SqlDbType.NVarChar, 100, "CodeVersion");
                    da.UpdateCommand.Parameters.Add("@Flastop_time", SqlDbType.DateTime, 10, "Flastop_time");
                    break;
            }
            return da;
        }


    }
}
