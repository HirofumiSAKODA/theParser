using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.Linq;

namespace theParser
{
    [Serializable()]
    public class epgHeader : epgUnitHeader
    {
        public epgHeader(Int32 v) : base(v) { }
    }

    [Serializable()]
    public class epgEncoderRate : epgUnitDec
    {
        public epgEncoderRate()
            : base(5)
        {
            this.value = 0;
            this.max = 99999;
        }

        public epgEncoderRate(Int32 value)
            : this()
        {
            this.value = value;
        }

    }
    [Serializable()]
    public class epgSubtitle : epgUnitBin
    {
        public epgSubtitle()
            : base(1)
        {
            this.value = 0;
        }

        public epgSubtitle(Int32 value)
            : this()
        {
            this.value = value;
        }
    }
    [Serializable()]
    public class epgCopyRestrictionMode : epgUnitDec
    {
        public epgCopyRestrictionMode()
            : base(1)
        {
            this.value = 0;
            this.max = 1;
            this.min = -1;
        }

        public epgCopyRestrictionMode(Int32 value)
            : this()
        {
            this.value = value;
        }
    }

    [Serializable()]
    public class epgImageConstraintToken : epgUnitBin
    {
        public epgImageConstraintToken() : base(1)
        {
            this.value = 1;
        }
        public epgImageConstraintToken(Int32 v)
            : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgRetentionMode : epgUnitBin
    {
        public epgRetentionMode() : base(1)
        {
            this.value = 0;
        }
        public epgRetentionMode(Int32 v)
            : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgRetentionState : epgUnitDec
    {
        public epgRetentionState() : base(1)
        {
            this.value = 7;
        }
        public epgRetentionState(Int32 value)
            : this()
        {
            this.value = value;
        }
    }

    [Serializable()]
    public class epgEncryptionMode : epgUnitBin
    {
        public epgEncryptionMode() : base(1)
        {
            this.value = 0;
        }
        public epgEncryptionMode(Int32 value)
            : this()
        {
            this.value = value;
        }
    }

    [Serializable()]
    public class epgDigitalRecordingControlData : epgUnitDec
    {
        public epgDigitalRecordingControlData()
            : base(1)
        {
            this.value = 0;
            this.max = 3;
            this.min = -1;
        }
        public epgDigitalRecordingControlData(Int32 value)
            : this()
        {
            this.value = value;
        }

        public epgDigitalRecordingControlData(Int32 value, bool debug)
            : this(value)
        {
            if (debug == true)
            {
                this.max = 4;
            }
            this.value = value;
        }

        override public Int32 value
        {
            get { return localvalue; }
            set
            {
                Int32 v = value;
                if (this.max == 4)
                {
                    v = value % (this.max + 1);
                }
                if (v < this.min)
                {
                    localvalue = this.min;
                }
                else if (v > this.max)
                {
                    localvalue = this.max;
                }
                else if (v == 1)
                {
                    localvalue = 3;
                }
                else
                {
                    localvalue = v;
                }
            }
        }
    }
    [Serializable()]
    public class epgCopyControlType : epgUnitDec
    {
        public epgCopyControlType()
            : base(1)
        {
            this.value = 0;
            this.max = 1;
        }

        public epgCopyControlType(Int32 value)
            : this()
        {
            this.value = value;
        }

        public epgCopyControlType(Int32 value, bool debug)
            :this(value)
        {
            if (debug == true)
            {
                this.max = 2;
            }
            this.value = value;
        }

        override public Int32 value
        {
            get
            {
                return base.value;
            }
            set
            {
                Int32 v = value;
                if (this.max == 2)
                {
                    v = value % (this.max + 1);
                }
                base.value = v;
            }
        }
    }
    [Serializable()]
    public class epgAPSControlData : epgUnitDec
    {
        public epgAPSControlData()
            : base(1)
        {
            this.value = 0;
            this.max = 3;
        }

        public epgAPSControlData(Int32 value)
            : this()
        {
            this.value = value;
        }

        public epgAPSControlData(Int32 value, bool debug)
            : this(value)
        {
            if (debug == true)
            {
                this.max = 4;
            }
            this.value = value;
        }

        public override int value
        {
            get
            {
                return base.value;
            }
            set
            {
                Int32 v = value;
                if (this.max == 4)
                {
                    v = (value % (this.max + 1));
                }
                base.value = v;
            }
        }

    }

    [Serializable()]
    public class epgMadaraGroupType : epgUnitDec
    {
        public epgMadaraGroupType() : base(1)
        {
            this.value = 1;
        }
        public epgMadaraGroupType(Int32 value) : this()
        {
            this.value = value;
        }
    }

    [Serializable()]
    public class epgMadaraList : epgUnitText
    {
        public List<string> list;

        public epgMadaraList(): base(80){
            this.list = new List<string>();
        }
        public epgMadaraList(Int32 num) : base(num)
        {
            this.list = new List<string>();
        }

        public override string ToOutput()
        {
            string ret = "";
            for (Int32 count = 0; count < list.Count; count++)
            {
                ret += list[count];
                if (count < (list.Count - 1))
                {
                    ret += "/";
                }
            }
            return ret;
        }
        public void Clear()
        {
            this.list.Clear();
        }
        public void Add(string item)
        {
            this.list.Add(item);
        }
    }


    [Serializable()]
    public class epgGenleBig : epgUnitHex
    {
        // 0～F まで
        public epgGenleBig()
            : base(1)
        {
        }

        public epgGenleBig(Int32 value) : this()
        {
            this.value = value;
        }

        public void valueSetFromParse(string str)
        {
            // 10進数6桁で来るはず。そのうちの上3桁を採用
            if (str.Length != 6)
            {
                this.value = 0x0f;
                return;
            }
            Int32 p = Int32.Parse(str.Substring(0, 3));
            if (p > 100) // 先頭に1がセットされている場合は値あり。
            {
                p -= 100;
            }
            else
            {
                p = 0;
            }
            this.value = p;
            return;
        }
    }

    [Serializable()]
    public class epgGenleSmall : epgUnitHex
    {
        // 0～F まで
        public epgGenleSmall()
            : base(1)
        {
        }
        public epgGenleSmall(Int32 value) : this()
        {
            this.value = value;
        }
        public void valueSetFromParse(string str)
        {
            // 10進数6桁で来るはず。そのうちの上3桁を採用
            if (str.Length != 6)
            {
                this.value = 0x0f;
                return;
            }
            Int32 p = Int32.Parse(str.Substring(3, 3));
            if (p > 100) // 先頭に1がセットされている場合は値あり。
            {
                p -= 100;
            }
            else
            {
                p = 0;
            }
            this.value = p;
            return;
        }
    }

    public enum AUDIOCODE
    {
        JAPANESE /*日本語*/ = 0x6A706E,
        ENGLISH /*英語*/ = 0x656E67,
        DEUTSCH /*ドイツ語*/ = 0x646575,
        FRENCH /*フランス語*/ = 0x667261,
        ITALIANO /*イタリア語*/ = 0x697461,
        RUSSIAN /*ロシア語 */ = 0x727573,
        CHINESE /*中国語 */= 0x7A686F,
        KOREAN /*韓国語*/ = 0x6B6F72,
        SPANISH /*スペイン語*/ = 0x737061,
        OTHER /*その他 */ = 0x657463
    };

    public class AudioCodeData {
        public AUDIOCODE code;
        public Int32 value;
        public string name;

        public AudioCodeData(AUDIOCODE code, Int32 value, string name){
            this.code = code;
            this.value = value;
            this.name = name;
        }
    }

    public class AudioCodeClass {
        public List<AudioCodeData> AudioCodeDataAry;

        public AudioCodeClass()
        {
            AudioCodeDataAry = new List<AudioCodeData>();
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.JAPANESE,0x6A706E, "にほんご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.ENGLISH,0x656E67, "えいご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.DEUTSCH,0x646575, "どいつご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.FRENCH,0x667261, "ふらんすご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.ITALIANO,0x697461, "いたりあご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.RUSSIAN,0x727573, "ろしあご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.CHINESE,0x7A686F, "ちゅうごくご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.KOREAN,0x6B6F72, "かんこくご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.SPANISH,0x737061, "すぺいんご"));
            AudioCodeDataAry.Add(new AudioCodeData(AUDIOCODE.OTHER,0x657463, "そのほか"));
        }

