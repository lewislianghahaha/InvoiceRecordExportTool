using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
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
        /// 运算
        /// 核心:以相同的'客户';相同的‘产品名称’进行组合,将‘数量’(根据情况获取‘计价数量’ 或 ‘库存数量’) '金额' ‘折扣额’ 进行合拼SUM()
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <returns></returns>
        public DataTable GenerateDt(string sdt, string edt)
        {
            #region 参数
            //产品名称
            var materialcode = string.Empty;
            //规格型号
            var kui = string.Empty;
            //单位
            var unit = string.Empty;
            //数量
            decimal qty = 0;
            //金额
            decimal amount = 0;
            //折扣额
            decimal distcount = 0;

            #endregion

            //获取导出临时表
            var exportdt = tempDtList.ExportDt();
            //保存从‘K3数据集’内得出的（唯一）客户名称记录
            var customtemp = tempDtList.MakeCustomerTemp();

            //用于保存-将各客户‘明细行’整合数据
            var detailtemp = tempDtList.MakeDetailTemp();
            //用于保存-根据‘产品名称’汇总的数据
            var sumtemp = tempDtList.MakeDetailTemp();

            //保存从‘detailtemp临时表’内得出的（唯一）产品名称记录
            var materialtemp = tempDtList.MakeMaterialTemp();

            //分别获取‘K3数据源’ ‘客户基础资料’及‘物料基础资料’
            var k3Dt = searchDt.SearchK3Record(sdt, edt).Copy();
            var customerBasicdt = searchDt.SearchCustomerBaseRecord().Copy();
            var materialBasicdt = searchDt.SearchMaterialBaseRecord().Copy();

            //从'K3DT'获取唯一的客户列表信息
            var k3Custtempdt = InsertCustomerList(customtemp, k3Dt);

            //循环‘k3Custtempdt',并以‘k3Dt’为条件,获取对应的明细记录
            for (var rowid = 0; rowid < k3Custtempdt.Rows.Count; rowid++)
            {
                //根据‘客户’获取K3记录集明细记录
                var custdtlrows = k3Dt.Select("客户名称='"+ Convert.ToString(k3Custtempdt.Rows[rowid][0]) +"'");
                //将获取明细行中的各项进行数据转换,转换后插入至D内
                for (var i = 0; i < custdtlrows.Length; i++)
                {
                    //获取‘客户开票特殊要求’记录
                    var notice = Convert.ToString(custdtlrows[i][23]);

                    //以下为根据‘客户开票特殊要求’记录，判断获取相关值
                    //产品名称
                    materialcode = Convert.ToString(notice.Contains("明细") ? custdtlrows[i][7] : custdtlrows[i][22]);
                    //规格型号
                    kui = notice.Contains("规格") ? Convert.ToString(custdtlrows[i][8]) : DBNull.Value.ToString();
                    //单位
                    unit = Convert.ToString(notice.Contains("库存") ? custdtlrows[i][18] : custdtlrows[i][9]);
                    //数量
                    qty = Convert.ToDecimal(notice.Contains("库存") ? custdtlrows[i][19] : custdtlrows[i][10]);
                    //金额=价税合计+折扣额
                    amount = Convert.ToDecimal(Convert.ToDecimal(custdtlrows[i][17])+Convert.ToDecimal(custdtlrows[i][15]));
                    //折扣额
                    distcount = Convert.ToDecimal(custdtlrows[i][15]);

                    //将数据集插入至detailtemp内
                    detailtemp.Merge(GetDetailDt(materialcode, kui, unit, qty, amount,Convert.ToDecimal(custdtlrows[i][13])
                                                 ,distcount,Convert.ToString(custdtlrows[i][22]), Convert.ToString(k3Custtempdt.Rows[rowid][0])
                                                 , detailtemp));
                }

                //将‘数量’ ‘金额’ ‘折扣额’变量初始化
                qty = 0;
                amount = 0;
                distcount = 0;

                var a1 = detailtemp.Copy();

                //将插入的detailtemp作为条件,获取唯一的‘产品名称’信息,并插入至materialtemp内(重)
                materialtemp.Merge(InsertMaterialList(materialtemp,detailtemp));

                var a3 = materialtemp.Copy();

                //循环materialtemp;并将‘数量’ ‘金额’ ‘折扣额’进行数据汇总
                foreach (DataRow rows in materialtemp.Rows)
                {
                    var dtlrows = detailtemp.Select("产品名称='" + Convert.ToString(rows[0]) + "'");
                    //根据‘产品名称’将以下三项进行累加
                    for (var i = 0; i < dtlrows.Length; i++)
                    {
                        //数量
                        qty += Convert.ToDecimal(dtlrows[i][3]);
                        //金额
                        amount += Convert.ToDecimal(dtlrows[i][4]);
                        //折扣额
                        distcount += Convert.ToDecimal(dtlrows[i][6]);
                    }
                    //将相关数据汇总处理后,将记录插入至sumtemp内
                    sumtemp.Merge(GetSumDt(Convert.ToString(dtlrows[0][0]),Convert.ToString(dtlrows[0][1]),
                                           Convert.ToString(dtlrows[0][2]),qty,
                                           amount,Convert.ToDecimal(dtlrows[0][5]),distcount,
                                           Convert.ToString(dtlrows[0][7]),Convert.ToString(dtlrows[0][8])
                                           ,sumtemp));


                    //每次循环后 将‘数量’ ‘金额’ ‘折扣额’变量初始化
                    qty = 0;
                    amount = 0;
                    distcount = 0;
                }

                var a2 = sumtemp.Copy();

                //最后通过循环将整合后的记录插入至exportdt内
                for (var sumid = 0; sumid < sumtemp.Rows.Count; sumid++)
                {
                    var id = rowid + 1;

                    //根据‘客户名称’获取customerBasicdt内对应的记录
                    var custdtl = customerBasicdt.Select("CustomerCode='"+ Convert.ToString(sumtemp.Rows[sumid][8]) +"'");

                    //根据‘物料开票信息’获取materialBasicdt内对应的记录
                    var materialdtl = materialBasicdt.Select("Name='"+ Convert.ToString(sumtemp.Rows[sumid][7]) + "'");

                    var xu = materialdtl.Length == 0 ? "" : Convert.ToString(materialdtl[0][2]);  //税收分类编码
                    var version = materialdtl.Length==0? "": Convert.ToString(materialdtl[0][3]); //分类编码版本号

                    var suCode = custdtl.Length == 0 ? "" : Convert.ToString(custdtl[0][2]);      //购方税号
                    var add = custdtl.Length == 0 ? "" : Convert.ToString(custdtl[0][3]);         //购方地址电话
                    var bank = custdtl.Length == 0 ? "": Convert.ToString(custdtl[0][4]);         //购方银行账号

                    exportdt.Merge(GetExrportDt(exportdt, sumtemp.Rows[sumid],id,xu, version, suCode, add, bank, sumid));
                }

                //循环完成一行‘客户’信息数据处理后，插入空行
                exportdt.Merge(InsertNullRow(exportdt));

                //循环完成后将detailtemp 及 materialtemp记录清空
                sumtemp.Rows.Clear();
                detailtemp.Rows.Clear();
                materialtemp.Rows.Clear();
            }
            return exportdt;
        }

        /// <summary>
        /// 导出临时表-数据整理
        /// </summary>
        /// <param name="dt">导出临时表</param>
        /// <param name="row">detailtemp行记录</param>
        /// <param name="id"></param>
        /// <param name="xu">税收分类编码</param>
        /// <param name="version">分类编码版本号</param>
        /// <param name="suCode">购方税号</param>
        /// <param name="add">购方地址电话</param>
        /// <param name="bank">购方银行账号</param>
        /// <param name="sumid">循环行ID</param>
        /// <returns></returns>
        private DataTable GetExrportDt(DataTable dt,DataRow row,
                                       int id,string xu,string version,string suCode,string add,string bank,int sumid)
        {
            var newrow = dt.NewRow();
            newrow[0] = "aqsd" + id;                                 //编号
            newrow[1] = Convert.ToString(row[0]);                    //产品名称
            newrow[2] = Convert.ToString(row[1]);                    //规格型号
            newrow[3] = Convert.ToString(row[2]);                    //单位
            newrow[4] = Convert.ToDecimal(row[3]);                   //数量
            newrow[5] = Math.Round(Convert.ToDecimal(Convert.ToDecimal(row[4])/ Convert.ToDecimal(row[3])),4);   //单价 公式:金额/数量
            newrow[6] = Math.Round(Convert.ToDecimal(row[4]),2);     //金额
            newrow[7] = Convert.ToDecimal(row[5]);                   //税率
            newrow[8] = Math.Round(Convert.ToDecimal(row[6]),2);     //折扣额
            newrow[9] = "";                                          //备注
            newrow[10] = xu;                                         //税收分类编码
            newrow[11] = version;                                    //分类编码版本号
            //以下为在'同一个客户内'只显示一行
            newrow[12] = sumid == 0 ? Convert.ToString(row[8]) : ""; //购方名称
            newrow[13] = sumid == 0 ? suCode: "";                    //购方税号
            newrow[14] = sumid == 0 ? add: "";                       //购方地址电话
            newrow[15] = sumid == 0 ? bank: "";                      //购方银行账号

            dt.Rows.Add(newrow);

            return dt;
        }

        /// <summary>
        /// 将整理后的记录插入至汇总表内
        /// </summary>
        /// <param name="materialcode">产品名称</param>
        /// <param name="kui">规格型号</param>
        /// <param name="unit">单位</param>
        /// <param name="qty">数量</param>
        /// <param name="amount">金额</param>
        /// <param name="rate">税率</param>
        /// <param name="distcount">折扣额</param>
        /// <param name="message">物料开票信息</param>
        /// <param name="custname">客户名称</param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable GetSumDt(string materialcode, string kui, string unit,
                                      decimal qty, decimal amount, decimal rate, decimal distcount, string message,
                                      string custname, DataTable dt)
        {
            var newrow = dt.NewRow();
            newrow[0] = materialcode; //产品名称
            newrow[1] = kui;          //规格型号
            newrow[2] = unit;         //单位
            newrow[3] = qty;          //数量
            newrow[4] = amount;       //金额
            newrow[5] = rate;         //税率
            newrow[6] = distcount;    //折扣额
            newrow[7] = message;      //物料开票信息
            newrow[8] = custname;     //客户名称
            dt.Rows.Add(newrow);
            return dt;
        }

        /// <summary>
        /// 将整理后的记录插入至明细表内
        /// </summary>
        /// <param name="materialcode">产品名称</param>
        /// <param name="kui">规格型号</param>
        /// <param name="unit">单位</param>
        /// <param name="qty">数量</param>
        /// <param name="amount">金额</param>
        /// <param name="rate">税率</param>
        /// <param name="distcount">折扣额</param>
        /// <param name="message">物料开票信息</param>
        /// <param name="custname">客户名称</param>
        /// <param name="dt">临时表</param>
        /// <returns></returns>
        private DataTable GetDetailDt(string materialcode,string kui,string unit,
                                      decimal qty,decimal amount,decimal rate,decimal distcount,string message,
                                      string custname,DataTable dt)
        {
            var newrow = dt.NewRow();
            newrow[0] = materialcode; //产品名称
            newrow[1] = kui;          //规格型号
            newrow[2] = unit;         //单位
            newrow[3] = qty;          //数量
            newrow[4] = amount;       //金额
            newrow[5] = rate;         //税率
            newrow[6] = distcount;    //折扣额
            newrow[7] = message;      //物料开票信息
            newrow[8] = custname;     //客户名称
            dt.Rows.Add(newrow);

            return dt;
        }

        /// <summary>
        /// 从'detaildt'获取唯一的产品名称列表信息
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="detaildt"></param>
        /// <returns></returns>
        private DataTable InsertMaterialList(DataTable temp, DataTable detaildt)
        {
            foreach (DataRow row in detaildt.Rows)
            {
                var newrow = temp.NewRow();
                if (temp.Rows.Count == 0)
                {
                    newrow[0] = Convert.ToString(row[0]);
                }
                //将循环的行放到temp内进行查找,若不存在,才将ROW记录插入至temp内
                else
                {
                    var dtlrow = temp.Select("MaterialCode='"+Convert.ToString(row[0])+"'");
                    if (dtlrow.Length > 0) continue;
                    newrow[0] = Convert.ToString(row[0]);
                }
                temp.Rows.Add(newrow);
            }
            return temp;
        }

        /// <summary>
        /// 从'K3DT'获取唯一的客户列表信息
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="k3Dt"></param>
        /// <returns></returns>
        private DataTable InsertCustomerList(DataTable temp,DataTable k3Dt)
        {
            //循环获取唯一的‘客户信息’并插入至temp内;
            foreach (DataRow row in k3Dt.Rows)
            {
                var newrow = temp.NewRow();
                if (temp.Rows.Count == 0)
                {
                    newrow[0] = Convert.ToString(row[2]);
                }
                //将循环的行放到temp内进行查找,若不存在,才将ROW记录插入至temp内
                else
                {
                    var dtlrow = temp.Select("CustomerCode='" + Convert.ToString(row[2]) + "'");
                    if (dtlrow.Length > 0) continue;
                    newrow[0] = Convert.ToString(row[2]);
                }
                temp.Rows.Add(newrow);
            }
             return temp;
        }

        /// <summary>
        /// 插入空白行
        /// </summary>
        /// <returns></returns>
        private DataTable InsertNullRow(DataTable sourcedt)
        {
            var newrow = sourcedt.NewRow();

            for (var i = 0; i < sourcedt.Columns.Count; i++)
            {
                newrow[i] = DBNull.Value;
            }

            sourcedt.Rows.Add(newrow);
            return sourcedt;
        }




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

            try
            {
                //通过fileaddress获取EXCEL数据;若返回DT为空,即返回result为false
                var exceldt = import.ImportExcelToDt(fileaddress, typeid);

                if (exceldt.Rows.Count > 0)
                {
                    //按照typeid获取不同的临时表数据结构
                    var inserttemp = typeid == 0 ? tempDtList.InsertCustomerBasicTemp() : tempDtList.InsertMaterialBasicTemp();

                    var uptemp = typeid == 0 ? tempDtList.InsertCustomerBasicTemp() : tempDtList.InsertMaterialBasicTemp();

                    //从数据库内获取对应表格内的数据(用于数据源比较)
                    var resourcedt = typeid == 0 ? searchDt.SearchCustomerBaseRecord().Copy() : searchDt.SearchMaterialBaseRecord().Copy();

                    //若resourcedt为0,即将exceldt记录全插入至inserttemp临时表内
                    if (resourcedt.Rows.Count == 0)
                    {
                        #region mark
                        //for (var i = 0; i < exceldt.Rows.Count; i++)
                        //{
                        //    inserttemp.ImportRow(exceldt.Rows[i]);
                        //}
                        #endregion

                        foreach (DataRow row in exceldt.Rows)
                        {
                            inserttemp.Merge(MakeTempRecord(inserttemp, typeid, row));
                        }
                    }
                    //循环比较得出;若存在,就插入至更新临时表;反之,插入至插入临时表(注:使用typeid区分)
                    else
                    {
                        var colname = typeid == 0 ? "CustomerCode" : "Name";

                        foreach (DataRow row in exceldt.Rows)
                        {
                            //判断EXCEL的值是否在数据表内存在
                            var dtlrows = resourcedt.Select($"{colname}='" + Convert.ToString(row[0]) + "'");
                            //若存在,将excel数据存放至uptemp;反之存放至inserttemp
                            if (dtlrows.Length > 0)
                            {
                                uptemp.Merge(MakeTempRecord(uptemp, typeid, row));
                            }
                            else
                            {
                                inserttemp.Merge(MakeTempRecord(inserttemp, typeid, row));
                            }
                        }
                    }

                    //将得出的结果进行插入或更新
                    if (inserttemp.Rows.Count>0 && typeid==0)
                        ImportDtToDb("T_BD_CustomerList", inserttemp);
                    if(inserttemp.Rows.Count>0 && typeid==1)
                        ImportDtToDb("T_BD_MaterialBarcode", inserttemp);
                    if(uptemp.Rows.Count>0 && typeid==0)
                        UpdateDbFromDt(uptemp,0);
                    if(uptemp.Rows.Count>0 && typeid==1)
                        UpdateDbFromDt(uptemp,1);
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="typeid">0:T_BD_CustomerList 1:T_BD_MaterialBarcode</param>
        /// <param name="row"></param>
        /// <returns></returns>
        private DataTable MakeTempRecord(DataTable temp,int typeid, DataRow row)
        {
            //按照T_BD_CustomerList 字段进行插入
            if (typeid == 0)
            {
                var newrow = temp.NewRow();
                newrow[1] = Convert.ToString(row[0]);   //CustomerCode
                newrow[2] = Convert.ToString(row[1]);   //CustomerSuCode
                newrow[3] = Convert.ToString(row[2]);   //CustomerAdd
                newrow[4] = Convert.ToString(row[3]);   //CustomerBrank
                newrow[5] = DateTime.Now.ToLocalTime(); //Flastop_time
                temp.Rows.Add(newrow);
            }
            //按照T_BD_MaterialBarcode字段进行插入
            else
            {
                var newrow = temp.NewRow();
                newrow[1] = Convert.ToString(row[0]);   //Name
                newrow[2] = Convert.ToString(row[1]);   //Code
                newrow[3] = Convert.ToString(row[2]);   //CodeVersion
                newrow[4] = DateTime.Now.ToLocalTime(); //Flastop_time
                temp.Rows.Add(newrow);
            }

            return temp;
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
        /// <param name="dt"></param>
        /// <param name="typeid">0:更新T_BD_CustomerList 1:更新T_BD_MaterialBarcode</param>
        public void UpdateDbFromDt(DataTable dt, int typeid)
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
