using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ExGrip.WinRT.Logging;
using ExGrip.WinRT.Logging.Channels;
using ExGrip.WinRT.Logging.Helpers;
using ExGrip.WinRT.Logging.Sessions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace LoggerPhone {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
        private async void Button_Click(object sender, RoutedEventArgs e) {

            //Get  the LoggingSession instance
            LoggingSession sess = LoggingSession.Instance;

            //Create a new LogChannel
            MyFileLogChannel lgChannel = new MyFileLogChannel("mylog.txt", Windows.Storage.ApplicationData.Current.LocalFolder);

            //Init the channel
            await lgChannel.Init();

            //Activate the channel
            lgChannel.IsActive = true;

            //Add the channel to the logging session
            sess.LoggingChannels.Add("filelogger", lgChannel);


            //Try the concurrent file accesss
            Parallel.For(0, 10000, async (i) => {

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
            var testObject = new { FirstName = "Nikola", LastName = "Tesla", Time = new DateTime(1856, 7, 10) };

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
