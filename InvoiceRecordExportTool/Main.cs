using System;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using InvoiceRecordExportTool.Task;

namespace InvoiceRecordExportTool
{
    public partial class Main : Form
    {
        TaskLogic taskLogic=new TaskLogic();
        Load load=new Load();

        public Main()
        {
            InitializeComponent();
            OnRegisterEvents();
        }

        private void OnRegisterEvents()
        {
            tmCustomer.Click += TmCustomer_Click;
            tmMaterial.Click += TmMaterial_Click;
            tmClose.Click += TmClose_Click;
            btngenerate.Click += Btngenerate_Click;
        }

        /// <summary>
        /// 导入-客户基础信息列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmCustomer_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = $"Xlsx文件|*.xlsx" };
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                var fileAdd = openFileDialog.FileName;

                taskLogic.TaskId = 0;
                taskLogic.Typeid = 0;
                taskLogic.FileAddress = fileAdd;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                //返回是否成功标记
                if(!taskLogic.ResultMark)throw new Exception("更新导入异常,请联系管理员");
                else
                {
                    MessageBox.Show($"已完成,请执行运算操作", $"信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导入-物料对应分类编码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmMaterial_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = $"Xlsx文件|*.xlsx" };
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                var fileAdd = openFileDialog.FileName;

                taskLogic.TaskId = 1;
                taskLogic.Typeid = 1;
                taskLogic.FileAddress = fileAdd;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                //返回是否成功标记
                if (!taskLogic.ResultMark) throw new Exception("更新导入异常,请联系管理员");
                else
                {
                    MessageBox.Show($"已完成,请执行运算操作", $"信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 运算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btngenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var sdt = dtstr.Value.Date;
                var edt = dtend.Value.Date;

                //若结束日期小于开始日期,报异常提示
                if(sdt>edt) throw new Exception("异常:结束日期不能小于开始日期,请重新选择日期并进行运算");

                taskLogic.TaskId = 2;
                taskLogic.Sdt = sdt.ToShortDateString();
                taskLogic.Edt = edt.ToShortDateString();

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                if (taskLogic.ResultTable.Rows.Count == 0) throw new Exception("运算出现异常,请联系管理员");
                else
                {
                    if (MessageBox.Show($"运算成功,是否进行导出至Excel?", $"提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        if(!Export(taskLogic.ResultTable)) throw new Exception("导出异常,请联系管理员");
                        else
                        {
                            MessageBox.Show($"导出成功", $"信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        private bool Export(DataTable exportdt)
        {
            var result = true;

            try
            {
                var saveFileDialog = new SaveFileDialog { Filter = $"Xlsx文件|*.xlsx" };
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return false;
                var fileAdd = saveFileDialog.FileName;

                taskLogic.TaskId = 3;
                taskLogic.Exportdt = exportdt.Copy();
                taskLogic.FileAddress = fileAdd;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                result = taskLogic.ResultMark;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmClose_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        /// <summary>
        ///子线程使用(重:用于监视功能调用情况,当完成时进行关闭LoadForm)
        /// </summary>
        private void Start()
        {
            taskLogic.StartTask();

            //当完成后将Form2子窗体关闭
            this.Invoke((ThreadStart)(() =>
            {
                load.Close();
            }));
        }

    }
}
