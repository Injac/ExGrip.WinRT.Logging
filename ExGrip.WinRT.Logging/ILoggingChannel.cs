using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrip.WinRT.Logging
{
    public interface ILoggingChannel
    {
        LogSeverity DefaultSeverity { get; set;}

        bool IsActive { get;set;}

        string ChannelName { get;set;}

        Task<ILogEntry> LogMessage(ILogEntry entry);
         
       
    }
}
