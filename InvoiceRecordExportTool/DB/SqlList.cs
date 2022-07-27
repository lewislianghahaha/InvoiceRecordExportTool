//SQL语句集合
namespace InvoiceRecordExportTool.DB
{
    public class SqlList
    {
        //根据SQLID返回对应的SQL语句  
        private string _result;

        public string GetSourceRecord(string sdt,string edt)
        {
            //--注:带*号表示在最终输出时需要使用(共12项)

            _result = $@"   
                            SELECT 
                                       A.FBILLNO 单据编号,A.FDATE 业务日期 
		                               ,D.FNAME 客户名称      --*
		                               ,X3.FNAME 币别
		                               ,A.FALLAMOUNTFOR 表头价税合计 ,A.FENDDATE 到期日
		                               ,E.FNUMBER 物料编码
		                               ,F.FNAME 物料名称    --*
		                                ,F.FSPECIFICATION 规格型号  --*
		                               ,X1.FNAME 计价单位  --*
		                               ,B.FPRICEQTY 计价数量  --*
		                               ,B.FTAXPRICE 含税单价
		                               ,B.FPRICE 单价
		                               ,ISNULL(B.FENTRYTAXRATE,1)/100 税率  --*
		                               ,B.FNOTAXAMOUNTFOR 不含税金额
		                               ,B.FDISCOUNTAMOUNTFOR 折扣额   --*
		                               ,B.FTAXAMOUNTFOR 税额
		                               ,B.FALLAMOUNTFOR 明细价税合计  --*

		                               ,X2.FNAME 库存单位   --*
		                               ,B1.FSTOCKQTY 库存数量   --*
		                               ,B1.FSTOCKBASEQTY 库存基本数量

		                               ,A.F_YTC_TEXT10 纸质发票号

		                               ,X0.FDATAVALUE 物料开票信息   --*
		                               ,C.F_YTC_REMARK13 客户开票特殊要求  --*
		   
                            FROM dbo.T_AR_RECEIVABLE A
                            INNER JOIN dbo.T_AR_RECEIVABLEENTRY B ON A.FID=B.FID
                            INNER JOIN T_AR_RECEIVABLEENTRY_O B1 ON B.FENTRYID=B1.FENTRYID

                            INNER JOIN dbo.T_BD_CUSTOMER  C ON A.FCUSTOMERID=C.FCUSTID
                            INNER JOIN dbo.T_BD_CUSTOMER_L D ON C.FCUSTID=D.FCUSTID AND D.FLOCALEID=2052
                            INNER JOIN dbo.T_BD_MATERIAL E ON B.FMATERIALID=E.FMATERIALID
                            INNER JOIN dbo.T_BD_MATERIAL_L F ON E.FMATERIALID=F.FMATERIALID AND F.FLOCALEID=2052

                            LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L X0 ON E.F_YTC_ASSISTANT4=X0.FENTRYID

                            LEFT JOIN dbo.T_BD_UNIT_L X1 ON B.FPRICEUNITID=x1.FUNITID AND x1.FLOCALEID=2052
                            LEFT JOIN dbo.T_BD_UNIT_L X2 ON B1.FSTOCKUNITID=X2.FUNITID AND X2.FLOCALEID=2052

                            INNER JOIN dbo.T_BD_CURRENCY_L X3 ON A.FCURRENCYID=X3.FCURRENCYID AND X3.FLOCALEID=2052

                            WHERE EXISTS (
							                            SELECT NULL 
							                            FROM dbo.T_IV_SALESICENTRY_LK X
							                            WHERE B.FID=X.FSBILLID
							                            AND B.FENTRYID=X.FSID
                                                    )   --必须为已下推至销售增值税专用发票

                            AND A.FBILLTYPEID = '180ecd4afd5d44b5be78a6efe4a7e041'
                            AND A.FDOCUMENTSTATUS = 'C'--已审核
                            AND A.FSALEDEPTID <> '106048'--销售部门不包含‘国际事业部’
                            AND CONVERT(VARCHAR(100), A.FDATE, 23)>= '{sdt}'
                            AND CONVERT(VARCHAR(100), A.FDATE, 23)<= '{edt}'

                            ORDER BY A.FDATE,A.FBILLNO
                        ";

            return _result;
        }

        /// <summary>
        /// 获取FinancialRecords内的T_BD_CustomerList记录
        /// </summary>
        /// <returns></returns>
        public string Get_SearchCustomerList()
        {
            _result = $@"
                            SELECT * FROM dbo.T_BD_CustomerList A
                        ";

            return _result;
        }

        /// <summary>
        /// 获取FinancialRecords内的T_BD_MaterialBarcode记录
        /// </summary>
        /// <returns></returns>
        public string Get_SearchMaterialBarcode()
        {
            _result = $@"
                            SELECT * FROM dbo.T_BD_MaterialBarcode A
                        ";

            return _result;
        }

        /// <summary>
        /// 根据typeid获取对应的模板表记录 只显示TOP 1记录(批量更新使用)
        /// </summary>
        /// <param name="typeid">0:更新T_BD_CustomerList 1:更新T_BD_MaterialBarcode</param>
        /// <returns></returns>
        public string SearchUpdateTable(int typeid)
        {
            switch (typeid)
            {
                case 0:
                    _result = $@"
                          SELECT Top 1 a.CustomerCode,a.CustomerSuCode,a.CustomerAdd,a.CustomerBrank,a.Flastop_time
                          FROM T_BD_CustomerList a
                        ";
                    break;
                case 1:
                    _result = $@"
                          SELECT Top 1 a.Name,a.Code,a.CodeVersion,a.Flastop_time
                          FROM T_BD_MaterialBarcode a
                        ";
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 批量更新使用
        /// </summary>
        /// <param name="typeid">0:更新T_BD_CustomerList 1:更新T_BD_MaterialBarcode</param>
        /// <returns></returns>
        public string UpdateEntry(int typeid)
        {
            switch (typeid)
            {
                case 0:
                    _result = $@"
                                    UPDATE dbo.T_BD_CustomerList SET CustomerSuCode=@CustomerSuCode,CustomerAdd=@CustomerAdd,CustomerBrank=@CustomerBrank,
                                                                     Flastop_time=@Flastop_time
                                    WHERE CustomerCode=@CustomerCode
                               ";
                    break;
                case 1:
                    _result = $@"
                                    UPDATE dbo.T_BD_MaterialBarcode SET Code=@Code,CodeVersion=@CodeVersion,Flastop_time=@Flastop_time
                                    WHERE Name=@Name
                               ";
                    break;
            }
            return _result;
        }

    }
}