        public Int32 GetCodeCycle(Int32 index)
        {
            AudioCodeData p = AudioCodeDataAry[ index % AudioCodeDataAry.Count() ];
            return p.value;
        }
        public string GetNameCycle(Int32 index)
        {
            AudioCodeData p = AudioCodeDataAry[ index % AudioCodeDataAry.Count() ];
            return p.name;
        }

        public string GetName(AUDIOCODE code){
            var o = from p in AudioCodeDataAry
                where p.code == code
                select p;
            if( o.Count() != 1 ){
                return AudioCodeDataAry.Last().name;
            }
            return o.First().name;
        }
        public string GetName(Int32 value)
        {
            var o = from p in AudioCodeDataAry
                    where p.value == value
                    select p;
            if (o.Count() != 1)
            {
                return AudioCodeDataAry.Last().name;
            }
            return o.First().name;
        }

    }

    [Serializable()]
    public class epgAudioCode : epgUnitHex
    {
        public epgAudioCode() : base(6)
        {
        }
        public epgAudioCode(AUDIOCODE v) : this()
        {
            this.value = (Int32) v;
        }
        public epgAudioCode(Int32 v, AudioCodeClass au) : this()
        {
            this.value = au.GetCodeCycle(v);
        }
        public epgAudioCode(Int32 v) : this()
        {
            v = v % Enum.GetNames(typeof(AUDIOCODE)).Length;
            if (Enum.IsDefined(typeof(AUDIOCODE), v) == true)
            {
                this.value = v;
            }
            else
            {
                this.value = (Int32)AUDIOCODE.JAPANESE;
            }
        }

    }

