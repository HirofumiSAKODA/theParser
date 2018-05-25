using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Collections;

namespace MTimerClass
{
    /// <summary>
    /// Miharu Timer Class
    /// TimerというかAlarm機能。「指定された日時にイベントを起こす」を目指す。
    /// Event通知は、delegate による call、または Queue による積み、を想定。
    /// </summary>
    public class MTimerClass
    {
        private string timerText = "autoActionList.xml";
        private string folder = "";

        public struct mTimerTimeStruct
        {
            public string title;
            public Int32 hour;
            public Int32 min;
            public string action;

            public mTimerTimeStruct(string p1, int p2, int p3, string p4)
            {
                this.title = p1;
                this.min = p3 % 60;
                this.hour = (p2 + (p3 / 60)) % 24; // DateTime が「60分」はThrow しやがるため。いけずー
                this.action = p4;
            }

            public mTimerTimeStruct Clone()
            {
                return new mTimerTimeStruct(this.title, this.hour, this.min, this.action);
            }
        }

        public class mTimerTimeTable
        {
            private mTimerTimeStruct settings;
            public DateTime next;

            public string action
            {
                get { return this.settings.action; }
            }

            public mTimerTimeTable(mTimerTimeStruct t)
            {
                this.settings = t;
            }

            public DateTime ToNextDateTime()
            {
                DateTime result = this.ToDateTime();
                if (result.CompareTo(DateTime.Now) < 0)
                {
                    result = result.AddDays(1);
                }
                return result;
            }

            public DateTime ToDateTime()
            {
                return this.ToDateTime(this);
            }

            public DateTime ToDateTime(mTimerTimeTable o){
                DateTime now = DateTime.Now;
                return new DateTime(now.Year, now.Month, now.Day, o.settings.hour, o.settings.min, 0);
            }
        }      

        public List<mTimerTimeStruct> timeTable;
        public List<mTimerTimeTable> timeList;
    
        /// <summary>
        /// コンストラクタ。
	/// 保存フォルダは HOME。
        /// </summary>
        public MTimerClass()
	    : this(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"))
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="folder">設定ファイルが存在するフォルダへのパス</param>
        public MTimerClass(string folder)
        {
            this.timeTable = new List<mTimerTimeStruct>();
            this.timeList = new List<mTimerTimeTable>();
            this.folder = folder;

            if (this.loadSettings() == false)
            {
                timeTable.Add(new mTimerTimeStruct( "sample", 1, 0, "doNow" ));
                this.saveSettings();
            }
            if (timeTable.Count() != 0)
            {
                foreach (mTimerTimeStruct t in timeTable)
                {
                    if (t.title != "sample")
                    {
                        mTimerTimeTable p = new mTimerTimeTable(t.Clone());
                        timeList.Add(p);
                    }
                }
            }
        }

        

        public List<DateTime> getTimerDateTimeList()
        {
            List<DateTime> result = new List<DateTime>();

            if (this.timeList.Count() > 0)
            {
                foreach (mTimerTimeTable t in timeList)
                {
                    result.Add(t.ToNextDateTime());
                }
            }

            result.Sort();
            return result;
        }

        public int compareTimerTimeTable(mTimerTimeTable a, mTimerTimeTable b)
        {
            return a.next.CompareTo(b.next);
        }

        public List<mTimerTimeTable> getTimerTableList()
        {
            List<mTimerTimeTable> result = new List<mTimerTimeTable>(this.timeList);
            List<mTimerTimeTable> result2 = new List<mTimerTimeTable>();
            if (result.Count() > 0)
            {
                foreach (mTimerTimeTable t in result)
                {
                    t.next = t.ToNextDateTime();
                }
                foreach (mTimerTimeTable t in result)
                {
                    if (result2.Count() == 0)
                    {
                        result2.Add(t);
                    }
                    else
                    {
                        bool dup = false;
                        foreach (mTimerTimeTable t2 in result2)
                        {
                            if (t.next == t2.next)
                            {
                                dup = true;
                                break;
                            }
                        }
                        if (dup == false)
                        {
                            result2.Add(t);
                        }
                    }
                }
                result2.Sort(compareTimerTimeTable);
            }
            return result2;
        }


	    /// <summary>
	    /// 設定ふぁいる読み込み
	    /// </summary>
	    /// <returns>bool ファイルがあったら true。</returns>
        private bool loadSettings()
        {
            XmlSerializer serial = new XmlSerializer(this.timeTable.GetType());
            string filename = Path.Combine(this.folder,this.timerText);

            if (System.IO.File.Exists(filename) != true)
            {
                return false;
            }
            else
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        this.timeTable = (List<mTimerTimeStruct>)serial.Deserialize(reader);
                    }
                }
                catch
                {
                    ;
                }
            }
            return true;
        }

	    /// <summary>
	    /// 設定ふぁいる書き込み
	    /// </summary>
	    /// <returns></returns>
        private bool saveSettings()
        {
            bool result=false;
            XmlSerializer serial = new XmlSerializer(this.timeTable.GetType());

            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(this.folder,this.timerText)))
                {
                    serial.Serialize(writer, this.timeTable);
                }
            }
            catch
            {

            }

            return result;
        }
       
    }
}
