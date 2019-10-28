using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.ComponentModel;
using System.Threading;
using System.Net;

namespace theParser
{
    public class parseMain
    {
        //private string[] divIdList = new string[]{
        //    "101024","101032","101040","101064","101048","101072","101056","123608","101088"};

        public settings set;

        public String XpathDefault = @"/html/body/div/div[contains(@id,'cell-{0}')]";
        public string TargetFolder = "";
        private string siteUri = @"https://tv.so-net.ne.jp"; //"/chart/23.action?head=201208190000"
        /// <summary>最大でも7日分しかサイトに置いてないっぽが…</summary>
        public Int32 DAYCOUNTMAX = 10;
        /// <summary>FORM上の設定</summary>
        public Int32 daycount;
        /// <summary>FORM上の設定が反映される</summary>
        public bool LoadFromInternet = false;
        /// <summary>FORM上の設定が反映される</summary>
        public bool LoadFromFile = false;
        public string temporaryFolderName;

        public Random rnd = new Random();
        public Int32 copyControlValue = 0;

        private BackgroundWorker bw;
        private Int32 _progressValue = 0;
        private Int32 progressValue
        {
            get { _progressValue++; return _progressValue;  }
            set { _progressValue = value; }
        }

        public string action = "";
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public parseMain()
        {
            this.set = new settings();
            this.TargetFolder = set.conf.TargetFolder;
            this.daycount = set.conf.daycount;
            // 先にコンパイルしておく
            this.durationPattern  = new Regex(@"（(?<duration>\d{1,})分）");
            this.genlePattern = new Regex(@"condition\.genres\[0\]\.id=(?<genle>\d{1,})");
            this.getDescMainPattern = new Regex(@"^(?<description>.*?)【"); // , RegexOptions.  .RightToLeft);
            this.getDescPattern = new Regex(@"【(?<title>.*?)】(?<contents>.*?)", RegexOptions.RightToLeft);
        }
        /// <summary>
        /// 実行押下時の backgroundWorker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            this.TargetFolder = set.conf.TargetFolder;
            this.daycount = set.conf.daycount;

            bw = sender as BackgroundWorker;

            this.set.write();

            if (doParse() == false)
            {
                e.Cancel = true;
            }
            
        }

        /// <summary>
        /// 進捗表示用。ただうまく動いてない 2013/10/25 現在
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="v"></param>
        public void sendProgress(Int32 prog, object v)
        {
            this.progressValue += prog;
            bw.ReportProgress(this.progressValue, v);
            this.progressValue++;
        }

