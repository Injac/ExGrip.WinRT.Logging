using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExGrip.WinRT.Logging.Exceptions;
using ExGrip.WinRT.Logging.Helpers;
using ExGrip.WinRT.Logging.LogEntries;
using SQLitePCL;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace ExGrip.WinRT.Logging.Channels {
    public class SqliteLoggingChannel : ILoggingChannel {

        //Inspired by
        //http://stackoverflow.com/questions/17801995/c-sharp-async-await-limit-of-calls-to-async-methods-locking
        //Used to lock db-access to one thread at a time
        private SemaphoreSlim _dbLock = new SemaphoreSlim(1);

        private SQLiteConnection sqlLiteConnection {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the SQL lite database.
        /// </summary>
        /// <value>
        /// The name of the SQL lite database.
        /// </value>
        private string sqlLiteDatabaseName {
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
        /// Gets or sets the log database path.
        /// </summary>
        /// <value>
        /// The log database path.
        /// </value>
        public string LogDBPath {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the maximum file size in bytes.
        /// </summary>
        /// <value>
        /// The maximum file size in bytes.
        /// </value>
        public virtual ulong MaxFileSizeInBytes {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteLoggingChannel"/> class.
        /// </summary>
        /// <param name="dabaseFileName">Name of the dabase file.</param>
        /// <exception cref="System.ArgumentException">Parameter cannot be null or empty.;databaseFileName</exception>
        public SqliteLoggingChannel(string dabaseFileName) {

            if (string.IsNullOrEmpty(dabaseFileName) || string.IsNullOrWhiteSpace(dabaseFileName)) {
                throw new ArgumentException("Parameter cannot be null or empty.", "databaseFileName");
            }

            this.sqlLiteDatabaseName = dabaseFileName;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Init() {

            await Task.Factory.StartNew(() => {
                StringBuilder query = new StringBuilder();

                query.AppendLine("CREATE TABLE IF NOT EXISTS LogTable (");
                query.AppendLine("Id INTEGER PRIMARY KEY AUTOINCREMENT,");
                query.AppendLine("Entry	TEXT,");
                query.AppendLine("LogSeverity	TEXT,");
                query.AppendLine("Time DATETIME DEFAULT CURRENT_TIMESTAMP");
                query.AppendLine(");");

                this.sqlLiteConnection = new SQLiteConnection(this.sqlLiteDatabaseName);

                var statement = this.sqlLiteConnection.Prepare(query.ToString());

                var result = statement.Step();

                if (result == SQLiteResult.ERROR) {
                    throw new SQLiteException("Could not create log-table.");

                }

                this.LogDBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, this.sqlLiteDatabaseName);
            });
        }



        /// <summary>
        /// Creates a zip file of the
        /// current log-file.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ArchiveLog() {

            //Implement your SQLLite backup logic here...

            await this.Init();
            this.IsActive = true;

        }


        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        /// <exception cref="SQLitePCL.SQLiteException">Could not add log entry.</exception>
        public async Task<ILogEntry> LogMessage(ILogEntry entry) {


            if (entry is SqliteLogEntry) {

                await _dbLock.WaitAsync();

                var sqlLiteEntry = entry as SqliteLogEntry;

                bool noSpaceLeft = false;


                try {

                    var spaceAvailable = await ApplicationData.Current.LocalFolder.GetFreeSpace();

                    var fullPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, this.sqlLiteDatabaseName);

                    var storageFile = await StorageFile.GetFileFromPathAsync(fullPath);

                    var fileSizeInBytes = await storageFile.GetFileSizeInBytes();


                    noSpaceLeft = (spaceAvailable < fileSizeInBytes);


                    if (!noSpaceLeft) {

                        if (fileSizeInBytes < this.MaxFileSizeInBytes) {


                            var statement = this.sqlLiteConnection.Prepare(sqlLiteEntry.ToString());

                            var result = statement.Step();

                            if (result == SQLiteResult.ERROR) {
                                throw new SQLiteException("Could not add log entry.");
                            }
                        }

                        else {


                            this.IsActive = false;

                            this.sqlLiteConnection.Dispose();

                            this.sqlLiteConnection = null;

                            await this.ArchiveLog();

                            this.IsActive = true;

                        }


                    }



                }

                finally {


                    _dbLock.Release();


                    if (noSpaceLeft) {

                        throw new StorageSpaceExceededException("No space left for the for the sqlite-logger.");
                    }
                }


            }

            return entry;

        }
    }
}
