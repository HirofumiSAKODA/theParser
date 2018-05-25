﻿using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace theParser
{
    // TODO: SomeType のインスタンスをデバッグするときに、このビジュアライザーを表示するために SomeType の定義に次のコードを追加します:
    // 
    //  [DebuggerVisualizer(typeof(Visualizer1))]
    //  [Serializable]
    //  public class SomeType
    //  {
    //   ...
    //  }
    // 
    /// <summary>
    /// SomeType のビジュアライザーです。  
    /// </summary>
    public class Visualizer1 : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            if (windowService == null)
                throw new ArgumentNullException("windowService");
            if (objectProvider == null)
                throw new ArgumentNullException("objectProvider");

            // TODO: ビジュアライザーを表示する目的のオブジェクトを取得します。
            //       objectProvider.GetObject() の結果を視覚化 
            //       されるオブジェクトの型にキャストします。
            object data = (object)objectProvider.GetObject();

            // TODO: オブジェクトのビューを表示します。
            //       displayForm をユーザー独自のカスタム フォームまたはコントロールで置き換えます。
            using (Form displayForm = new Form())
            {
                displayForm.Text = data.ToString();
                windowService.ShowDialog(displayForm);
            }
        }

        // TODO: ビジュアライザーをテストするために、次のコードをユーザーのコードに追加します:
        // 
        //    Visualizer1.TestShowVisualizer(new SomeType());
        // 
        /// <summary>
        /// デバッガーの外部にホストすることにより、ビジュアライザーをテストします。
        /// </summary>
        /// <param name="objectToVisualize">ビジュアライザーに表示するオブジェクトです。</param>
        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(Visualizer1));
            visualizerHost.ShowVisualizer();
        }
    }
}
