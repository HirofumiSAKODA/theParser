using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace theParser
{
    public class loopPatter : buildTestData
    {
        public loopPatter()
        {
//            this.readFile();
        }

        public string path;
        public string fileName = "LoopData.txt";

        public override List<outputFormatBasic> getData()
        {
            List<outputFormatBasic> result = new List<outputFormatBasic>();

            Int32 index = 0;
            Int32.TryParse(this.service.uri, out index);
            if( index > data.Count ) {
                index = data.Count -1;
            }
            serviceData conf = this.data.First();

            // 特殊 1日当たり96event、9:30AM-17:30で5分間隔。
            // StartTime は、this.today の 日付のみ借りて 9:30AM強制
            // これを this.daycount日分繰り返す

            for (Int32 days = 0; days < 24 /* this.daycount */; days++)
            {
                DateTime startTime = new DateTime(today.Year, today.Month, today.Day, conf.startTime.Hour, conf.startTime.Minute, 0).AddDays(days);

                int counterEncodeRate = 0;

                for (Int32 eventCount = 0; eventCount < 96; eventCount++)
                {
                    outputFormatBasic ou = new outputFormatBasic(
                    service.networkId.value,
                    service,
                    null);


                    ou.eventtime = new epgEventTime(startTime);
                    ou.eventid.value = ou.eventid.setEventIdFromDate(ou.eventtime.value);
                    ou.duration = new epgDuration(conf.duration);

                    ou.videoInfo = new List<outputFormatExtList>();
                    outputFormatVideo p = new outputFormatVideo();
                    // p.componentType = new epgVideoComponentType(0xb3);
                    p.componentType = new epgVideoComponentType(service.DefaultVideoComponentType.value);
                    p.videoType = new epgVideoType("HD映像");
                    ou.videoInfo.Add(p);

                    ou.audioInfo = new List<outputFormatExtList>();
                    ou.audioInfo.Add(new outputFormatAudio(ou.service.forceJlab035)); 

                    ou.title.value = conf.title;
                    ou.contents.set(conf.desc.Trim());

                    if (conf.addEncRateText == true)
                    {
                        ou.encoderInfo = new outputFormatEncoderInfo();
                        Int32 rate = conf.rate[counterEncodeRate % conf.rate.Length];
                        ou.encoderInfo.rate.value = rate;
                        ou.title.value += " " + ((Int32)(rate / 1000)).ToString() + "Mbps";
                    }
                    else
                    {
                        ou.encoderInfo.clear();
                    }

                    ou.genleInfo = new List<outputFormatExtList>();
                    outputFormatGenle genle = new outputFormatGenle();
                    genle.genleBig = new epgGenleBig(0x02); // 情報/ワイドショー
                    genle.genleSmall = new epgGenleSmall(0x04); // ショッピング・通販
                    ou.genleInfo.Add(genle);

                    ou.extDescriptorInfo = new List<outputFormatExtList>();
                    foreach (TagData tag in conf.tags)
                    {
                        outputFormatExtDescription exDesc = new outputFormatExtDescription();
                        exDesc.makeExtDescription(tag.name, tag.desc);
                        ou.extDescriptorInfo.Add(exDesc);
                    }
                    counterEncodeRate++;
                    startTime = startTime.AddMinutes(conf.duration);
                    result.Add(ou);
                }

            }

            return result;
        }
        

        public struct TagData
        {
            public string name;
            public string desc;
        }
        
        public struct serviceData
        {
            public Int32 index;
            public string title;
            public bool addEncRateText;
            public string desc;
            public Int32 duration;
            public Int32[] rate;
            public List<TagData> tags;
            public DateTime startTime;
        }

        public List<serviceData> data;

        private void initializeData()
        {
            serviceData p;
            TagData pp;
            DateTime baseTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            this.data = new List<serviceData>();

            p = new serviceData();
            p.index = 1;
            p.title = "ショップチャンネル";
            p.addEncRateText = true;
            p.desc = "ショップチャンネルは、ファッション、コスメ、家庭用品、健康グッズなど、世界中から厳選した商品を、生放送でご紹介。";
            p.duration = 5;
            p.startTime = baseTime.AddHours(9).AddMinutes(30);
            p.rate = new Int32[] { 8000, 10000 };
            p.tags = new List<TagData>();

            pp = new TagData();
            pp.name = "URL";
            pp.desc = "http://www.shopch.jp";
            p.tags.Add(pp);

            pp = new TagData();
            pp.name = "TEL";
            pp.desc = "フリーダイヤル:0120-000123";

            p.tags.Add(pp);

            pp = new TagData();
            pp.name = "番組詳細";
            pp.desc = "商品の人気度や残量がリアルタイムでわかる、生放送ならではの臨場感をぜひ、お楽しみ下さい。ご注文は24時間年中無休!  在庫状況により番組に変更が生じる場合がございます。ご了承ください。";
            p.tags.Add(pp);

            this.data.Add(p);

            p = new serviceData();
            p.index = 2;
            p.title = "ＱＶＣ";
            p.addEncRateText = true;
            p.desc = "世界最大24時間完全生放送のTVショッピングチャンネル<QVC>。";
            p.duration = 5;
            p.startTime = baseTime.AddHours(9).AddMinutes(30);
            p.rate = new Int32[] { 10000, 8000 };
            p.tags = new List<TagData>();

            pp = new TagData();
            pp.name = "URL";
            pp.desc = "http://qvc.jp (パソコン・携帯電話から)";
            p.tags.Add(pp);

            pp = new TagData();
            pp.name = "TEL";
            pp.desc = "<ご注文> 0120-945-009 (アドバイザー) 0120-945-007 (音声自動応答)";

            p.tags.Add(pp);

            pp = new TagData();
            pp.name = "番組詳細";
            pp.desc = "驚きの商品が、つぎつぎ登場しますので、欲しい商品を見つけたらスグにお電話ください。24時間、オペレーターが注文を承ります。 ";
            p.tags.Add(pp);

            this.data.Add(p);

            // Pattern 3
            p = new serviceData();
            p.index = 3;
            p.title = "壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.addEncRateText = false;
            p.desc = "壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.duration = 15;
            p.startTime = baseTime;
            p.rate = new Int32[] { 10000, 8000 ,4000, };
            p.tags = new List<TagData>();

            pp = new TagData();
            pp.name = "最大長の試験～壱";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～弐";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～参";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～四";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～五";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～六";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～七";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～八";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            pp = new TagData();
            pp.name = "最大長の試験～九";
            pp.desc = "壱弐参四五六七八九10壱弐参四五六七八九20壱弐参四五六七八九30壱弐参四五六七八九40壱弐参四五六七八九50壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾壱弐参四五六七八九拾";
            p.tags.Add(pp);
            this.data.Add(p);
        }

        public bool readFile()
        {
            string filename = System.IO.Path.Combine(this.path, this.fileName);
            bool flag = true;
            this.data = new List<serviceData>();
            XmlSerializer serial = new XmlSerializer(this.data.GetType());
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    this.data = (List<serviceData>)serial.Deserialize(reader);
                }
            }
            catch 
            {
                flag = false;
            }
            if (this.data.Count == 0 || flag ==false)
            {
                this.initializeData(); // 初期化
                flag = false;
                try
                {
                    using (StreamWriter writer = new StreamWriter(filename))
                    {
                        serial.Serialize(writer, this.data);
                    }
                }
                catch
                {
                    ;
                }
            }
            return flag;
        }



    }
}
