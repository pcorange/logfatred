using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace WindowsFormsApp2
{   
    enum ProcessResult
    {
        SUCCESS = 0,
        FAIL = 1
    }

    public partial class Form1 : Form
    {
        int count = 0;
        bool status;
        string[] path;
        bool path_isset;
        object[] param = new object[2];
        SynchronizationContext work_SyncContext = null;

        public Form1()
        {
            InitializeComponent();
            label2.Text = count.ToString();
            status = false;
            path_isset = false;
            work_SyncContext = SynchronizationContext.Current;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (path_isset == true)
            {
                if (status == false)
                {
                    status = true;
                    ThreadPool.QueueUserWorkItem(Work, null);
                }
            }
            else
            {
                label3.Text = "失败！未选择文件";
            }
            
        }

        public void Work(object obj)
        {
            while (status)
            {
                foreach (string p in path)
                {
                    try
                    {
                        FileStream file = new FileStream(p, FileMode.Open);
                        StreamReader sr = new StreamReader(file);
                        string content = sr.ReadToEnd();
                        StringBuilder newcontent = new StringBuilder();
                        string[] sArray = content.Split('');
                        for (int i = 0; i < sArray.Length; i++)
                        {
                            if (sArray[i].IndexOf("RESULT : PASS") >= 0)
                            {
                                newcontent.Append(sArray[i]);
                                newcontent.Append('');
                            }
                            else if (sArray[i].IndexOf("RESULT : FAIL") >= 0)
                            {
                                count++;
                            }
                        }
                        sr.Close();
                        file.Close();

                        FileStream file2 = new FileStream(p, FileMode.Create);
                        StreamWriter sw = new StreamWriter(file2);
                        sw.Write(newcontent);
                        sw.Close();
                        file2.Close();

                        param[0] = ProcessResult.SUCCESS;
                        param[1] = Path.GetFileNameWithoutExtension(p);
                        work_SyncContext.Post(UpdateMessage, param);

                        GC.Collect();
                    }
                    catch (Exception e)
                    {
                        param[0] = ProcessResult.FAIL;
                        param[1] = Path.GetFileNameWithoutExtension(p);
                        work_SyncContext.Post(UpdateMessage, param);

                        DateTime dt = DateTime.Now;
                        string[] stime = dt.ToShortDateString().Split('/');
                        StringBuilder time = new StringBuilder();
                        foreach (string t in stime)
                        {
                            time.Append(t);
                        }
                        if (!System.IO.Directory.Exists(@"C:\log_fatred"))
                        {
                            System.IO.Directory.CreateDirectory(@"C:\log_fatred");
                        }
                        string address = @"C:\log_fatred\" + time + "_dialog.txt";
                        FileStream file3 = new FileStream(address, FileMode.Append, FileAccess.Write);
                        StreamWriter sw_log = new StreamWriter(file3);
                        sw_log.WriteLine("********************************************************************************");
                        sw_log.WriteLine(dt.ToLocalTime().ToString());
                        sw_log.WriteLine(e.ToString());
                        sw_log.WriteLine("********************************************************************************");
                        sw_log.WriteLine("\n");
                        sw_log.Close();
                        file3.Close();

                        GC.Collect();
                    }
                    /*
                    finally
                    {
                        Thread.CurrentThread.Join(3000);
                    }
                    */
                }
                Thread.CurrentThread.Join(3000);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            status = false;
    
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog filename = new OpenFileDialog();
            filename.InitialDirectory = Application.StartupPath;
            filename.Filter = "csv文件|*.csv";
            filename.RestoreDirectory = true;
            filename.Multiselect = true;
            if (filename.ShowDialog() == DialogResult.OK)
            {
                path = filename.FileNames;
                path_isset = true;
                label3.Text = "文件设置成功！";
                /*
                foreach (string p in path)
                {
                    textBox1.AppendText(p+"\n");
                }
                */
            }
        }

        private void UpdateMessage(object obj)
        {
            object[] param = obj as object[];
            ProcessResult result = (ProcessResult)param[0];
            string filename = param[1] as string;
            string showtext;

            if (result == ProcessResult.SUCCESS)
            {
                showtext = string.Format("处理{0}：SUCCESS\n", filename);
                textBox1.AppendText(showtext);
                label2.Text = count.ToString();
            }
            if (result == ProcessResult.FAIL)
            {
                showtext = string.Format("处理{0}：FAIL\n", filename);
                textBox1.AppendText(showtext);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
