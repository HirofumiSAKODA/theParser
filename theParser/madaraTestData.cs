using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace theParser
{
    public class madaraTestData : buildTestData
    {
        private enum TYPE { HD, SD, F };

        private struct ptn
        {
            public Int32 pointTo;
            public TYPE type;

            public ptn(Int32 point, TYPE type)
            {
                this.pointTo = point; // まだらを構成する場合はその宛先を、構成しない場合は0を。
                this.type = type;
            }
        }


        private class myTable
        {
            public List<List<ptn>> table;

            public myTable(ptn[][] myPattern)
            {
                this.table = new List<List<ptn>>();
                for (Int32 i = 0; i < myPattern.Count(); i++)
                {
                    List<ptn> p = new List<ptn>(myPattern[i]);
                    table.Add(p);
                }
            }
        }

        public class madaraTable
        {
            private List<myTable> table;
            public Int32 Count { get { return table.Count(); } }

            public Int32 getPointTo(Int32 tableId, Int32 serviceNum, Int32 patternNum)
            {
                return this.table[tableId-1].table[patternNum][serviceNum].pointTo;
            }

            public string getResolution(Int32 tableId, Int32 serviceNum, Int32 patternNum)
            {
                if (this.table[tableId-1].table[patternNum][serviceNum].type == TYPE.HD)
                {
                    return "HD";
                }
                return "SD";
            }

            public madaraTable()
            {
                this.table = new List<myTable>();
                this.table.Add(new myTable(new ptn[][] { /* 1 */
                    new ptn[]{ new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) }
                }));
                this.table.Add(new myTable(new ptn[][] { /* 2 */
                    new ptn[]{ new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.F),  new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(0, TYPE.HD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(1, TYPE.SD), new ptn(1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) },
                }));
                this.table.Add(new myTable(new ptn[][] { /* 3 */
                    new ptn[]{ new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(0, TYPE.HD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(2, TYPE.HD), new ptn(2, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) ,new ptn(0, TYPE.HD) },
                }));
                this.table.Add(new myTable(new ptn[][] {/* 4 */
                    new ptn[]{ new ptn(-1, TYPE.SD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1,TYPE.HD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0,TYPE.HD)},
                }));
                this.table.Add(new myTable(new ptn[][] { /* 5 */
                    new ptn[]{ new ptn(-1, TYPE.SD), new ptn(-1,TYPE.SD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1,TYPE.HD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0,TYPE.HD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1,TYPE.SD), new ptn(0,TYPE.HD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1,TYPE.HD), new ptn(0,TYPE.HD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(1,TYPE.SD), new ptn(1,TYPE.SD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(1,TYPE.HD), new ptn(1,TYPE.HD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0,TYPE.HD), new ptn(0,TYPE.HD)},
                }));
                this.table.Add(new myTable(new ptn[][] { /* 6 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) }
                }));
                this.table.Add(new myTable(new ptn[][] { /* 7 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD),  new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1, TYPE.SD), new ptn(0, TYPE.HD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(1, TYPE.SD), new ptn(1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) },
                }));
                this.table.Add(new myTable(new ptn[][] { /* 8 */
                    new ptn[]{ new ptn(-1, TYPE.HD)},
                    new ptn[]{ new ptn(-1, TYPE.HD)},
                }));
                this.table.Add(new myTable(new ptn[][] { /* 9 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) }
                }));
                this.table.Add(new myTable(new ptn[][] { /* 10 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) }
                }));
                this.table.Add(new myTable(new ptn[][] { /* 11 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD),  new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1, TYPE.HD), new ptn(0, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) },
                }));
                this.table.Add(new myTable(new ptn[][] { /* 12 */
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1, TYPE.HD), new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD),  new ptn(-1, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD),  new ptn(-1, TYPE.SD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1, TYPE.HD), new ptn(0, TYPE.HD) },
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0, TYPE.HD), new ptn(0, TYPE.HD) },
                }));
                this.table.Add(new myTable(new ptn[][] { /* 5 の亜種 13*/
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(-1,TYPE.HD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0,TYPE.HD), new ptn(-1,TYPE.SD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(-1,TYPE.HD), new ptn(0,TYPE.HD)},
                    new ptn[]{ new ptn(-1, TYPE.HD), new ptn(1,TYPE.HD), new ptn(1,TYPE.HD)},
                    new ptn[]{ new ptn(0, TYPE.HD), new ptn(0,TYPE.HD), new ptn(0,TYPE.HD)},
                }));
                    

            }

            public int getServiceNum(int p)
            {
                return this.table[p-1].table[0].Count;
            }
            public int getLineNum(int tableNum)
            {
                return this.table[tableNum-1].table.Count;
            }
        }


        private madaraTable table;

        public Int32 TableCount { get { return this.table.Count; } }

        Random rnd = null;

        public madaraTestData()
        {
            this.rnd = new Random();
            this.table = new madaraTable();
        }
        public string getMyResolution(Int32 tableid, Int32 service, Int32 lineNum){
            return this.table.getResolution(tableid, service, lineNum);
        }
        public string getMyResolution(serviceFormat service)
        {
            return this.table.getResolution(service.madaraType, service.madaraSeqNum, 0);
        }
        public Int32 getMyServiceNum(serviceFormat service)
        {
            return this.table.getServiceNum(service.madaraType);
        }

        public List<List<outputFormatBasic>> madaraList;

        public override List<outputFormatBasic> getData()
        {
            List<outputFormatBasic> result = new List<outputFormatBasic>();

            return result;
        }

        public void editToMadara(serviceFormat service)
        {
            epgEventTime current = new epgEventTime(DateTime.MinValue);
            List<outputFormatBasic> eventList;
            Int32 counter = 0;
            this.service = service;

            Int32 serviceTotalNum = 1 /* primary */ + service.children.Count();

            while ((eventList = getNextEvent(service, current)).Count != 0)
            {
                // eventList には、各サービスの current 時刻以降で最も近いevent が並んでいる。
                // このうち、「eventtime」が同じで「duration」も同じevent が並んでいたら、
                // それを「まだら」にする。
                if (eventList.Count == 1)
                {
                    current = eventList[0].eventtime;
                    continue;
                }


                // eventtimeのうち最小のイベントを抽出
                DateTime thisCurrent = DateTime.MaxValue;
                outputFormatBasic target = null;
                foreach (outputFormatBasic x in eventList)
                {
                    if (x.eventtime.value < thisCurrent)
                    {
                        target = x;
                        thisCurrent = x.eventtime.value;
                    }
                }
                current.value = thisCurrent;
                if (target == null) continue;

                // その eventtime と duration が同じeventをさがしてみる
                var list = from x in eventList
                           where x.eventtime.value == target.eventtime.value && x.duration.value == target.duration.value
                           select x;

                // そのeventtime の時間の eventlist全部も作る。
                List<outputFormatBasic> alllist = this.getEventListsCurrent(current.value, service);

                if (list.Count() > 1) // まだらに出来るぞ！
                {
                    this.setMadara(list.ToList<outputFormatBasic>(), alllist.ToList<outputFormatBasic>(), counter);
                }
                counter++;
            }
        }

        private List<outputFormatBasic> getEventListsCurrent(DateTime eventTime, serviceFormat service)
        {
            List<outputFormatBasic> eventList = new List<outputFormatBasic>();

            var list = from x in service.outList
                       where x.eventtime.value <= eventTime
                             && x.eventtime.value.AddMinutes(x.duration.value) > eventTime
                       select x;
            foreach (outputFormatBasic p in list)
            {
                eventList.Add(p);
                break;
            }

            for (Int32 i = 0; i < service.children.Count; i++)
            {
                list = from x in service.children[i].outList
                       where x.eventtime.value <= eventTime
                             && x.eventtime.value.AddMinutes(x.duration.value) > eventTime
                       select x;
                foreach (outputFormatBasic p in list)
                {
                    eventList.Add(p);
                    break;
                }
            }
            return eventList;
        }

        private int getBitrateForMadara(IEnumerable<outputFormatBasic> list, IEnumerable<outputFormatBasic> alllist)
        {
            Int32 nonMadaraBitRate = 0;
            // まずまだら対象でないイベントのbitrateを集計
            foreach (outputFormatBasic ou in alllist)
            {
                if (list.Contains(ou) != true)
                {
                    nonMadaraBitRate += ou.encoderInfo.rate.value;
                }
            }
            return 18000 - nonMadaraBitRate;
        }

        public void editToMadara(serviceFormat service, int counter)
        {
            switch (service.madaraMethod)
            {
            case 1: // 1:順序通り繰り返し、
                editToMadaraSeq(service, counter);
                break;
            case 2: // 2:ランダムのレア(頻度 1/10)、
                break;
            case 3:// 3:ランダムのレア(頻度1/100)、
                break;
            case 4: // 4:ランダムのレア(頻度1/200)
                break;
            default: // 0:ランダム、
                editToMadaraRandom(service, counter);
                break;
            }
        }

        /// <summary>
        /// service.outList, child[].outList の イベントについて
        /// サービス毎の startTime以降で最も「近い」イベントの epgEventTime を配列にして返す
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private List<outputFormatBasic> getNextEvent(serviceFormat service, epgEventTime startEvtTime)
        {
            List<outputFormatBasic> eventList = new List<outputFormatBasic>();
            outputFormatBasic p;
            p = this.getEventListLater(service.outList, startEvtTime);
            if (p != null)
            {
                eventList.Add(p);
            }

            for (Int32 i=0; i< service.children.Count; i++)
            {
                p = this.getEventListLater(service.children[i].outList, startEvtTime);
                if (p != null)
                {
                    eventList.Add(p);
                }
            }

            return eventList;
        }

        private outputFormatBasic getEventListLater(List<outputFormatBasic> outList, epgEventTime startEvtTime)
        {
            var obj = (from p in outList
                       where startEvtTime.value < p.eventtime.value 
                       orderby p.eventtime.value
                       select p).Take(1);
            foreach (outputFormatBasic ret in obj)
            {
                return ret as outputFormatBasic;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="madaraList">startTime, Durationが同一のイベント群へのリンクリスト</param>
        private void setMadara(List<outputFormatBasic> madaraList, List<outputFormatBasic> allList, Int32 count)
        {
/*
            if (this.service.madaraType > 0 && IsMadara(this.service.madaraType, madaraList, allList, count) == true)
            {
                return;
            }
*/
            Int32 bitrateBase = 18000 / allList.Count() - 100;

            // 親を探す → madaraList内、サービスIDの最も若いイベント

            Int32 s = Int32.MaxValue;
            outputFormatBasic parent = null;
            foreach (outputFormatBasic ou in allList)
            {
                if (madaraList.Contains(ou))
                {
                    if (ou.serviceid.value < s)
                    {
                        parent = ou;
                        s = ou.serviceid.value;
                    }
                }
                else
                {
                    ou.madara = null; // 削除
                }
            }

            // bitrate 調整

#if true // bitrate考えすぎ
            Int32 nonMaedaraBitrate = 0;
            Int32 bitrateUsable = 18000 - this.calcFixRate(parent.eventtime.value, allList);
            Int32 madaraBitrate = 0;
            // 残ったbitrateを乱数で割り振る。
            foreach (outputFormatBasic ou in allList)
            {
                //eventTime は一緒だが durationが違う！ → まだら以外のbitrateを決める
                if (ou.madara == null)
                {
                    if (ou.service.madaraResolution == "SD") // 残念、SDだった
                    {
                        ou.encoderInfo.rate.value = 6000; // 6Mbps 固定
                    }
                    else
                    {
                        Int32 rndMax = bitrateUsable;
                        if (bitrateUsable < 4000)
                        {
                            rndMax = 4000;
                        }
                        ou.encoderInfo.rate.value = rnd.Next(4000, rndMax);
                    }
                    nonMaedaraBitrate += ou.encoderInfo.rate.value;
                }
            }
            madaraBitrate = bitrateUsable - nonMaedaraBitrate;
#endif

            // 親は「自分含む全部の EventId,ServiceIdを収集」
            // 子は「親のEventId,ServiceId をメモ」
            List<string> childEventList = new List<string>();
            List<string> childServiceList = new List<string>();
            foreach (outputFormatBasic p in madaraList)
            {
                if (p == parent)
                {
                    foreach (outputFormatBasic pp in madaraList)
                    {
                        parent.madara.eventIdList.Add(pp.eventid.ToOutput());
                        // parent.madara.serviceList.Add(pp.service.madaraToChNum.ToString());
                        parent.madara.serviceList.Add(pp.service.serviceId.ToOutput());
                    }
                }
                else
                {
                    childEventList.Add(p.eventid.value.ToString("X"));
                    childServiceList.Add(p.service.madaraToChNum.ToString());
                    p.madara.eventIdList.Add(parent.eventid.ToOutput());
                    p.madara.serviceList.Add(parent.service.serviceId.ToOutput());
                }
#if true

                p.encoderInfo.rate.value = madaraBitrate;
#else
                p.encoderInfo.rate.value = bitrateBase * madaraList.Count();
#endif
            }


            foreach (outputFormatBasic p in madaraList)
            {
                string title = this.madaraTitle[rnd.Next(this.madaraTitle.Count())];
                p.title.value = title; //  "●我はまだらなり●";
                p.contents.value = "Bitrate:" + parent.encoderInfo.rate.value.ToString() + @"kbps\&"
                                 + "プライマリ:" + parent.service.madaraToChNum.ToString()
                                 + "(EventID:" + parent.eventid.value.ToString("X") + @")\&"
                                 + "従属:" + String.Join(@"\,", childServiceList.ToArray()) 
                                 + "(EventID:" + String.Join(@"\,", childEventList.ToArray()) + @"\&)";
            }
        }

        private string[] madaraTitle = new string[] { "●我はまだらなり●", "●まーだら●", "●まだーら●", "●まだだら●",
                                                      "●まるち●","●まるちち●" ,"●まるちっち●" };

        /// <summary>
        /// ある時刻の瞬間のイベントが並ぶalllistで、もう動かせない bitrate の総和を計算する。
        /// ※既に動かせない ＝ eventTime が startTime と違うイベント群の総和
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="alllist"></param>
        /// <returns></returns>
        private Int32 calcFixRate(DateTime startTime, List<outputFormatBasic> alllist)
        {
            Int32 fixBitrate = 0;
            bool madaraFlag = false;
            foreach (outputFormatBasic ou in alllist)
            {
                if (ou.eventtime.value != startTime)
                {
                    if (madaraFlag == false)
                    {
                        fixBitrate += ou.encoderInfo.rate.value;
                    }
                    if (ou.madara != null)
                    {
                        madaraFlag = true;
                    }
                }
            }
            return fixBitrate;
        }

        /// <summary>
        /// まだらtableに、今合致しているmadaraのパターンが含まれているかどうかをチェック
        /// </summary>
        /// <param name="madaraType"></param>
        /// <param name="madaraList"></param>
        /// <param name="allList"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool IsMadara(int madaraType, List<outputFormatBasic> madaraList, List<outputFormatBasic> allList, Int32 count)
        {
            if (this.table.Count <= madaraType) return false;

            foreach (outputFormatBasic p in madaraList)
            {
                Int32 raw = p.service.madaraSeqNum;
                Int32 pointTo = this.table.getPointTo(madaraType, raw, count % this.table.getServiceNum(madaraType));
                if (pointTo >= 0) // まだーら
                {
                    foreach (outputFormatBasic pp in madaraList)
                    {
                        if (pp.service.madaraSeqNum == pointTo) // まだら対象が存在する
                        {
                            // 自分(p)にまだらセット
                            break;
                        }
                    }
                    
                }
                string reso = this.table.getResolution(madaraType, raw, count % this.table.getServiceNum(madaraType));
            }
            return true;
        }


        private void setMadara(serviceFormat service, Int32 counter, Int32 serviceNum, Int32 rawMax, Int32 index, outputFormatVideo video, Int32 point, ref bool parent)
        {
            outputFormatBasic target;

            // serviceNum:0 は service, それ以上は serviceNum.children に設定する必要がある。
            if (serviceNum == 0)
            {
                target = service.outList[counter];
            }
            else
            {
                //target = DeepCopyHelper.DeepCopy<outputFormatBasic>(service.children[serviceNum - 1].outList[counter]);
                //service.children[serviceNum - 1].outList[counter] = target;
                target = service.children[serviceNum - 1].outList[counter];
            }
            target.videoInfo.Clear();
            target.videoInfo.Add(video);

            if (point == -1)
            {
                target.madara.Clear();
                return;
            }
            if (parent == true)
            {
                parent = false;
                // まず自分を追加
                serviceFormat from = (serviceNum == 0) ? service : (service.children[serviceNum - 1]);
                target.madara.eventIdList.Add(from.outList[counter].eventid.ToOutput());
                target.madara.serviceList.Add(from.serviceId.ToOutput());
                // serviceNum これ以降の event から、自分を指している serviceId/eventIdを収集
                for (Int32 i = (serviceNum + 1); i < rawMax; i++)
                {
                    Int32 pointRef = this.table.getPointTo(service.madaraType, i, index);
                    if (pointRef != -1)
                    {
                        target.madara.serviceList.Add(service.children[i - 1].serviceId.ToOutput());
                        target.madara.eventIdList.Add(service.children[i - 1].outList[counter].eventid.ToOutput());
                    }
                }
            }
            else
            { // 子。
                if (point == 0)
                {
                    target.madara.serviceList.Add(service.serviceId.ToOutput());
                    target.madara.eventIdList.Add(service.outList[counter].eventid.ToOutput());
                }
                else
                {
                    target.madara.serviceList.Add(service.children[point - 1].serviceId.ToOutput());
                    target.madara.eventIdList.Add(service.children[point - 1].outList[counter].eventid.ToOutput());
                }
            }
        }

        private void editToMadaraRandom(serviceFormat service, int counter)
        {
            Int32 lineMax = this.table.getLineNum(service.madaraType);
            Int32 rawMax = this.table.getServiceNum(service.madaraType);
            Int32 index = this.rnd.Next(lineMax);

            bool parentFlag = true;
            for (Int32 serviceNum = 0; serviceNum < rawMax; serviceNum++)
            {
                Int32 point = this.table.getPointTo(service.madaraType, serviceNum, index);
                outputFormatVideo video = new outputFormatVideo(this.table.getResolution(service.madaraType, service.madaraSeqNum, index));
                this.setMadara(service,counter,serviceNum,rawMax, index,video,point, ref parentFlag);
            }
        }
        private void editToMadaraSeq(serviceFormat service, int counter)
        {
            Int32 lineMax = this.table.getLineNum(service.madaraType);
            Int32 rawMax = this.table.getServiceNum(service.madaraType);
            Int32 index = counter % lineMax;

            bool parentFlag = true;
            for (Int32 serviceNum = 0; serviceNum < rawMax; serviceNum++)
            {
                Int32 point = this.table.getPointTo(service.madaraType, serviceNum, index);
                outputFormatVideo video = new outputFormatVideo(this.table.getResolution(service.madaraType, service.madaraSeqNum, index));
                this.setMadara(service, counter, serviceNum, rawMax, index, video, point, ref parentFlag);
            }

        }

        public string dispServiceListResult(List<serviceFormat> services)
        {
            string result = "";
            string CR = "\r\n";

            result += "ServiceFile.csv 解析結果" + CR;
            foreach (serviceFormat service in services)
            {
                result += service.networkId.value.ToString();
                result += ",";
                result += service.serviceId.value.ToString();
                result += ",";
                result += service.serviceName.ToString();
                result += ",";
                result += "type:" + service.type.ToString();
                result += ",";
                result += "uri:" + service.uri.ToString() + CR;
                if (service.type == serviceFormat.MyType.MADARA)
                {
                    result += "    まだら: " + service.madaraSeqNum.ToString()
                        + ",GroupName:" + service.madaraGroupName.ToString()
                        + ",Table:" + service.madaraType.ToString()
                        + ",method:" + service.madaraMethod.ToString() + CR;

                    foreach (serviceFormat child in service.children)
                    {
                        result += this.dispServiceListMadara(child);
                    }
                }
            }
            return result;
        }
        private string dispServiceListMadara(serviceFormat child)
        {
            string result = "";
            result += "        child" + child.madaraSeqNum.ToString();
            result += "," + child.serviceName.ToString();
            result += "," + child.serviceId.value.ToString();
            result += "(exchanged channel No.:" + child.madaraToChNum.ToString() + ")" + "\r\n";
            return result;
        }

    }
}