    [Serializable()]
    public class epgAudioName : epgUnitText
    {
        private string _main = "";
        public string main
        {
            get { return _main; }
            set { Convert(value, out _main); }
        }
        private string _sub = "";
        public string sub
        {
            get { return _sub; }
            set { Convert(value, out _sub); }
        }
        public epgAudioName()
            : base(16)
        {
        }

        public epgAudioName(string v) : this()
        {
            this.main = v;
        }

        public bool SetSubName(string v)
        {
            this.sub = v;
            return true;
        }


        public override string ToOutput()
        {
            string ret = this.main;
            if (this.sub.Length != 0)
            {
                ret += @"\&" + this.sub;
            }
            return ret;
        }
    }

    [Serializable()]
    public class epgAudioQuority : epgUnitDec
    {
        public epgAudioQuority() : base(1)
        {
        }
        public epgAudioQuority(Int32 v) : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgAudioSamplingRate : epgUnitDec
    {
        public epgAudioSamplingRate()
            : base(1)
        {
        }
        public epgAudioSamplingRate(Int32 v)
            : this()
        {
            this.value = v;
        }
    }
    [Serializable()]
    public class epgAudioSimulGroup : epgUnitHex
    {
        public epgAudioSimulGroup()
            : base(2)
        {
        }
        public epgAudioSimulGroup(Int32 v)
            : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgTag : epgUnitHex
    {
        public epgTag() : base(2)
        {
        }
        public epgTag(Int32 v) : this()
        {
            this.value = v;
        }
    }


    [Serializable()]
    public class epgAudioStreamType : epgUnitHex
    {
        public epgAudioStreamType() : base(2)
        {
        }
        public epgAudioStreamType(Int32 v) : this()
        {
            this.set(v);
        }
        public class streamTypeClass
        {
            public int code;
            public string name;
            public string nameL;
            public List<epgAudioComponentType> typeList;

            public streamTypeClass(int code, string name, int[] list)
            {
                this.code = code;
                this.name = name;
                this.nameL = Microsoft.VisualBasic.Strings.StrConv(
                    name, Microsoft.VisualBasic.VbStrConv.Wide, 0x411); // 全角変換
                typeList = new List<epgAudioComponentType>();
                foreach( int p in list ){
                    typeList.Add(new epgAudioComponentType(p));
                }
            }
        }

        streamTypeClass[] AudioDataAry = {
                new streamTypeClass(0x04, "MPEG2BC", new int[]{1,2,3}),
                new streamTypeClass(0x0F, "MPEG2AAC", new int[]{ 1, 3, 2, 9 }), 
                new streamTypeClass(0x11, "MPEG4AAC", new int[]{ 3,9,0xe, 0x11}),
                new streamTypeClass(0x1C, "MPEG4ALC", new int[]{ 3, 9} ),
        };

        public bool set(Int32 v)
        {
            bool ret = true;
            var o = from p in AudioDataAry
                    where p.code == v
                    select p;
            if(o.Count() == 1){
                this.value = v;
            } else {
                System.Diagnostics.Debug.Assert(true,"その値は使えない");
                ret = false;
                this.clear();
            }
            return ret;
        }

        public bool IsOkType( epgAudioComponentType v ){
            bool ret = true;
            var o = from p in AudioDataAry
                    where p.code == this.value
                    select p;
            if( o.Count() != 1 ){
                return false;
            }
            streamTypeClass my = o.First();

            var oo = from pp in my.typeList
                where pp == v
                select pp;
            if(oo.Count() != 1){
            return false;
            }
            return true;
        }

        public bool IsOkType( Int32 v ){
            return IsOkType(new epgAudioComponentType(v));
        }