        /// <summary>
        /// HTMLファイルをインターネット/ローカルから取得する。
        /// FORM上の設定を参照する。
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="filename"></param>
        /// <param name="existsFile"></param>
        /// <returns></returns>
        private string readHtmlFile(string uri, string filename, ref bool existsFile)
        {
            uri = this.siteUri + uri;
            string html = "";

            if (this.LoadFromFile)
            {
                if (System.IO.File.Exists(filename) )
                {
                    using (System.IO.StreamReader fr = new System.IO.StreamReader(filename))
                    {
                        html = fr.ReadToEnd();
                    }
                    existsFile = true;
                }
                else
                {
                    existsFile = false;
                }
            }
            if (this.LoadFromInternet && existsFile == false)
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    do {
                        try{
                            html = wc.DownloadString(uri);
                        } catch ( WebException ex){
                            if( ex.Status == WebExceptionStatus.SendFailure ){
                                System.Threading.Thread.Sleep(500);
                                continue;
                            }
                            bw.ReportProgress(progressValue, "OpenRead Error(" + uri.ToString() + ")" + ex.ToString());
                            throw ex;
                        }
                    } while ( false);
                    if (filename.Length != 0)
                    {
                        System.IO.StreamWriter wr = new System.IO.StreamWriter(filename);
                        wr.Write(html);
                        wr.Dispose();
                    }
                    wc.Dispose();
                    System.Threading.Thread.Sleep(2000);
                }
            }
            return html;
        }

        public void getEventDataDetail(string subUri, ref outputFormatBasic ou, ref bool existsFile)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                string filename = "";
                if (true) // this.LoadFromFile
                {
                    string[] parse = subUri.Split(new Char[] { '/' });
                    if (parse.Length > 0)
                    {
                        filename = (string)parse.GetValue(parse.Length - 1);
                        filename = Path.Combine(this.temporaryFolderName, filename);
                    }
                }
                string html = this.readHtmlFile(subUri, filename, ref existsFile);
                if (html.Length == 0)
                {
                    return;
                }
                try
                {
                    doc.LoadHtml(html);
                    // 101024201208190005.action.htm
                    //doc.Load(@"C:\Users\kooh\Downloads\101024201208190005.action.htm", Encoding.UTF8, true, (512 * 1024));//

                    HtmlNodeCollection ddList = doc.DocumentNode.SelectNodes(@"/html/body/div/div/div/dl/dd");
                    if (ddList == null || ddList.Count == 0)
                    {
                        // this.textBox1.Text = "(nothing...)";
                        throw new Exception("no dd list");
                    }

                    // ddList[1].InnerText

                    // get duration
                    ou.duration.value = getDurationFromFromText(ddList[1].InnerText);
                    HtmlNodeCollection spanList = doc.DocumentNode.SelectNodes(@"/html/body/div/div/div/div/span");
                    if (spanList == null || spanList.Count == 0)
                    {
                        throw new Exception("no span list");
                    }
                    List<string> attrList = new List<string>();
                    foreach (HtmlNode span in spanList)
                    {
                        // this.textBox1.Text += span.InnerText + ",";
                        attrList.Add(span.InnerText);
                    }
                    // <span class="bgChipGry">HD</span>
                    // <span class="bgChipGry">16:9</span>
                    // <span class="bgChipGry">コピー可</span>
                    HtmlNodeCollection videoInfo = doc.DocumentNode.SelectNodes(@"/html[1]/body[1]/div[5]/div[1]/div[3]/div[1]");

                    // ジャンル調査
                    HtmlNodeCollection genleLinkList = doc.DocumentNode.SelectNodes(@"/html[1]/body[1]/div[5]/div[1]/div[1]/dl[1]/dd[4]/a");
                    if (genleLinkList == null || genleLinkList.Count == 0)
                    {
                        // 見つからず。
                    }
                    else
                    {   // ジャンルは今のところ一つ
                        outputFormatGenle outGenle = getGenleData(genleLinkList);
                        ou.genleInfo.Add(outGenle);
                    }

                    // コピー制御
                    copyControlData cp = new copyControlData(5); // コピワン
#if COPYCONTROLALLPATTERN
                    copyControlData cp = new copyControlData(copyControlValue);
                    copyControlValue++;
                    if (cp.copyControl != null && cp.contents != null)
                    {
                        ou.copyControlMain = cp.copyControl;
                        ou.contentAvailability = cp.contents;
                        // コピー制御: 制御結果を拡張EPGをデータの1行目に記述
                        addExtDesctioptionList("コピー制御", cp.descripion, ref ou.extDescriptorInfo);
                    }
#endif

                    // エンコーダ情報
                    ou.encoderInfo.rate.value = rnd.Next(100000);

                    // 拡張EPGデータ
                    HtmlNodeCollection extDescriptionData = doc.DocumentNode.SelectNodes(@"/html[1]/body[1]/div[5]/div[1]/div[3]/p[1]");
                    if (extDescriptionData == null || extDescriptionData.Count == 0)
                    {
                        // 見つからず
                    }
                    else
                    {
                        setExtDescriptionList(extDescriptionData[0].InnerText.ToString(), ref ou.extDescriptorInfo);
                        // extDescriptionData[0].InnerText.ToString() をゴニョゴニョする @@@
                    }
                }
                catch (Exception e)
                {
                    ou.duration.value = 5;
                    bw.ReportProgress(progressValue, e.Message);
                    // this.textBox1.Text += e.Message + "\r\n";
                }
                // VideoInfo
                outputFormatVideo video = new outputFormatVideo();
                ou.videoInfo.Add(video);


                // AudioInfo
                outputFormatAudio audio = new outputFormatAudio(ou.service.forceJlab035);
                ou.audioInfo.Add(audio);


                

                // this.textBox1.Text += "\r\n";
                return;
            }
            catch (Exception e)
            {
                bw.ReportProgress(progressValue, "Parse Error(" + subUri.ToString() + ")" + e.ToString());
                // this.textBox1.Text += "Parse Error(" + subUri.ToString() + ")" + e.ToString() + "\r\n";
                // MessageBox.Show(e.ToString());
            }
            return;
        }

        Regex getDescMainPattern = null; // コンストラクタ内で初期化
        Regex getDescPattern = null; // コンストラクタ内で初期化

        private void setExtDescriptionList(string p, ref List<outputFormatExtList> list)
        {
            List<outputFormatExtDescription> resultList = new List<outputFormatExtDescription>();
            string desc = p.Trim();
#if false // 未完成。「番組詳細」を作ってみたかったけど、メモリ食い過ぎるかも？
            Match mDescriptor = this.getDescMainPattern.Match(desc);
            if(mDescriptor.Success == true){
                string description = mDescriptor.Groups["description"].ToString().Trim();
                while (description.Length > 0)
                {
                    outputFormatExtDescription exDesc = new outputFormatExtDescription();
                    string partDescription = description.Substring(0, 100);
                    description = description.Substring(100);
                    exDesc.makeExtDescription("番組詳細", partDescription);
                    resultList.Add(exDesc);                }
            }
#endif

            MatchCollection mDesc = this.getDescPattern.Matches(desc);

            foreach (Match m in mDesc)
            {
                string title = m.Groups["title"].ToString();
                string contents = m.Groups["contents"].ToString();
                outputFormatExtDescription exDesc = new outputFormatExtDescription();
                exDesc.makeExtDescription(title, contents);
                resultList.Add(exDesc);
            }

            while (resultList.Count != 0)
            {
                outputFormatExtDescription temp = resultList[0];
                list.Add(temp);
                resultList.RemoveAt(0);
                // resultList.RemoveAll(target => target.title.value.Equals(temp.title.value)); // 重複チェック
            }
        }

        private void addExtDesctioptionList(string title, string contents, ref List<outputFormatExtList> list)
        {
            outputFormatExtDescription exDesc = new outputFormatExtDescription();
            exDesc.makeExtDescription(title, contents);
            list.Add(exDesc);
        }

        private Regex genlePattern; // コンストラクタ内で初期化

        private outputFormatGenle getGenleData(HtmlNodeCollection genleLinkList)
        {
            outputFormatGenle outGenle = new outputFormatGenle();
            Match mgenle = null;

            foreach (HtmlNode genle in genleLinkList)
            {
                mgenle = genlePattern.Match(genle.Attributes[0].Value);
                if (mgenle.Groups["genle"].Length != 0)
                {
                    if (outGenle.genleBig.isEmpty() == true)
                    {
                        outGenle.genleBig.valueSetFromParse(mgenle.Groups["genle"].ToString());
                    }
                    if (outGenle.genleSmall.isEmpty() == true)
                    {
                        outGenle.genleSmall.valueSetFromParse(mgenle.Groups["genle"].ToString());
                    }
                }
            }
            return outGenle;
        }

        private Regex durationPattern; // コンストラクタ内で初期化

        private int getDurationFromFromText(string p)
        {
            int result;
            Match m = durationPattern.Match(p);
            if (Int32.TryParse(m.Groups["duration"].ToString(), out result) == true)
            {
                return result;
            }
            return 5;
        }

        /// <summary>
        /// DateTime t で指定された日時より古いファイル名を持つ CacheFile と csv ファイルを消去する。
        /// </summary>
        /// <param name="t">DateTime t</param>
        /// <returns></returns>
        public bool cleanOldFile(DateTime t)
        {
            this.cleanCsvFile(t,false);
            this.cleanCacheActionFile(t,false);
            this.cleanCacheHtmlFile(t,false);

            if (this.action.Contains("clean") == true)
            {
                DateTime reCacheDate = t.AddDays(-3);
                this.bw.ReportProgress(1, "remove cache file(" + reCacheDate.ToShortDateString() + ")");
                this.cleanCsvFile(reCacheDate, true);
                this.cleanCacheActionFile(reCacheDate, true);
                this.cleanCacheHtmlFile(reCacheDate, true);
            }
            return true;
        }

        /// <summary>
        /// 出力ファイルを Append で作るので、先に消すための関数
        /// </summary>
        /// <param name="t">DateTime t</param>
        /// <returns></returns>
        public bool clearnFile(string filename)
        {
            try
            {
                if (System.IO.File.Exists(filename) == true)
                {
                    System.IO.File.Delete(filename);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// DateTime t より古い APC出力ファイルの消去。
        /// </summary>
        /// <param name="t"></param>
        /// <param name="sameOnly">trueの時は、tと「同日」の範囲のファイルを消す</param>
        /// <returns></returns>
        public bool cleanCsvFile(DateTime t, bool sameOnly)
        {
            string filename = @"*.csv";
            string[] targetfiles = System.IO.Directory.GetFiles(
                 this.TargetFolder, filename, System.IO.SearchOption.TopDirectoryOnly);
            char[] separator = new char[]{'_','.'};
            TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
            foreach (string targetFileName in targetfiles)
            {
                try
                {
                    string[] targetFileNameParts = System.IO.Path.GetFileName(targetFileName).Split(separator, 3);
                    if (targetFileNameParts.Length >= 3 && targetFileNameParts[2].Length >= 6)
                    {
                        DateTime targetFileDate = this.getDateTimeFromString("20" + targetFileNameParts[2].Substring(0, 6));
                        removeFileForClean(t, targetFileDate, targetFileName, sameOnly);
                    }
                }
                catch (Exception)
                {
                    ; // 無視
                }
            }
            return true;
        }

        public bool removeFileForClean(DateTime t, DateTime targetFileDate, string targetFileName, bool sameOnly)
        {
            string filename = System.IO.Path.GetFileName(targetFileName);
            if (sameOnly == true)
            {
                TimeSpan diffDay;
                if (t > targetFileDate)
                {
                    diffDay = t - targetFileDate;
                }
                else
                {
                    diffDay = targetFileDate - t;
                }
                if (diffDay <= new TimeSpan(1, 0, 0, 0))
                {
                    System.IO.File.Delete(targetFileName);
                    this.bw.ReportProgress(1, "remove " + filename);
                }
            }
            else
            {
                if (t.CompareTo(targetFileDate) > 0)
                {
                    System.IO.File.Delete(targetFileName);
                    this.bw.ReportProgress(1, "remove " + filename);
                }
            }
            return true;
        }


        /// <summary>
        /// DateTime t より古い cache ファイル(html)の消去
        /// </summary>
        /// <param name="t"></param>
        /// <param name="sameOnly">trueの時は、tと「同日」の範囲のファイルを消す</param>
        /// /// <returns></returns>
        public bool cleanCacheHtmlFile(DateTime t, bool sameOnly)
        {
            this.cleanCacheFile(t, @"*.html",0, sameOnly);
            return true;
        }

        /// <summary>
        /// DateTime t より古い cache ファイル(action)の消去
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool cleanCacheActionFile(DateTime t, bool sameOnly)
        {
            this.cleanCacheFile(t, @"*.action",6, sameOnly);
            return true;
        }
        /// <summary>
        /// cacheファイル削除処理。拡張子 action/html 両対応
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fileType"></param>
        /// <param name="datetimeStartIndex"></param>
        /// <returns></returns>
        public bool cleanCacheFile(DateTime t, string fileType, Int32 datetimeStartIndex, bool sameOnly)
        {
            string filename = fileType;
            string[] targetfiles = System.IO.Directory.GetFiles(
                 this.temporaryFolderName, filename, System.IO.SearchOption.TopDirectoryOnly);
            TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
            foreach (string targetFileName in targetfiles)
            {
                try
                {
                    string targetDateStr = System.IO.Path.GetFileName(targetFileName).Substring(datetimeStartIndex, 8);
                    DateTime targetFileDate = this.getDateTimeFromString(targetDateStr);
                    removeFileForClean(t, targetFileDate, targetFileName, sameOnly);
                }
                catch
                {
                    ; // 無視
                }
            }
            return true;
        }

        /// <summary>
        /// YYYYMMDD という文字列から DateTime型を作る。（DateTimeのParseに失敗しちゃったので無理やり)
        /// 失敗したら DateTimeの最大値を返す。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private  DateTime getDateTimeFromString(string str)
        {
            DateTime result;
            try
            {
                Int32 year = Int32.Parse(str.Substring(0, 4));
                Int32 month = Int32.Parse(str.Substring(4, 2));
                Int32 day = Int32.Parse(str.Substring(6, 2));
                result = new DateTime(year, month, day);
            }
            catch
            {
                result = DateTime.MaxValue;
            }
            return result;
        }

        /// <summary>
        /// ServiceFile.csv読込処理
        /// allservice.services に class serviceFormat で溜める。
        /// </summary>
        /// <returns></returns>
        public bool getServiceFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = System.IO.Path.Combine(this.TargetFolder, "ServiceFile.csv");
            using (TextFieldParser parser
                      = new TextFieldParser(ofd.FileName, Encoding.GetEncoding("Shift_JIS")))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");                    // ","区切り
                //parser.SetDelimiters("\t");                    // タブ区切り(TSVファイルの場合)
                while (parser.EndOfData == false)
                {
                    string[] column = parser.ReadFields(); // 1行目
                    if (column.Length == 0 || (column.Length > 0 && column[0].StartsWith("#")))
                    {
                        continue;
                    }
                    if (allservice.updateTime == DateTime.MinValue)
                    {
                        if (column.Length >= 2)
                        {
                            allservice.updateTime = DateTime.ParseExact(column[0],
                                               "yyyyMMddHHmmss",
                                               null);
                        }
                    }
                    else
                    {
                        if (column.Length >= 4)
                        {
                            allservice.services.Add(new serviceFormat(column)); // column[0], column[1], column[2], column[3]));
                        }
                    }
                }
            }

            this.checkMadaraService(allservice.services, madaraTest);
            this.bw.ReportProgress(progressValue, madaraTest.dispServiceListResult(allservice.services));
            return true;
        }
       /// <summary>
        /// madaraが一つでもいたら呼ばれる。
        /// madaraを実現するためには、以下条件が必要
        /// 1) 同一まだらグループ名の集合の中で
        /// 2) まず「親(一番若いServiceId)」と「子（それ以外）」を確定させて
        /// 2) パターン適応タイプが用意している物で
        /// 3) 適応処理が入ってて(範囲外の場合は「0」
        /// 4) 変換後の ServiceID が「同じチャンネル番号内(上位２桁が同一)」で「重複不可(重複時は後の登録が無効化)」
        /// 5) 初期解像度（空の場合はパターン適応タイプの頭のを採用)
        /// 6) 「親（一番若いServiceID)」にEPGデータの設定があること。ない場合は allkanji
        /// → OKの場合、親サービスに子サービスを積む。(積まれた子サービスは allservices から削除)
        /// 
        /// 処理: service単位のループ(in parseMain)で、「親」が回ってきたら（子は削除されているからいないはず）
        /// 「親子含めた全サービス分のデータを作成」する。
        ///  → 最後に、サービス毎にSortして outList として parseMain に出力させる。
        /// </summary>
        /// <param name="services">全サービスへのリスト</param>
        /// <returns></returns>
        public bool checkMadaraService(List<serviceFormat> services, madaraTestData madara)
        {
            bool ret = false;
            serviceFormat parent = null;

            var madaraList = from x in services
                             where x.type == serviceFormat.MyType.MADARA
                             select x;

            var result = from x in madaraList
                         group x by x.madaraGroupName;
            foreach(var lists in result)
            // foreach (List<serviceFormat> lists in result.GroupBy<serviceFormat>)
            {
                var sortedlist = from y in lists
                                 orderby y.madaraToChNum
                                 select y;
                Int32 count = 0;
                foreach (serviceFormat service in sortedlist)
                {
                    if (count == 0) // 親設定
                    {
                        if (service.madaraType > madara.TableCount)
                        {
                            service.madaraType = -1;
                            service.type = serviceFormat.MyType.ALLKANJI;
                            break;
                        }
                        service.madaraSeqNum = 0;
                        if (service.madaraMethod > 4)
                        {
                            service.madaraMethod = 0;
                        }
                        if (service.madaraResolution != "HD" && service.madaraResolution != "SD")
                        {
                            service.madaraResolution = madara.getMyResolution(service);
                        }

                        parent = service; // 親へのリンクをメモ
                    }
                    else
                    {
                        if (parent == null) // 親なし状態
                        {
                            foreach (serviceFormat s in sortedlist)
                            {
                                services.Remove(s); // service登録そのものを削除
                            }
                            return false;
                        }
                        service.madaraType = parent.madaraType;
                        service.type = parent.type;
                        service.madaraMethod = parent.madaraMethod;
                        service.madaraResolution = parent.madaraResolution;
                        if (service.uri.Length == 0)
                        {
                            service.uri = parent.uri;
                        }
                        service.madaraSource = parent.madaraSource;
                        service.madaraSeqNum = count;
                        // service.serviceId = new epgServiceId(service.madaraToChNum);
                        parent.children.Add(service);
                        services.Remove(service);
                    }
                    count++;
                }
                if (parent.children.Count + 1 /* parent の分 */ < madara.getMyServiceNum(parent)) // まだらパターンよりサービス数が少ない→ 設定ミス
                {
                    services.Remove(parent);
                }
            }
            return ret;
        }


        ServiceFileItem allservice = null;
        ExchangeLetter exchangeLetter = null;
        madaraTestData madaraTest = null;

        /// <summary>
        /// allservice.services の service 毎に 処理を呼ぶ。
        /// </summary>
        /// <returns></returns>
        public bool doParse() // string[] args
        {
            // Int32 cnt = new Int32();
            allservice = new ServiceFileItem();
            exchangeLetter = new ExchangeLetter(this.TargetFolder);
            List<outputFormatBasic> outList = new List<outputFormatBasic>();
            madaraTest = new madaraTestData();
            this.rnd = new Random();


            // 拡張変換処理の評価用
            // Int32 exCounter = 0;

            try
            {
                this.getServiceFile();
                DateTime t = DateTime.Now;
                DateTime today = new DateTime(t.Year, t.Month, t.Day);
                DateTime lastDay = today.AddDays(this.daycount);
                this.temporaryFolderName = Path.Combine(Environment.GetFolderPath(
                                Environment.SpecialFolder.LocalApplicationData),
                                "theParser");
                Directory.CreateDirectory(this.temporaryFolderName);

                this.cleanOldFile(today.AddDays(-2));

                foreach (serviceFormat service in allservice.services)
                {
                    // 今、出力先に存在する「自serviceのデータ」の末尾を検索し、最終日時を取得する。
                    DateTime lastEventDate = DateTime.MaxValue;
                    Int32 lastEventDuration = 0;
                    if (getLastEventDate(service, today, lastDay, out lastEventDate, out lastEventDuration) == false)
                    {
                        lastEventDate = DateTime.MinValue;
                        lastEventDuration = 0;
                    }

                    switch( service.type ){
                    case serviceFormat.MyType.ALLKANJI:
                        {
                        KanjiMaxData epgEngine = new KanjiMaxData(set);
                        epgEngine.service = service;
                        epgEngine.today = today;
                        epgEngine.lastDay = lastDay;
                        epgEngine.daycount = this.daycount;
                        epgEngine.exchangeLetter = exchangeLetter;
                        outList.AddRange(epgEngine.getData());

                        // this.getKanjiMaxData(ref outList, service, today);

                        checkAndOutput(ref outList, service, today, lastDay);
                        }
                        break;
                    case serviceFormat.MyType.MADARA:
                        /*if (lastEventDate != DateTime.MinValue)
                        {
                            today = lastEventDate;// 日付的に上書きしない策 @@@
                        }*/
                        if (this.getEventDataMadara(service, today, lastDay) == false)
                        {
                            return false; // cancel
                        }
                        checkAndOutput(ref service.outList, service, today, lastDay);
                        foreach (serviceFormat child in service.children)
                        {
                            checkAndOutput(ref child.outList, child, today, lastDay);
                        }
                        break;
                    case serviceFormat.MyType.URI:
                        if (this.getEventData(ref outList, service, today) == false)
                        {
                            return false; // cancel ボタン押下
                        }
                        checkAndOutput(ref outList, service, today, lastDay);
                        break;
                    case serviceFormat.MyType.INTERBEE:
                        {
                        loopPatter epgEngine = new loopPatter();
                        epgEngine.service = service;
                        epgEngine.today = today;
                        epgEngine.lastDay = lastDay;
                        epgEngine.daycount = this.daycount;
                        epgEngine.path = this.TargetFolder;
                        epgEngine.readFile();
                        outList.AddRange(epgEngine.getData());

                        checkAndOutput(ref outList, service, today, lastDay);
                        }
                        break;
                    case serviceFormat.MyType.MAXDATA:
                        {
                        loopPatter epgEngine = new loopPatter();
                        epgEngine.service = service;
                        epgEngine.today = today;
                        epgEngine.lastDay = lastDay;
                        epgEngine.daycount = this.daycount;
                        epgEngine.path = this.TargetFolder;
                        epgEngine.readFile();
                        outList.AddRange(epgEngine.getData());

                        checkAndOutput(ref outList, service, today, lastDay);
                        }
                        break;
                    }
                    outList.Clear();
                }
            }
            catch (Exception e)
            {
                // WEB接続失敗したら多分ここ
                this.bw.ReportProgress(progressValue, e.ToString());
            }

            return true;
        }

        private bool getLastEventDate(serviceFormat service, DateTime start,DateTime end, out DateTime lastEventDate, out Int32 lastEventDuration)
        {
            lastEventDate = DateTime.MinValue;
            lastEventDuration = 0;
            DateTime targetDate = new DateTime(start.Year, start.Month, start.Day);
            string filename = Path.Combine(this.TargetFolder,this.makeFilename(service, targetDate));
            string lastFileName = "";
            while(true){
                if (File.Exists(filename) == false)
                {
                    break;
                }
                lastFileName = filename;
                targetDate = targetDate.AddDays(1);
                if (targetDate > end)
                {
                    break;
                }
                filename = Path.Combine(this.TargetFolder,this.makeFilename(service, targetDate));
            }
            if (lastFileName.Length == 0)
            {
                lastEventDate = DateTime.MinValue;
                lastEventDuration = 0;
                return false;
            }
            targetDate = targetDate.AddDays(-1);
            string lastLine = "";
            using (StreamReader sr = new StreamReader(lastFileName, Encoding.GetEncoding(932)))
            {
                while (sr.EndOfStream != true)
                {
                    string line = sr.ReadLine();
                    if (line.StartsWith("\a") == true)
                    {
                        lastLine = line;
                    }
                }
            }
            string[] word = lastLine.Split(',');
            if (word.Count() > 5)
            {
                string dura = word[5];

                try
                {
                    
                    lastEventDate = DateTime.ParseExact(targetDate.ToString("yyyyMMdd") + word[4], "yyyyMMddHHmmss",
                        System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    lastEventDuration = Int32.Parse(word[5]);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool removeParamWithoutBasic(ref List<outputFormatBasic> outList){
            foreach (outputFormatBasic oneEvent in outList)
            {
                oneEvent.clearWithoutBasic();
            }
            return true;
        }

        private Int32 manageAudioIndex = 0;

        private int manage035Audio(ref List<outputFormatBasic> outList){
            foreach (outputFormatBasic oneEvent in outList)
            {
                int tag = 0x10;
                oneEvent.audioInfo.Clear();  
                oneEvent.audioInfo.Add(new outputFormatAudio(tag, 0x0f, ref manageAudioIndex , oneEvent.service.forceJlab035));
                manageAudioIndex++;
                tag++;
                oneEvent.audioInfo.Add(new outputFormatAudio(tag, 0x0f, ref manageAudioIndex , oneEvent.service.forceJlab035));
                manageAudioIndex++;
                tag++;
                oneEvent.audioInfo.Add(new outputFormatAudio(tag, 0x0f, ref manageAudioIndex , oneEvent.service.forceJlab035));
                manageAudioIndex++;
                tag++;
                oneEvent.audioInfo.Add(new outputFormatAudio(tag, 0x0f, ref manageAudioIndex , oneEvent.service.forceJlab035));
                manageAudioIndex++;
                tag++;
            }
            return 1;
        }

        private Int32 manageVideoIndex = 0;

        /// <summary>
        /// Video要素を、035向けに捏造する。
        /// H.264 → 1080i(b1,b3,b4)  H.265 → 1080i(b1,b3,b4),1080p(e1,e3,e4),2160p(93)
        /// </summary>
        /// <param name="outList"></param>
        /// <returns></returns>
        private int manage035Video(ref List<outputFormatBasic> outList, serviceFormat service)
        {
            int ret=0;
            foreach (outputFormatBasic oneEvent in outList)
            {
                oneEvent.videoInfo.Clear();
                oneEvent.videoInfo.Add(new outputFormatVideo(service.forceJlab035_videokind,manageVideoIndex));
                manageVideoIndex++;
            }
            return ret;
        }
        private int manage035Parent(ref List<outputFormatBasic> outList, serviceFormat service)
        {
            int index = 0;
            int ret=0;
            foreach (outputFormatBasic oneEvent in outList){
            }
            return ret;
        }

        private int manage035CopyControl(ref List<outputFormatBasic> outList, serviceFormat service)
        {
            int index = 0;
            int ret=0;
            foreach (outputFormatBasic oneEvent in outList)
            {
                setCopyControlPatternData(index, ref oneEvent.contentAvailability, ref oneEvent.copyControlMain);
                index++;
            }
            return ret;
        }

        public bool setCopyControlPatternData(int index, ref outputFormatContentAvailability contTarget, ref outputFormatCopyControl target)
        {
            if( CopyControlPatternAry == null ){
                CopyControlPatternAry = new List<CopyControlPattern>();
            }
            CopyControlPatternAry.Add(new CopyControlPattern(-1,0,-1,0,0)); // 初期
            CopyControlPatternAry.Add(new CopyControlPattern( 0,0, 0,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern(-1,0, 2,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern( 0,0, 3,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern( 0,1, 0,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern( 0,1, 3,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern( 1,0, 2,1,0)); // copy control on
            CopyControlPatternAry.Add(new CopyControlPattern( 1,1, 3,1,0)); // copy control on
#if nouse
            if( CopyControlPatternAry.Count() == 0){
                foreach(int a in new int[]{-1,0,1} ){ // restriction
                  foreach(int b in new int[]{0,1} ){ // encrypt
                    foreach(int i in new int[]{-1,0,2,3} ){ // record
                      foreach(int j in new int[]{0,1} ){ // CopyControl
                        foreach(int k in new int[]{0,1,2,3} ) // APS
                        { 
                            CopyControlPatternAry.Add(new CopyControlPattern(a,b,i,j,k));
                            //public CopyControlPattern(int rest, int encrypt,int record, int copycntl, int aps){
                        }
                      }
                    }
                  }
                }
            }
#endif
            CopyControlPattern p =CopyControlPatternAry[index % CopyControlPatternAry.Count()];
            contTarget.restrictionmode.value = p.rest;
            contTarget.encryptionMode.value = p.enc;
            target.recoriding.value = p.rec;
            target.copyControl.value = p.copycntl;
            target.aps.value = p.aps;

            return true;
        }

        public bool setParentalPatternData(int index, ref outputFormatParental parental)
        {
            parental.rate.value = (index % (parental.rate.max + 1));
            return true;
        }

        private class CopyControlPattern {
            public int rest;
            public int enc;
            public int rec;
            public int copycntl;
            public int aps;

            public CopyControlPattern(int rest, int encrypt,int record, int copycntl, int aps){
                this.rest = rest;
                this.enc = encrypt;
                this.rec = record;
                this.copycntl = copycntl;
                this.aps = aps;
            }
        }

        private List<CopyControlPattern> CopyControlPatternAry;


        private void checkAndOutput035(ref List<outputFormatBasic> outList, serviceFormat service, DateTime start, DateTime end)
        {
            outList = this.checkOutList(service, outList, start, end);
            this.manage035Audio(ref outList);
            this.manage035Video(ref outList, service);
            this.manage035CopyControl(ref outList, service);
            Int32 lines = this.outputCsvFile(outList);
            this.bw.ReportProgress(progressValue,
                                   "output File:" + service.serviceName.ToString() + "(" + lines.ToString() + " lines)");
            this.outputRenewFile(allservice.services);
        }

        private void checkAndOutput(ref List<outputFormatBasic> outList, serviceFormat service, DateTime start, DateTime end)
        {
            if( service.forceJlab035 ){
                checkAndOutput035(ref outList, service, start,end);
                return;
            }
            outList = this.checkOutList(service, outList, start, end);
            Int32 lines = this.outputCsvFile(outList);
            this.bw.ReportProgress(progressValue,
                                   "output File:" + service.serviceName.ToString() + "(" + lines.ToString() + " lines)");
            this.outputRenewFile(allservice.services);
        }



        private bool getEventDataMadara(serviceFormat service, DateTime today, DateTime lastDay)
        {
            string msg = "";
            List<List<outputFormatBasic>> eventListRaw = new List<List<outputFormatBasic>>();

            service.outList = new List<outputFormatBasic>();
            if (this.getEventDataEveryService(service, today, lastDay, ref service.outList) == false)
            {
                return false;
            }
            eventListRaw.Add(service.outList);
            msg += "parent:" + service.outList.Count().ToString();
            foreach (serviceFormat child in service.children)
            {
                child.outList = new List<outputFormatBasic>();
                if (this.getEventDataEveryService(child, today, lastDay, ref child.outList) == false)
                {
                    return false;
                }
                eventListRaw.Add(child.outList);
                // getEventData(ref child.outList, child, today);
                msg += "child:" + child.outList.Count().ToString();
            }

            madaraTest.editToMadara(service);
/*
            Int32 counter = 0;
            foreach (outputFormatBasic ou in service.outList)
            {
                madaraTest.editToMadara(service,counter);
                counter++;
            }
*/
            return true;
        }


        private bool getEventDataEveryService(serviceFormat service, DateTime today, DateTime lastDay, ref List<outputFormatBasic> outList)
        {
            outList.Clear();
            if (service.madaraSource == "allkanji")
            {
                KanjiMaxData epgEngine = new KanjiMaxData(set);
                epgEngine.service = service;
                epgEngine.today = today;
                epgEngine.lastDay = lastDay;
                epgEngine.daycount = this.daycount;
                epgEngine.exchangeLetter = exchangeLetter;
                outList.AddRange(epgEngine.getData());
            }
            else if (service.madaraSource == "maxdata")
            {
                KanjiMaxData epgEngine = new KanjiMaxData(set);
                epgEngine.service = service;
                epgEngine.today = today;
                epgEngine.lastDay = lastDay;
                epgEngine.daycount = this.daycount;
                epgEngine.exchangeLetter = exchangeLetter;
                epgEngine.MyMethod = MyMethodType.TIMEPATTERN1;
                outList.AddRange(epgEngine.getData());

            }
            else if (getEventData(ref outList, service, today) == false)
            {
                return false;
            }
            return true;
        }
        

        private bool getEventData(ref List<outputFormatBasic> outList, serviceFormat service, DateTime today)
        {
            HtmlAgilityPack.HtmlDocument doc = null;
            bool existsFile = false;

            for (double dayCount = 0; dayCount <= this.daycount; dayCount++)
            {
                DateTime targetDay = today.AddDays(dayCount);
                // this.clearnFile(targetDay);
                string uriGetHeader = targetDay.ToString(@"yyyyMMdd");
                for (Int32 hourCount = 0; hourCount < 24; hourCount += 6)
                {
                    // remote から取得
                    string subUriGet = uriGetHeader + hourCount.ToString("00") + @"00";
                    string subUri = @"/chart/23.action?head=" + subUriGet;
                    string filename = Path.Combine(this.temporaryFolderName, subUriGet + ".html");

                    string html = this.readHtmlFile(subUri, filename, ref existsFile);

                    if (html.Length == 0)
                    {
                        this.bw.ReportProgress(progressValue, targetDay.ToShortDateString() + " " + hourCount.ToString() + ":00 no Html File");
                        continue;
                    }
                    doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    // ローカルファイルから取得
                    //doc.Load(@"C:\Users\kooh\Downloads\23.action.htm", Encoding.UTF8, true, (512 * 1024));//

                    // チャンネル毎リスト divIdList から１つずつ
                    //                        Int32 serviceIndex = -1;
                    string divid = service.uri;
                    //                            serviceIndex++;
                    string xpath = String.Format(this.XpathDefault, service.uri);
                    if (existsFile == false)
                    {
                        this.bw.ReportProgress(progressValue, targetDay.ToShortDateString() + " " + hourCount.ToString() + ":00 " + service.serviceId.value.ToString());
                    }
                    //this.textBox2.Text = xpath;
                    HtmlNodeCollection divList = doc.DocumentNode.SelectNodes(xpath);
                    if (divList == null || divList.Count == 0)
                    {
                        // MessageBox.Show(doc.ParseErrors.ToString());
                        if (this.removeHtmlFile(filename) == false)
                        {
                            this.bw.ReportProgress(progressValue,
                                targetDay.ToShortDateString() + ":" + service.serviceId.ToString() + " failed to remove " + filename);
                        }
                        this.bw.ReportProgress(progressValue, targetDay.ToShortDateString() + " " + hourCount.ToString() + ":00 " + service.serviceId.value.ToString() + " no div!!");
                        //this.textBox1.Text += "(" + xpath + " nothing...)" + subUri + "\r\n";
                    }
                    else
                    {
                        bool dispMessageFlag = false;

                        foreach (HtmlNode div in divList) // "//div[contain(@id,'cell-')]"[contain(div/@id,'cell-')]
                        {
                            outputFormatBasic ou = new outputFormatBasic(
                                       service.networkId.value,
                                       service,
                                       ((service.useExchangeLetter == true)?(exchangeLetter):(null)));
                            String idmsg = "";
                            string titleMsg = "";

                            if (this.bw.CancellationPending == true)
                            {
                                return false;
                            }

                            if (div.Attributes.Contains("id") == false)
                            {
                                idmsg = "";
                            }
                            else
                            {
                                HtmlAttribute att = div.Attributes["id"];
                                idmsg = att.Value;
                            }
                            ou.eventtime = new epgEventTime(idmsg);
                            if (ou.eventtime.value.CompareTo(DateTime.MinValue) == 0) // error
                            {
                                continue;
                            }
                            ou.eventid.value = ou.eventid.setEventIdFromDate(ou.eventtime.value);

                            try
                            {
                                if (div.ChildNodes[1].ChildNodes[1].ChildNodes.Count > 3)
                                {
                                    // title
                                    titleMsg = div.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[1].InnerText.Trim();
                                    // summary
                                    //ou.contents.set(div.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[3].InnerText.Trim());
                                    // summary 文字変換処理を hack し、「ARIB外字前出力処理」に差し替える。
                                    //if (divid == "101040")
                                    //{
                                    //    exCounter = ou.contents.setExText(exCounter, 6);
                                    //}
                                    //else
                                    //{
                                    ou.contents.set(div.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[3].InnerText.Trim());
                                    //}

                                    // uri
                                    ou.detailUri = div.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[1].Attributes["href"].Value;
                                }
                                else if (div.ChildNodes[1].ChildNodes[1].ChildNodes.Count != 0)
                                {
                                    // title
                                    titleMsg = div.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[3].InnerText.Trim();
                                    // summary
                                    //ou.contents.set(div.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[3].InnerText.Trim());
                                    // summary 文字変換処理を hack し、「ARIB外字前出力処理」に差し替える。
                                    //if (divid == "101040")
                                    //{
                                    //    exCounter = ou.contents.setExText(exCounter, 6);
                                    //}
                                    //else
                                    //{
                                    ou.contents.set(div.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[3].InnerText.Trim());
                                    //}
                                    // uri
                                    ou.detailUri = div.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[3].Attributes["href"].Value;
                                }
                                else
                                {
                                    ;
                                }

                                this.getEventDataDetail(ou.detailUri, ref ou, ref existsFile);
                                this.setVideoComponentTypeValue(ref ou, service);

                                if (existsFile == false)
                                {
                                    this.bw.ReportProgress(progressValue, "get from Internet " + ou.eventtime.value.ToString() + " " + titleMsg.ToString());
                                }
                                else
                                {
                                    if (dispMessageFlag == false)
                                    {
                                        this.bw.ReportProgress(progressValue, "get from Local Cache(first) " + ou.eventtime.value.ToString() + " " + titleMsg.ToString());
                                        dispMessageFlag = true;
                                    }
                                }


                                ou.title.set(titleMsg, ou.duration.value);
                            }
                            catch (Exception e)
                            {
                                this.bw.ReportProgress(progressValue, e.ToString());
                                break;
                            }
                            // cnt++;
                            outList.Add(ou);
                        }
                    }
                }
            }
            return true;
        }

        private bool removeHtmlFile(string filename)
        {
            bool ret = true;
            try
            {
                if (System.IO.File.Exists(filename) == true)
                {
                    System.IO.File.Delete(filename);
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        private bool setVideoComponentTypeValue(ref outputFormatBasic ou, serviceFormat service)
        {
            outputFormatVideo v = new outputFormatVideo(service.DefaultVideoComponentType.value, ref this.rnd);
            ou.videoInfo.RemoveRange(0, 1);
            ou.videoInfo.Insert(0, v);
            return true;
        }
           

        /// <summary>
        /// checkする。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        private List<outputFormatBasic> checkOutList(serviceFormat service, List<outputFormatBasic> org, DateTime start, DateTime end)
        {
            List<outputFormatBasic> result = new List<outputFormatBasic>();
            if(org.Count() == 0){
                return  result;
            }
            IEnumerable<Int32> noDupIdListQuery = (from one in org
                                                   where one.serviceid.value == service.serviceId.value
                                                   select one.eventid.value).Distinct();
            List<Int32> noDupIdList = noDupIdListQuery.ToList<Int32>();
            outputFormatBasic lastEvent = org.First();
            foreach (outputFormatBasic one in org)
            {
                lastEvent = one;
                if (one.serviceid.value != service.serviceId.value)
                {
                    continue;
                }

                if (noDupIdList.Contains(one.eventid.value) == true)
                {
                    noDupIdList.Remove(one.eventid.value);
                    // one.contents.value = one.encoderInfo.rate.value.ToString() + @"kbps\&" + one.contents.value;
                    result.Add(one);
                }
            }
            TimeSpan span = end - start;
            if (span.Days < 10)
            {
                DateTime startTime = lastEvent.eventtime.value.AddMinutes(lastEvent.duration.value);
                for (Int32 i = 1; i <= span.Days; i++)
                {
                    DateTime endTime = new DateTime(startTime.Year, startTime.Month, startTime.Day).AddDays(1);
                    TimeSpan duration = endTime - startTime;
                    outputFormatBasic ou = new outputFormatBasic(service.networkId.value,
                        service,
                        exchangeLetter);
                    ou.eventtime = new epgEventTime(startTime);
                    ou.eventid.value = ou.eventid.setEventIdFromDate(ou.eventtime.value);
                    ou.duration.value = (int)duration.TotalMinutes;
                    ou.videoInfo = lastEvent.videoInfo;
                    ou.audioInfo = lastEvent.audioInfo;
                    ou.title.set("いべんとないのよー");
                    ou.contents.set(@"いべんとないのよー\&でもMEDT-02でエラーさせないためのダミーヽ(´ー｀)ノ");
                    result.Add(ou);

                    startTime = endTime;
                }
            }
            return result;
        }


#if false
            foreach (serviceFormat service in services) 
            {
                IEnumerable<Int32> noDupIdListQuery = (from one in org
                                                       where one.serviceid.value == service.serviceId.value
                                                       select one.eventid.value).Distinct();
                List<Int32> noDupIdList = noDupIdListQuery.ToList<Int32>();
                foreach (outputFormatBasic one in org)
                {
                    if (one.serviceid.value != service.serviceId.value)
                    {
                        continue;
                    }

                    if (noDupIdList.Contains(one.eventid.value) == true)
                    {
                        noDupIdList.Remove(one.eventid.value);
                        result.Add(one);
                    }
                }
            }
            return result;
        }
#endif

        private Int32 outputCsvFile(List<outputFormatBasic> outList) // ServiceFileItem allservice)
        {
            string wname = "";
            FileStream fs = null;
            BinaryWriter binwriteFd = null;
            Int32 ret = 0;
            try
            {
                // 今から出力する予定のファイルを一旦消す
                foreach (outputFormatBasic entry in outList)
                {
                    string filename = makeFilename(entry);
                    this.clearnFile(Path.Combine(this.TargetFolder ,filename));
                }
                foreach (outputFormatBasic entry in outList)
                {
                    string filename = makeFilename(entry);
                    if (wname == "" || wname.CompareTo(filename) != 0)
                    {
                        wname = filename;
                        if (fs != null)
                        {
                            fs.Close();
                            binwriteFd.Close();
                        }
                        fs = new FileStream(Path.Combine(this.TargetFolder ,wname), FileMode.Append);
                        binwriteFd = new BinaryWriter(fs);
                    }

                    entry.makeList();
                    // this.bw.ReportProgress(progressValue, entry.getResult());
                    // this.textBox1.Text += entry.getResult() + "\r\n";
                    binwriteFd.Write(entry.getResultListByte( entry.service));
                    ret++;
                }
            }
            catch (Exception e)
            {
                this.bw.ReportProgress(progressValue, "WriteRoutine Error\r\n" + e.ToString());
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    binwriteFd.Close();
                }
            }
            return ret;

        }
        private bool outputRenewFile(List<serviceFormat> services)
        {
                // Renew.txt 強制作成
            try
            {
                using (StreamWriter wnew = new StreamWriter(Path.Combine(this.TargetFolder ,"Renew.txt"), false))
                {
                    wnew.WriteLine(DateTime.Now.ToString("yyyyMMddHHmmss") + "," + services.Count.ToString());
                    foreach (serviceFormat s in services)
                    {
                        wnew.WriteLine(s.serviceId.value.ToString("000"));
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private string makeFilename(serviceFormat service, DateTime p)
        {
            string filename = service.networkId.value.ToString("00000")
                + "_"
                + service.serviceId.value.ToString("00000")
                + "_"
                + p.ToString("yyMMdd")
                + ".csv";
            return filename;
        }


        private string makeFilename(outputFormatBasic o)
        {
            string filename = o.networkid.value.ToString("00000")
                        + "_"
                        + o.serviceid.value.ToString("00000")
                        + "_"
                        + o.eventtime.value.ToString("yyMMdd")
                        + ".csv";
            return filename;
        }

        private string makeFolder(Int32 service, DateTime date)
        {
            CultureInfo culture = new CultureInfo("en-US");
            string folder = this.temporaryFolderName
                 + service.ToString("000") + @"\"
                 + date.ToString("yyyy") + @"\"
                 + date.ToString("MM") + date.ToString("MMM", culture) + @"\";

            if (System.IO.Directory.Exists(folder) != true)
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private DateTime exchangeIdToDateTime(string idmsg)
        {
            //                        cell-101024     2012        08             14              00            25     -   95
            const string pattern = @"^cell-101024(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})(?<hour>\d{2})(?<min>\d{2})-(?<sep>\d{1,})$";

            Regex x = new Regex(pattern);
            Match m = x.Match(idmsg);
            Int32 year = Int32.Parse(m.Groups["year"].Value);
            Int32 month = Int32.Parse(m.Groups["month"].Value);
            Int32 day = Int32.Parse(m.Groups["day"].Value);
            Int32 hour = Int32.Parse(m.Groups["hour"].Value);
            Int32 min = Int32.Parse(m.Groups["min"].Value);
            Int32 sep = Int32.Parse(m.Groups["sep"].Value);
            return new DateTime(year, month, day, hour, min, 0);
        }


        public static string exchangeJIScodeToString(List<byte> letter)
        {
            List<byte> pre = new List<byte> { 0x1b, 0x24, 0x42 }; // k-in
            List<byte>post = new List<byte>{ 0x1b, 0x28, 0x4a }; // k-out

            pre.AddRange(letter);
            pre.AddRange(post);

           return  System.Text.Encoding.GetEncoding(50220).GetString(pre.ToArray());
        }
    }
}
