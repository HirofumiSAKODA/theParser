using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Threading;
using MTimerClass;

namespace theParser
{
    public partial class Form1 : Form
    {
        public parseMain pm;
        BackgroundWorker bw;


        bool AutoCloseFlag = false;

        #region alarmSetup
        
        MTimerClass.MTimerClass sch;
        public string action = "";

        public void alarmEvent(string action)
        {
            // 他ThreadのEventで起動する→Formを操作する権限がない。
            // InvokeRequiredをチェックして再度delegateを起こす。
            if (this.textBox1.InvokeRequired)
            {
                // 同一メソッドへのコールバックを作成する
                MTimerClass.timerWakeupHandler delegateMethod = new MTimerClass.timerWakeupHandler(alarmEvent);

                // コントロールの親のInvokeメソッドを呼び出すことで、呼び出し元の
                // コントロールのスレッドでこのメソッドを実行する
                this.Invoke(delegateMethod, new object[] { action });
            }
            else
            {
                // コントロールを直接呼び出す
                this.textBox1.Text += action + "\r\n";
                this.action = action;
                if (this.button1.CanSelect == true && this.button1.Text == "START")
                {
                    this.button1.PerformClick();
                }
                // alarmThread.setAlarmTime(sch.getTimerTableList());
                // Timerを再設定
                alarmThread.alarmTime = sch.getTimerTableList();
                this.printTimerList(alarmThread.alarmTime);
            }
        }

        private void printTimerList(List<MTimerClass.MTimerClass.mTimerTimeTable> timeList)
        {
            foreach (MTimerClass.MTimerClass.mTimerTimeTable p in timeList)
            {
                this.textBox1.Text += p.next.ToString() + " action:" + p.action + "\r\n";
            }
        }

        MTimerClass.timerWakeupEvent alarmThread = null;
        private void alarmSetup()
        {
            sch = new MTimerClass.MTimerClass();
            List<MTimerClass.MTimerClass.mTimerTimeTable> timeList = sch.getTimerTableList();
            alarmThread = new MTimerClass.timerWakeupEvent(alarmEvent);
            if (timeList.Count > 0)
            {
                alarmThread.alarmTime = timeList;
                this.printTimerList(alarmThread.alarmTime);
                alarmThread.Start();
            }
        }

        private void closeAlarmThread()
        {
            if (alarmThread != null && alarmThread.IsAlive == true)
            {
                alarmThread.End();
            }
        }
        #endregion // alarmSetup

