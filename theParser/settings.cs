using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace theParser
{
    public class settings
    {
        public struct settingsStruct
        {
            public string TargetFolder;
            public Int32 daycount;
            public Int32 mode;
            public bool aribGaiji;

            public settingsStruct(string target, Int32 daycount, Int32 mode, bool aribGaiji)
            {
                this.TargetFolder = target;
                this.daycount = daycount;
                this.mode = mode;
                this.aribGaiji = aribGaiji;
            }
        }

        public settingsStruct conf;

        private string filename = "theParserSettings.xml";
        private string path = "";
        public settings(string path)
        {
            this.conf = new settingsStruct();
            this.path = path;
            if (this.read() == false)
            {
                this.conf.TargetFolder = path;
                this.conf.daycount = 7;
                this.conf.mode = 3;
                this.conf.aribGaiji = true;
                this.write();
            }
        }

        public settings()
            : this(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"))
        {
        }

        public void write()
        {
            XmlSerializer serial = new XmlSerializer(this.conf.GetType());
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(this.path, this.filename)))
                {
                    serial.Serialize(writer, this.conf);
                }
            }
            catch
            {

            }
        }

        public bool read()
        {
            bool flag = true;
            string fullFileName = System.IO.Path.Combine(path, this.filename);
            XmlSerializer serial = new XmlSerializer(this.conf.GetType());
            try
            {
                using (StreamReader reader = new StreamReader(fullFileName))
                {
                    this.conf = (settingsStruct)serial.Deserialize(reader);
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}
