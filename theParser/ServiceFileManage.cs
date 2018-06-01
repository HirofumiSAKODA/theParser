using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace theParser
{
    public class serviceFormat
    {
        public epgNetowrkId networkId = null;
        public epgServiceId serviceId = null;
        public string serviceName = null;
        public string uri = "";
        public bool useExchangeLetter = true;
        public bool forceJlab035 = false;
        public int forceJlab035_videokind = 265;
        public epgVideoComponentType DefaultVideoComponentType = new epgVideoComponentType(0xb3);

        /// <summary>まだらグループ名 </summary>
        public string madaraGroupName = "";
        /// <summary>パターン表の番号(１origin)</summary>
        public Int32 madaraType = -1;
        /// <summary>まだらパターン適応規則(0origin)</summary>
        public Int32 madaraMethod = -1;
        /// <summary>変換後のServiceID</summary>
        public Int32 madaraToChNum = -1;
        /// <summary>ServiceIDの順序でカウントした「同まだらグループ内の順序」を入れる。</summary>
        public Int32 madaraSeqNum = -1;
        /// <summary>初期解像度。無効値の場合はパターン表のTOPを適応</summary>
        public string madaraResolution = "";
        /// <summary>EPG情報のソース。無効値の場合は「allkanji」</summary>
        public string madaraSource = "";
        public Int32[] madaraDurations = new Int32[] { 5, 10, 60 };
        /// <summary>最若ServiceIDを「親」。親へのリンク</summary>
        public serviceFormat parent = null;
        /// <summary>親以外を全部リストにして積む</summary>
        public List<serviceFormat> children = new List<serviceFormat>();

        public List<outputFormatBasic> outList;

        public List<string> paramList;

        public enum MyType { NONE, URI, ALLKANJI, MADARA, INTERBEE,MAXDATA };
        public MyType type = MyType.NONE;

        public serviceFormat(string net, string service, string name)
        {
            this.networkId = new epgNetowrkId(Convert.ToInt32(net, 16));
            this.serviceId = new epgServiceId(Convert.ToInt32(service));
            this.paramList = new List<string>();
            // this.serviceName = new epgTitle(name);
        }
        public serviceFormat(string net, string service, string name, string uri)
        {
            this.networkId = new epgNetowrkId(Convert.ToInt32(net, 16));
            this.serviceId = new epgServiceId(Convert.ToInt32(service));
            this.serviceName = name;
            this.paramList = new List<string>();

        }
        public serviceFormat(string[] param)
        {
            if (param.Count() < 4) return;
            this.paramList = new List<string>(param);
            this.networkId = new epgNetowrkId(Convert.ToInt32(paramList[0].Trim(), 16));
            this.serviceId = new epgServiceId(Convert.ToInt32(paramList[1].Trim()));
            this.serviceName = paramList[2].Trim();

            switch (paramList[3].Trim().ToLower())
            {
            case "interbee":
                this.type = MyType.INTERBEE;
                this.uri = paramList[4].Trim(); // Table 1 か 2 か
                break;
            case "maxdata":
                this.type = MyType.MAXDATA;
                this.uri = paramList[4].Trim();
                break;
            case "allkanji":
                this.type = MyType.ALLKANJI;
                break;
            case "madara":
                if (param.Count() < 9 ) return;
                this.type = MyType.MADARA;
                this.madaraGroupName = param[4].Trim();
                Int32.TryParse(paramList[5].Trim(), out this.madaraType);
                Int32.TryParse(paramList[6].Trim(), out this.madaraMethod);
                Int32.TryParse(paramList[7].Trim(), out this.madaraToChNum);
                this.madaraResolution = paramList[8].Trim();
                if (param.Count() > 9)
                {
                    this.madaraSource = paramList[9].Trim();
                    this.uri = paramList[9].Trim();
                }
                if (paramList.Count() > 10)
                {
                    string[] durations = paramList[10].Trim().Split('/');
                    List<Int32> durationAry = new List<int>();
                    foreach (string duration in durations)
                    {
                        Int32 du = 30;
                        Int32.TryParse(duration, out du);
                        durationAry.Add(du);
                    }
                    this.madaraDurations = durationAry.ToArray();
                }

                break;
            default:
                this.type = MyType.URI;
                this.uri = paramList[3].Trim();
                this.useExchangeLetter = true;
                break;
            }
            if (paramList.Count() > 3) // 他のパラメータを取得する。
            {
                
                if ( paramList.Find(x => x == "jlab035") != ""){
                    this.forceJlab035 = true;
                }

                if( string.IsNullOrEmpty(paramList.Find(x => x == "035-264"))){
                    this.forceJlab035_videokind = 265;
                } else {
                    this.forceJlab035_videokind = 264;
                }
                // ARIB 外字強制変換を使わない？
                if ( paramList.Find(x => x == "noExtChar") != ""){
                    this.useExchangeLetter = false;
                }
                // VideoComponentType 値 初期値変更
                List<string> videoList =  paramList.FindAll( x => x.StartsWith("videotype=0x"));
                if (videoList.Count() > 0)
                {
                    foreach(string v in videoList){
                        int typeValue = 0xb3;
                        string value = v.Trim().Substring("videotype=0x".Length);
                        int.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out typeValue);
                        this.DefaultVideoComponentType.value = typeValue;
                    }
                }
            }
        }

     }


    public class ServiceFileItem
    {
        public DateTime updateTime = DateTime.MinValue;
        public List<serviceFormat> services = new List<serviceFormat>();

        public bool makeRenewTxtFile(string path)
        {


            return true;
        }
    }

}
