using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;

namespace theParser
{
    [Serializable()]
    public abstract class outputFormatExtList : IComparable
    {
        public List<epgUnit> paramList{set;get;}
        public abstract void makeList();
        public abstract int CompareTo(object obj);
    }

    [Serializable()]
    public abstract class outputFormatExtLine : IComparable
    {
        public List<epgUnit> paramList{set;get;}
        public abstract void makeList();
        public abstract int CompareTo(object obj);
    }

    [Serializable()]
    public class outputFormatEncoderInfo : outputFormatExtLine
    {
        private epgTitle tag = null;
        public epgEncoderRate rate = null;

        public outputFormatEncoderInfo()
        {
            this.tag = new epgTitle("ENC");
            this.rate = new epgEncoderRate();
        }
        public override int CompareTo(object obj)
        {
            outputFormatEncoderInfo p = (obj as outputFormatEncoderInfo);
            return this.rate.CompareTo(p.rate);
        }

        /// <summary>
        /// 本行を表示したくない場合はこれを呼ぶ。
        /// </summary>
        public void clear()
        {
            this.rate = null;
        }

        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            if (this.rate != null)
            {
                this.paramList.Add(this.tag);
                this.paramList.Add(this.rate);
            }
        }

    }
    [Serializable()]
    public class outputFormatCCJ : outputFormatExtLine
    {
        private epgTitle tag = null;
        public epgSubtitle subtitle = null;

        public outputFormatCCJ()
        {
            this.tag = new epgTitle("CCJ");
            this.subtitle = new epgSubtitle(0);
        }
        public override int CompareTo(object obj)
        {
            return this.subtitle.value.CompareTo((obj as epgSubtitle).value);
        }
        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            this.paramList.Add(this.tag);
            this.paramList.Add(this.subtitle);
        }
    }

    [Serializable()]
    public class outputFormatParental : outputFormatExtLine
    {
        private epgTitle tag = null;
        private epgParentalCode code;
        public epgParentalRate rate = null;

        public outputFormatParental()
        {
            this.tag = new epgTitle("55");
            this.code = new epgParentalCode("4A504E");
            this.rate = new epgParentalRate(-1); // 未使用
        }
        public override int CompareTo(object obj)
        {
            return this.rate.value.CompareTo((obj as epgParentalRate).value);
        }
        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            this.paramList.Add(this.tag);
            this.paramList.Add(this.code);
            this.paramList.Add(this.rate);
        }
    }


    [Serializable()]
    public class outputFormatContentAvailability : outputFormatExtLine
    {
        private epgTag tag = null;
        public epgCopyRestrictionMode restrictionmode = null;
        public epgImageConstraintToken imageConstraint = null;
        public epgRetentionMode retentionMode = null;
        public epgRetentionState retentionState = null;
        public epgEncryptionMode encryptionMode = null;

        public Int32 maxPattern
        {
            get
            {
                return this.restrictionmode.max;
            }
        }

        public outputFormatContentAvailability()
        {
            this.tag = new epgTag(0xDE);
            this.restrictionmode = new epgCopyRestrictionMode();
            this.imageConstraint = new epgImageConstraintToken();
            this.retentionMode = new epgRetentionMode();
            this.retentionState = new epgRetentionState();
            this.encryptionMode = new epgEncryptionMode();
        }

        public override int CompareTo(object obj)
        {
            int ret;
            outputFormatContentAvailability p = (obj as outputFormatContentAvailability);
            ret = this.restrictionmode.CompareTo(p.restrictionmode);
            if (ret != 0) return ret;
            ret = this.imageConstraint.CompareTo(p.imageConstraint);
            if (ret != 0) return ret;
            ret = this.retentionMode.CompareTo(p.retentionMode);
            if (ret != 0) return ret;
            ret = this.retentionState.CompareTo(p.retentionState);
            if (ret != 0) return ret;
            return this.encryptionMode.CompareTo(p.encryptionMode);
        }
        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            this.paramList.Add(this.tag);
            this.paramList.Add(this.restrictionmode);
            this.paramList.Add(this.imageConstraint);
            this.paramList.Add(this.retentionMode);
            this.paramList.Add(this.retentionState);
            this.paramList.Add(this.encryptionMode);
        }
    }

    [Serializable()]
    public class outputFormatCopyControl : outputFormatExtLine
    {
        private epgTag tag = null;
        public epgDigitalRecordingControlData recoriding;
        public epgCopyControlType copyControl;
        public epgAPSControlData aps;

        public Int32 maxPattern
        {
            get { return this.copyControl.max * this.recoriding.max * this.aps.max; }
        }

        public outputFormatCopyControl()
        {
            this.tag = new epgTag(0xC1);
            this.recoriding = new epgDigitalRecordingControlData();
            this.copyControl = new epgCopyControlType();
            this.aps = new epgAPSControlData();
        }


        public void Clear()
        {
            this.recoriding = new epgDigitalRecordingControlData();
            this.copyControl = new epgCopyControlType();
            this.aps = new epgAPSControlData();
        }
        public override int CompareTo(object obj)
        {
            int ret;
            outputFormatCopyControl p = (obj as outputFormatCopyControl);
            ret = this.recoriding.CompareTo(p.recoriding);
            if (ret != 0) return ret;
            ret = this.copyControl.CompareTo(p.copyControl);
            if (ret != 0) return ret;
            return this.aps.CompareTo(p.aps);
        }
        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            /*
            if (this.copyControl.value == 0)
            {
                return;
            }
            */
            this.paramList.Add(this.tag);
            this.paramList.Add(this.recoriding);
            this.paramList.Add(this.copyControl);
            this.paramList.Add(this.aps);
        }
    }



    [Serializable()]
    public class outputFormatMadara : outputFormatExtLine
    {

        private epgTag tag = null;
        private epgMadaraGroupType groupType;
        public epgMadaraList serviceList;
        public epgMadaraList eventIdList;


        public outputFormatMadara()
        {
            this.tag = new epgTag(0xd6);
            this.groupType = new epgMadaraGroupType();
            this.serviceList = new epgMadaraList();
            this.eventIdList = new epgMadaraList();
        }

        public void Clear()
        {
            this.serviceList.Clear();
            this.eventIdList.Clear();
        }


        public override int CompareTo(object obj)
        {
            string a = this.serviceList.ToOutput();
            string aa = (obj as outputFormatMadara).serviceList.ToOutput();
            string b = this.eventIdList.ToOutput();
            string bb = (obj as outputFormatMadara).eventIdList.ToOutput();

            Int32 ret = a.CompareTo(aa);
            if (ret == 0)
            {
                return b.CompareTo(bb);
            }
            return ret;
        }

        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            if (this.serviceList.list.Count == 0 || this.eventIdList.list.Count == 0)
            {
                return;
            }
            this.paramList.Add(this.tag);
            this.paramList.Add(this.groupType);
            this.paramList.Add(this.serviceList);
            this.paramList.Add(this.eventIdList);
        }

    }




    [Serializable()]
    public class outputFormatGenle : outputFormatExtList
    {
        public epgTag tag = null;
        public epgGenleBig genleBig = null;
        public epgGenleSmall genleSmall = null;

        public override int CompareTo(object obj)
        {
            return this.genleBig.CompareTo((obj as outputFormatGenle).genleBig);
        }
        public outputFormatGenle()
        {
            this.tag = new epgTag(0x54);
            this.genleBig = new epgGenleBig();
            this.genleSmall = new epgGenleSmall();
            this.paramList = new List<epgUnit>();
        }

        // public List<epgUnit> paramList {set;get;}

        public override void makeList()
        {
            paramList.Add(this.tag);
            paramList.Add(this.genleBig);
            paramList.Add(this.genleSmall);
        }

        [Serializable()]
        public struct genleTableStruct
        {
            public Int32 big;
            public Int32 smallLimit;

            public genleTableStruct(int p1, int p2)
            {
                // TODO: Complete member initialization
                this.big = p1;
                this.smallLimit = p2;
            }
        }

        List<genleTableStruct> genleTable =
new List<genleTableStruct>(){
new genleTableStruct(0,0xf),
new genleTableStruct(1,0xf),
new genleTableStruct(2,0xf),
new genleTableStruct(3,0xf),
new genleTableStruct(4,0xf),
new genleTableStruct(5,0xf),
new genleTableStruct(6,0xf),
new genleTableStruct(7,0xf),
new genleTableStruct(8,0xf),
new genleTableStruct(9,0xf),
new genleTableStruct(0xa,0xf),
new genleTableStruct(0xb,0xf),
new genleTableStruct(0xc,0xf),
new genleTableStruct(0xd,0xf),
new genleTableStruct(0xe,0xf),
new genleTableStruct(0xf,0xf),
};

// new genleTableStruct(0,0xa),
// new genleTableStruct(1,0xa),
// new genleTableStruct(2,0x7),
// new genleTableStruct(3,0x2),
// new genleTableStruct(4,0xa),
// new genleTableStruct(5,0x6),
// new genleTableStruct(6,0x2),
// new genleTableStruct(7,0x2),
// new genleTableStruct(8,0x8),
// new genleTableStruct(9,0x4),
// new genleTableStruct(0xa,0xc),
// new genleTableStruct(0xb,0x6),
// new genleTableStruct(0xf,0x0),


        public bool getGenleNext(ref Int32 big, ref Int32 small)
        {
            if (small == 0xf)
            {
                big++;
                small = 0x0;
            }
            else
            {
                small++;
            }

            Int32 bigTemp = big;

            var nums = from p in genleTable
                       where p.big == bigTemp
                       select p;
            if (nums.Count() == 0)
            {
                big = 0;
                small = 0;
                return true;
            }
            genleTableStruct pp = nums.First<genleTableStruct>();
            if (pp.smallLimit < small)
            {
                small = 0xf;
                // big はそのまま
                return true;
            }
            return true;
        }
    }

    [Serializable()]
    public class outputFormatVideo : outputFormatExtList
    {
        public epgTag tag = null;
        public epgVideoComponentTag componentTag = null;
        public epgVideoComponentType componentType = null;
        public epgVideoType videoType = null;
        private List<int> videoTypeAry = new List<int> { 1, 3, 4, 0xb1, 0xb2, 0xb4, 0x98 ,0xe1,0xe3,0xe4};
        private List<int> videoTypeAry265 = new List<int> { 0xb1, 0xb2, 0xb4, 0x93 ,0xe1,0xe3,0xe4};
        private List<int> videoTypeAry264 = new List<int> { 0xb1, 0xb2, 0xb4};

        // public List<epgUnit> paramList { get; set; }

        public override int CompareTo(object obj)
        {
            return this.componentTag.CompareTo((obj as outputFormatVideo).componentTag);
        }
        public outputFormatVideo()
        {
            this.tag = new epgTag(0x50);
            this.componentTag = new epgVideoComponentTag(0);
            this.componentType = new epgVideoComponentType(0xb3);
            this.videoType = new epgVideoType("HD動画");
            this.paramList = new List<epgUnit>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">265/264</param>
        /// <param name="index"></param>
        public outputFormatVideo(int type, int index) : this() 
        {
            if( type == 264 ){
                this.componentType = new epgVideoComponentType(videoTypeAry264[index % videoTypeAry264.Count()]);
            } else {
                this.componentType = new epgVideoComponentType(videoTypeAry265[index % videoTypeAry265.Count()]);
            }
            setType(this.componentType.value);
        }

        public outputFormatVideo(string type)
            : this()
        {
            string name = "HD映像";
            Int32 num = 0xb3;
            if (type == "HD")
            {
                name = "HD";
                num = 0xb3;
            }
            else // if (type == "SD")
            {
                name = "SD";
                num = 1;
            }
            this.tag = new epgTag(0x50);
            this.componentTag = new epgVideoComponentTag(0);
            this.componentType = new epgVideoComponentType(num);
            this.videoType = new epgVideoType(name);
        }


        public outputFormatVideo(Int32 type, ref Random rnd)
            : this()
        {
            int v = type;
            if (v == 0)
            {
                v = this.videoTypeAry[rnd.Next(this.videoTypeAry.Count())];
            }
            setType(v);
            this.tag = new epgTag(0x50);
            this.componentTag = new epgVideoComponentTag(0);
        }
        public outputFormatVideo(Int32 type)
            : this()
        {
            setType(type);
            this.tag = new epgTag(0x50);
            this.componentTag = new epgVideoComponentTag(0);
        }
        private void setType( Int32 type)
        {
            string name = "";
            switch (type)
            {
            case 1:
                name = "480i-4:3";
                break;
            case 3:
                name = "480i-16:9";
                break;
            case 4:
                name = "480i-16:9上";
                break;
            case 0xb1:
                name = "1080i-4:3";
                break;
            case 0xb3:
                name = "1080i-16:9";
                break;
            case 0xb4:
                name = "1080i-16:9上";
                break;
            case 0xe1:
                name = "1080p-4:3";
                break;
            case 0xe3:
                name = "1080p-16:9";
                break;
            case 0xe4:
                name = "1080p-16:9上";
                break;
            case 0x93:
                name = "2160p-16:9";
                break;
            default:
                type = 0xb3;
                name = "1080i-16:9";
                break;
            }
            this.componentType = new epgVideoComponentType(type);
            this.videoType = new epgVideoType(name);
        }

        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            paramList.Add(this.tag);
            paramList.Add(this.componentTag);
            paramList.Add(this.componentType);
            paramList.Add(this.videoType);
        }
    }

    [Serializable()]
    public class outputFormatAudio : outputFormatExtList
    {
        private bool bForce035 = false;

        public epgTag tag = null;
        public epgAudioComponentTag componentTag = null;
        public epgAudioComponentType componentType = null;
        public epgAudioMultiLingul esMultiFlag = null;
        public epgMainFlag mainFlag = null;
        public epgAudioQuority quority = null;
        public epgAudioSamplingRate samplingRate = null;
        public epgAudioCode mainCode = null;
        public epgAudioCode subCode = null;
        public epgAudioName name = null;
        public epgAudioStreamType streamType = null;
        public epgAudioSimulGroup simulGroup = null;

        // public List<epgUnit> paramList {get;set;}

        public override int CompareTo(object obj)
        {
            return this.componentTag.CompareTo((obj as outputFormatAudio).componentTag);
        }
        public outputFormatAudio(bool Force035 = false)
        {
            this.bForce035 = Force035;

            this.paramList = new List<epgUnit>();
            this.tag = new epgTag(0xc4);
            this.componentTag = new epgAudioComponentTag(0x10);
            this.componentType = new epgAudioComponentType(3);
            this.esMultiFlag = new epgAudioMultiLingul(0);
            this.mainFlag = new epgMainFlag(1);
            this.quority = new epgAudioQuority(2);
            this.samplingRate = new epgAudioSamplingRate(5);
            this.mainCode = new epgAudioCode(AUDIOCODE.JAPANESE);
            this.subCode = null; // new epgAudioCode(0);
            this.name = new epgAudioName("日本語");
            this.streamType = new epgAudioStreamType(0x0f);
            this.simulGroup = new epgAudioSimulGroup(-1);
        }

        private Int32[] valuesOfSimulGroup = { -1, 0, 0x1, 0x10, 0xff };

        public outputFormatAudio(int tag, int type, ref int index, bool Force035 = false) : this(Force035){
            this.componentTag = new epgAudioComponentTag(tag);
            this.streamType = new epgAudioStreamType(type);
            Debug.Assert(this.streamType.value > 0 );
            this.componentType = new epgAudioComponentType(this.streamType.GetComponentType(index));
            this.mainFlag = new epgMainFlag(0);
            this.quority = new epgAudioQuority(2);
            this.samplingRate = new epgAudioSamplingRate(5);
            AudioCodeClass au = new AudioCodeClass();
            this.mainCode = new epgAudioCode(index, au);
            this.name = new epgAudioName( au.GetName(this.mainCode.value));
            if( this.componentType.IsDualMono() ){
                this.subCode = new epgAudioCode(index+1, au);
                this.name.SetSubName(au.GetName(this.subCode.value));
                this.esMultiFlag = new epgAudioMultiLingul(1);
            }
            else {
                this.esMultiFlag = new epgAudioMultiLingul(0);
                this.subCode = null; // new epgAudioCode(0);
            }
            // とりあえず simul group は、なし、00、01、10、FF を回してみる。
            Int32 simul = valuesOfSimulGroup[index % valuesOfSimulGroup.Count() ];
            this.simulGroup = new epgAudioSimulGroup(simul);
            // return true;
        }

        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            this.paramList.Add(this.tag);
            this.paramList.Add(this.componentTag);
            this.paramList.Add(this.componentType);
            this.paramList.Add(this.esMultiFlag);
            this.paramList.Add(this.mainFlag);
            this.paramList.Add(this.quority);
            this.paramList.Add(this.samplingRate);
            this.paramList.Add(this.mainCode);
            this.paramList.Add(this.subCode);
            this.paramList.Add(this.name);
            this.paramList.Add(this.streamType);
            if( bForce035 ){
                this.paramList.Add(this.simulGroup);
            }
        }


        /*

            1音声毎に、要求される都度に以下を順番に取得し、加えてその日本語の名称を(*2),(*3)に
            並べる。					
            -------------------
			日本語:6A706E
			英語:656E67
			ドイツ語:646575
			フランス語:667261
			イタリア語:697461
			ロシア語:727573
			中国語:7A686F
			韓国語:6B6F72
			スペイン語:737061
			その他:657463
			なし:#
            -------------------

            component Type
            1=1/0（シングルモノ）、2=1/0+1/0（デュアルモノ）、
            3=2/0（ステレオ）、7=3/1、8=3/2、9=3/2+LFE

        */
        [Serializable()]
        public struct audioDispTable {
            public string name;
            public Int32 code;

            public audioDispTable(string a,Int32 b){
                this.name = a;
                this.code = b;
            }
        }

        List<audioDispTable> audioDisp =
            new List<audioDispTable>(){
                new audioDispTable("日本語",0x6A706E),
                new audioDispTable("英語",0x656E67),
                new audioDispTable("ドイツ語",0x646575),
                new audioDispTable("フランス語",0x667261),
                new audioDispTable("イタリア語",0x697461),
                new audioDispTable("ロシア語",0x727573),
                new audioDispTable("中国語",0x7A686F),
                new audioDispTable("韓国語",0x6B6F72),
                new audioDispTable("スペイン語",0x737061),
                new audioDispTable("その他",0x657463),
                new audioDispTable("Japanese",0x6A706E),
                new audioDispTable("English",0x656E67),
                new audioDispTable("Niederdeutsch",0x646575),
                new audioDispTable("français",0x667261),
                new audioDispTable("Italian",0x697461),
                new audioDispTable("русский",0x727573),
                new audioDispTable("Chinese",0x7A686F),
                new audioDispTable("Korean",0x6B6F72),
                new audioDispTable("español",0x737061),
                new audioDispTable("other",0x657463),
                new audioDispTable("nothing",0x0)
            };

        [Serializable()]
        public struct audioComponentStruct
        {
            public Int32 type;
            public bool existSub;
            public string name;

            public audioComponentStruct(int p1, bool p2, string p3)
            {
                // TODO: Complete member initialization
                this.type = p1;
                this.existSub = p2;
                this.name = p3;
            }
        };

        List<audioComponentStruct> audioComponent =
        new List<audioComponentStruct>(){
            new audioComponentStruct(1,false,"シングルモノ"),
            new audioComponentStruct(3,false,"ステレオ"),
            new audioComponentStruct(7,false,"3/1"),
            new audioComponentStruct(8,false,"3/2"),
            new audioComponentStruct(9,false,"3/2+LFE"),
            new audioComponentStruct(14 /* 0x0E */,false,"7.1"),
            new audioComponentStruct(17 /* 0x11 */,false,"22"),
        };

        public audioComponentStruct getNextComponent(ref Int32 counter, bool dualMonoFlag)
        {
            if (dualMonoFlag == true)
            {
                return new audioComponentStruct(2, true, "デュアルモノ");
            }
            counter++;
            return audioComponent[counter % audioComponent.Count];
        }

        /*
         *  MPEG2AAC(LC) ... モノ、ステレオ、デュアルモノ、5.1
         *  MPEG4AAC     ... ステレオ、5.1、7.1ch、22.2ch
         *  MPEG4ALS     ... ステレオ、5.1
         * 
         */

        public bool makeTestDataSound(Int32 esNumber, ref Int32 counter, ref Int32 soundKindCounter, ref Int32 soundNameCounter, bool multiFlag, bool dualMonoFlag){
            this.tag = new epgTag(0xc4);
            Int32 es = 0x10;
            if (esNumber != 1)
            {
                es = 0x11;
            }
            this.componentTag = new epgAudioComponentTag(es);

            audioComponentStruct compo = getNextComponent(ref soundKindCounter, dualMonoFlag);
            this.componentType = new epgAudioComponentType(compo.type);
            this.esMultiFlag = new epgAudioMultiLingul((this.componentType.IsDualMono() == true) ? 1 : 0);

            if (esNumber == 1)
            {
                this.mainFlag = new epgMainFlag(1);
            }
            else
            {
                this.mainFlag = new epgMainFlag(0);
            }

            this.quority = new epgAudioQuority(1);

            this.samplingRate = new epgAudioSamplingRate(((soundKindCounter % 2)==1)?(5):(7));


            Int32 code = soundNameCounter % Enum.GetValues(typeof(AUDIOCODE)).Length;
            this.mainCode = new epgAudioCode((Int32)(Enum.GetValues(typeof(AUDIOCODE))).GetValue(code));
            this.name.main = getAudoNameFromCode(this.mainCode.value,soundNameCounter);
            soundNameCounter++;
            if (compo.existSub != true)
            {
                this.subCode = null;
            }
            else
            {
                compo = getNextComponent(ref soundKindCounter, dualMonoFlag);
                code = soundNameCounter % Enum.GetValues(typeof(AUDIOCODE)).Length;
                this.subCode = new epgAudioCode((Int32)(Enum.GetValues(typeof(AUDIOCODE))).GetValue(code));
//                new epgAudioCode((epgAudioCode.AUDIOCODE)Enum.ToObject(typeof(epgAudioCode.AUDIOCODE), code));
                this.name.sub = getAudoNameFromCode(this.subCode.value,soundNameCounter);
                soundNameCounter++;
            }
            this.streamType = new epgAudioStreamType(0x0f);
            return true;
        }
        private string getAudoNameFromCode(Int32 code,Int32 counter)
        {
            string result = "";

            var ret = from p in audioDisp
                      where p.code == code
                      select p.name;

            if (ret.Count() > 1)
            {
                if ((counter % Enum.GetValues(typeof(AUDIOCODE)).Length) > (Enum.GetValues(typeof(AUDIOCODE)).Length / 2))
                {
                    result = ret.ElementAt(1);
                }
                else
                {
                    result = ret.ElementAt(0);
                }
            }
            else
            {
                result = ret.ElementAt(0);
            }

            return result;
        }

    }

    [Serializable()]
    public class outputFormatExtDescription : outputFormatExtList
    {
        public epgTag tag = null;
        public epgExtTitle title = null;
        public epgExtDescription description = null;

        public override int CompareTo(object obj)
        {
            return this.title.CompareTo((obj as outputFormatExtDescription).title);
        }

        public outputFormatExtDescription()
        {
            this.paramList = new List<epgUnit>();
            this.tag = new epgTag(0xE4);
            //this.title = new epgExtTitle();
            //this.description = new epgExtDescription();
        }

        public override void makeList()
        {
            this.paramList = new List<epgUnit>();
            paramList.Add(this.tag);
            paramList.Add(this.title);
            paramList.Add(this.description);
        }

        public bool makeExtDescription(string title, string description)
        {
            this.title = new epgExtTitle(title);
            this.description = new epgExtDescription(description);
            return true;
        }
    }

    [Serializable()]
    public class outputFormatBasic : IComparable
    {
        public epgHeader header = new epgHeader(0x07);
        public epgNetowrkId networkid = null;
        public epgServiceId serviceid = null;
        public epgEventId eventid = null;
        public epgEventTime eventtime = null;
        public epgDuration duration = null;
        public epgTitle title = null;
        public epgDescribe contents = null;
        public epgPayment payment = null;

        public ExchangeLetter exchangeLetter = null;


        public List<outputFormatExtList> videoInfo = new List<outputFormatExtList>();
        public List<outputFormatExtList> audioInfo = new List<outputFormatExtList>();
        public List<outputFormatExtList> genleInfo = new List<outputFormatExtList>();
        //public List<outputFormatEncoder> encoderInfo = new List<outputFormatEncoder>();
        //public List<outputFormatSubtitle> subtitleInfo = new List<outputFormatSubtitle>();
        public List<outputFormatExtList> extDescriptorInfo = new List<outputFormatExtList>();
        public outputFormatCopyControl copyControlMain = new outputFormatCopyControl();
        public outputFormatContentAvailability contentAvailability = new outputFormatContentAvailability();
        public outputFormatEncoderInfo encoderInfo = new outputFormatEncoderInfo();
        public outputFormatMadara madara = new outputFormatMadara();
        public outputFormatCCJ ccj = new outputFormatCCJ();
        public outputFormatParental parental = new outputFormatParental();

        public string detailUri = "";
        public serviceFormat service = null;

        List<epgUnit> paramList = new List<epgUnit>();

        public outputFormatBasic(int net, serviceFormat service, ExchangeLetter ex)
        {
            this.service = service;
            this.networkid = new epgNetowrkId(net);
            this.serviceid = new epgServiceId(service.serviceId.value);
            this.eventid = new epgEventId(1);
            this.eventtime = new epgEventTime();
            this.duration = new epgDuration(0);
            this.title = new epgTitle("", ex);
            this.contents = new epgDescribe("", ex);
            this.payment = new epgPayment(0);

            this.exchangeLetter = ex;
        }

        public void clearWithoutBasic()
        {
            this.paramList.Clear();
            this.payment.clear();
        }

        public void clear()
        {
            this.paramList.Clear();
            // header, networkId, ServiceId は継続
            // eventidは10進める
            this.eventid.clear();
            this.eventtime.clear();
            this.duration.clear();
            this.title.clear();
            this.contents.clear();
            this.payment.clear();
        }

        public void makeList()
        {
            this.paramList = new List<epgUnit>();
            paramList.Add(this.header);
            paramList.Add(this.networkid);
            paramList.Add(this.serviceid);
            paramList.Add(this.eventid);
            paramList.Add(this.eventtime);
            paramList.Add(this.duration);
            paramList.Add(this.title);
            paramList.Add(this.contents);
            paramList.Add(this.payment);
        }

        public string getResult()
        {
            List<string> result = new List<string>();
            if (paramList == null || paramList.Count == 0)
            {
                return "";
            }

            foreach (epgUnit item in this.paramList)
            {
                if (item == null)
                {
                    result.Add("#");
                }
                else
                {
                    string output = (string)item.ToOutput();
                    result.Add(output);
                }
            }
            return String.Join(",",result.ToArray());
        }

        public byte[] getResultListByte( serviceFormat sf )
        {
            List<byte> result = new List<byte>();

            setInfoLineToResult(this.paramList, ref result);
            if( sf.mostSimple == true){
                return result.ToArray();
            }
            if (this.madara != null)
            {
                this.madara.makeList();
                setInfoLineToResult(this.madara.paramList, ref result);
            }
            setInfoListToResult(this.videoInfo, ref result);
            setInfoListToResult(this.audioInfo, ref result);
            setInfoListToResult(this.genleInfo, ref result);
            this.copyControlMain.makeList();
            setInfoLineToResult(this.copyControlMain.paramList, ref result);
            this.contentAvailability.makeList();
            setInfoLineToResult(this.contentAvailability.paramList, ref result);
            // this.encoderInfo.makeList();
            // setInfoLineToResult(this.encoderInfo.paramList, ref result);
            // this.ccj.makeList();
            // setInfoLineToResult(this.ccj.paramList, ref result);

            setInfoListToResult(this.extDescriptorInfo, ref result);

            this.parental.makeList();
            setInfoLineToResult(this.parental.paramList, ref result);


            return result.ToArray();
        }

        private bool setInfoListToResult(List<outputFormatExtList> lists, ref List<byte> result)
        {
            Int32 count;
            foreach (outputFormatExtList list in lists)
            {
                list.makeList();
                count = list.paramList.Count;
                foreach (epgUnit item in list.paramList)
                {
                    result.AddRange(getShiftJisAry(item));
                    count--;
                    if (count > 0)
                    {
                        result.Add(Convert.ToByte(','));
                    }
                }
                result.Add(Convert.ToByte('\r'));
                result.Add(Convert.ToByte('\n'));
            }
            return true;
        }

        private bool setInfoLineToResult(List<epgUnit> list, ref List<byte> result)
        {
            Int32 count;

            if (list == null || list.Count == 0)
            {
                // result.Add(0);
                return false;
            }

            count = list.Count;
            foreach (epgUnit item in list)
            {
                result.AddRange(getShiftJisAry(item));
                count--;
                if (count > 0)
                {
                    result.Add(Convert.ToByte(','));
                }
            }
            result.Add(Convert.ToByte('\r'));
            result.Add(Convert.ToByte('\n'));
            return true;
        }


        private List<byte> getShiftJisAry(epgUnit item)
        {
            List<byte> ret = new List<byte>();
            if (item == null)
            {
                ret.Add(Convert.ToByte('#')); // new Convert.ToByte('#'));
            }
            else
            {
                if (item is epgHeader)
                {
                    ret.AddRange(((epgHeader)item).getByteAry());
                }
                else
                {
                    Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                    Byte[] write = sjisEnc.GetBytes(item.ToOutput());
                    ret.AddRange(write);
                }
            }
            return ret;
        }


        public int CompareTo(object obj)
        {
            Int32 ret = 0;
            outputFormatBasic from = obj as outputFormatBasic;
            ret = this.networkid.value.CompareTo(from.networkid.value);
            if (ret != 0) return ret;
            ret = this.serviceid.value.CompareTo(from.serviceid.value);
            if (ret != 0) return ret;
            ret = this.eventtime.value.CompareTo(from.eventtime.value);
            if (ret != 0) return ret;

            return ret;
        }
    }

    /// <summary>
    /// http://d.hatena.ne.jp/tekk/20100131/1264913887 より拝借。
    /// </summary>
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(T target)
        {

            T result;
            BinaryFormatter b = new BinaryFormatter();

            MemoryStream mem = new MemoryStream();

            try
            {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = (T)b.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;

        }
    }

}
