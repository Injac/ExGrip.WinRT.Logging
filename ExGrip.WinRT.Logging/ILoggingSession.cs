using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrip.WinRT.Logging
{
    public interface ILoggingSession
    {

        Dictionary<string, ILoggingChannel> LoggingChannels { get;}

        bool AddLoggingChannel(string channelName,ILoggingChannel channel);

        bool RemoveLoggingChannel(string channelName);

        Task<ILogEntry> LogToAllChannels(ILogEntry logEntry);
        
        Task<ILogEntry> LogToSpecificChannel(string channelName,ILogEntry logEntry);
                
    }
}
