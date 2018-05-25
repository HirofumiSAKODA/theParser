using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MTimerClass
{
    public delegate void timerWakeupHandler(string action);

    public class timerWakeupEvent
    {
        Thread thread;
        public event timerWakeupHandler OnTime;

        public timerWakeupEvent() : this(null) { }
        public timerWakeupEvent(timerWakeupHandler onTime)
        {
            this.alarmTime = new List<MTimerClass.mTimerTimeTable>();
            this.OnTime += onTime;
        }

        public void Start()
        {
            this.thread = new Thread(new ThreadStart(this.watchTime));
            this.thread.Start();
        }

        public void End()
        {
            this.thread.Abort();
        }
        public bool IsAlive
        {
            get
            {
                if (this.thread != null)
                {
                    return this.thread.IsAlive;
                }
                return false;
            }
        }

        public List<MTimerClass.mTimerTimeTable> alarmTime;

        public void setAlarmTime(List<MTimerClass.mTimerTimeTable> value){
            Monitor.Enter(this.alarmTime);
            this.alarmTime = value;
            Monitor.Exit(this.alarmTime);
        }
        public void delAlarmTImeFirst()
        {
            Monitor.Enter(this.alarmTime);
            this.alarmTime.RemoveAt(0);
            Monitor.Exit(this.alarmTime);
        }

        void watchTime()
        {
            try
            {
                // イベントループ
                while (true){
                    if(this.alarmTime.Count() > 0)
                    {
                        // 1秒毎にalarmTime を越えたかどうか確認する。 
                        if (DateTime.Now.CompareTo(this.alarmTime[0].next) >= 0)
                        {
                            string action = this.alarmTime[0].action;
                            // delAlarmTImeFirst();
                            this.alarmTime.RemoveAt(0);
                            // イベント処理はデリゲートを通して他のメソッドに任せる。
                            if (this.OnTime != null)
                            {
                                this.OnTime(action);
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            {
            }


        }


    }

}
