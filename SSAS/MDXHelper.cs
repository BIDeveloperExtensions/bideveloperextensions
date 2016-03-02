using System;
using System.Collections.Generic;
using System.Text;

namespace BIDSHelper
{
    class MDXHelper
    {
        private enum MDXSplitStatus { InMDX, InDashComment, InSlashComment, InBlockComment, InBrackets, InDoubleQuotes, InSingleQuotes };

        private static string[] MDXStatementSplit(string sMDX, bool bRemoveComments)
        {
            MDXSplitStatus status = MDXSplitStatus.InMDX;
            int iPos = 0;
            int iLastSplit = 0;
            int iCommentStart = 0;
            System.Collections.Generic.List<string> arrSplits = new System.Collections.Generic.List<string>();

            while (iPos < sMDX.Length)
            {
                try
                {
                    if (status == MDXSplitStatus.InMDX)
                    {
                        if (sMDX.Substring(iPos, 2) == "/*")
                        {
                            status = MDXSplitStatus.InBlockComment;
                            iCommentStart = iPos;
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 2) == "--")
                        {
                            status = MDXSplitStatus.InDashComment;
                            iCommentStart = iPos;
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 2) == "//")
                        {
                            status = MDXSplitStatus.InSlashComment;
                            iCommentStart = iPos;
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 1) == "[")
                        {
                            status = MDXSplitStatus.InBrackets;
                        }
                        else if (sMDX.Substring(iPos, 1) == "\"")
                        {
                            status = MDXSplitStatus.InDoubleQuotes;
                        }
                        else if (sMDX.Substring(iPos, 1) == "'")
                        {
                            status = MDXSplitStatus.InSingleQuotes;
                        }
                        else if (sMDX.Substring(iPos, 1) == ";") //split on semicolon only when it's in general MDX context
                        {
                            arrSplits.Add(TrimBlankLines(sMDX.Substring(iLastSplit, iPos - iLastSplit)) + ";");
                            iLastSplit = iPos + 1;
                        }
                    }
                    else if (status == MDXSplitStatus.InDashComment || status == MDXSplitStatus.InSlashComment)
                    {
                        if (Environment.NewLine.Contains(sMDX.Substring(iPos, 1)))
                        {
                            status = MDXSplitStatus.InMDX;
                            if (bRemoveComments)
                            {
                                sMDX = sMDX.Remove(iCommentStart, iPos - iCommentStart);
                                iPos -= (iPos - iCommentStart);
                            }
                        }
                    }
                    else if (status == MDXSplitStatus.InBlockComment)
                    {
                        if (sMDX.Substring(iPos, 2) == "*/")
                        {
                            status = MDXSplitStatus.InMDX;
                            iPos += 1;
                            if (bRemoveComments)
                            {
                                sMDX = sMDX.Remove(iCommentStart, iPos - iCommentStart);
                                iPos -= (iPos - iCommentStart);
                            }
                        }
                    }
                    else if (status == MDXSplitStatus.InBrackets)
                    {
                        if (sMDX.Substring(iPos, 1) == "]" && sMDX.Substring(iPos, 2) == "]]")
                        {
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 1) == "]" && sMDX.Substring(iPos, 2) != "]]")
                        {
                            status = MDXSplitStatus.InMDX;
                        }
                    }
                    else if (status == MDXSplitStatus.InDoubleQuotes)
                    {
                        if (sMDX.Substring(iPos, 1) == "\"" && sMDX.Substring(iPos, 2) == "\"\"")
                        {
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 1) == "\"" && sMDX.Substring(iPos, 2) != "\"\"")
                        {
                            status = MDXSplitStatus.InMDX;
                        }
                    }
                    else if (status == MDXSplitStatus.InSingleQuotes)
                    {
                        if (sMDX.Substring(iPos, 1) == "'" && sMDX.Substring(iPos, 2) == "''")
                        {
                            iPos += 1;
                        }
                        else if (sMDX.Substring(iPos, 1) == "'" && sMDX.Substring(iPos, 2) != "''")
                        {
                            status = MDXSplitStatus.InMDX;
                        }
                    }
                    iPos++;
                }
                catch
                {
                    if (TrimBlankLines(sMDX.Substring(iLastSplit)).Length > 0)
                    {
                        arrSplits.Add(TrimBlankLines(sMDX.Substring(iLastSplit)));
                    }
                    iPos = sMDX.Length;
                }
            }
            return arrSplits.ToArray();
        }

        public static string TrimBlankLines(string sString)
        {
            //normalize newlines
            sString = sString.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] arrLines = sString.Split("\n".ToCharArray());
            sString = "";
            string sSkipped = "";
            foreach (string sLine in arrLines)
            {
                if (sLine.Trim().Length > 0)
                {
                    if (sString == "")
                        sString = sLine;
                    else
                        sString += "\n" + sSkipped + sLine;
                    sSkipped = "";
                }
                else
                {
                    sSkipped += sLine + "\n";
                }
            }
            return sString;
        }

        public static bool IsDrillthroughQuery(string sMDX)
        {
            string[] statements = MDXStatementSplit(sMDX, true);

            if (statements.Length > 0 && statements[0].Length > "drillthrough".Length && statements[0].Trim().Substring(0, "drillthrough".Length).ToLower() == "drillthrough")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
