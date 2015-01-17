 /*************************************************************************
	Author: Robert A. Olliff
	Date  : 1/16/2015 12:00:00 AM  

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paralizer.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LameUI());
        }
    }
}
