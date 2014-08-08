using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExGrip.WinRT.Logging
{
    public interface ILogEntry
    {
        LogSeverity EntrySeverity { get;set;}

        DateTime Time { get;set;}
        
        string Entry { get;set;}
    }
}
