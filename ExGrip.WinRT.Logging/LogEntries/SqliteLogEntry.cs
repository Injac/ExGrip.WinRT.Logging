using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrip.WinRT.Logging.LogEntries {

    public class SqliteLogEntry : ILogEntry {
        public string Entry {
            get;
            set;
        }

        public LogSeverity EntrySeverity {
            get;
            set;
        }

        public DateTime Time {
            get;
            set;
        }


        public override string ToString() {

            var insertStatement =
                Helpers.StringFormatter.FormatLogEntry("INSERT INTO LogTable (Entry,LogSeverity) VALUES ('{Entry}','{EntrySeverity}')", this);

            return insertStatement;
        }
    }
}
