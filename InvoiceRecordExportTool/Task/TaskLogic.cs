//功能分配

using System.Data;

namespace InvoiceRecordExportTool.Task
{
    public class TaskLogic
    {
        Generate generate=new Generate();
        ExportDt exportDt=new ExportDt();

        #region  变量参数

        private int _taskid;
        private string _fileAddress;   //文件地址
        private DataTable _exportdt;   //导出DT
        private int _typeid;           //导入基础资料时使用; 0:导入客户基础资料 1:导入物料基础资料

        private string _sdt;           //开始日期(运算时使用)
        private string _edt;           //结束日期(运算时使用)

        private bool _resultmark;      //返回是否成功标记
        private DataTable _resultTable;   //返回DT(运算使用)

        #endregion

        #region Set
        /// <summary>
        /// 中转ID
        /// </summary>
        public int TaskId { set { _taskid = value; } }

        /// <summary>
        /// //接收文件地址信息
        /// </summary>
        public string FileAddress { set { _fileAddress = value; } }


        /// <summary>
        /// 获取从运算得出的DT相同记录集
        /// </summary>
        public DataTable Exportdt { set { _exportdt = value; } }

        /// <summary>
        /// 导入基础资料时使用; 0:导入客户基础资料 1:导入物料基础资料
        /// </summary>
        public int Typeid { set { _typeid = value; } }

        /// <summary>
        ///开始日期(运算时使用)
        /// </summary>
        public string Sdt { set { _sdt = value; } }

        /// <summary>
        ///结束日期(运算时使用)
        /// </summary>
        public string Edt { set { _edt = value; } }

        #endregion

        #region Get

        /// <summary>
        ///  返回是否成功标记
        /// </summary>
        public bool ResultMark => _resultmark;

        /// <summary>
        ///返回DataTable至主窗体
        /// </summary>
        public DataTable ResultTable => _resultTable;
        #endregion

        public void StartTask()
        {
            switch (_taskid)
            {
                //导入-客户基础信息列表
                case 0:
                    GenerateCustomerBasicRecord(_typeid, _fileAddress);
                break;
                //导入-物料对应分类编码
                case 1:
                    GenerateMaterialBasicRecord(_typeid,_fileAddress);
                    break;
                //运算
                case 2:
                    Generate(_sdt, _edt);
                    break;
                //导出
                case 3:
                    ExportDt(_fileAddress,_exportdt);
                    break;
            }
        }

        /// <summary>
        /// 导入及生成‘客户基础列表’记录
        /// </summary>
        /// <param name="typeid">0:导入客户基础资料 1:导入物料基础资料</param>
        /// <param name="fileAddress"></param>
        private void GenerateCustomerBasicRecord(int typeid,string fileAddress)
        {
            _resultmark = generate.MakeBasicInfo(typeid,fileAddress);
        }

        /// <summary>
        /// 导入及生成‘物料基础列表’记录
        /// </summary>
        /// <param name="typeid">0:导入客户基础资料 1:导入物料基础资料</param>
        /// <param name="fileAddress"></param>
        private void GenerateMaterialBasicRecord(int typeid, string fileAddress)
        {
            _resultmark = generate.MakeBasicInfo(typeid,fileAddress);
        }

        /// <summary>
        /// 运算
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        private void Generate(string sdt,string edt)
        {
            
        }

        /// <summary>
        /// 导出
        /// </summary>
        private void ExportDt(string fileAddress,DataTable exportdt)
        {
            _resultmark = exportDt.ExportDtToExcel(fileAddress, exportdt);
        }

    }
}