        public Int32 GetComponentType(Int32 index){
            Int32 ret = 3; // streo
            var o = from p in AudioDataAry
                    where p.code == this.value
                    select p;
            if (o.Count() != 1)
            {
                return ret;
            }
            ret = o.First().typeList[index % o.First().typeList.Count()].value;
            return ret;
        }

    }

    [Serializable()]
    public class epgVideoComponentTag : epgUnitHex
    {
        public epgVideoComponentTag() : base(2)
        {
        }
        public epgVideoComponentTag(Int32 v) : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgVideoComponentType : epgUnitHex
    {
        public epgVideoComponentType() : base(2)
        {
        }
        public epgVideoComponentType(Int32 v) : this()
        {
            this.value = v;
        }
    }

    [Serializable()]
    public class epgVideoType : epgUnitText
    {
        public epgVideoType(string str) : base(16)
        {
            this.value = str;
        }
    }





    [Serializable()]
    public class epgAudioComponentType : epgUnitHex
    {
    /*
     * 1/0（シングルモノ）		:1
1/0+1/0（デュアルモノ）	:2
2/0（ステレオ）		:3
3/1				:7
3/2				:8
3/2+LFE(5.1ch)		:9   
7.1ch				:E
22ch				:11
*/
        public epgAudioComponentType() : base(2)
        {
        }
        public epgAudioComponentType(Int32 v) : this()
        {
            this.value = v;
        }
        public bool IsDualMono()
        {
            return (this.value == 2);
        }

    }
    [Serializable()]
    public class epgAudioComponentTag : epgUnitHex
    {
        public epgAudioComponentTag() : base(0)
        {
        }
        public epgAudioComponentTag(Int32 v) : this()
        {
            this.value = v;
        }
    }
    [Serializable()]
    public class epgAudioMultiLingul : epgUnitBin
    {
        public epgAudioMultiLingul() : base(1)
        {
            this.value = 0;
        }
        public epgAudioMultiLingul(Int32 v) : this()
        {
            this.value = v;
        }
    }
    [Serializable()]
    public class epgMainFlag : epgUnitBin
    {
        public epgMainFlag() : base(1)
        {
        }

        public epgMainFlag(Int32 v) : this()
        {
            if (v == 0)
            {
                this.value = 0;
            }
            else
            {
                this.value = 1;
            }
        }
        public epgMainFlag(bool f) : this()
        {
            this.value = (f==true)?1:0;
        }
    }

    [Serializable()]
    public class epgPayment : epgUnitBin
    {
        public epgPayment() : base(1)
        {
            this.value = 0;
        }
        public epgPayment(Int32 v) : this()
        {
            if (v == 0)
            {
                this.value = 0;
            }
            else
            {
                this.value = 1;
            }
        }

    }

    [Serializable()]
    public class epgDuration : epgUnitDec
    {
        public Int32 oneDay;

        public epgDuration() : base(0)
        {
            this.setMinMax(1, 60 * 24 * 2);
            this.oneDay = 60 * 24;
        }

        public epgDuration(Int32 v) : this()
        {
            this.setMinMax(1, 60 * 24 * 2);
            this.oneDay = 60 * 24;
            base.value = v;
        }

        public void setFromHeight(Int32 v)
        {
            this.value = (v + 3) / 3;
        }

        public override string ToOutput()
        {
            Int32 h = this.value / 60;
            Int32 m = this.value % 60;
            return h.ToString("00") + m.ToString("00") + "00";
        }
    }

    [Serializable()]
    public class epgTitle : epgUnitText
    {
        public epgTitle(string str) : base(80)
        {
            this.value = str;
        }
        public epgTitle(string p, ExchangeLetter exchangeLetter) : this(p)
        {
            this.exchangeLetter = exchangeLetter;
        }

        public void set(string str, Int32 duration)
        {
            if (duration < 31)
            {
                this.numberOf = 40;
            }
            this.Convert(str, out this.value);
        }


    }

    [Serializable()]
    public class epgDescribe : epgUnitText
    {
        public epgDescribe(string str) : base(160)
        {
            this.value = str;
        }

        public epgDescribe(string p, ExchangeLetter exchangeLetter) : this(p)
        {
            this.exchangeLetter = exchangeLetter;
        }
    }

    [Serializable()]
    public class epgExtTitle : epgUnitText
    {
        public epgExtTitle()
            : base(16)
        {
        }

        public epgExtTitle(string str) : this()
        {
            this.Convert(str.Trim(), out this.value);
        }
        public epgExtTitle(string p, ExchangeLetter exchangeLetter) : this(p)
        {
            this.exchangeLetter = exchangeLetter;
        }
    }

