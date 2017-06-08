using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KindleGenCaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtKindleDir.Text = AppDomain.CurrentDomain.BaseDirectory + "\\KindleGen";
            txtOutDir.Text = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }

        private void btnSelDir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(txtKindleDir.Text))
            {
                m_Dialog.SelectedPath = txtKindleDir.Text;
            }
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.txtKindleDir.Text = m_Dir;
        }

        private void btnSelHtml_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "html文件|*.html|xhtml文件|*.xhtml|所有文件|*.*";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "html";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            txtHtmlDir.Text = openFileDialog.FileName;
            
        }

        private void btnSelOut_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(txtOutDir.Text))
            {
                m_Dialog.SelectedPath = txtOutDir.Text;
            }
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.txtOutDir.Text = m_Dir;
        }

        private string TransferLog;
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(txtKindleDir.Text) || string.IsNullOrEmpty(txtHtmlDir.Text)) return;
                this.Cursor = System.Windows.Input.Cursors.Wait;
                TransferLog = string.Empty;
                lblStatus.Content = "转换中...";
                btnGenerate.IsEnabled = false;
                btnExit.IsEnabled = false;
                HtmlFolderRenamed = false;

                InvokeKindleGen();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "错误！");
                lblStatus.Content = "转换失败！请查看转换日志！";
            }
            finally
            {
                btnGenerate.IsEnabled = true;
                btnExit.IsEnabled = true;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void InvokeKindleGen()
        {
            string openFolder = "cd " + txtKindleDir.Text;
            string cmd = "kindlegen " + txtHtmlDir.Text;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
            p.StartInfo.CreateNoWindow = true; //不显示程序窗口
            p.Start(); //启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(openFolder + "&" + cmd + "&exit"); //+ "&exit"

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            TransferLog = p.StandardOutput.ReadToEnd();
            TransferLog = TransferLog.Substring(TransferLog.IndexOf("***"));

            string str = "";
            StreamReader reader = p.StandardOutput;
            string line = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                str += line + "  ";
                line = reader.ReadLine();
            }

            p.WaitForExit(); //等待程序执行完退出进程
            p.Close();

            bool ret = CutMobiFile();
            if (ret && checkBox.IsChecked == true)
            {
                DeleteOrgHtml();
            }
        }

        private bool HtmlFolderRenamed;
        private bool CutMobiFile()
        {
            string fileName = txtHtmlDir.Text;
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }
            int index = fileName.LastIndexOf(".");
            string folderName = fileName.Substring(0, index);
            fileName = folderName + ".mobi";
            index = fileName.LastIndexOf("\\");
            string tarName = txtOutDir.Text + "\\" + fileName.Substring(index + 1);
            if (!File.Exists(fileName))
            {
                if (!HtmlFolderRenamed)
                {
                    HtmlFolderRenamed = true;
                    if (Directory.Exists(folderName + "_files"))
                    {
                        Directory.Move(folderName + "_files", folderName);
                        InvokeKindleGen();
                    }
                    return false;
                }
                System.Windows.MessageBox.Show("转换失败", "错误！");
                lblStatus.Content = "转换失败！请查看转换日志！";
                return false;
            }
            if (!fileName.Equals(tarName))
            {
                if (File.Exists(tarName))
                {
                    File.Delete(tarName);
                }
                File.Move(fileName, tarName);
            }
            lblStatus.Content = "转换完成...mobi文件路径：" + tarName;
            lastTarMobiName = tarName;
            System.Windows.MessageBox.Show("转换完成", "成功！");
            return true;
        }

        private string lastTarMobiName;
        private void btnOpenOutDir_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOutDir.Text))
            {
                string filePath = string.IsNullOrEmpty(lastTarMobiName) ? txtOutDir.Text : 
                    (File.Exists(lastTarMobiName)?lastTarMobiName:txtOutDir.Text);
                System.Diagnostics.Process.Start("Explorer", "/select," + filePath);   
            }
        }

        private void DeleteOrgHtml()
        {
            try
            {
                string fileName = txtHtmlDir.Text;
                int index = fileName.LastIndexOf(".");
                fileName = fileName.Substring(0, index);
                if (Directory.Exists(fileName))
                {
                    Directory.Delete(fileName,  true);
                }
                if (Directory.Exists(fileName+"_files"))
                {
                    Directory.Delete(fileName+"_files", true);
                }
                fileName = fileName + ".html";
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    lblStatus.Content += "       源html文档已删除";
                }
            }
            catch (Exception ex)
            {
            }
            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnViewError_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TransferLog))
            {
                System.Windows.MessageBox.Show("没有信息可供查看", "错误！");
                return;
            }
            ViewDetail vd = new ViewDetail();
            vd.Text = TransferLog;
            vd.ShowDialog();
        }
    }
}
