using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace theParser
{
    public interface epgUnit : IComparable
    {
        // public object value;

        void clear();
        string ToOutput();
        bool isEmpty();
    }

    [Serializable()]
    public class epgUnitHeader : epgUnit
    {
        const Int32 DEFAULT = 0;
        public Int32 value;

        public epgUnitHeader(Int32 v)
        {
            this.value = v;
        }

        public bool isEmpty()
        {
            if (value == DEFAULT)
            {
                return true;
            }
            return false;
        }

        public virtual string ToOutput()
        {
            return ""; //  new byte[] { (byte)value };
        }

        public byte[] getByteAry()
        {
            return new byte[] { (byte)value };
        }

        public void clear()
        {
            this.value = DEFAULT;
        }
        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitHeader).value);
        }

    }
    [Serializable()]
    public abstract class epgUnitText : epgUnit
    {
        const string DEFAULT = "";
        public string value = DEFAULT;
        public Int32 numberOf = 80;
        public ExchangeLetter exchangeLetter = null;

#if false
        struct structExchangeTable
        {
            // 0x7521,0x3402,0xF040,"追加漢字　※1"
            public structExchangeTable(Int32 code, Int32 utf8Code, Int32 sjisCode, string text, string orig)
            {
                this.orig = orig;
                this.code = code;
                this.utf8Code = utf8Code;
                this.sjisCode = sjisCode;
                this.text = text;
            }
            /// <summary>
            /// この文字が含まれていたら変換する。
            /// </summary>
            public string orig;
            /// <summary>
            /// 8単位符号コード
            /// </summary>
            public Int32 code;
            /// <summary>
            /// UTF-8コード
            /// </summary>
            public Int32 utf8Code;
            /// <summary>
            /// SJISコード
            /// </summary>
            public Int32 sjisCode;
            /// <summary>
            /// 説明文
            /// </summary>
            public string text;
        }
#endif
        public bool isEmpty()
        {
            if (value == DEFAULT)
            {
                return true;
            }
            return false;
        }

        public epgUnitText(Int32 num)
        {
            this.numberOf = num;
            // exchangeTable = exchangeTableLocal;
        }
        public epgUnitText(Int32 num, string str) : this(num)
        {
            this.Convert(str, out this.value);
        }
        public epgUnitText(Int32 num, string str, ExchangeLetter ex) : this(num)
        {
            this.Convert(str, out this.value);
            this.exchangeLetter = ex;
        }

        public string convertUTF8GaijiForCheck(string s)
        {
             return System.Text.RegularExpressions.Regex.Replace(
                  s,
                  @"\\#[0-9a-fA-F]{4};",
                  "■");
        }

        public void Convert(string from, out string to)
        {
            if (from.Length == 0)
            {
                to = "";
                return;
            }
            string fromReplaced = convertUTF8GaijiForCheck(from);

            byte[] sjis = null;
            byte[] sjisLengthCheck = null;
            sjisLengthCheck = this.getSJISarray(fromReplaced);
            sjis = this.getSJISarray(from);
            while (sjisLengthCheck.Length > this.numberOf)
            {
                if (from.Length == 0)
                {
                    to = "";
                    break;
                }
                from = from.Remove(from.Length - 1);
                sjis = this.getSJISarray(from);
                fromReplaced = convertUTF8GaijiForCheck(from);
                sjisLengthCheck = this.getSJISarray(fromReplaced);
            }
            to = System.Text.Encoding.GetEncoding(932).GetString(sjis);
        }

        public void set(string str)
        {
            this.Convert(str, out this.value);
        }

        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitText).value);
        }

        /// <summary>
        /// 特殊変換処理のTVチェック用
        /// start番目からcount個分の特殊変換テーブルを文字列にして
        /// さも イベントの内容文章 のようにしてセットする。
        /// </summary>
        /// <param name="count"></param>
        public Int32 setExText(Int32 start, Int32 count)
        {
            string line = "";
            ExchangeLetter.structExchangeTable p;
            string code,dmy;
            Int32 startNo = start;
            for (Int32 cnt = 0; cnt < count; cnt++)
            {
                // <8単位文字コードHEX> ■ <SJISコードHEX> ■<改行>
                if (startNo + cnt >= this.exchangeLetter.exchangeTable.Count)
                {
                    startNo = -cnt;
                }
                try
                {
                    p = this.exchangeLetter.exchangeTable[startNo + cnt];
                    code = p.code.ToString("X");
                    if (p.sjisCodeString == null)
                    {
                        p.exchangeSjisCode();
                        dmy = p.sjisCodeString.ToString();
                    }
                    else
                    {
                        dmy = "dmy";
                    }
                    line += code + @" \#" + code + "; " + p.sjisCode.ToString("X") + " " + dmy + @"\&";
                }
                catch (Exception e)
                {
                    dmy = e.Message;
                }
            }
            this.value = line;
            return startNo + count;
        }



        public virtual string ToOutput()
        {
            if (this.value.Length == 0)
            {
                return "#";
            } 
            return this.value.ToString();
        }

        public string ConvertEncoding(string src, System.Text.Encoding destEnc)
        {
            byte[] src_temp = System.Text.Encoding.ASCII.GetBytes(src);
            byte[] dest_temp = System.Text.Encoding.Convert(System.Text.Encoding.ASCII, destEnc, src_temp);
            string ret = destEnc.GetString(dest_temp);
            return ret;
        }

        /// <summary>
        /// 入力文字列を（ごにょごにょ変換して）byte array ShifJISコードに変換
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public byte[] getSJISarray(string src)
        {
            //  = System.Text.Encoding.ASCII.GetBytes();
            string srcEdited = exchangeStrings(src);
            byte[] src_temp = System.Text.Encoding.GetEncoding(932).GetBytes(srcEdited);
            return src_temp;
            // System.Text.Encoding.Convert(System.Text.Encoding.ASCII, Encoding.GetEncoding("Shift_JIS"), src_temp);
        }

        public void clear()
        {
            this.value = "";
        }
#if false
// UTF-8 code
new ExchangeLetter.structExchangeTable("[HV]",0xE0F8),
new ExchangeLetter.structExchangeTable("[SD]",0xE0F9),
new ExchangeLetter.structExchangeTable("[手]",0xE0FD),
new ExchangeLetter.structExchangeTable("[字]",0xE0FE),
new ExchangeLetter.structExchangeTable("[双]",0xE0FF),
new ExchangeLetter.structExchangeTable("[デ]",0xE180),
new ExchangeLetter.structExchangeTable("[S]",0xE181),
new ExchangeLetter.structExchangeTable("[二]",0xE182),
new ExchangeLetter.structExchangeTable("[多]",0xE183),
new ExchangeLetter.structExchangeTable("[解]",0xE184),
new ExchangeLetter.structExchangeTable("[SS]",0xE185),
new ExchangeLetter.structExchangeTable("[B]",0xE186),
new ExchangeLetter.structExchangeTable("[N]",0xE187),
new ExchangeLetter.structExchangeTable("[天]",0xE18A),
new ExchangeLetter.structExchangeTable("[交]",0xE18B),
new ExchangeLetter.structExchangeTable("[映]",0xE18C),
new ExchangeLetter.structExchangeTable("[料]",0xE18D),
new ExchangeLetter.structExchangeTable("[前]",0xE190),
new ExchangeLetter.structExchangeTable("[後]",0xE191),
new ExchangeLetter.structExchangeTable("[再]",0xE192),
new ExchangeLetter.structExchangeTable("[新]",0xE193),
new ExchangeLetter.structExchangeTable("[初]",0xE194),
new ExchangeLetter.structExchangeTable("[終]",0xE195),
new ExchangeLetter.structExchangeTable("[生]",0xE196),
new ExchangeLetter.structExchangeTable("[PV]",0xE19A),

        new ExchangeLetter.structExchangeTable("お",0x82a0),
new ExchangeLetter.structExchangeTable("[HV]",0xF2CE),
new ExchangeLetter.structExchangeTable("[SD]",0xF2CF),
new ExchangeLetter.structExchangeTable("[手]",0xf2d3),
new ExchangeLetter.structExchangeTable("[字]",0xf2d4),
new ExchangeLetter.structExchangeTable("[双]",0xf2d5),
new ExchangeLetter.structExchangeTable("[デ]",0xf2d6),
new ExchangeLetter.structExchangeTable("[S]",0xf2d7),
new ExchangeLetter.structExchangeTable("[二]",0xf2d8),
new ExchangeLetter.structExchangeTable("[多]",0xf2d9),
new ExchangeLetter.structExchangeTable("[解]",0xf2da),
new ExchangeLetter.structExchangeTable("[SS]",0xf2db),
new ExchangeLetter.structExchangeTable("[B]",0xf2dc),
new ExchangeLetter.structExchangeTable("[N]",0xf2dd),
new ExchangeLetter.structExchangeTable("[天]",0xf2e0),
new ExchangeLetter.structExchangeTable("[交]",0xf2e1),
new ExchangeLetter.structExchangeTable("[映]",0xf2e2),
new ExchangeLetter.structExchangeTable("[料]",0xf2e4),
new ExchangeLetter.structExchangeTable("[前]",0xf2e6),
new ExchangeLetter.structExchangeTable("[後]",0xf2e7),
new ExchangeLetter.structExchangeTable("[再]",0xf2ec),
new ExchangeLetter.structExchangeTable("[新]",0xf2e9),
new ExchangeLetter.structExchangeTable("[初]",0xf2ea),
new ExchangeLetter.structExchangeTable("[終]",0xf2eb),
new ExchangeLetter.structExchangeTable("[生]",0xf2ec),
new ExchangeLetter.structExchangeTable("[PV]",0xf2f0),

