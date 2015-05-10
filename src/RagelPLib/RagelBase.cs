 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;


namespace RagelP
{
    public abstract class RagelBase
    {
        // 
        // Ragel vars
        //

        /// <summary>
        /// Ragel stack
        /// </summary>
        protected int[] stack = new int[256];

        /// <summary>
        /// Stack top
        /// </summary>
        protected int top = 0;

        /// <summary>
        /// Token Start
        /// </summary>
        protected int ts = -1;

        /// <summary>
        /// Token End
        /// </summary>
        protected int te = -1;
        protected int act = 0;

        /// <summary>
        /// Index of current character
        /// </summary>
        protected int p = 0;

        /// <summary>
        /// Last character index
        /// </summary>
        protected int pe = 0;

        /// <summary>
        /// Full data set
        /// </summary>
        protected char[] _data;

        /// <summary>
        /// Current line number
        /// </summary>
        /// Just a count of the '\n' s
        protected int line = 1;

        /// <summary>
        /// Current column
        /// </summary>
        protected int col = 0;

        //
        // Public Crap
        //        
        public void ScanFiles(IEnumerable<FileInfo> files)
        {            
            foreach (var file in files)
            {         
                ScanFile(file.FullName);
            }
        }

        public void ScanFiles(string dir, string ext, Func<string, bool> filter = null)
        {
            var files = Directory.GetFiles(dir, ext, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (filter != null && filter(file))
                    continue;


                ScanFile(file);
            }
        }
        public void ScanFile(string file)
        {
            OnFile(file);
            var text = File.ReadAllText(file,Encoding.Default);
            exec(text);
        }
        public void exec(string data)
        {
            var chars = data.ToCharArray();
            exec(chars, chars.Length);
        }
        public static TokenIterator QuickScan(string text)
        {
            Scanner s = new Scanner();
            s.init();
            s.exec(text);

            return new TokenIterator(s.finish().List);

        }
        protected string cur_line(int start)
        {
            char c = _data[start];
            int n = 0;
            for (int i = start; c != '\n' && i < _data.Length; ++i, ++n)
            {
                c = _data[i];
            }
            if (n > 0)
            {
                return new string(_data, start, n);
            }
            return null;
        }

        abstract public void exec(char[] data, int len);

        public String CurrentFile { get; protected set; }
        protected virtual void OnFile(string file)
        {
            CurrentFile = file;
            Debug.Print("File={0}", file);
        }
    }
}