    [Serializable()]
    public class epgExtDescription : epgUnitText
    {
        public epgExtDescription() : base(220) { }
        public epgExtDescription(string str) : this()
        {
            this.Convert(str.Trim(), out this.value);
        }
        public epgExtDescription(string p, ExchangeLetter exchangeLetter) : this(p)
        {
            this.exchangeLetter = exchangeLetter;
        }


    }


    public enum NETWORKID : int
    {
        PERFECT = 1,
        SKY = 3,
        SELF = 65406,
        IHITS = 65534
    }

    [Serializable()]
    public class epgNetowrkId : epgUnitDec
    {
        public epgNetowrkId(): base(0)
        {
        }

        public epgNetowrkId(Int32 v) : this()
        {
            this.setMinMax(1, 65535);
            NETWORKID kind = (NETWORKID)v;
            if (kind == NETWORKID.PERFECT || kind == NETWORKID.SKY 
                || kind == NETWORKID.SELF || kind == NETWORKID.IHITS)
            {
                base.value = v;
            }
            else
            {
                this.value = (int)NETWORKID.SELF;
            }
        }

    }

    [Serializable()]
    public class epgServiceId : epgUnitDec
    {
        public epgServiceId()
            : base(0)
        {
        }
        public epgServiceId(Int32 v) : this()
        {
            this.setMinMax(0, 999);
            this.value = v;
        }
    }

    [Serializable()]
    public class epgParentalRate : epgUnitHex
    {
        public epgParentalRate()
            : base(0)
        {
            this.prefix = false;
            this.numberOf = 2;
        }
        public epgParentalRate(Int32 v)
            : this()
        {
            this.setMinMax(-1,0xff);
            this.value = v;
        }
    }

    [Serializable()]
    public class epgEventId : epgUnitDec
    {
        public override void clear()
        {
            this.value += 10;
            if (this.value > this.max)
            {
                this.value = this.min;
            }
        }

        public epgEventId()
            : base(0)
        {
        }

        public epgEventId(Int32 v) : this()
        {
            this.setMinMax(1, 65535);
            this.value = v;
        }

        public Int32 setEventIdFromDate(DateTime p)
        {
            Int32 ret = 1;
            // Day,Hour,Min を Min に換算し、EventId化して返す

            ret = (p.Day * 24 + p.Hour) * 60 + p.Minute;
            return ret;
        }
    }
    [Serializable()]
    public class epgParentalCode : epgUnitText
    {
        public epgParentalCode() : base(10)
        {
        }
        public epgParentalCode(string code) : base(10,code)
        {
        }
    }

    [Serializable()]
    public class epgEventTime : epgUnitDateTime
    {
        public epgEventTime(string str)
            : base(str)
        {
            this.timeOnly = true;
        }

        public epgEventTime(DateTime t)
            : base(t)
        {
            this.timeOnly = true;
        }

        public epgEventTime() : base() { }
    }

    public class copyControlData
    {
        public string descripion;
        public outputFormatCopyControl copyControl;
        public outputFormatContentAvailability contents;
        /*
            コピーコントロールの候補
            CopyControlTYpe   DigitalRecordingControl  RestrictionMode
  制御しない     #または0                #                       #
  無制限         1                       0                       #
  ねばー         1                       3                       #
  ダビ10         1                       2                       1
  コピワン       1                       2                       0
         */
        public copyControlData(Int32 pattern)
        {
            this.contents = new outputFormatContentAvailability();
            this.copyControl = new outputFormatCopyControl();
            Int32 p = (pattern % 6);
            switch (p)
            {
            case 1:
                this.descripion = "制御しない";
                this.copyControl.copyControl.value = 0;
                this.copyControl.recoriding.value = 0;
                this.contents.restrictionmode.value = 0;
                break;
            case 2:
                this.descripion = "無制限";
                this.copyControl.copyControl.value = 1;
                this.copyControl.recoriding.value = 1;
                this.contents.restrictionmode.value = 0;
                break;
            case 3:
                this.descripion = "ねばー";
                this.copyControl.copyControl.value = 1;
                this.copyControl.recoriding.value = 3;
                this.contents.restrictionmode.value = 0;
                break;
            case 4: // ダビ10
                this.descripion = "ダビ10";
                this.copyControl.copyControl.value = 1;
                this.copyControl.recoriding.value = 2;
                this.contents.restrictionmode.value = 1;
                break;
            case 5: // こぴわん
                this.descripion = "コピワン";
                this.copyControl.copyControl.value = 1;
                this.copyControl.recoriding.value = 2;
                this.contents.restrictionmode.value = 0;
                break;
            default:
                this.contents = null;
                this.copyControl = null;
                break;
            }
        }

    }
}