        public Form1(string[] args)
        {
            InitializeComponent();
            this.pm = new parseMain();

            System.Reflection.AssemblyName myName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            this.Text =myName.Name.ToString() + "(" + myName.Version.ToString() + ")";

            this.textBox2.Text = pm.XpathDefault;
            this.textBox3.Text = pm.TargetFolder;
            this.button1.Text = "START";
            this.SetStyle(ControlStyles.Selectable, true); 
            this.textBox4.Text = pm.set.conf.daycount.ToString();
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bw.DoWork += new DoWorkEventHandler(pm.bw_DoWork);

            switch (this.pm.set.conf.mode)
            {
            case 1:
                this.radioButton1.Checked = true;
                break;
            case 2:
                this.radioButton2.Checked = true;
                break;
            case 3:
                this.radioButton3.Checked = true;
                break;
            default:
                this.radioButton3.Checked = true;
                break;
            }

            this.alarmSetup();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Int32 days = 0;
            
            Int32.TryParse(this.textBox4.Text, out days);
            if (days < 1 || days > 7)
            {
                days = pm.DAYCOUNTMAX;
            }
            pm.set.conf.daycount = days;

            if (this.button1.Text == "START")
            {
                this.textBox1.Text = "";
                pm.action = action;
                this.action = "";
                pm.XpathDefault = this.textBox2.Text;
                pm.set.conf.TargetFolder = this.textBox3.Text;

                if (this.radioButton1.Checked == true)
                {
                    pm.LoadFromInternet = true;
                    pm.LoadFromFile = false;
                }
                else if (this.radioButton2.Checked == true)
                {
                    pm.LoadFromInternet = false;
                    pm.LoadFromFile = true;
                }
                else
                {
                    pm.LoadFromInternet = true;
                    pm.LoadFromFile = true;
                }


                // button1.Enabled = false;
                this.button1.Text = "STOP";
                bw.RunWorkerAsync();
            }
            else
            {
                bw.CancelAsync();
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                textBox1.AppendText("キャンセル処理完了" + "\r\n");
            }
            else
            {
                textBox1.AppendText("終了" + "\r\n");
            }
            this.button1.Text = "START";
            // button1.Enabled = true;
            if (this.AutoCloseFlag == true)
            {
                Application.Exit();
            }
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Int32 num = 0;
            Int32.TryParse(this.label2.Text, out num);
            num++;
            this.label2.Text = num.ToString();
            if ((e.UserState as String).Length != 0)
            {
                this.textBox1.AppendText(e.UserState as String + "\r\n");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.closeAlarmThread();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        public struct kanjiMark
        {
            public Int32 x;
            public Int32 y;
            public List<byte> jisCode;

            public kanjiMark(byte x, byte y, List<byte> code)
            {
                this.x = (Int32)x;
                this.y = (Int32)y;
                this.jisCode = new List<byte>(code);
                if (code[0] == 0x5c && code[1] == 0x23) // "\" + "#"
                {
                    string num = Encoding.ASCII.GetString(new byte[] { code[2], code[3] });
                    Int32.TryParse(num, System.Globalization.NumberStyles.HexNumber, null, out this.x);
                    num = Encoding.ASCII.GetString(new byte[] { code[4], code[5] });
                    Int32.TryParse(num, System.Globalization.NumberStyles.HexNumber, null, out this.y);
                }
            }
        };

        private void button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            arib2byteChar kanji = new arib2byteChar(true, this.textBox4.Text);
            List<byte> letter = new List<byte>();
            List<byte> line = new List<byte>();
            Int32 prevKanjiTabeNum = 0;
            List<kanjiMark> kanjiList = new List<kanjiMark>();
            byte[,] alllist = new byte[256, 256];

            byte[] byteData = Encoding.UTF8.GetBytes("あ①い");
            string byteLine = "";
            foreach (byte b in byteData)
            {
                byteLine += b.ToString("X2");
            }
            MessageBox.Show("result:" + byteLine);

            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(this.textBox3.Text, "KanjiList.txt"),false))
            {
                using (StreamWriter sw2 = new StreamWriter(System.IO.Path.Combine(this.textBox3.Text, "KanjiDoc.txt"),false))
                {
                    using (StreamWriter swUtf8 = new StreamWriter(System.IO.Path.Combine(this.textBox3.Text, "KanjiListUtf8.txt"), false, Encoding.UTF8))
                    {
                        Int32 counter = 0;
                        while (true)
                        {
                            counter++;
                            if (kanji.getNextCharAryJIS(ref letter) < 0)
                            {
                                kanji.setFirst();
                                if (prevKanjiTabeNum < kanji.currentTableNumber)
                                {
                                    break;
                                }
                                prevKanjiTabeNum = kanji.currentTableNumber;
                                continue;
                            }

                            kanjiMark p = new kanjiMark(letter[3], letter[4], letter);
                            kanjiList.Add(p);
                            line.AddRange(letter);
                            if ((counter % 20) == 0)
                            {
                                string outputLine = System.Text.Encoding.GetEncoding(50220).GetString(line.ToArray()); //parseMain.exchangeJIScodeToString(line);
                                sw2.WriteLine(outputLine);
                                swUtf8.WriteLine(outputLine);
                                line.Clear();
                            }
                            letter.Clear();
                        }
                        if (line.Count != 0)
                        {
                            sw2.WriteLine(parseMain.exchangeJIScodeToString(line));
                        }
                        string utfline = "";
                        kanji.getNextLetterUtf8Gaiji(ref utfline); // 初回
                        while (this.makeStringUtf8Gaiji(kanji, ref utfline) == true)
                        {
                            swUtf8.WriteLine(utfline);
                        }
                        if (utfline.Length > 0)
                        {
                            swUtf8.WriteLine(utfline);
                        }
                    }
                }
                sw.WriteLine("total:" + kanjiList.Count);
                foreach (kanjiMark p in kanjiList)
                {
                    sw.WriteLine(p.x.ToString() + "," + p.y.ToString() + ",[" + parseMain.exchangeJIScodeToString(p.jisCode) + "]");
                    alllist[p.x, p.y]++;
                }
            }
            makeKanjiTable(alllist);
        }

        private bool makeStringUtf8Gaiji(arib2byteChar kanji , ref string line)
        {
            string letter = "";
            line = "";
            while (kanji.getNextLetterUtf8Gaiji(ref letter) != false)
            {
                line += letter;
                if (line.Length >= 20)
                {
                    return true;
                }
            }
            return false;
        }

        private void makeKanjiTable(byte[,] alllist)
        {
            string l = "   ";
            string n = "   ";
            Int32 lineNum = 32;
            for (Int32 i = 0; i < 256; i++)
            {
                if ((i % lineNum) == 0)
                {
                    l += "|";
                    n += "|";
                }
                n += (i % 10).ToString("0");
                l += "-";
            }
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(this.textBox3.Text, "KanjiTable.txt")))
            {
                for (Int32 x = 0; x < 256; x++)
                {
                    if ((x % lineNum) == 0)
                    {
                        sw.WriteLine(l);
                        sw.WriteLine(n);
                        sw.WriteLine(l);

                    }
                    sw.Write(x.ToString("000"));
                    for (Int32 y = 0; y < 256; y++)
                    {
                        if ((y % lineNum) == 0) sw.Write("|");
                        if (alllist[x, y] == 0)
                        {
                            sw.Write(" ");
                        }
                        else
                        {
                            sw.Write(alllist[x, y].ToString());
                        }
                    }
                    sw.WriteLine();
                }
            }
            this.button2.Enabled = true;
        }
    }
}
