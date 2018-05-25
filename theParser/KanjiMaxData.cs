using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace theParser
{
    public interface buildTestDataInterface
    {
        List<outputFormatBasic> getData();
    }

    public abstract class buildTestData : buildTestDataInterface
    {
        public serviceFormat service = null;
        public DateTime today;
        public DateTime lastDay;
        public Int32 daycount;

        abstract public List<outputFormatBasic> getData();
    }

    public enum MyMethodType { NORMAL, TIMEPATTERN1 };

    public class KanjiMaxData : buildTestData
    {
        private arib2byteChar kanji = null;
        public ExchangeLetter exchangeLetter = null;
        public MyMethodType MyMethod = MyMethodType.NORMAL;

        public KanjiMaxData(settings set)
        {
            this.kanji = new arib2byteChar(set.conf.aribGaiji);
        }

        public override List<outputFormatBasic> getData()
        {
            switch (MyMethod)
            {
            case MyMethodType.NORMAL:
                return this.getDataNormal();
            case MyMethodType.TIMEPATTERN1:
                return this.getDataTimePattern1();
            default:
                return this.getDataNormal();
            }
        }

        /// <summary>
        /// duration を、候補の中から「乱数」で選ぶ。
        /// 乱数を使いはするが、「底」を日毎＋自分のサービス番号と決めて使い、日が変わる時に再度「底」を宣言する。
        /// </summary>
        /// <returns></returns>
        private List<outputFormatBasic> getDataTimePattern1()
        {
            List<outputFormatBasic> result = new List<outputFormatBasic>();
            Int32 counter = 0;
            Int32 soundNameCounter = 0;
            Int32 soundKindCounter = 0;
            Int32 genleBig = 0;
            Int32 genleSmall = 0;
            string line = "";
            bool keepTableFlag = true;
            Int32[] durationItems = this.service.madaraDurations;
            Int32 seed = 0;
            Int32 dayNum = -1;
            Random rnd = null;
            Int32 copyControlValue = 0;
            for (DateTime current = today; current < this.lastDay; ) // 指定した日付「まで」作る
            {
                if (dayNum != current.Day)
                {
                    seed = makeSeed(this.today, this.service);
                    dayNum = current.Day;
                    rnd = new Random(seed);
                }
                Int32 duration = durationItems[rnd.Next(durationItems.Count())];
                outputFormatBasic ou = new outputFormatBasic(
                    service.networkId.value,
                    service,
                    exchangeLetter);
                ou.eventtime = new epgEventTime(current);
                ou.eventid.value = ou.eventid.setEventIdFromDate(ou.eventtime.value);
                ou.duration.value = duration;
                //                ou.contents.set("Contents:" + current.ToLongDateString() + "," + current.ToLongTimeString());
                // ou.videoInfo = makeVideoInfoList(ref counter, this.service.madaraResolution); // HDで
                ou.videoInfo = makeVideoInfoList(ref counter, this.service.DefaultVideoComponentType.value);
                ou.audioInfo = makeAudioInfoList(ref counter, ref soundKindCounter, ref soundNameCounter);

                line = "";
                keepTableFlag = makeContentsWithAllKanji(ref kanji, ref ou, ref line);
                ou.contents.set(line);
                ou.title.set(makeTitleWithAllKanji(ref kanji));   // "title" + current.ToShortTimeString() , ou.duration.value);
                if (keepTableFlag == false)
                {
                    kanji.setFirst();
                }

                copyControlData cp = new copyControlData(copyControlValue);
                copyControlValue++;
                if (cp.copyControl != null && cp.contents != null)
                {
                    ou.copyControlMain = cp.copyControl;
                    ou.contentAvailability = cp.contents;
                }

                outputFormatGenle ougenle = new outputFormatGenle();
                ougenle.getGenleNext(ref genleBig, ref genleSmall);
                ougenle.genleBig.value = genleBig;
                ougenle.genleSmall.value = genleSmall;
                ou.genleInfo.Add(ougenle);
                ou.service = service;

                outputFormatEncoderInfo enc = new outputFormatEncoderInfo();
                enc.rate.value = 6000; // 6Mbps
                ou.encoderInfo = enc;

                result.Add(ou);
                current = current.AddMinutes(duration);
                counter++;
            }
            return result;
        }

        private Int32 makeSeed(DateTime p, serviceFormat service)
        {
            Int32 ret = 0;
            Int32 p1 = 0;
            Int32.TryParse(p.ToString("yyyyMMdd"), out p1);
            ret = p1 + service.serviceId.value;
            return ret;
        }
        private List<outputFormatBasic> getDataNormal(){
            List<outputFormatBasic> result = new List<outputFormatBasic>();
            Int32 duration = 15;
            Int32 counter = 0;
            Int32 soundNameCounter = 0;
            Int32 soundKindCounter = 0;
            Int32 genleBig = 0;
            Int32 genleSmall = 0;
            string line = "";
            bool keepTableFlag = true;
            for (DateTime current = today; current < this.lastDay; current = current.AddMinutes(duration)) // 指定した日付「まで」作る
            {
                counter++;
                outputFormatBasic ou = new outputFormatBasic(
                    service.networkId.value,
                    service,
                    exchangeLetter);
                ou.eventtime = new epgEventTime(current);
                ou.eventid.value = ou.eventid.setEventIdFromDate(ou.eventtime.value);
                ou.duration.value = duration;
//                ou.contents.set("Contents:" + current.ToLongDateString() + "," + current.ToLongTimeString());
                ou.videoInfo = makeVideoInfoList(ref counter); // HDで
                ou.audioInfo = makeAudioInfoList(ref counter,ref soundKindCounter, ref soundNameCounter);

                line = "";
                keepTableFlag = makeContentsWithAllKanji(ref kanji, ref ou, ref line);
                ou.contents.set(line);
                ou.title.set(makeTitleWithAllKanji(ref kanji));   // "title" + current.ToShortTimeString() , ou.duration.value);
                if (keepTableFlag == false)
                {
                    kanji.setFirst();
                }

                outputFormatGenle ougenle = new outputFormatGenle();
                ougenle.getGenleNext(ref genleBig, ref genleSmall);
                ougenle.genleBig.value = genleBig;
                ougenle.genleSmall.value = genleSmall;
                ou.genleInfo.Add(ougenle);

                ou.service = service;
                result.Add(ou);
            }
            return result;
        }

        private string makeTitleWithAllKanji(ref arib2byteChar kanji)
        {
            string result = "";
            result += kanji.currentTable.myDescribe; //  "漢字系集合(" + kanji.currentTableNumber.ToString() + ")より";
            return result;
        }

        private bool makeContentsWithAllKanji(ref arib2byteChar kanji , ref outputFormatBasic ou, ref string result)
        {
            //映像:(*1),音声１:(*2),音声２:(*3)＋改行
            //18文字＋改行
            //18文字

            bool ret = true;

            if (getOneLine(ref kanji, ref result) != true)
            {
                ret = false;
            }
            result+= @"\&";
            if (ret == true && getOneLine(ref kanji, ref result) != true)
            {
                ret = false;
            }
            result+= @"\&";
            if (ret == true && getOneLine(ref kanji, ref result) != true)
            {
                 ret = false;
            }
/*
            string audio0 = ou.audioInfo[0].name.ToOutput();
            string audio1 = "";
            if (ou.audioInfo.Count > 1)
            {
                audio1 = ou.audioInfo[1].name.ToOutput();
            }

            result += string.Format("映像:{0} 音声1:{1} 音声2:{2}" , ou.videoInfo[0].videoType.value, audio0, audio1);
*/ 
            return ret;
        }

        public bool getOneLine(ref arib2byteChar kanji, ref string result)
        {
            List<byte> letter = new List<byte>();
            bool ret = true;
            for (Int32 i = 0; i < 16; i++)
            {
                if (kanji.getNextCharAry(ref letter) < 0)
                {
                    // kanji.setFirst();
                    ret = false;
                    break;
                }
            }
            result+= System.Text.Encoding.GetEncoding(932).GetString(letter.ToArray()); //  exchangeJIScodeToString(letter);
            return ret;
        }

        public string exchangeJIScodeToString(List<byte> letter)
        {
            List<byte> pre = new List<byte> { 0x1b, 0x24, 0x42 }; // k-in
            List<byte>post = new List<byte>{ 0x1b, 0x28, 0x4a }; // k-out

            pre.AddRange(letter);
            pre.AddRange(post);

           return  System.Text.Encoding.GetEncoding(50220).GetString(pre.ToArray());
        }


        public class videoDataStruct
        {
            public Int32 group;
            public Int32 type;
            public string name;
            public string nameZ;

            public videoDataStruct(Int32 g, Int32 a, string b, string c)
            {
                group = g;
                type = a;
                name = b;
                nameZ = c;
            }
        }

        videoDataStruct[] videoDataAry = {
new videoDataStruct(0,01,"480i-4:3","４８０ｉ１"),
new videoDataStruct(0,03,"480i-16:9","４８０ｉ２"),
new videoDataStruct(0,04,"480i-16:9上","４８０ｉ３"),
new videoDataStruct(2,0xa1,"480p-4:3","４８０ｐ１"),
new videoDataStruct(2,0xa3,"480p-16:9","４８０ｐ２"),
new videoDataStruct(2,0xA4,"480p-16:9上","４８０ｐ３"),
new videoDataStruct(1,0xB1,"1080i-4:3","１０８０ｉ１"),
new videoDataStruct(1,0xB3,"1080i-16:9","１０８０ｉ２"),
new videoDataStruct(1,0xB4,"1080i-16:9上","１０８０ｉ３"),
new videoDataStruct(1,0xE1,"1080p-4:3","１０８０ｐ１"),
new videoDataStruct(1,0xE3,"1080p-16:9","１０８０ｐ２"),
new videoDataStruct(1,0xE4,"1080p-16:9上","１０８０ｐ３"),
new videoDataStruct(1,0x93,"2160p-16:9", "２１６０ｐ１"),
                                         };


        private List<outputFormatExtList> makeVideoInfoList(ref Int32 counter)
        {
            List<outputFormatExtList> result = new List<outputFormatExtList>();

            videoDataStruct data = videoDataAry[counter % videoDataAry.Count()];

            outputFormatVideo p = new outputFormatVideo();
            p.componentType = new epgVideoComponentType(data.type);
            p.videoType = new epgVideoType(data.name);
            result.Add(p);

            return result;
        }
        private List<outputFormatExtList> makeVideoInfoListFromType(ref Int32 type)
        {
            List<outputFormatExtList> result = new List<outputFormatExtList>();

            int t = type;
            videoDataStruct data = videoDataAry[7];   
            var obj = from v in videoDataAry
                      where v.type == t
                      select v;
            if (obj.Count() > 0)
            {
                data = obj.First();
            }

            outputFormatVideo p = new outputFormatVideo();
            p.componentType = new epgVideoComponentType(data.type);
            p.videoType = new epgVideoType(data.name);
            result.Add(p);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="resoType">0:SD, 1:HD</param>
        /// <returns></returns>
        private List<outputFormatExtList> makeVideoInfoList(ref Int32 counter, string group)
        {
            Int32 counterMod = counter % videoDataAry.Count();
            if ( group == "SD") // SD
            {
                counterMod = counterMod % 3;
                return makeVideoInfoList(ref counterMod);
            }
            else // HD // 480p 系は出力させない
            {
                counterMod = (counterMod % 3) + 6 ;
                return makeVideoInfoList(ref counterMod);
            }
        }
        private List<outputFormatExtList> makeVideoInfoList(ref Int32 counter, int typeValue)
        {
            return makeVideoInfoListFromType(ref typeValue);
        }

        private List<outputFormatExtList> makeAudioInfoList(ref int counter, ref Int32 soundKindCounter, ref Int32 soundNameCounter)
        {
/*
         → Audio 以下を、Video同様、Indexの順番で適合させる。
            1ES 1音声
            1ES 2音声
            2ES 1音声 + 1音声
            2ES 2音声 + 1音声
            2ES 1音声 + 2音声
            2ES 2音声 + 2音声

            component tag
            ES1: 10, ES2: 11

            
            multilingul 2音声なら 1(両方のESとも）
            maincomponent Main(ES1): 1
            quority : 1
            sample rate: 5 or 7
 */

            List<outputFormatExtList> audio = new List<outputFormatExtList>();
            outputFormatAudio es1 = new outputFormatAudio();
            es1 = new outputFormatAudio();
            outputFormatAudio es2 = null;

            switch (counter % 6)
            {
            case 0:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, false, false);
                break;
            case 1:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, false, true);
                break;
            case 2:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, true, false);
                es2 = new outputFormatAudio();
                es2.makeTestDataSound(2, ref counter, ref soundKindCounter, ref soundNameCounter, true, false);
                break;
            case 3:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, true, true);
                es2 = new outputFormatAudio();
                es2.makeTestDataSound(2, ref counter, ref soundKindCounter, ref soundNameCounter, true, false);
                break;
            case 4:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, true, false);
                es2 = new outputFormatAudio();
                es2.makeTestDataSound(2, ref counter, ref soundKindCounter, ref soundNameCounter, true, true);
                break;
            case 5:
                es1.makeTestDataSound(1, ref counter, ref soundKindCounter, ref soundNameCounter, true, true);
                es2 = new outputFormatAudio();
                es2.makeTestDataSound(2, ref counter, ref soundKindCounter, ref soundNameCounter, true, true);
                break;
            default:
                break;
            }

            audio.Add(es1);
            if(es2 != null)
            {
                audio.Add(es2);
            }
            return audio;
        }



    }
}
