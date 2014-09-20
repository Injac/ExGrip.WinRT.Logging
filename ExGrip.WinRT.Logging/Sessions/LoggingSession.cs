using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrip.WinRT.Logging.Sessions {
    /// <summary>
    /// Standard implementation for
    /// a logging session. A logging
    /// session is used to manage logging
    /// to single or multiple logging-channels.
    /// </summary>
    public class LoggingSession:ILoggingSession {

        /// <summary>
        /// The singleton _instance
        /// </summary>
        private static readonly Lazy<LoggingSession> _instance =
            new Lazy<LoggingSession>(()=> new LoggingSession());

        private Dictionary<string,ILoggingChannel> _channels;

        /// <summary>
        /// Gets the logging channels.
        /// </summary>
        /// <value>
        /// The logging channels.
        /// </value>
        public Dictionary<string, ILoggingChannel> LoggingChannels {
            get {
                return this._channels;
            }

            private set {
                this._channels = value;
            }
        }

        /// <summary>
        /// Gets the LoggingSession instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static LoggingSession Instance {
            get {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LoggingSession"/> class from being created.
        /// </summary>
        private LoggingSession() {
            this.LoggingChannels = new Dictionary<string,ILoggingChannel>();
        }


        /// <summary>
        /// Adds the logging channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Parameter cannot be null or empty or whitespace;channelName</exception>
        public bool AddLoggingChannel(string channelName, ILoggingChannel channel) {
            if(string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName)) {
                throw new ArgumentException("Parameter cannot be null or empty or whitespace","channelName");
            }

            var channelAdded = this.LoggingChannels.ContainsKey(channelName.ToUpper()) && (!this.LoggingChannels.ContainsValue(channel));

            if(!channelAdded) {
                this.LoggingChannels.Add(channelName.ToUpper(),channel);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the logging channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Parameter cannot be null or empty or whitespace;channelName</exception>
        public bool RemoveLoggingChannel(string channelName) {

            if(string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName)) {
                throw new ArgumentException("Parameter cannot be null or empty or whitespace","channelName");
            }

            var channelExists = this.LoggingChannels.ContainsKey(channelName.ToUpper());

            if(channelExists) {
                this.LoggingChannels.Remove(channelName.ToUpper());

                return true;
            }

            return false;
        }


        /// <summary>
        /// Logs to all channels.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        /// <returns></returns>
        public async Task<ILogEntry> LogToAllChannels(ILogEntry logEntry) {
            return await Task.Run<ILogEntry>(async ()=> {
                if(logEntry == null) {
                    throw new ArgumentException("Parameter cannot be null.","logEntry");
                }

                foreach(var channel in this.LoggingChannels) {
                    if(channel.Value.IsActive) {
                        await channel.Value.LogMessage(logEntry);
                    }
                }

                return logEntry;
            });
        }


        /// <summary>
        /// Logs to specific channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="logEntry">The log entry.</param>
        /// <returns></returns>
        public async Task<ILogEntry> LogToSpecificChannel(string channelName, ILogEntry logEntry) {

            return await Task.Run<ILogEntry>( async ()=> {

                try {
                    if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName)) {
                        throw new ArgumentException("Parameter cannot be null or empty or whitespace", "channelName");
                    }

                    if (logEntry == null) {
                        throw new ArgumentException("Parameter cannot be null.", "logEntry");
                    }

                    var channelExists = this.LoggingChannels.ContainsKey(channelName);

                    if (channelExists) {
                        var channelToLogTo = this.LoggingChannels.FirstOrDefault(c => c.Key.Equals(channelName)).Value;

                        if (channelToLogTo != null) {
                            if (channelToLogTo.IsActive) {
                                await channelToLogTo.LogMessage(logEntry);
                            }

                            else {
                                return null;
                            }

                            return logEntry;
                        }
                    }
                }

                catch (Exception ex) {

                    throw ex;
                }

                return null;
            });
        }
    }
}
