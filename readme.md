## ExGrip.WinRT.Logging ##

Yes, another logging framework for WinRT/Universal apps. It works with Windows Phone 8.1 and Windows 8.1 based XAML apps. It is written in C# and inspired by the classes available in the Windows.Foundation.Diagnostics namespace. Because those are not very extensible, I picked up the main concept and created an environment that will allow you to implement your own LoggingChannels or extend existing ones.

## Prerequisites ##

Since release 1.1.0 you need to install the SQLite extensions for Windows Phone and Windows Store apps, as well as the SQLite PCL NuGet-Package. The solution will try to restore the NuGet packages automatically, but you need to install the SQLite Visual Studio 2013 extensions for SQLite, you can read how to that here:

[Portable Class Library for SQLite](https://sqlitepcl.codeplex.com/documentation "Portable Class Library for SQLite")

**NOTE**:
If you have any problems with the "Create New NuGet Package From Project After Each Build"  - just delete that package and compile. 

## NuGet Packages ##

Currently there are three different NuGet packages for each platform. This will change soon and a single package will be available with the next release.

[X86](https://www.nuget.org/packages/ExGrip.WinRT.LoggingX86/ "X86 Package")
[X64](https://www.nuget.org/packages/ExGrip.WinRT.LoggingX64/ "X64 Package")
[ARM](https://www.nuget.org/packages/ExGrip.WinRT.LoggingARM/ "ARM Package")


## How it works ##

It's very simple. The whole story is, that there is a class called **LoggingSession**. This class can hold one or more **LoggingChannels** you can send your log-data to. The **LoggingSession** class is a singleton, that will only create one instance of itself at any time. That way it is the centralized point for all of you logging requirements within your app. On Windows Phone or Windows 8.

Each **LoggingChannel** represents something like the physical storage for your log-data. This can be a file or a database, or for example Windows Azure Storage like table-storage for example.

A **LoggingChannel** can also be active or inactive. If you add a new **LoggingChannel** to your **LoggingSession** instance and the channel is inactive, the **LoggingSession** will not send you log-messages to that channel. The **LoggingSession** instance can send your log-messages to all registered LogChannels at once or to a specific channel.

The solution contains a basic FileLoggingChannel implementation and two sample projects: One for Windows Phone 8.1 and one for Windows 8.1 . 

Both projects work with the same source on Windows Phone 8.1 and on Windows 8.1. Here is the sample source:

        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Runtime.InteropServices.WindowsRuntime;
        using System.Threading.Tasks;
        using ExGrip.WinRT.Logging;
        using ExGrip.WinRT.Logging.Channels;
        using ExGrip.WinRT.Logging.Helpers;
        using ExGrip.WinRT.Logging.LogEntries;
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

                    lgChannel.MaxFileSizeInBytes = 500000;

                    //Init the channel
                    await lgChannel.Init();

                    //Activate the channel
                    lgChannel.IsActive = true;


                    //Create a new Sqlite log-channel
                    MySqlLiteLogChannel sqlLogChannel = new MySqlLiteLogChannel("Log.db");
                    sqlLogChannel.IsActive = true;
                    sqlLogChannel.MaxFileSizeInBytes = 1048576; //1MB
                    await sqlLogChannel.Init();

                    //Add the channel to the logging session
                    sess.LoggingChannels.Add("filelogger", lgChannel);
                    sess.LoggingChannels.Add("sqlitelogger", sqlLogChannel);

                    //Try the concurrent file accesss
                    Parallel.For(0, 10000, async (i) => {

                        MyFileLogEntry lgEntry = new MyFileLogEntry() {
                            Entry = "Hello World " + i,
                            EntrySeverity = LogSeverity.Informational,
                            Time = DateTime.Now
                        };

                        SqliteLogEntry sqlEntry = new SqliteLogEntry() {
                            Entry = "Hello World " + i,
                            EntrySeverity = LogSeverity.Informational
                        };

                        //Log to a specific channel
                        var entry = await sess.LogToSpecificChannel("filelogger", lgEntry);

                        var sqLiteEntry = await sess.LogToSpecificChannel("sqlitelogger", sqlEntry);
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

            //Sample SqliteLoggingChannel implementation
            public class MySqlLiteLogChannel : SqliteLoggingChannel {
                public MySqlLiteLogChannel(string databaseFileName) : base(databaseFileName) {
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


First implement the abstract class **FileLoggingChannel**  and the interface **ILogEntry**. This will allow you to add your custom file-logging channel to the **LoggingSession** singleton. You instantiate it like this:

     LoggingSession sess = LoggingSession.Instance;

Then instantiate your custom file-logging channel and set it active and set the maximum log-file size in bytes. In this sample it is 500KB. If a file grows larger than 500KB it will be archived and zipped. Logging however does/can and will continue. Then call Init() on your file-logging channel. This will create the logfile. Without the call to Init() it will not work. Then add your log-channel to the log-session like this:

        sess.LoggingChannels.Add("filelogger", lgChannel);

Do that for any of your custom implemented channels as well. Because the sample is using only one channel, we simply log to a specific channel:

        //Log to a specific channel
        var entry = await sess.LogToSpecificChannel("filelogger", lgEntry);

Within the parallel for-loop you can see how to create a new ILogEntry that can be passed to your channel(s).

@awsomedevsigner

The licensing part:

The MIT License (MIT)

Copyright (c) <year> <copyright holders>

Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.

