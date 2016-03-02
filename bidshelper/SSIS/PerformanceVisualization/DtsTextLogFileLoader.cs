using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    /// <summary>
    /// Lets you read an in-progress DTS log file and start capturing events before the package is done
    /// </summary>
    public class DtsTextLogFileLoader
    {
        private long _FilePosition = 0;
        private long _CharPosition = 0;
        private long _EventParsedPosition = 0;
        private string _LogFilePath;
        private List<string> _ListMonitoredEventNames = new List<string>(System.Enum.GetNames(typeof(BidsHelperCapturedDtsLogEvent)));
        private StringBuilder _LogFileCache = new StringBuilder();
        private object syncRoot = new object();

        public DtsTextLogFileLoader(string LogFilePath)
        {
            _LogFilePath = LogFilePath;
        }

        private void ReadToEnd()
        {
            FileStream stream = new FileStream(_LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Position = _FilePosition;
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.Unicode);
            _LogFileCache.Append(reader.ReadToEnd());
            _FilePosition = stream.Position;
            stream.Close();
        }

        private string ReadLine()
        {
            char[] arChars = new char[1];
            StringBuilder builder = new StringBuilder();
            while (_CharPosition < _LogFileCache.Length)
            {
                _LogFileCache.CopyTo((int)_CharPosition, arChars, 0, 1);
                char c = arChars[0];
                _CharPosition++;
                builder.Append(c);
                if (c == '\n')
                {
                    break;
                }
            }
            return builder.ToString();
        }
        
        public DtsLogEvent[] GetEvents(bool LogFileIsComplete)
        {
            lock (syncRoot)
            {
                ReadToEnd();

                _CharPosition = _EventParsedPosition;

                List<DtsLogEvent> list = new List<DtsLogEvent>();
                if (_EventParsedPosition == 0)
                {
                    //skip header row
                    string sHeader = ReadLine();
                    _EventParsedPosition += sHeader.Length;
                }

                StringBuilder sUnusedLines = new StringBuilder();
                while (_CharPosition < _LogFileCache.Length || LogFileIsComplete)
                {
                    long lngLastPosition = _CharPosition;
                    string sLine = ReadLine();

                    if (_CharPosition >= _LogFileCache.Length && LogFileIsComplete)
                    {
                        //if we've reached the end of the file, do one more loop and parse the last event
                        sLine = sUnusedLines.ToString() + sLine;
                    }

                    bool bIsNewEvent = false;
                    foreach (string sEvent in System.Enum.GetNames(typeof(BidsHelperCapturedDtsLogEvent)))
                    {
                        if (sLine.StartsWith(sEvent + ",") || sLine.StartsWith("User:" + sEvent + ","))
                        {
                            bIsNewEvent = true;
                            break;
                        }
                    }
                    if (bIsNewEvent && sUnusedLines.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("parsing event:");
                        System.Diagnostics.Debug.WriteLine(sUnusedLines.ToString());
                        DtsLogEvent e = ProcessEvent(sUnusedLines.ToString());
                        if (e != null)
                        {
                            list.Add(e);
                            sUnusedLines = new StringBuilder();
                            _EventParsedPosition = lngLastPosition;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("couldn't parse event: " + sUnusedLines.ToString());
                        }
                    }
                    
                    if (_CharPosition >= _LogFileCache.Length && LogFileIsComplete) break;

                    sUnusedLines.AppendLine(sLine);
                }
                System.Diagnostics.Debug.WriteLine("end of file");
                return list.ToArray();
            }
        }

        private DtsLogEvent ProcessEvent(string sFullEventRow)
        {
            //Columns: event,computer,operator,source,sourceid,executionid,starttime,endtime,datacode,databytes,message
            string[] sColumns = sFullEventRow.Split(new char[] { ',' }, 11);
            if (sColumns[0].StartsWith("User:")) sColumns[0] = sColumns[0].Substring("User:".Length); //trim "User:" off the beginning of the event
            if (sColumns.Length != 11 || !_ListMonitoredEventNames.Contains(sColumns[0]))
                return null;

            DtsLogEvent e = new DtsLogEvent();
            e.Event = (BidsHelperCapturedDtsLogEvent)System.Enum.Parse(typeof(BidsHelperCapturedDtsLogEvent), sColumns[0]);
            e.SourceName = sColumns[3];
            e.SourceId = sColumns[4];

            if (!DateTime.TryParse(sColumns[6], out e.StartTime))
            {
                System.Diagnostics.Debug.WriteLine("could not parse start time");
                return null;
            }
            if (!DateTime.TryParse(sColumns[7], out e.EndTime))
            {
                System.Diagnostics.Debug.WriteLine("could not parse end time");
                return null;
            }

            e.Message = sColumns[10];
            return e;
        }
    }
}