#endif



        public string exchangeStrings(string org)
        {
            string ret;
#if false // moved to ExcchangeLetter.cs
            byte[] toAry = new byte[2];
            string to;
#endif
            StringBuilder sb = new StringBuilder(HtmlEntity.DeEntitize(org.Replace(",", @"\,")));

            if (this.exchangeLetter != null)
            {
                foreach (ExchangeLetter.structExchangeTable table in this.exchangeLetter.exchangeTable)
                {
                    if (table.orig.Length == 0)
                    {
                        continue;
                    }
                    table.exchangeSjisCode();
                    sb.Replace(table.orig, table.sjisCodeString);
#if false // moved to ExchangeLetter.cs
                    toAry[0] = (byte)((table.code >> 8) & 0xff);
                    if (toAry[0] != 0)
                    {
                        toAry[1] = (byte)(table.code & 0xff);
                    }
                    else
                    {
                        toAry[0] = (byte)(table.code & 0xff);
                        toAry[1] = 0;
                    }
                    to = System.Text.Encoding.GetEncoding(932).GetString(toAry);
                    sb.Replace(table.orig, to);
#endif
                }
            }
            ret = sb.ToString();

            // [HV] HDTV       E0F8  F2CE
            // [SD] SDTV       E0F9  F2CF
            // [手]手話通訳放送 E0FD  F23D3
            // [字]字幕放送     E0FE F2D4
            // [双]双方向放送    E0FF F2D5
            // [デ]番組連動データ放送 E180 F2D6
            // [S]ステレオ放送     E181   F2D7
            // [二]二ヶ国語放送    E182   F2D8
            // [多]音声多重放送    E183   F2D9
            // [解]音声解説        E184   F2DA
            // [SS] サラウンドステレオ E185 F2DB
            // [B] 圧縮Bモードステレオ E186 D2DC
            // [N] ニュース    E187 F2DD
            // [天] 天気予報   E18A F2E0
            // [交] 交通情報   E18B F2E1
            // [映] 劇場映画   E18C F2E2
            // [料] 有料放送   E18D F2E4
            // [前] 前編       E190 F2E6
            // [後] 後編       E191 F2E7
            // [再] 再放送     E192 F2E8
            // [新] 新番組     E193 F2E9
            // [初] 初回放送   E194 F2EA
            // [終] 最終回     E195 F2EB
            // [生] 生放送     E196 F2EC
            // [PV] ペーパービュー E19A F2F0
            



//            {"[HV]",{0xE0,0xF8}},
//{"[SD]",{0xE0,0xF9}
//"[手]",0xE0,0xFD
//"[字]",0xE0,0xFE
//"[双]",0xE0,0xFF
//"[デ]",0xE1,0x80
//"[S]",0xE1,0x81
//"[二]",0xE1,0x82
//"[多]",0xE1,0x83
//"[解]",0xE1,0x84
//"[SS]",0xE1,0x85
//"[B]",0xE1,0x86
//"[N]",0xE1,0x87
//"[天]",0xE1,0x8A
//"[交]",0xE1,0x8B
//"[映]",0xE1,0x8C
//"[料]",0xE1,0x8D
//"[前]",0xE1,0x90
//"[後]",0xE1,0x91
//"[再]",0xE1,0x92
//"[新]",0xE1,0x93
//"[初]",0xE1,0x94
//"[終]",0xE1,0x95
//"[生]",0xE1,0x96
//"[PV]",0xE1,0x9A
            
#if comment
// 現在のHTML界隈で使われることがある HTML Entities 群。
    &#009;	タブ
    &#010;	改行
    &#013;	復帰
    &#032;	スペース
!	&#033;	感嘆符
"	&quot;	&#034;	ダブルクォーテーション
//#   &#035;	シャープ
$	&#036;	ドル
%	&#037;	パーセント
&	&amp;	&#038;	アンパサンド
'	&#039;	アポストロフィ
(	&#040;	開く括弧
)	&#041;	閉じる括弧
*	&#042;	アスタリスク
+	&#043;	プラス
,	&#044;	コンマ
-	&#045;	ハイフン
.	&#046;	ピリオド
/	&#047;	スラッシュ
0~9	&#048;~&#057	数字
 	&nbsp;	&#160;	改行されないスペース
:	&#058;	コロン
;	&#059;	セミコロン
<	&lt;	&#060;	小なり
=	&#061;	イコール
>	&gt;	&#062;	大なり
?	&#063;	疑問符
@	&#064;	アットマーク
A~Z	&#065;~&#090	大文字アルファペット
[	&#091;	開く角括弧
\	&#092;	バックスラッシュ
]	&#093;	閉じる角括弧
^	&#094;	キャレット
_	&#095;	アンダーバー
`	&#096;	バッククォート
a~z	&#097;~&#122	小文字アルファペット
{	&#123;	開く中括弧
|	&#124;	縦線
}	&#125;	閉じる中括弧
~	&#126;	チルダ
‚	&#130;	左下のシングルクォーテーション
ƒ	&#131;	フォルテ
„	&#132;	左下のダブルクォーテーション
…	&#133;	省略
†	&#134;	ダガー
‡	&#135;	ダブルダガー
ˆ	&#136;	アクセント
‰	&#137;	
Š	&#138;	小なり
‹	&#139;	
Œ	&#140;	
Ž	&#142;	
‘	&#145;	左のシングルクォーテーション
’	&#146;	右のシングルクォーテーション
“	&#147;	左のダブルクォーテーション
”	&#148;	右のダブルクォーテーション
•	&#149;	点
–	&#150;	
—	&#151;	
˜	&#152;	
™	&#153;	商標
š	&#154;	
›	&#155;	大なり
œ	&#156;	
ž	&#158;	
Ÿ	&#159;	
 	&nbsp;	&#160;	スペース
¡	&iexcl;	&#161;	反転感嘆符
¢	&cent;	&#162;	セント
£	&pound;	&#163;	ポンド
¤	&curren;	&#164;	汎用通貨
¥	&yen;	&#165;	円
¦	&brvbar;	&#166;	縦破線
§	&sect;	&#167;	セクション
¨	&uml;	&#168;	ウムラウト
©	&copy;	&#169;	コピーライト
ª	&ordf;	&#170;	女性序数
«	&laquo;	&#171;	左引用
¬	&not;	&#172;	ノット
    &shy;	&#173;	ソフトハイフン
®	&reg;	&#174;	登録商標
¯	&macr;	&#175;	
°	&deg;	&#176;	度
±	&plusmn;	&#177;	プラスマイナス
²	&sup2;	&#178;	上付き2
³	&sup3;	&#179;	上付き3
´	&acute;	&#180;	アクセント
µ	&micro;	&#181;	マイクロ
¶	&para;	&#182;	パラグラフ
·	&middot;	&#183;	中点
¸	&sedil;	&#184;	
¹	&sup1;	&#185;	上付き1
º	&ordm;	&#186;	男性序数
»	&raquo;	&#187;	右引用
¼	&frac14;	&#188;	4分の1
½	&frac12;	&#189;	2分の1
¾	&frac34;	&#190;	4分の4
¿	&iquest;	&#191;	反転疑問符
×	&times;	&#215;	乗算
÷	&divide;	&#215;	除算
#endif
            return ret;
        }
#if false // moved to ExchangeLetter.cs
        private List<ExchangeLetter.structExchangeTable> exchangeTable;
        private List<ExchangeLetter.structExchangeTable> exchangeTableLocal = new List<ExchangeLetter.structExchangeTable>{
            new ExchangeLetter.structExchangeTable(0x7521,0x3402,0xF040,"追加漢字　※1",""),
            new ExchangeLetter.structExchangeTable(0x7522,0xD840,0xF041,"追加漢字　※2",""),
new ExchangeLetter.structExchangeTable(0x7523,0x4EFD,0xF042,"追加漢字　※3",""),
new ExchangeLetter.structExchangeTable(0x7524,0x4EFF,0xF043,"追加漢字　※4",""),
new ExchangeLetter.structExchangeTable(0x7525,0x4F9A,0xF044,"追加漢字　※5",""),
new ExchangeLetter.structExchangeTable(0x7526,0x4FC9,0xF045,"追加漢字　※6",""),
new ExchangeLetter.structExchangeTable(0x7527,0x509C,0xF046,"追加漢字　※7",""),
new ExchangeLetter.structExchangeTable(0x7528,0x511E,0xF047,"追加漢字　※8",""),
new ExchangeLetter.structExchangeTable(0x7529,0x51BC,0xF048,"追加漢字　※9",""),
new ExchangeLetter.structExchangeTable(0x752A,0x351F,0xF049,"追加漢字　※10",""),
new ExchangeLetter.structExchangeTable(0x752B,0x5307,0xF04A,"追加漢字　※11",""),
new ExchangeLetter.structExchangeTable(0x752C,0x5361,0xF04B,"追加漢字　※12",""),
new ExchangeLetter.structExchangeTable(0x752D,0x536C,0xF04C,"追加漢字　※13",""),
new ExchangeLetter.structExchangeTable(0x752E,0x8A79,0xF04D,"追加漢字　※14",""),
new ExchangeLetter.structExchangeTable(0x752F,0xD842,0xF04E,"追加漢字　※15",""),
new ExchangeLetter.structExchangeTable(0x7530,0x544D,0xF04F,"追加漢字　※16",""),
new ExchangeLetter.structExchangeTable(0x7531,0x5496,0xF050,"追加漢字　※17",""),
new ExchangeLetter.structExchangeTable(0x7532,0x549C,0xF051,"追加漢字　※18",""),
new ExchangeLetter.structExchangeTable(0x7533,0x54A9,0xF052,"追加漢字　※19",""),
new ExchangeLetter.structExchangeTable(0x7534,0x550E,0xF053,"追加漢字　※20",""),
new ExchangeLetter.structExchangeTable(0x7535,0x554A,0xF054,"追加漢字　※21",""),
new ExchangeLetter.structExchangeTable(0x7536,0x5672,0xF055,"追加漢字　※22",""),
new ExchangeLetter.structExchangeTable(0x7537,0x56E4,0xF056,"追加漢字　※23",""),
new ExchangeLetter.structExchangeTable(0x7538,0x5733,0xF057,"追加漢字　※24",""),
new ExchangeLetter.structExchangeTable(0x7539,0x5734,0xF058,"追加漢字　※25",""),
new ExchangeLetter.structExchangeTable(0x753A,0xFA10,0xF059,"追加漢字　※26",""),
new ExchangeLetter.structExchangeTable(0x753B,0x5880,0xF05A,"追加漢字　※27",""),
new ExchangeLetter.structExchangeTable(0x753C,0x59E4,0xF05B,"追加漢字　※28",""),
new ExchangeLetter.structExchangeTable(0x753D,0x5A23,0xF05C,"追加漢字　※29",""),
new ExchangeLetter.structExchangeTable(0x753E,0x5A55,0xF05D,"追加漢字　※30",""),
new ExchangeLetter.structExchangeTable(0x753F,0x5BEC,0xF05E,"追加漢字　※31",""),
new ExchangeLetter.structExchangeTable(0x7540,0xFA11,0xF05F,"追加漢字　※32",""),
new ExchangeLetter.structExchangeTable(0x7541,0x37E2,0xF060,"追加漢字　※33",""),
new ExchangeLetter.structExchangeTable(0x7542,0x5EAC,0xF061,"追加漢字　※34",""),
new ExchangeLetter.structExchangeTable(0x7543,0x5F34,0xF062,"追加漢字　※35",""),
new ExchangeLetter.structExchangeTable(0x7544,0x5F45,0xF063,"追加漢字　※36",""),
new ExchangeLetter.structExchangeTable(0x7545,0x5FB7,0xF064,"追加漢字　※37",""),
new ExchangeLetter.structExchangeTable(0x7546,0x6017,0xF065,"追加漢字　※38",""),
new ExchangeLetter.structExchangeTable(0x7547,0xFA6B,0xF066,"追加漢字　※39",""),
new ExchangeLetter.structExchangeTable(0x7548,0x6130,0xF067,"追加漢字　※40",""),
new ExchangeLetter.structExchangeTable(0x7549,0x6624,0xF068,"追加漢字　※41",""),
new ExchangeLetter.structExchangeTable(0x754A,0x66C8,0xF069,"追加漢字　※42",""),
new ExchangeLetter.structExchangeTable(0x754B,0x66D9,0xF06A,"追加漢字　※43",""),
new ExchangeLetter.structExchangeTable(0x754C,0x66FA,0xF06B,"追加漢字　※44",""),
new ExchangeLetter.structExchangeTable(0x754D,0x66FB,0xF06C,"追加漢字　※45",""),
new ExchangeLetter.structExchangeTable(0x754E,0x6852,0xF06D,"追加漢字　※46",""),
new ExchangeLetter.structExchangeTable(0x754F,0x9FC4,0xF06E,"追加漢字　※47",""),
new ExchangeLetter.structExchangeTable(0x7550,0x6911,0xF06F,"追加漢字　※48",""),
new ExchangeLetter.structExchangeTable(0x7551,0x693B,0xF070,"追加漢字　※49",""),
new ExchangeLetter.structExchangeTable(0x7552,0x6A45,0xF071,"追加漢字　※50",""),
new ExchangeLetter.structExchangeTable(0x7553,0x6A91,0xF072,"追加漢字　※51",""),
new ExchangeLetter.structExchangeTable(0x7554,0x6ADB,0xF073,"追加漢字　※52",""),
new ExchangeLetter.structExchangeTable(0x7555,0xD84C,0xF074,"追加漢字　※53",""),
new ExchangeLetter.structExchangeTable(0x7556,0xDFFE,0xF075,"追加漢字　※54",""),
new ExchangeLetter.structExchangeTable(0x7557,0xD84D,0xF076,"追加漢字　※55",""),
new ExchangeLetter.structExchangeTable(0x7558,0x6BF1,0xF077,"追加漢字　※56",""),
new ExchangeLetter.structExchangeTable(0x7559,0x6CE0,0xF078,"追加漢字　※57",""),
new ExchangeLetter.structExchangeTable(0x755A,0x6D2E,0xF079,"追加漢字　※58",""),
new ExchangeLetter.structExchangeTable(0x755B,0xFA45,0xF07A,"追加漢字　※59",""),
new ExchangeLetter.structExchangeTable(0x755C,0x6DBF,0xF07B,"追加漢字　※60",""),
new ExchangeLetter.structExchangeTable(0x755D,0x6DCA,0xF07C,"追加漢字　※61",""),
new ExchangeLetter.structExchangeTable(0x755E,0x6DF8,0xF07D,"追加漢字　※62",""),
new ExchangeLetter.structExchangeTable(0x755F,0xFA46,0xF07E,"追加漢字　※63",""),
new ExchangeLetter.structExchangeTable(0x7560,0x6F5E,0xF080,"追加漢字　※64",""),
new ExchangeLetter.structExchangeTable(0x7561,0x6FF9,0xF081,"追加漢字　※65",""),
new ExchangeLetter.structExchangeTable(0x7562,0x7064,0xF082,"追加漢字　※66",""),
new ExchangeLetter.structExchangeTable(0x7563,0xFA6C,0xF083,"追加漢字　※67",""),
new ExchangeLetter.structExchangeTable(0x7564,0xD850,0xF084,"追加漢字　※68",""),
new ExchangeLetter.structExchangeTable(0x7565,0x7147,0xF085,"追加漢字　※69",""),
new ExchangeLetter.structExchangeTable(0x7566,0x71C1,0xF086,"追加漢字　※70",""),
new ExchangeLetter.structExchangeTable(0x7567,0x7200,0xF087,"追加漢字　※71",""),
new ExchangeLetter.structExchangeTable(0x7568,0x739F,0xF088,"追加漢字　※72",""),
new ExchangeLetter.structExchangeTable(0x7569,0x73A8,0xF089,"追加漢字　※73",""),
new ExchangeLetter.structExchangeTable(0x756A,0x73C9,0xF08A,"追加漢字　※74",""),
new ExchangeLetter.structExchangeTable(0x756B,0x73D6,0xF08B,"追加漢字　※75",""),
new ExchangeLetter.structExchangeTable(0x756C,0x741B,0xF08C,"追加漢字　※76",""),
new ExchangeLetter.structExchangeTable(0x756D,0x7421,0xF08D,"追加漢字　※77",""),
new ExchangeLetter.structExchangeTable(0x756E,0xFA4A,0xF08E,"追加漢字　※78",""),
new ExchangeLetter.structExchangeTable(0x756F,0x7426,0xF08F,"追加漢字　※79",""),
new ExchangeLetter.structExchangeTable(0x7570,0x742A,0xF090,"追加漢字　※80",""),
new ExchangeLetter.structExchangeTable(0x7571,0x742C,0xF091,"追加漢字　※81",""),
new ExchangeLetter.structExchangeTable(0x7572,0x7439,0xF092,"追加漢字　※82",""),
new ExchangeLetter.structExchangeTable(0x7573,0x744B,0xF093,"追加漢字　※83",""),
new ExchangeLetter.structExchangeTable(0x7574,0x3EDA,0xF094,"追加漢字　※84",""),
new ExchangeLetter.structExchangeTable(0x7575,0x7575,0xF095,"追加漢字　※85",""),
new ExchangeLetter.structExchangeTable(0x7576,0x7581,0xF096,"追加漢字　※86",""),
new ExchangeLetter.structExchangeTable(0x7577,0x7772,0xF097,"追加漢字　※87",""),
new ExchangeLetter.structExchangeTable(0x7578,0x4093,0xF098,"追加漢字　※88",""),
new ExchangeLetter.structExchangeTable(0x7579,0x78C8,0xF099,"追加漢字　※89",""),
new ExchangeLetter.structExchangeTable(0x757A,0x78E0,0xF09A,"追加漢字　※90",""),
new ExchangeLetter.structExchangeTable(0x757B,0x7947,0xF09B,"追加漢字　※91",""),
new ExchangeLetter.structExchangeTable(0x757C,0x79AE,0xF09C,"追加漢字　※92",""),
new ExchangeLetter.structExchangeTable(0x757D,0x9FC6,0xF09D,"追加漢字　※93",""),
new ExchangeLetter.structExchangeTable(0x757E,0x4103,0xF09E,"追加漢字　※94",""),
new ExchangeLetter.structExchangeTable(0x7621,0x9FC5,0xF09F,"追加漢字　※95",""),
new ExchangeLetter.structExchangeTable(0x7622,0x79DA,0xF0A0,"追加漢字　※96",""),
new ExchangeLetter.structExchangeTable(0x7623,0x7A1E,0xF0A1,"追加漢字　※97",""),
new ExchangeLetter.structExchangeTable(0x7624,0x7B7F,0xF0A2,"追加漢字　※98",""),
new ExchangeLetter.structExchangeTable(0x7625,0x7C31,0xF0A3,"追加漢字　※99",""),
new ExchangeLetter.structExchangeTable(0x7626,0x4264,0xF0A4,"追加漢字　※100",""),
new ExchangeLetter.structExchangeTable(0x7627,0x7D8B,0xF0A5,"追加漢字　※101",""),
new ExchangeLetter.structExchangeTable(0x7628,0x7FA1,0xF0A6,"追加漢字　※102",""),
new ExchangeLetter.structExchangeTable(0x7629,0x8118,0xF0A7,"追加漢字　※103",""),
new ExchangeLetter.structExchangeTable(0x762A,0x813A,0xF0A8,"追加漢字　※104",""),
new ExchangeLetter.structExchangeTable(0x762B,0xFA6D,0xF0A9,"追加漢字　※105",""),
new ExchangeLetter.structExchangeTable(0x762C,0x82AE,0xF0AA,"追加漢字　※106",""),
new ExchangeLetter.structExchangeTable(0x762D,0x845B,0xF0AB,"追加漢字　※107",""),
new ExchangeLetter.structExchangeTable(0x762E,0x84DC,0xF0AC,"追加漢字　※108",""),
new ExchangeLetter.structExchangeTable(0x762F,0x84EC,0xF0AD,"追加漢字　※109",""),
new ExchangeLetter.structExchangeTable(0x7630,0x8559,0xF0AE,"追加漢字　※110",""),
new ExchangeLetter.structExchangeTable(0x7631,0x85CE,0xF0AF,"追加漢字　※111",""),
new ExchangeLetter.structExchangeTable(0x7632,0x8755,0xF0B0,"追加漢字　※112",""),
new ExchangeLetter.structExchangeTable(0x7633,0x87EC,0xF0B1,"追加漢字　※113",""),
new ExchangeLetter.structExchangeTable(0x7634,0x880B,0xF0B2,"追加漢字　※114",""),
new ExchangeLetter.structExchangeTable(0x7635,0x88F5,0xF0B3,"追加漢字　※115",""),
new ExchangeLetter.structExchangeTable(0x7636,0x89D2,0xF0B4,"追加漢字　※116",""),
new ExchangeLetter.structExchangeTable(0x7637,0x8AF6,0xF0B5,"追加漢字　※117",""),
new ExchangeLetter.structExchangeTable(0x7638,0x8DCE,0xF0B6,"追加漢字　※118",""),
new ExchangeLetter.structExchangeTable(0x7639,0x8FBB,0xF0B7,"追加漢字　※119",""),
new ExchangeLetter.structExchangeTable(0x763A,0x8FF6,0xF0B8,"追加漢字　※120",""),
new ExchangeLetter.structExchangeTable(0x763B,0x90DD,0xF0B9,"追加漢字　※121",""),
new ExchangeLetter.structExchangeTable(0x763C,0x9127,0xF0BA,"追加漢字　※122",""),
new ExchangeLetter.structExchangeTable(0x763D,0x912D,0xF0BB,"追加漢字　※123",""),
new ExchangeLetter.structExchangeTable(0x763E,0x91B2,0xF0BC,"追加漢字　※124",""),
new ExchangeLetter.structExchangeTable(0x763F,0x9233,0xF0BD,"追加漢字　※125",""),
new ExchangeLetter.structExchangeTable(0x7640,0x9288,0xF0BE,"追加漢字　※126",""),
new ExchangeLetter.structExchangeTable(0x7641,0x9321,0xF0BF,"追加漢字　※127",""),
new ExchangeLetter.structExchangeTable(0x7642,0x9348,0xF0C0,"追加漢字　※128",""),
new ExchangeLetter.structExchangeTable(0x7643,0x9592,0xF0C1,"追加漢字　※129",""),
new ExchangeLetter.structExchangeTable(0x7644,0x96DE,0xF0C2,"追加漢字　※130",""),
new ExchangeLetter.structExchangeTable(0x7645,0x9903,0xF0C3,"追加漢字　※131",""),
new ExchangeLetter.structExchangeTable(0x7646,0x9940,0xF0C4,"追加漢字　※132",""),
new ExchangeLetter.structExchangeTable(0x7647,0x9AD9,0xF0C5,"追加漢字　※133",""),
new ExchangeLetter.structExchangeTable(0x7648,0x9BD6,0xF0C6,"追加漢字　※134",""),
new ExchangeLetter.structExchangeTable(0x7649,0x9DD7,0xF0C7,"追加漢字　※135",""),
new ExchangeLetter.structExchangeTable(0x764A,0x9EB4,0xF0C8,"追加漢字　※136",""),
new ExchangeLetter.structExchangeTable(0x764B,0x9EB5,0xF0C9,"追加漢字　※137",""),
new ExchangeLetter.structExchangeTable(0x7A21,0xE0C9,0xF29F,"事故",""),
new ExchangeLetter.structExchangeTable(0x7A22,0xE0CA,0xF2A0,"故障者",""),
new ExchangeLetter.structExchangeTable(0x7A23,0xE0CB,0xF2A1,"障害物",""),
new ExchangeLetter.structExchangeTable(0x7A24,0xE0CC,0xF2A2,"工事",""),
new ExchangeLetter.structExchangeTable(0x7A25,0xE0CD,0xF2A3,"凍結",""),
new ExchangeLetter.structExchangeTable(0x7A26,0xE0CE,0xF2A4,"作業",""),
new ExchangeLetter.structExchangeTable(0x7A28,0xE0D0,0xF2A6,"通行止め",""),
new ExchangeLetter.structExchangeTable(0x7A29,0xE0D1,0xF2A7,"片側交互通行",""),
new ExchangeLetter.structExchangeTable(0x7A2A,0xE0D2,0xF2A8,"チェーン規制",""),
new ExchangeLetter.structExchangeTable(0x7A2B,0xE0D3,0xF2A9,"進入禁止",""),
new ExchangeLetter.structExchangeTable(0x7A30,0xE0D8,0xF2AE,"駐車場",""),
new ExchangeLetter.structExchangeTable(0x7A31,0xE0D9,0xF2AF,"駐車場(閉）",""),
new ExchangeLetter.structExchangeTable(0x7A34,0xE0DC,0xF2B2,"対面通行１",""),
new ExchangeLetter.structExchangeTable(0x7A35,0xE0DD,0xF2B3,"対面通行２",""),
new ExchangeLetter.structExchangeTable(0x7A36,0xE0DE,0xF2B4,"車線規制１",""),
new ExchangeLetter.structExchangeTable(0x7A37,0xE0DF,0xF2B5,"車線規制２",""),
new ExchangeLetter.structExchangeTable(0x7A38,0xE0E0,0xF2B6,"徐行１",""),
new ExchangeLetter.structExchangeTable(0x7A39,0xE0E1,0xF2B7,"徐行２",""),
new ExchangeLetter.structExchangeTable(0x7A3A,0xE0E2,0xF2B8,"入口閉鎖１",""),
new ExchangeLetter.structExchangeTable(0x7A3B,0xE0E3,0xF2B9,"入口閉鎖２",""),
new ExchangeLetter.structExchangeTable(0x7A3C,0xE0E4,0xF2BA,"大型通行止め１",""),
new ExchangeLetter.structExchangeTable(0x7A3D,0xE0E5,0xF2BB,"大型通行止め２",""),
new ExchangeLetter.structExchangeTable(0x7A3E,0xE0E6,0xF2BC,"入口制限１",""),
new ExchangeLetter.structExchangeTable(0x7A3F,0xE0E7,0xF2BD,"入口制限２",""),
new ExchangeLetter.structExchangeTable(0x7A40,0xE0E8,0xF2BE,"速度制限基本",""),
new ExchangeLetter.structExchangeTable(0x7A41,0xE0E9,0xF2BF,"10km/h",""),
new ExchangeLetter.structExchangeTable(0x7A42,0xE0EA,0xF2C0,"20km/h",""),
new ExchangeLetter.structExchangeTable(0x7A43,0xE0EB,0xF2C1,"30km/h",""),
new ExchangeLetter.structExchangeTable(0x7A44,0xE0EC,0xF2C2,"40km/h",""),
new ExchangeLetter.structExchangeTable(0x7A45,0xE0ED,0xF2C3,"50km/h",""),
new ExchangeLetter.structExchangeTable(0x7A46,0xE0EE,0xF2C4,"60km/h",""),
new ExchangeLetter.structExchangeTable(0x7A47,0xE0EF,0xF2C5,"70km/h",""),
new ExchangeLetter.structExchangeTable(0x7A48,0xE0F0,0xF2C6,"80km/h",""),
new ExchangeLetter.structExchangeTable(0x7A4D,0x2491,0xF2CB,"10",""),
new ExchangeLetter.structExchangeTable(0x7A4E,0x2492,0xF2CC,"11",""),
new ExchangeLetter.structExchangeTable(0x7A4F,0x2493,0xF2CD,"12",""),
new ExchangeLetter.structExchangeTable(0x7A50,0xE0F8,0xF2CE,"ＨＤＴＶ","[HV]"),
new ExchangeLetter.structExchangeTable(0x7A51,0xE0F9,0xF2CF,"ＳＤＴＶ","[SD]"),
new ExchangeLetter.structExchangeTable(0x7A52,0xE0FA,0xF2D0,"プログレッシブ放送","[P]"),
new ExchangeLetter.structExchangeTable(0x7A53,0xE0FB,0xF2D1,"ワイド放送","[W]"),
new ExchangeLetter.structExchangeTable(0x7A54,0xE0FC,0xF2D2,"マルチビューテレビ",""),
new ExchangeLetter.structExchangeTable(0x7A55,0xE0FD,0xF2D3,"手話通訳放送","[手]"),
new ExchangeLetter.structExchangeTable(0x7A56,0xE0FE,0xF2D4,"字幕放送","[字]"),
new ExchangeLetter.structExchangeTable(0x7A57,0xE0FF,0xF2D5,"双方向放送","[双]"),
new ExchangeLetter.structExchangeTable(0x7A58,0xE180,0xF2D6,"番組連動データ放送","[デ]"),
new ExchangeLetter.structExchangeTable(0x7A59,0xE181,0xF2D7,"ステレオ放送","[S]"),
new ExchangeLetter.structExchangeTable(0x7A5A,0xE182,0xF2D8,"二ヶ国語放送","[二]"),
new ExchangeLetter.structExchangeTable(0x7A5B,0xE183,0xF2D9,"音声多重放送","[多]"),
new ExchangeLetter.structExchangeTable(0x7A5C,0xE184,0xF2DA,"音声解説","[解]"),
new ExchangeLetter.structExchangeTable(0x7A5D,0xE185,0xF2DB,"サラウンドステレオ","[SS]"),
new ExchangeLetter.structExchangeTable(0x7A5E,0xE186,0xF2DC,"圧縮モードステレオ","[B]"),
new ExchangeLetter.structExchangeTable(0x7A5F,0xE187,0xF2DD,"ニュース","[N]"),
new ExchangeLetter.structExchangeTable(0x7A60,0x25A0,0xF2DE,"四角（黒）",""),
new ExchangeLetter.structExchangeTable(0x7A61,0x25CF,0xF2DF,"丸（黒）",""),
new ExchangeLetter.structExchangeTable(0x7A62,0xE18A,0xF2E0,"天気予報",""),
new ExchangeLetter.structExchangeTable(0x7A63,0xE18B,0xF2E1,"交通情報",""),
new ExchangeLetter.structExchangeTable(0x7A64,0xE18C,0xF2E2,"劇映画",""),
new ExchangeLetter.structExchangeTable(0x7A65,0xE18D,0xF2E3,"無料放送",""),
new ExchangeLetter.structExchangeTable(0x7A66,0xE18E,0xF2E4,"有料放送",""),
new ExchangeLetter.structExchangeTable(0x7A67,0xE18F,0xF2E5,"パレンタルロック",""),
new ExchangeLetter.structExchangeTable(0x7A68,0xE190,0xF2E6,"前編","[前]"),
new ExchangeLetter.structExchangeTable(0x7A69,0xE191,0xF2E7,"後編","[後]"),
new ExchangeLetter.structExchangeTable(0x7A6A,0xE192,0xF2E8,"再放送","[再]"),
new ExchangeLetter.structExchangeTable(0x7A6B,0xE193,0xF2E9,"新番組","[新]"),
new ExchangeLetter.structExchangeTable(0x7A6C,0xE194,0xF2EA,"初回放送","[初]"),
new ExchangeLetter.structExchangeTable(0x7A6D,0xE195,0xF2EB,"最終回","[終]"),
new ExchangeLetter.structExchangeTable(0x7A6E,0xE196,0xF2EC,"生放送","[生]"),
new ExchangeLetter.structExchangeTable(0x7A6F,0xE197,0xF2ED,"通販",""),
new ExchangeLetter.structExchangeTable(0x7A70,0xE198,0xF2EE,"声優・声の出演",""),
new ExchangeLetter.structExchangeTable(0x7A71,0xE199,0xF2EF,"吹き替え",""),
new ExchangeLetter.structExchangeTable(0x7A72,0xE19A,0xF2F0,"ペーパービュー",""),
new ExchangeLetter.structExchangeTable(0x7A73,0x3299,0xF2F1,"マル秘",""),
new ExchangeLetter.structExchangeTable(0x7A74,0xE19C,0xF2F2,"〜ほか",""),
new ExchangeLetter.structExchangeTable(0x7B21,0xE1A7,0,"官公庁",""),
new ExchangeLetter.structExchangeTable(0x7B22,0xE1A8,0,"都道府県庁",""),
new ExchangeLetter.structExchangeTable(0x7B23,0x25CE,0,"市役所",""),
new ExchangeLetter.structExchangeTable(0x7B24,0x3007,0,"町村役場",""),
new ExchangeLetter.structExchangeTable(0x7B25,0x2A02,0,"警察署",""),
new ExchangeLetter.structExchangeTable(0x7B26,0xE1AC,0,"派出所",""),
new ExchangeLetter.structExchangeTable(0x7B27,0x328B,0,"消防署",""),
new ExchangeLetter.structExchangeTable(0x7B28,0x3012,0,"郵便局",""),
new ExchangeLetter.structExchangeTable(0x7B29,0xE1AF,0,"病院",""),
new ExchangeLetter.structExchangeTable(0x7B2A,0xE1B0,0,"学校",""),
new ExchangeLetter.structExchangeTable(0x7B2B,0xE1B1,0,"幼稚園",""),
new ExchangeLetter.structExchangeTable(0x7B2C,0xE1B2,0,"神社",""),
new ExchangeLetter.structExchangeTable(0x7B2D,0x534D,0,"寺院",""),
new ExchangeLetter.structExchangeTable(0x7B2E,0xE1B4,0,"教会",""),
new ExchangeLetter.structExchangeTable(0x7B2F,0xE1B5,0,"城跡",""),
new ExchangeLetter.structExchangeTable(0x7B30,0x2234,0,"史跡",""),
new ExchangeLetter.structExchangeTable(0x7B31,0x2668,0,"温泉",""),
new ExchangeLetter.structExchangeTable(0x7B32,0xE1B8,0,"工場",""),
new ExchangeLetter.structExchangeTable(0x7B33,0xE1B9,0,"発電所",""),
new ExchangeLetter.structExchangeTable(0x7B34,0xE1BA,0,"灯台",""),
new ExchangeLetter.structExchangeTable(0x7B35,0x2693,0,"港湾",""),
new ExchangeLetter.structExchangeTable(0x7B36,0x2708,0,"空港",""),
new ExchangeLetter.structExchangeTable(0x7B37,0x25B2,0,"山",""),
new ExchangeLetter.structExchangeTable(0x7B38,0xE1BE,0,"海水浴場",""),
new ExchangeLetter.structExchangeTable(0x7B39,0xE1BF,0,"公園",""),
new ExchangeLetter.structExchangeTable(0x7B3A,0xE1C0,0,"ゴルフ場",""),
new ExchangeLetter.structExchangeTable(0x7B3B,0xE1C1,0,"フェリー発着所",""),
new ExchangeLetter.structExchangeTable(0x7B3C,0xE1C2,0,"マリーナ",""),
new ExchangeLetter.structExchangeTable(0x7B3D,0xE1C3,0,"ホテル",""),
new ExchangeLetter.structExchangeTable(0x7B3E,0x24B9,0,"デパート",""),
new ExchangeLetter.structExchangeTable(0x7B3F,0x24C8,0,"駅",""),
new ExchangeLetter.structExchangeTable(0x7B40,0xE1C6,0,"交差点",""),
new ExchangeLetter.structExchangeTable(0x7B41,0xE1C7,0,"駐車場",""),
new ExchangeLetter.structExchangeTable(0x7B42,0xE1C8,0,"インターチェンジ",""),
new ExchangeLetter.structExchangeTable(0x7B43,0xE1C9,0,"サービスエリア",""),
new ExchangeLetter.structExchangeTable(0x7B44,0xE1CA,0,"パーキングエリア",""),
new ExchangeLetter.structExchangeTable(0x7B45,0xE1CB,0,"ジャンクション",""),
new ExchangeLetter.structExchangeTable(0x7B46,0xE1CC,0,"スキー場",""),
new ExchangeLetter.structExchangeTable(0x7B47,0xE1CD,0,"アイススケート場",""),
new ExchangeLetter.structExchangeTable(0x7B48,0xE1CE,0,"体育館",""),
new ExchangeLetter.structExchangeTable(0x7B49,0xE1CF,0,"キャンプ場",""),
new ExchangeLetter.structExchangeTable(0x7B4A,0xE1D0,0,"レジャーランド",""),
new ExchangeLetter.structExchangeTable(0x7B4B,0x260E,0,"電話会社",""),
new ExchangeLetter.structExchangeTable(0x7B4C,0xE1D2,0,"銀行",""),
new ExchangeLetter.structExchangeTable(0x7B4D,0xE1D3,0,"墓地",""),
new ExchangeLetter.structExchangeTable(0x7B4E,0xE1D4,0,"ガソリンスタンド",""),
new ExchangeLetter.structExchangeTable(0x7B4F,0xE1D5,0,"ドライブイン",""),
new ExchangeLetter.structExchangeTable(0x7B50,0xE1D6,0,"文化施設",""),
new ExchangeLetter.structExchangeTable(0x7B51,0xE1D7,0,"自衛隊",""),
new ExchangeLetter.structExchangeTable(0x7C21,0x27A1,0xF39F,"右矢印（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7C22,0x2B05,0xF3A0,"左矢印（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7C23,0x2B06,0xF3A1,"上矢印（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7C24,0x2B07,0xF3A2,"下矢印（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7C25,0x2B2D,0xF3A3,"縦楕円（白抜き）",""),
new ExchangeLetter.structExchangeTable(0x7C26,0x2B2E,0xF3A4,"縦楕円（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7C27,0x5E74,0xF3A5,"年",""),
new ExchangeLetter.structExchangeTable(0x7C28,0x6708,0xF3A6,"月",""),
new ExchangeLetter.structExchangeTable(0x7C29,0x65E5,0xF3A7,"日",""),
new ExchangeLetter.structExchangeTable(0x7C2A,0x5186,0xF3A8,"エン",""),
new ExchangeLetter.structExchangeTable(0x7C2B,0x33A1,0xF3A9,"平方メートル（㎡）",""),
new ExchangeLetter.structExchangeTable(0x7C2C,0x33A5,0xF3AA,"立方メートル（?）",""),
new ExchangeLetter.structExchangeTable(0x7C2D,0x339D,0xF3AB,"センチメートル（㎝）",""),
new ExchangeLetter.structExchangeTable(0x7C2E,0x33A0,0xF3AC,"平方センチメートル",""),
new ExchangeLetter.structExchangeTable(0x7C2F,0x33A4,0xF3AD,"立方センチメートル",""),
new ExchangeLetter.structExchangeTable(0x7C30,0xE28F,0xF3AE,"0.",""),
new ExchangeLetter.structExchangeTable(0x7C31,0x2488,0xF3AF ,"1.",""),
new ExchangeLetter.structExchangeTable(0x7C32,0x2489,0xF3B0 ,"2.",""),
new ExchangeLetter.structExchangeTable(0x7C33,0x248A,0xF3B1 ,"3.",""),
new ExchangeLetter.structExchangeTable(0x7C34,0x248B,0xF3B2 ,"4.",""),
new ExchangeLetter.structExchangeTable(0x7C35,0x248C,0xF3B3 ,"5.",""),
new ExchangeLetter.structExchangeTable(0x7C36,0x248D,0xF3B4 ,"6.",""),
new ExchangeLetter.structExchangeTable(0x7C37,0x248E,0xF3B5 ,"7.",""),
new ExchangeLetter.structExchangeTable(0x7C38,0x248F,0xF3B6 ,"8.",""),
new ExchangeLetter.structExchangeTable(0x7C39,0x2490,0xF3B7 ,"9.",""),
new ExchangeLetter.structExchangeTable(0x7C3A,0x6C0F,0xF3B8,"氏",""),
new ExchangeLetter.structExchangeTable(0x7C3B,0x526F,0xF3B9,"副",""),
new ExchangeLetter.structExchangeTable(0x7C3C,0x5143,0xF3BA,"元",""),
new ExchangeLetter.structExchangeTable(0x7C3D,0x6545,0xF3BB,"故",""),
new ExchangeLetter.structExchangeTable(0x7C3E,0x524D,0xF3BC,"前",""),
new ExchangeLetter.structExchangeTable(0x7C3F,0x65B0,0xF3BD,"新",""),
new ExchangeLetter.structExchangeTable(0x7C40,0xE296,0xF3BE ,"0,",""),
new ExchangeLetter.structExchangeTable(0x7C41,0xE297,0xF3BF ,"1,",""),
new ExchangeLetter.structExchangeTable(0x7C42,0xE298,0xF3C0 ,"2,",""),
new ExchangeLetter.structExchangeTable(0x7C43,0xE299,0xF3C1,"3,",""),
new ExchangeLetter.structExchangeTable(0x7C44,0xE29A,0xF3C2 ,"4,",""),
new ExchangeLetter.structExchangeTable(0x7C45,0xE29B,0xF3C3 ,"5,",""),
new ExchangeLetter.structExchangeTable(0x7C46,0xE29C,0xF3C4 ,"6,",""),
new ExchangeLetter.structExchangeTable(0x7C47,0xE29D,0xF3C5 ,"7,",""),
new ExchangeLetter.structExchangeTable(0x7C48,0xE29E,0xF3C6 ,"8,",""),
new ExchangeLetter.structExchangeTable(0x7C49,0xE29F,0xF3C7 ,"9,",""),
new ExchangeLetter.structExchangeTable(0x7C4A,0x3233,0xF3C8,"〔社〕",""),
new ExchangeLetter.structExchangeTable(0x7C4B,0x3236,0xF3C9,"〔財〕",""),
new ExchangeLetter.structExchangeTable(0x7C4C,0x3232,0xF3CA,"〔有〕",""),
new ExchangeLetter.structExchangeTable(0x7C4D,0x3231,0xF3CB,"〔株〕",""),
new ExchangeLetter.structExchangeTable(0x7C4E,0x3239,0xF3CC,"〔代〕",""),
new ExchangeLetter.structExchangeTable(0x7C4F,0xE2A0,0xF3CD,"問+丸",""),
new ExchangeLetter.structExchangeTable(0x7C50,0x25B6,0xF3CE,"右向き三角",""),
new ExchangeLetter.structExchangeTable(0x7C51,0x25C0,0xF3CF,"左向き三角",""),
new ExchangeLetter.structExchangeTable(0x7C52,0x3016,0xF3D0,"すみ付きカッコ【",""),
new ExchangeLetter.structExchangeTable(0x7C53,0x3017,0xF3D1,"すみ付きカッコ】",""),
new ExchangeLetter.structExchangeTable(0x7C54,0xE2A1,0xF3D2,"四角ダイヤ+点",""),
new ExchangeLetter.structExchangeTable(0x7C55,0x00B2,0xF3D3,"二乗記号",""),
new ExchangeLetter.structExchangeTable(0x7C56,0x00B3,0xF3D4,"三乗記号",""),
new ExchangeLetter.structExchangeTable(0x7C57,0xE2A4,0xF3D5,"CD+丸",""),
new ExchangeLetter.structExchangeTable(0x7C58,0xE2A5,0xF3D6,"(vn)",""),
new ExchangeLetter.structExchangeTable(0x7C59,0xE2A6,0xF3D7,"(ob)",""),
new ExchangeLetter.structExchangeTable(0x7C5A,0xE2A7,0xF3D8,"(cb)",""),
new ExchangeLetter.structExchangeTable(0x7C5B,0xE2A8,0xF3D9,"(ce",""),
new ExchangeLetter.structExchangeTable(0x7C5C,0xE2A9,0xF3DA,"mb)",""),
new ExchangeLetter.structExchangeTable(0x7C5D,0xE2AA,0xF3DB,"(hp)",""),
new ExchangeLetter.structExchangeTable(0x7C5E,0xE2AB,0xF3DC,"（ｂｒ）",""),
new ExchangeLetter.structExchangeTable(0x7C5F,0xE2AC,0xF3DD,"(p)",""),
new ExchangeLetter.structExchangeTable(0x7C60,0xE2AD,0xF3DE,"(s)",""),
new ExchangeLetter.structExchangeTable(0x7C61,0xE2AE,0xF3DF,"(ms)",""),
new ExchangeLetter.structExchangeTable(0x7C62,0xE2AF,0xF3E0,"（ｔ）",""),
new ExchangeLetter.structExchangeTable(0x7C63,0xE2B0,0xF3E1,"(bs)",""),
new ExchangeLetter.structExchangeTable(0x7C64,0xE2B1,0xF3E2,"（ｂ）",""),
new ExchangeLetter.structExchangeTable(0x7C65,0xE2B2,0xF3E3,"(tb)",""),
new ExchangeLetter.structExchangeTable(0x7C66,0xE2B3,0xF3E4,"（ｔｐ）",""),
new ExchangeLetter.structExchangeTable(0x7C67,0xE2B4,0xF3E5,"（ｄｓ）",""),
new ExchangeLetter.structExchangeTable(0x7C68,0xE2B5,0xF3E6,"(ag)",""),
new ExchangeLetter.structExchangeTable(0x7C69,0xE2B6,0xF3E7,"(eg)",""),
new ExchangeLetter.structExchangeTable(0x7C6A,0xE2B7,0xF3E8,"(vo)",""),
new ExchangeLetter.structExchangeTable(0x7C6B,0xE2B8,0xF3E9,"(fl)",""),
new ExchangeLetter.structExchangeTable(0x7C6C,0xE2B9,0xF3EA,"(ke",""),
new ExchangeLetter.structExchangeTable(0x7C6D,0xE2BA,0xF3EB,"y)",""),
new ExchangeLetter.structExchangeTable(0x7C6E,0xE2BB,0xF3EC,"(sa",""),
new ExchangeLetter.structExchangeTable(0x7C6F,0xE2BC,0xF3ED,"x)",""),
new ExchangeLetter.structExchangeTable(0x7C70,0xE2BD,0xF3EE,"(sy",""),
new ExchangeLetter.structExchangeTable(0x7C71,0xE2BE,0xF3EF,"n)",""),
new ExchangeLetter.structExchangeTable(0x7C72,0xE2BF,0xF3F0,"(or",""),
new ExchangeLetter.structExchangeTable(0x7C73,0xE2C0,0xF3F1,"ｇ）",""),
new ExchangeLetter.structExchangeTable(0x7C74,0xE2C1,0xF3F2,"(pe",""),
new ExchangeLetter.structExchangeTable(0x7C75,0xE2C2,0xF3F3,"r)",""),
new ExchangeLetter.structExchangeTable(0x7C76,0xE3A7,0xF3F4,"R+丸",""),
new ExchangeLetter.structExchangeTable(0x7C77,0xE3A8,0xF3F5,"C+丸",""),
new ExchangeLetter.structExchangeTable(0x7C78,0xE2C3,0xF3F6,"箏+丸",""),
new ExchangeLetter.structExchangeTable(0x7C79,0xE2C4,0xF3F7,"DJ",""),
new ExchangeLetter.structExchangeTable(0x7C7A,0xE2C5,0xF3F8,"演+四角",""),
new ExchangeLetter.structExchangeTable(0x7C7B,0x213B,0xF3F9,"Fax",""),
new ExchangeLetter.structExchangeTable(0x7D21,0x322A,0xF440,"(月)",""),
new ExchangeLetter.structExchangeTable(0x7D22,0x322B,0xF441,"(火)",""),
new ExchangeLetter.structExchangeTable(0x7D23,0x322C,0xF442,"(水)",""),
new ExchangeLetter.structExchangeTable(0x7D24,0x322D,0xF443,"(木)",""),
new ExchangeLetter.structExchangeTable(0x7D25,0x322E,0xF444,"(金)",""),
new ExchangeLetter.structExchangeTable(0x7D26,0x322F,0xF445,"(土)",""),
new ExchangeLetter.structExchangeTable(0x7D27,0x2230,0xF446,"(日)",""),
new ExchangeLetter.structExchangeTable(0x7D28,0x3237,0xF447,"(祝)",""),
new ExchangeLetter.structExchangeTable(0x7D29,0x337E,0xF448,"明治",""),
new ExchangeLetter.structExchangeTable(0x7D2A,0x337D,0xF449,"大正",""),
new ExchangeLetter.structExchangeTable(0x7D2B,0x337C,0xF44A,"昭和",""),
new ExchangeLetter.structExchangeTable(0x7D2C,0x337B,0xF44B,"平成",""),
new ExchangeLetter.structExchangeTable(0x7D2D,0x2116,0xF44C,"No.",""),
new ExchangeLetter.structExchangeTable(0x7D2E,0x2121,0xF44D,"Tel",""),
new ExchangeLetter.structExchangeTable(0x7D2F,0x3036,0xF44E,"郵便記号",""),
new ExchangeLetter.structExchangeTable(0x7D30,0xE2CC,0xF44F,"野球の球",""),
new ExchangeLetter.structExchangeTable(0x7D31,0xE2C,0xF450,"〔本〕",""),
new ExchangeLetter.structExchangeTable(0x7D32,0xE2C,0xF451,"〔三〕",""),
new ExchangeLetter.structExchangeTable(0x7D33,0xE2C,0xF452,"〔二〕",""),
new ExchangeLetter.structExchangeTable(0x7D34,0xE2D0,0xF453,"〔安〕",""),
new ExchangeLetter.structExchangeTable(0x7D35,0xE2D1,0xF454,"〔点〕",""),
new ExchangeLetter.structExchangeTable(0x7D36,0xE2D2,0xF455,"〔打〕",""),
new ExchangeLetter.structExchangeTable(0x7D37,0xE2D3,0xF456,"〔盗〕",""),
new ExchangeLetter.structExchangeTable(0x7D38,0xE2D4,0xF457,"〔勝〕",""),
new ExchangeLetter.structExchangeTable(0x7D39,0xE2D5,0xF458,"〔敗〕",""),
new ExchangeLetter.structExchangeTable(0x7D3A,0xE2D6,0xF459,"〔S〕",""),
new ExchangeLetter.structExchangeTable(0x7D3B,0xE2D7,0xF45A,"投+四角",""),
new ExchangeLetter.structExchangeTable(0x7D3C,0xE2D8,0xF45B,"捕+四角",""),
new ExchangeLetter.structExchangeTable(0x7D3D,0xE2D9,0xF45C,"一+四角",""),
new ExchangeLetter.structExchangeTable(0x7D3E,0xE2D,0xF45D,"二+四角",""),
new ExchangeLetter.structExchangeTable(0x7D3F,0xE2D,0xF45E,"三+四角",""),
new ExchangeLetter.structExchangeTable(0x7D40,0xE2D,0xF45F,"遊+四角",""),
new ExchangeLetter.structExchangeTable(0x7D41,0xE2D,0xF460,"左+四角",""),
new ExchangeLetter.structExchangeTable(0x7D42,0xE2D,0xF461,"中+四角",""),
new ExchangeLetter.structExchangeTable(0x7D43,0xE2D,0xF462,"右+四角",""),
new ExchangeLetter.structExchangeTable(0x7D44,0xE2E0,0xF463,"指+四角",""),
new ExchangeLetter.structExchangeTable(0x7D45,0xE2E1,0xF464,"走+四角",""),
new ExchangeLetter.structExchangeTable(0x7D46,0xE2E2,0xF465,"打+四角",""),
new ExchangeLetter.structExchangeTable(0x7D47,0x2113,0xF466,"リットル（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D48,0x338F,0xF467,"キログラム（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D49,0x3390,0xF468,"ヘルツ（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D4A,0x33CA,0xF469,"ヘクタール（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D4B,0x339E,0xF46A,"キロメートル（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D4C,0x33A2,0xF46B,"平方キロメートル（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D4D,0x3371,0xF46C,"ヘクトパスカル（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D50,0x00BD,0xF46F,"1/2",""),
new ExchangeLetter.structExchangeTable(0x7D51,0xE2E5,0xF470,"0/3",""),
new ExchangeLetter.structExchangeTable(0x7D52,0x2153,0xF471,"1/3",""),
new ExchangeLetter.structExchangeTable(0x7D53,0x2154,0xF472,"2/3",""),
new ExchangeLetter.structExchangeTable(0x7D54,0x00BC,0xF473,"1/4",""),
new ExchangeLetter.structExchangeTable(0x7D55,0x00BE,0xF474,"3/4",""),
new ExchangeLetter.structExchangeTable(0x7D56,0x2155,0xF475,"1/5",""),
new ExchangeLetter.structExchangeTable(0x7D57,0x2156,0xF476,"2/5",""),
new ExchangeLetter.structExchangeTable(0x7D58,0x2157,0xF477,"3/5",""),
new ExchangeLetter.structExchangeTable(0x7D59,0x2158,0xF478,"4/5",""),
new ExchangeLetter.structExchangeTable(0x7D5A,0x2159,0xF479,"1/6",""),
new ExchangeLetter.structExchangeTable(0x7D5B,0x215A,0xF47A,"5/6",""),
new ExchangeLetter.structExchangeTable(0x7D5C,0xE2E6,0xF47B,"1/7",""),
new ExchangeLetter.structExchangeTable(0x7D5D,0x215B,0xF47C,"1/8",""),
new ExchangeLetter.structExchangeTable(0x7D5E,0xE2E7,0xF47D,"1/9",""),
new ExchangeLetter.structExchangeTable(0x7D5F,0xE2E8,0xF47E,"1/10",""),
new ExchangeLetter.structExchangeTable(0x7D60,0x2600,0xF480,"晴れ（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D61,0x2601,0xF481,"曇り（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D62,0x2602,0xF482,"傘（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D63,0x2603,0xF483,"雪だるま（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D64,0x2616,0xF484,"上向き〔白抜き〕",""),
new ExchangeLetter.structExchangeTable(0x7D65,0x2617,0xF485,"上向き（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7D66,0xE2EC,0xF486,"下向き〔白抜き〕",""),
new ExchangeLetter.structExchangeTable(0x7D67,0xE2ED,0xF487,"下向き（黒塗り）",""),
new ExchangeLetter.structExchangeTable(0x7D68,0x2666,0xF488,"ダイヤ",""),
new ExchangeLetter.structExchangeTable(0x7D69,0x2665,0xF489,"ハート",""),
new ExchangeLetter.structExchangeTable(0x7D6A,0x2663,0xF48A,"クローバー",""),
new ExchangeLetter.structExchangeTable(0x7D6B,0x2660,0xF48B,"スペード",""),
new ExchangeLetter.structExchangeTable(0x7D6C,0xE2EE,0xF48C,"四角+ダイヤ",""),
new ExchangeLetter.structExchangeTable(0x7D6D,0xE2EF,0xF48D,"丸+点",""),
new ExchangeLetter.structExchangeTable(0x7D6E,0x203C,0xF48E,"!!",""),
new ExchangeLetter.structExchangeTable(0x7D6F,0x2049,0xF48F,"!?",""),
new ExchangeLetter.structExchangeTable(0x7D70,0xE2F1,0xF490,"晴れ+曇り（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D71,0x2614,0xF491,"傘+雨（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D72,0xE2F3,0xF492,"雨（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D73,0x2603,0xF493,"雪だるま+雪（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D74,0xE2F5,0xF494,"雪だるま[塗]+雪（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D75,0x2607,0xF495,"雷（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D76,0x2608,0xF496,"雷+雨+雲（記号）",""),
new ExchangeLetter.structExchangeTable(0x7D78,0xE2F9,0xF498,"左ひげ",""),
new ExchangeLetter.structExchangeTable(0x7D79,0xE2FA,0xF499,"右ひげ",""),
new ExchangeLetter.structExchangeTable(0x7D7A,0x266C,0xF49A,"音符",""),
new ExchangeLetter.structExchangeTable(0x7D7B,0x260E,0xF49B,"電話",""),
new ExchangeLetter.structExchangeTable(0x7E21,0x2160,0xF49F,"Ⅰ(ローマ数字1）",""),
new ExchangeLetter.structExchangeTable(0x7E22,0x2161,0xF4A0,"Ⅱ(ローマ数字2）",""),
new ExchangeLetter.structExchangeTable(0x7E23,0x2162,0xF4A1,"Ⅲ(ローマ数字3）",""),
new ExchangeLetter.structExchangeTable(0x7E24,0x2163,0xF4A2,"Ⅳ(ローマ数字4）",""),
new ExchangeLetter.structExchangeTable(0x7E25,0x2164,0xF4A3,"Ⅴ(ローマ数字5）",""),
new ExchangeLetter.structExchangeTable(0x7E26,0x2165,0xF4A4,"Ⅵ(ローマ数字6）",""),
new ExchangeLetter.structExchangeTable(0x7E27,0x2166,0xF4A5,"Ⅶ(ローマ数字7）",""),
new ExchangeLetter.structExchangeTable(0x7E28,0x2167,0xF4A6,"Ⅷ(ローマ数字8）",""),
new ExchangeLetter.structExchangeTable(0x7E29,0x2168,0xF4A7,"Ⅸ(ローマ数字9）",""),
new ExchangeLetter.structExchangeTable(0x7E2A,0x2169,0xF4A8,"Ⅹ(ローマ数字10）",""),
new ExchangeLetter.structExchangeTable(0x7E2B,0x216A,0xF4A9,"ローマ数字11",""),
new ExchangeLetter.structExchangeTable(0x7E2C,0x216B,0xF4AA,"ローマ数字12",""),
new ExchangeLetter.structExchangeTable(0x7E2D,0x2470,0xF4AB,"17+丸",""),
new ExchangeLetter.structExchangeTable(0x7E2E,0x2471,0xF4AC,"18+丸",""),
new ExchangeLetter.structExchangeTable(0x7E2F,0x2472,0xF4AD,"19+丸",""),
new ExchangeLetter.structExchangeTable(0x7E30,0x2473,0xF4AE,"20+丸",""),
new ExchangeLetter.structExchangeTable(0x7E31,0x2474,0xF4AF ,"(1)",""),
new ExchangeLetter.structExchangeTable(0x7E32,0x2475,0xF4B0 ,"(2)",""),
new ExchangeLetter.structExchangeTable(0x7E33,0x2476,0xF4B1 ,"(3)",""),
new ExchangeLetter.structExchangeTable(0x7E34,0x2477,0xF4B2 ,"(4)",""),
new ExchangeLetter.structExchangeTable(0x7E35,0x2478,0xF4B3 ,"(5)",""),
new ExchangeLetter.structExchangeTable(0x7E36,0x2479,0xF4B4 ,"(6)",""),
new ExchangeLetter.structExchangeTable(0x7E37,0x247A,0xF4B5 ,"(7)",""),
new ExchangeLetter.structExchangeTable(0x7E38,0x247B,0xF4B6 ,"(8)",""),
new ExchangeLetter.structExchangeTable(0x7E39,0x247C,0xF4B7 ,"(9)",""),
new ExchangeLetter.structExchangeTable(0x7E3A,0x247D,0xF4B8 ,"(10)",""),
new ExchangeLetter.structExchangeTable(0x7E3B,0x247E,0xF4B9 ,"(11)",""),
new ExchangeLetter.structExchangeTable(0x7E3C,0x247F,0xF4BA ,"(12)",""),
new ExchangeLetter.structExchangeTable(0x7E3D,0x3251,0xF4BB,"21+丸",""),
new ExchangeLetter.structExchangeTable(0x7E3E,0x3252,0xF4BC,"22+丸",""),
new ExchangeLetter.structExchangeTable(0x7E3F,0x3253,0xF4BD,"23+丸",""),
new ExchangeLetter.structExchangeTable(0x7E40,0x3254,0xF4BE,"24+丸",""),
new ExchangeLetter.structExchangeTable(0x7E41,0xE383,0xF4BF,"(A)",""),
new ExchangeLetter.structExchangeTable(0x7E42,0xE384,0xF4C0,"(B)",""),
new ExchangeLetter.structExchangeTable(0x7E43,0xE385,0xF4C1,"（C）",""),
new ExchangeLetter.structExchangeTable(0x7E44,0xE386,0xF4C2,"（D）",""),
new ExchangeLetter.structExchangeTable(0x7E45,0xE387,0xF4C3,"（E）",""),
new ExchangeLetter.structExchangeTable(0x7E46,0xE388,0xF4C4,"（F）",""),
new ExchangeLetter.structExchangeTable(0x7E47,0xE389,0xF4C5,"（G）",""),
new ExchangeLetter.structExchangeTable(0x7E48,0xE38A,0xF4C6,"（H）",""),
new ExchangeLetter.structExchangeTable(0x7E49,0xE38B,0xF4C7,"（I）",""),
new ExchangeLetter.structExchangeTable(0x7E4A,0xE38C,0xF4C8,"（J）",""),
new ExchangeLetter.structExchangeTable(0x7E4B,0xE38D,0xF4C9,"（K）",""),
new ExchangeLetter.structExchangeTable(0x7E4C,0xE38E,0xF4CA,"（L）",""),
new ExchangeLetter.structExchangeTable(0x7E4D,0xE38F,0xF4CB,"（M）",""),
new ExchangeLetter.structExchangeTable(0x7E4E,0xE390,0xF4CC,"（N）",""),
new ExchangeLetter.structExchangeTable(0x7E4F,0xE391,0xF4CD,"(O)",""),
new ExchangeLetter.structExchangeTable(0x7E50,0xE392,0xF4CE,"(P)",""),
new ExchangeLetter.structExchangeTable(0x7E51,0xE393,0xF4CF,"(Q)",""),
new ExchangeLetter.structExchangeTable(0x7E52,0xE394,0xF4D0,"(R)",""),
new ExchangeLetter.structExchangeTable(0x7E53,0xE395,0xF4D1,"(S)",""),
new ExchangeLetter.structExchangeTable(0x7E54,0xE396,0xF4D2,"(T)",""),
new ExchangeLetter.structExchangeTable(0x7E55,0xE397,0xF4D3,"(U)",""),
new ExchangeLetter.structExchangeTable(0x7E56,0xE398,0xF4D4,"(V)",""),
new ExchangeLetter.structExchangeTable(0x7E57,0xE399,0xF4D5,"(W)",""),
new ExchangeLetter.structExchangeTable(0x7E58,0xE39A,0xF4D6,"(X)",""),
new ExchangeLetter.structExchangeTable(0x7E59,0xE39B,0xF4D7,"(Y)",""),
new ExchangeLetter.structExchangeTable(0x7E5A,0xE39C,0xF4D8,"(Z)",""),
new ExchangeLetter.structExchangeTable(0x7E5B,0x3255,0xF4D9,"25+丸",""),
new ExchangeLetter.structExchangeTable(0x7E5C,0x3256,0xF4DA,"26+丸",""),
new ExchangeLetter.structExchangeTable(0x7E5D,0x3257,0xF4DB,"27+丸",""),
new ExchangeLetter.structExchangeTable(0x7E5E,0x3258,0xF4DC,"28+丸",""),
new ExchangeLetter.structExchangeTable(0x7E5F,0x3259,0xF4DD,"29+丸",""),
new ExchangeLetter.structExchangeTable(0x7E60,0x325A,0xF4DE,"30+丸",""),
new ExchangeLetter.structExchangeTable(0x7E61,0x2460,0xF4DF,"1+丸",""),
new ExchangeLetter.structExchangeTable(0x7E62,0x2461,0xF4E0,"2+丸",""),
new ExchangeLetter.structExchangeTable(0x7E63,0x2462,0xF4E1,"3+丸",""),
new ExchangeLetter.structExchangeTable(0x7E64,0x2463,0xF4E2,"4+丸",""),
new ExchangeLetter.structExchangeTable(0x7E65,0x2464,0xF4E3,"5+丸",""),
new ExchangeLetter.structExchangeTable(0x7E66,0x2465,0xF4E4,"6+丸",""),
new ExchangeLetter.structExchangeTable(0x7E67,0x2466,0xF4E5,"7+丸",""),
new ExchangeLetter.structExchangeTable(0x7E68,0x2467,0xF4E6,"8+丸",""),
new ExchangeLetter.structExchangeTable(0x7E69,0x2468,0xF4E7,"9+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6A,0x2469,0xF4E8,"10+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6B,0x246A,0xF4E9,"11+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6C,0x246B,0xF4EA,"12+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6D,0x246C,0xF4EB,"13+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6E,0x246D,0xF4EC,"14+丸",""),
new ExchangeLetter.structExchangeTable(0x7E6F,0x246E,0xF4ED,"15+丸",""),
new ExchangeLetter.structExchangeTable(0x7E70,0x246F,0xF4EE,"16+丸",""),
new ExchangeLetter.structExchangeTable(0x7E71,0x278A,0xF4EF,"1+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E72,0x278B,0xF4F0,"2+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E73,0x278C,0xF4F1,"3+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E74,0x278D,0xF4F2,"4+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E75,0x278E,0xF4F3,"5+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E76,0x278F,0xF4F4,"6+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E77,0x2790,0xF4F5,"7+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E78,0x2791,0xF4F6,"8+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E79,0x2792,0xF4F7,"9+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E7A,0x2793,0xF4F8,"10+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E7B,0x24EB,0xF4F9,"11+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E7C,0x24EC,0xF4FA,"12+黒丸",""),
new ExchangeLetter.structExchangeTable(0x7E7D,0x325B,0xF4FB,"31+丸",""),
        };
#endif
    }


    [Serializable()]
    public abstract class epgUnitBin : epgUnit
    {
        const Int32 DEFAULT = -1;
        public Int32 value = DEFAULT;
        public Int32 numberOf = 1;

        public Int32 Count
        {
            get { return 2; }
        }

        public epgUnitBin(Int32 num)
        {
            this.numberOf = num;
        }
        public epgUnitBin(Int32 num, Int32 v) : this(num)
        {
            this.value = v;
        }
        public epgUnitBin(Int32 num, bool v) : this(num)
        {
            if (v == true)
            {
                this.value = 1;
            }
            else
            {
                this.value = 0;
            }
        }

        public bool isEmpty()
        {
            if (value == DEFAULT)
            {
                return true;
            }
            return false;
        }

        public void clear()
        {
            this.value = -1;
        }


        public string ToOutput()
        {
            string ret;

            if (this.value < 0)
            {
                return "#";
            }
            ret = Convert.ToString(this.value, 2).PadLeft(this.numberOf, '0');
            return ret.Substring(ret.Length - this.numberOf);
        }

        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitBin).value);
        }

    }

    [Serializable()]
    public abstract class epgUnitHex : epgUnit
    {
        const Int32 DEFAULT = -1;
        public Int32 value = DEFAULT;
        public bool prefix = false;
        public Int32 numberOf = 4;
        public Int32 max = Int32.MaxValue;
        public Int32 min = 0;

        public epgUnitHex(Int32 num)
        {
            this.numberOf = num;
        }
        public epgUnitHex(Int32 num, Int32 v)
        {
            this.value = v;
        }
        public bool isEmpty()
        {
            if (value == DEFAULT)
            {
                return true;
            }
            return false;
        }

        public void clear()
        {
            this.value = -1;
        }
        public void setMinMax(Int32 min, Int32 max){
            this.min = min;
            this.max = max;
        }

        public string ToOutput()
        {
            if( this.value == -1)
            {
                return "#";
            }
            string fmt = "X";
            if (this.numberOf > 0)
            {
                fmt += this.numberOf.ToString();
            }
            return ((this.prefix == true)?"0x":"") + value.ToString(fmt);
        }
        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitHex).value);
        }

    }

    [Serializable()]
    public abstract class epgUnitDec : epgUnit, IComparable
    {
        const Int32 DEFAULT = 0;
        protected Int32 localvalue = DEFAULT;
        public Int32 max = Int32.MaxValue;
        public Int32 min = 0;
        public Int32 numberOf = 0;
        
        virtual public Int32 value
        {
            get { return localvalue; }
            set
            {
                if (value < this.min)
                {
                    localvalue = this.min;
                }
                else if (value > this.max)
                {
                    localvalue = this.max;
                }
                else
                {
                    localvalue = value;
                }
            }
        }

        public Int32 Count
        {
            get { return this.max + 1; }
        }

        public epgUnitDec(Int32 num, Int32 v)
        {
            this.numberOf = num;
            if (v < min) v = min;
            if (v > max) v = max;
            this.value = v;
        }

        public epgUnitDec(Int32 num) 
        {
            this.numberOf = num;
            this.value = 0;
        }

        public bool isEmpty()
        {
            if (value == DEFAULT)
            {
                return true;
            }
            return false;
        }

        public void setMinMax(Int32 min, Int32 max)
        {
            this.min = min;
            this.max = max;
        }

        public virtual void clear()
        {
            this.value = 0;
        }
        public virtual string ToOutput()
        {
            if( this.value == -1){
                return "#";
            }
            string fmt = "0";
            if (this.numberOf != 0)
            {
                fmt = fmt.PadLeft(this.numberOf, '0');
            }

            return value.ToString(fmt);
        }
        public virtual string ToOutput(Int32 keta){
            if( this.value == -1){
                return "#";
            }
            string fmt = "0";
            fmt = fmt.PadLeft(keta, '0');
            return value.ToString(fmt);
        }
        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitDec).value);
        }

    }

    [Serializable()]
    public abstract class epgUnitDateTime : epgUnit
    {
        public DateTime value = DateTime.MinValue;
        public DateTime end;
        public bool timeOnly = false;

        public void clear()
        {
            this.value = DateTime.MinValue;
        }

        public epgUnitDateTime()
        {
        }

        public bool isEmpty()
        {
            if (value == DateTime.MinValue)
            {
                return true;
            }
            return false;
        }

        public epgUnitDateTime(string str)
        {
            this.setStart(str);
        }
        public epgUnitDateTime(DateTime t)
        {
            this.value = t;
        }


        public void setStart(string str, string pattern)
        {
            this.value = this.getValue(str, pattern);
        }

        public void setStart(string str)
        {
            if (str.StartsWith("cell-") == true)
            {
                this.value = this.getValue(str, @"cell-(?<programNo>\d{6})");
            }
            else
            {
                this.value = this.getValue(str);
            }
        }

        public void SetEnd(string str)
        {
            this.end = this.getValue(str);
        }

        public int CompareTo(object obj)
        {
            return this.value.CompareTo((obj as epgUnitDateTime).value);
        }


        //public void SetDuration(Int32 du)
        //{
        //    if (this.value != null)
        //    {
        //        this.end = this.value.AddMinutes((double)du);
        //    }
        //}

        private DateTime getValue(string str)
        {
            return this.getValue(str, "");
        }

        private DateTime getValue(string str, string pattern)
        {
            if (pattern.Length != 0)
            {
                pattern += @"(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})(?<hour>\d{2})(?<min>\d{2})";
            }
            Regex x = new Regex(pattern);
            Match m = x.Match(str);
            if (m.Success)
            {
                return new DateTime(
                    Int32.Parse(m.Groups["year"].Value),
                    Int32.Parse(m.Groups["month"].Value),
                    Int32.Parse(m.Groups["day"].Value),
                    Int32.Parse(m.Groups["hour"].Value),
                    Int32.Parse(m.Groups["min"].Value),
                    0
                    );
            }
            return DateTime.MinValue;
        }

        public string ToOutput()
        {
            string fmt = "yyyyMMdd HHmmss";
            if (this.timeOnly == true)
            {
                fmt = "HHmmss";
            }
            return value.ToString(fmt);
        }

        //public string ToOuputDuration()
        //{
        //    return (this.duration / 60).ToString("00") + (this.duration % 60).ToString("00") + "00";
        //}

    }
}
