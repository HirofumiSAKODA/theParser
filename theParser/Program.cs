using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace theParser
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            // String[] args = Environment.GetCommandLineArgs(); // { "start" }; //  
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
        }

    }
}
