using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExGrip.WinRT.Logging.Helpers;
using ExGrip.WinRT.Logging.Sessions;
using ExGrip.WinRT.Logging.Channels;
using Windows.Storage;
using ExGrip.WinRT.Logging;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Logger {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {



        public MainPage() {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {

            //Get  the LoggingSession instance
            LoggingSession sess = LoggingSession.Instance;

            //Create a new LogChannel
            MyFileLogChannel lgChannel = new MyFileLogChannel("mylog.txt", Windows.Storage.ApplicationData.Current.LocalFolder);

            lgChannel.MaxFileSizeInBytes = 500000;

            //Init the channel
            await lgChannel.Init();

            //Activate the channel
            lgChannel.IsActive = true;

            //Add the channel to the logging session
            sess.LoggingChannels.Add("filelogger", lgChannel);


            //Try the concurrent file accesss
            Parallel.For(0, 10000, async (i)=>  {

                MyFileLogEntry lgEntry = new MyFileLogEntry() {
                    Entry = "Hello World " + i,
                    EntrySeverity = LogSeverity.Informational,
                    Time = DateTime.Now
                };

                //Log to a specific channel
                var entry = await sess.LogToSpecificChannel("filelogger", lgEntry);
            });


            //How to use the string formatter
            //Create a format string based on the properties of your object....
            string format = "{FirstName}, {LastName} , {Time}";

            //... and of course a object to be parsed ...
            var testObject = new { FirstName = "Nikola", LastName = "Tesla", Time = new DateTime(1856,7,10) };

            //... then call StringFormatter.FormatLogEntry...
            var formatted = StringFormatter.FormatLogEntry(format, testObject);

            //... and enjoy the formatted text :)
            this.formatTest.Text = formatted;
        }
    }

    //Sample FileLoggingChannel implementation
    public class MyFileLogChannel : FileLoggingChannel {
        public MyFileLogChannel(string fileName, StorageFolder logFolder) : base(fileName, logFolder) {
        }
    }

    //Sample ILogEntry implementation
    public class MyFileLogEntry : ILogEntry {
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
    }
}
