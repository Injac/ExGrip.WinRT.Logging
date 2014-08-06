using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ExGrip.WinRT.Logging.Channels {
    /// <summary>
    ///
    /// </summary>
    public abstract class FileLoggingChannel:ILoggingChannel {


        //Inspired by
        //http://stackoverflow.com/questions/17801995/c-sharp-async-await-limit-of-calls-to-async-methods-locking
        //Used to lock file-access to one thread at a time
        private SemaphoreSlim _fileLock = new SemaphoreSlim(1);

        /// <summary>
        /// Gets or sets the name of the log file.
        /// </summary>
        /// <value>
        /// The name of the log file.
        /// </value>
        protected virtual string LogFileName {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        protected virtual StorageFolder Folder {
            get;
            set;
        }

        /// <summary>
        /// Gets the storage file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        protected virtual StorageFile File {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the default severity.
        /// </summary>
        /// <value>
        /// The default severity.
        /// </value>
        public LogSeverity DefaultSeverity {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        /// <value>
        /// The name of the channel.
        /// </value>
        public string ChannelName {
            get;
            set;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Init() {

            if (string.IsNullOrEmpty(this.LogFileName) || string.IsNullOrWhiteSpace(this.LogFileName)) {
                throw new ArgumentException(
                    "Parameter cannot be null, empty or whitespace. Please use the constructor to assign the folder and the filename.",
                    "LogFileName");
            }

            if (this.Folder == null) {
                throw new ArgumentException(
                    "Parameter cannot be null, empty or whitespace. Please use the constructor to assign the folder and the filename.",
                    "Folder");
            }

            this.File = await this.Folder.CreateFileAsync(this.LogFileName, CreationCollisionOption.OpenIfExists);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLoggingChannel"/> class.
        /// </summary>
        protected FileLoggingChannel(string fileName,StorageFolder logFolder) {

            if(string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName)) {
                throw new ArgumentException("Parameter cannot be null, empty or whitespace.", "fileName");
            }

            if(logFolder == null) {
                throw new ArgumentException("Parameter cannot be null.", "folder");
            }

            this.LogFileName = fileName;
            this.Folder = logFolder;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public async Task<ILogEntry> LogMessage(ILogEntry entry) {

            var logEntry = string.Format("{0}\t{1}\t{2}{3}", entry.Time, entry.EntrySeverity, entry.Entry,Environment.NewLine);


            await _fileLock.WaitAsync();

            try {


                await FileIO.AppendTextAsync(this.File, logEntry);
            }

            finally {
                _fileLock.Release();
            }


            return entry;

        }
    }
}
