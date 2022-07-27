using System;
using System.Data;

//临时表
namespace InvoiceRecordExportTool.DB
{
    public class TempDtList
    {
        //导入-客户基础信息列表
        public DataTable MakeCustomerBasicTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 4; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    //购方名称
                    case 0:
                        dc.ColumnName = "CustomerCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //购方税号
                    case 1:
                        dc.ColumnName = "CustomerSuCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //购方地址电话
                    case 2:
                        dc.ColumnName = "CustomerAdd";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //购方银行账号
                    case 3:
                        dc.ColumnName = "CustomerBrank";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 导入-物料对应分类编码列表
        /// </summary>
        /// <returns></returns>
        public DataTable MakeMaterialBasicTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 3; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    //开票信息名称
                    case 0:
                        dc.ColumnName = "Name";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //税收分类编码
                    case 1:
                        dc.ColumnName = "Code";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //编码版本号
                    case 2:
                        dc.ColumnName = "CodeVersion";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 导出临时表
        /// </summary>
        /// <returns></returns>
        public DataTable ExportDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 16; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0:
                        dc.ColumnName = "编号";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1:
                        dc.ColumnName = "产品名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2:
                        dc.ColumnName = "规格型号";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3:
                        dc.ColumnName = "单位";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4:
                        dc.ColumnName = "数量";
                        dc.DataType = Type.GetType("System.Decimal"); 
                        break;
                    case 5:
                        dc.ColumnName = "单价";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 6:
                        dc.ColumnName = "金额";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 7:
                        dc.ColumnName = "税率";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 8:
                        dc.ColumnName = "折扣额";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 9:
                        dc.ColumnName = "备注";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10:
                        dc.ColumnName = "税收分类编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 11:
                        dc.ColumnName = "分类编码版本号";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 12:
                        dc.ColumnName = "购方名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 13:
                        dc.ColumnName = "购方税号";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 14:
                        dc.ColumnName = "购方地址电话";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 15:
                        dc.ColumnName = "购方银行账号";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

    }
}
