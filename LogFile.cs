// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2015 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management;

namespace nRF5DFUTool
{
	public class LogFile
	{
		private static string m_fileName = null;

		private static object m_lockObject = null;

		public static EventHandler<LogFileEventArgs> LogFileEvent = null;

		static LogFile()
		{
			m_lockObject = new object();
		}

		public static void ClearLog()
		{
			if (String.IsNullOrEmpty(m_fileName))
				return;

			try
			{
				lock (m_lockObject)
				{
					string folder = Path.GetDirectoryName(m_fileName);

					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);

					using (System.IO.StreamWriter streamWriter = File.CreateText(m_fileName))
						streamWriter.Flush();
				}
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}

        public static void WriteLine(string format, params object[] args)
        {
            if (String.IsNullOrEmpty(m_fileName))
                return;

            try
            {
                lock (m_lockObject)
                {
                    using (System.IO.StreamWriter streamWriter = File.AppendText(m_fileName))
                    {
                        streamWriter.WriteLine(String.Format("{0}: {1}", DateTime.Now, String.Format(format, args)));
                        streamWriter.Flush();
                    }
                }

                if (LogFileEvent != null)
                    LogFileEvent(null, new LogFileEventArgs(String.Format(format, args)));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

		public static string FileName
		{
			get { return m_fileName; }
			set { m_fileName = value; }
		}
	}

    public class LogFileEventArgs
    {
        public string Text;

        public LogFileEventArgs(string text)
        {
            Text = text;
        }
    }
}
