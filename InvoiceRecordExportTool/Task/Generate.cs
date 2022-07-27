using System;

namespace InvoiceRecordExportTool.Task
{
    //生成
    public class Generate
    {
        Import import=new Import();
        SearchDt searchDt=new SearchDt();

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
                //通过fileaddress获取EXCEL数据;若返回DT为空,返回result为false
                var exceldt = import.ImportExcelToDt(fileaddress, typeid);

                if (exceldt.Rows.Count > 0)
                {
                    //todo:从数据库内获取对应表格内的数据


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




    }
}
