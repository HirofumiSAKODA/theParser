using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace theParser
{
    public enum INCLUDECHECK { OK, SMALL, BIG, NG };

    public class tableRange
    {
        public Int32 topHigh;
        public Int32 topLow;
        public Int32 bottomHigh;
        public Int32 bottomLow;


        public Int32 top
        {
            get { return (topHigh << 8) | topLow; }
        }
        public Int32 bottom
        {
            get { return (bottomHigh << 8) | bottomLow; }
        }

        public tableRange(Int32 top, Int32 bottom)
        {
            this.topHigh = (top & 0xff00) >> 8;
            this.topLow = (top & 0xff);
            this.bottomHigh = (bottom & 0xff00) >> 8;
            this.bottomLow = (bottom & 0xff);
        }

        public Int32 getMyRaw()
        {
            return bottomLow - topLow;
        }

        public Int32 getMyLine()
        {
            return bottomHigh - topHigh;
        }

        /// <summary>
        /// topからbottomの数
        /// ※topとbottomで構成される四角形範囲の文字数
        /// </summary>
        /// <returns></returns>
        public Int32 getCountSquare()
        {
            Int32 ret = 0;
            ret = getMyRaw() * getMyLine();

            return ret;
        }

        /// <summary>
        /// topからbottomの個数
        /// ※横にカウントし、実際の文字数を返す。
        /// </summary>
        /// <param name="baseTopLow"></param>
        /// <param name="baseBottomLow"></param>
        /// <returns></returns>
        public Int32 getCountReal(Int32 baseTopLow, Int32 baseBottomLow)
        {
            Int32 ret = 0;

            if (topHigh == bottomHigh)
            {
                ret = bottomLow - topLow + 1;
            }
            else
            {
                ret = baseBottomLow - this.topLow;
                for (Int32 line = topHigh; line < bottomHigh; line++)
                {
                    ret += baseBottomLow - baseTopLow + 1;
                }
                ret += bottomLow - baseTopLow + 1;
            }
            return ret;
        }

        public INCLUDECHECK isInclude(int p)
        {
            if (p < top ) {
                return INCLUDECHECK.SMALL;
            }
            if (p > bottom)
            {
                return INCLUDECHECK.BIG;
            }
            return INCLUDECHECK.OK;
        }
        /// <summary>
        /// p の文字コードに、文字が当てはまるかどうかを確認
        /// </summary>
        /// <param name="p"></param>
        /// <returns>0:OK, 1:本tableより小さい, 2</returns>
        public INCLUDECHECK isIncludeRange(int p){
            Int32 pHigh = (p & 0xff00) >> 8;
            Int32 pLow = (p & 0xff);

            if( pHigh < this.topHigh){
                return INCLUDECHECK.SMALL;
            }
            if( pHigh > this.bottomHigh){
                return INCLUDECHECK.BIG;
            }
            if( pLow < this.topLow || pLow > this.bottomLow){
                return INCLUDECHECK.NG;
            }
            return INCLUDECHECK.OK;
        }
    }

    public class arib2byteChar
    {
        public Int32 maxLowNumber = 47; // 0x7f - 0x21
        private List<charFromTableBase> aribTableList;
        public Int32 currentTableNumber = 1;
        public charFromTableBase currentTable;
        public Int32 currentChar = 0;
        public Int32 startChar = 0;
        public Int32 endChar = 0xffff;
        public bool aribGaiji = false;

        public abstract class charFromTableBase 
        {
            public tableRange myRange;
            public Int32 myNumber;
            public string myDescribe;
            public abstract INCLUDECHECK isInclude(Int32 p);
            public abstract List<byte> getByteAry(Int32 p);
            public abstract List<byte> getByteAryJIS(Int32 p);
            public abstract INCLUDECHECK getNextChar(ref int p);
        }

        private class charFromMiharuTable : charFromTableBase // @@@
        {
            private List<Int32> codeList;

            public charFromMiharuTable(Int32 num, List<Int32> codeList)
            {
                this.myNumber = num;
                this.codeList = codeList;
                this.codeList.Sort();
                this.myRange = new tableRange(codeList.First(), codeList.Last());
            }

            override public INCLUDECHECK isInclude(Int32 p)
            {
                if (codeList[0] > p)
                {
                    return INCLUDECHECK.SMALL;
                }
                if (codeList.Last() < p)
                {
                    return INCLUDECHECK.BIG;
                }
                var codes = from item in codeList
                            where item == p
                            select item;
                if (codes.Count() == 0)
                {
                    return INCLUDECHECK.NG;
                }
                return INCLUDECHECK.OK;
            }

            override public List<byte> getByteAry(Int32 p)
            {
                List<byte> ret = new List<byte>();
                ret.Add((byte)((p & 0xff00) >> 8));
                ret.Add((byte)(p & 0xff));
                return ret;
            }

            override public List<byte> getByteAryJIS(Int32 p)
            {
                List<byte> jisAry = getByteAry(p);
                string temp = System.Text.Encoding.GetEncoding(932).GetString(jisAry.ToArray());
                return new List<byte>(System.Text.Encoding.GetEncoding(50220).GetBytes(temp));
            }

            override public INCLUDECHECK getNextChar(ref int p)
            {
                p++;
                return isInclude(p);
            }
        }
        private class charFromTable : charFromTableBase
        {
            public Int32 myCount;
            public List<tableRange> emptyList = new List<tableRange>();
            public List<tableRange> gaijiList = new List<tableRange>();

            public charFromTable(Int32 num)
            {
                this.myNumber = num;
            }

            public bool countMyChar()
            {
                myCount = myRange.getCountSquare();

                foreach (tableRange empty in this.emptyList)
                {
                    myCount -= empty.getCountReal(this.myRange.topLow, this.myRange.bottomLow);
                }
                return true;
            }


            override public INCLUDECHECK isInclude(int p)
            {
                if (myRange.isIncludeRange(p) == INCLUDECHECK.OK)
                {
                    foreach (tableRange empty in emptyList)
                    {
                        if (empty.isInclude(p) == INCLUDECHECK.OK)
                        {
                            return INCLUDECHECK.NG;
                        }
                    }
                }
                return INCLUDECHECK.OK;
            }

            public INCLUDECHECK isIncludeGaiji(int p)
            {
                foreach (tableRange gaiji in gaijiList)
                {
                    if (gaiji.isInclude(p) == INCLUDECHECK.OK)
                    {
                        return INCLUDECHECK.OK;
                    }
                }
                return INCLUDECHECK.NG;
            }

            override public INCLUDECHECK getNextChar(ref int p)
            {
                Int32 pHigh = (p & 0xff00) >> 8;
                Int32 pLow = (p & 0xff);

                pLow++;
                if (pLow > this.myRange.bottomLow)
                {
                    pLow = this.myRange.topLow;
                    pHigh++;
                    if (pHigh > this.myRange.bottomHigh)
                    {
                        return INCLUDECHECK.BIG;
                    }
                }
                p = (pHigh << 8) + pLow;
                return INCLUDECHECK.OK;
            }

            List<byte> kin = new List<byte> { 0x1b, 0x24, 0x42 }; // k-in
            List<byte> kout = new List<byte>{ 0x1b, 0x28, 0x4a }; // k-out

            override public List<byte> getByteAry(Int32 p)
            {
                List<byte> s = getByteAryJIS(p);
                // s は JISコード。SJISコードのbyte arrayを返す。
                string temp = System.Text.Encoding.GetEncoding(50220).GetString(s.ToArray());
                return new List<byte>(System.Text.Encoding.GetEncoding(932).GetBytes(temp));
            }
            override public List<byte> getByteAryJIS(Int32 p)
            {
                List<byte> ret = new List<byte>();
#if false
                    ret.AddRange(kin);
                    ret.Add((byte)((p & 0xff00) >> 8));
                    ret.Add((byte)(p & 0xff));
                    ret.AddRange(kout);
#endif
                if (isIncludeGaiji(p) == INCLUDECHECK.OK)
                {
                    ret.AddRange(System.Text.Encoding.ASCII.GetBytes(String.Format("\\#{0:X4};", p)));
                }
                else
                {
                    ret.AddRange(kin);
                    ret.Add((byte)((p & 0xff00) >> 8));
                    ret.Add((byte)(p & 0xff));
                    ret.AddRange(kout);
                }
                return ret;
            }
        }
        /*
        public Int32 getNextCharAry(ref List<byte> ret)
        {
            Int32 result;
            List<byte> letter = new List<byte>();
            result = this.getNextCharAry(ref letter);
            ret.AddRange(letter);
            
            return result;
        }
*/        

        
        /// <summary>
        /// 文字コード表からJISコードが取れるので、それをSJISに変換して、ByteArrayにして返す。
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        public Int32 getNextCharAry(ref List<byte> ret)
        {
            List<byte> sjisAry = new List<byte>();
            this.getNextCharAryJIS(ref sjisAry);
            ret.AddRange(this.exchangeJISAryToSJISAry(sjisAry));
            return 0;
        }

        /// <summary>
        /// 文字コード表からJISコードが取れるので、ByteArrayにして返す。
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        public Int32 getNextCharAryJIS(ref List<byte> ret)
        {

            while (currentChar <= this.endChar)
            {
                if (this.currentTable.getNextChar(ref this.currentChar) == INCLUDECHECK.BIG)
                {
                    this.currentTableNumber++;
                    if (this.aribTableList.Count > this.currentTableNumber) // 0はdummy
                    {
                        this.currentTable = this.aribTableList[this.currentTableNumber];
                        this.currentChar = this.currentTable.myRange.top;
                    }
                    else
                    {
                        return -1;
                    }
                }
                switch (this.currentTable.isInclude(this.currentChar))
                {
                case INCLUDECHECK.OK:
                    //jisAry = this.currentTable.getByteAry(this.currentChar);
                    //ret.AddRange(this.exchangeJISAryToSJISAry(jisAry));
                    ret.AddRange(this.currentTable.getByteAryJIS(this.currentChar));
                    return 0;
                // break;
                case INCLUDECHECK.BIG:
                    break;
                case INCLUDECHECK.SMALL:
                    break;
                case INCLUDECHECK.NG:
                    break;
                }
            }
            return -1;
        }

        public List<byte> exchangeJISAryToSJISAry(List<byte> letter)
        {
            List<byte> pre = new List<byte> { 0x1b, 0x24, 0x42 }; // k-in
            List<byte>post = new List<byte>{ 0x1b, 0x28, 0x4a }; // k-out

            pre.AddRange(letter);
            pre.AddRange(post);

            string jisStr = System.Text.Encoding.GetEncoding(50220).GetString(pre.ToArray());
            return new List<byte>(System.Text.Encoding.GetEncoding(932).GetBytes(jisStr));
        }



        public bool getCharAry(ref List<byte> ret)
        {
            if (this.currentTable.isInclude(this.currentChar) == INCLUDECHECK.OK)
            {
                ret = this.currentTable.getByteAry(this.currentChar);
                return true;
            }
            return false;
        }

        public bool setFirst()
        {
            this.currentTableNumber = 1;
            this.currentTable = this.aribTableList[this.currentTableNumber];
            this.startChar = this.aribTableList[1].myRange.top;
            this.endChar = this.aribTableList.Last().myRange.bottom;
            this.currentChar = this.startChar;
            return true;
        }

        public arib2byteChar(bool gaiji)
        {
            this.aribGaiji = gaiji;
            arib2byteCharInit();
        }

        public arib2byteChar()
        {
            arib2byteCharInit();
        }

        public void arib2byteCharInit(){
            this.aribTableList = new List<charFromTableBase>();

            this.aribTableList.Add(new charFromTable(0));

            Int32 tableNumber;
            charFromTable p;

            tableNumber = 1;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(1) 漢字系(1)";
            p.myRange = new tableRange(0x2121, 0x394f);
            p.emptyList.Add(new tableRange(0x2121, 0x2132));
            p.emptyList.Add(new tableRange(0x222f, 0x2239));
            p.emptyList.Add(new tableRange(0x2242, 0x2249));
            p.emptyList.Add(new tableRange(0x2321, 0x232f));
            p.emptyList.Add(new tableRange(0x233a, 0x2340));
            p.emptyList.Add(new tableRange(0x2639, 0x2640));
            p.emptyList.Add(new tableRange(0x2742, 0x274f));
            p.emptyList.Add(new tableRange(0x2841, 0x2f4f));
            this.aribTableList.Add(p);

            tableNumber = 2;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(2) 漢字系(2)";
            p.myRange = new tableRange(0x2150, 0x397e);
            p.emptyList.Add(new tableRange(0x2251, 0x225b));
            p.emptyList.Add(new tableRange(0x226b, 0x2271));
            p.emptyList.Add(new tableRange(0x227a, 0x227d));
            p.emptyList.Add(new tableRange(0x235b, 0x2360));
            p.emptyList.Add(new tableRange(0x237b, 0x237e));
            p.emptyList.Add(new tableRange(0x2474, 0x247e));
            p.emptyList.Add(new tableRange(0x2577, 0x257e));
            p.emptyList.Add(new tableRange(0x2659, 0x2750));
            p.emptyList.Add(new tableRange(0x2772, 0x2f7e));
            this.aribTableList.Add(p);

            tableNumber = 3;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(3) 漢字系(3)";
            p.myRange = new tableRange(0x3a21, 0x4f4f);
            this.aribTableList.Add(p);

            tableNumber = 4;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(4) 漢字系(4)";
            p.myRange = new tableRange(0x3a50, 0x4f7e);
            p.emptyList.Add(new tableRange(0x4f54, 0x4f7e));
            this.aribTableList.Add(p);

            tableNumber = 5;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(5) 漢字系(5)";
            p.myRange = new tableRange(0x5021, 0x674f);
            this.aribTableList.Add(p);

            tableNumber = 6;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(6) 漢字系(6)";
            p.myRange = new tableRange(0x5050, 0x677e);
            this.aribTableList.Add(p);

            tableNumber = 7;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(7) 漢字系(7)";
            p.myRange = new tableRange(0x6821, 0x7e4f);
            p.emptyList.Add(new tableRange(0x7427, 0x794f));
            p.emptyList.Add(new tableRange(0x7a27, 0x7a27));
            p.emptyList.Add(new tableRange(0x7a2c, 0x7a2f));
            p.emptyList.Add(new tableRange(0x7a32, 0x7a32));
            p.emptyList.Add(new tableRange(0x7a33, 0x7a33));
            p.emptyList.Add(new tableRange(0x7a49, 0x7a4f));
            p.emptyList.Add(new tableRange(0x7d4e, 0x7d4f));
            p.gaijiList.Add(new tableRange(0x7a21, 0x7e4f));
            if (this.aribGaiji == true)
            {
                this.aribTableList.Add(p);
            }
            else
            {
                p.emptyList.Add(new tableRange(0x7a21, 0x7e4f));
            }

            tableNumber = 8;
            p = new charFromTable(tableNumber);
            p.myDescribe = @"ARIB STD-B24-1-2,表7-4(8) 漢字系(8)";
            p.myRange = new tableRange(0x6850, 0x7e7e);
            p.emptyList.Add(new tableRange(0x7450, 0x7a5e));
            p.emptyList.Add(new tableRange(0x7a62, 0x7a7e));
            p.emptyList.Add(new tableRange(0x7b52, 0x7b7f));
            p.emptyList.Add(new tableRange(0x7c7c, 0x7c7e));
            p.emptyList.Add(new tableRange(0x7d7c, 0x7d7e));
            p.emptyList.Add(new tableRange(0x7e7e, 0x7e7e));
            if (this.aribGaiji == true)
            {
                p.gaijiList.Add(new tableRange(0x7a50, 0x7e7e));
            }
            else
            {
                p.emptyList.Add(new tableRange(0x7a50, 0x7e7e));
            }
            this.aribTableList.Add(p);

            tableNumber = 9;
            charFromMiharuTable pp = new charFromMiharuTable(tableNumber, getSjisCodeList());
            pp.myDescribe = "ARIB外字指定ミハル独自形式(SJIS外字領域)";

            this.aribTableList.Add(pp);

            this.setFirst();
        }

        private List<Int32> getSjisCodeList()
        {
            return getSjisCodeList(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"));
        }
        private List<Int32> getUtf8CodeList()
        {
            return getUtf8CodeList(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"));
        }

        private Int32 utf8TableCounter = 0;
        private List<Int32> utf8GaijiList = new List<int>();

        public bool getNextLetterUtf8Gaiji(ref string result)
        {
            result = "";
            if (this.utf8GaijiList.Count() == 0 || this.utf8GaijiList.Count <= this.utf8TableCounter)
            {
                this.utf8TableCounter = 0;
                this.utf8GaijiList = this.getUtf8CodeList();
                return false; // テーブル初期化時は false を返す。文字は返さない。
            }

            Int32 code = this.utf8GaijiList[this.utf8TableCounter];
            List<byte> codeAry = new List<byte>() { (byte)(code & 0xff), (byte)((code & 0xff00) >> 8) };
            result = Encoding.Unicode.GetString(codeAry.ToArray());
            this.utf8TableCounter++;
            return true;
        }


        private List<Int32> getSjisCodeList(string path)
        {
            List<Int32> results = new List<int>();
            string filename = System.IO.Path.Combine(path,"SJIS-GAIJI-CODE.txt");
            string defaultFilename = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"SJIS-GAIJI-CODE.txt");
            if (System.IO.File.Exists(filename) == false){
                if (System.IO.File.Exists(defaultFilename) == true)
                {
                    System.IO.File.Copy(defaultFilename, filename);
                }
            }
            if (System.IO.File.Exists(filename))
            {
                using (System.IO.StreamReader fr = new System.IO.StreamReader(filename))
                {
                    string line;
                    while (fr.EndOfStream != true)
                    {
                        line = fr.ReadLine();
                        if (line.StartsWith("#") == true)
                        {
                            continue;
                        }
                        Int32 result;
                        bool ret = Int32.TryParse(line.Trim(), System.Globalization.NumberStyles.HexNumber, null, out result);
                        if (ret == true)
                        {
                            results.Add(result);
                        }
                    }
                }
            }
            return results;
        }
        private List<Int32> getUtf8CodeList(string path)
        {
            List<Int32> results = new List<int>();
            string filename = System.IO.Path.Combine(path,"UTF8-GAIJI-CODE.txt");
            string defaultFilename = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "UTF8-GAIJI-CODE.txt");
            if (System.IO.File.Exists(filename) == false){
                if (System.IO.File.Exists(defaultFilename) == true)
                {
                    System.IO.File.Copy(defaultFilename, filename);
                }
            }
            if (System.IO.File.Exists(filename))
            {
                using (System.IO.StreamReader fr = new System.IO.StreamReader(filename))
                {
                    string line;
                    while (fr.EndOfStream != true)
                    {
                        line = fr.ReadLine();
                        if (line.StartsWith("#") == true)
                        {
                            continue;
                        }
                        Int32 result;
                        bool ret = Int32.TryParse(line.Trim(), System.Globalization.NumberStyles.HexNumber, null, out result);
                        if (ret == true)
                        {
                            results.Add(result);
                        }
                    }
                }
            }
            return results;
        }
    }
}
