using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Storage.Streams;

namespace ExGrip.WinRT.Logging.Helpers {
    public static class ZipArchiver {



        /// <summary>
        /// Archives the text file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="archiveName">Name of the archive.</param>
        /// <returns></returns>
        public static async Task ArchiveTextFile(StorageFile file, string destinationFolder,string archiveName) {


            using (MemoryStream archiveStream = new MemoryStream()) {

                using (ZipArchive zip = new ZipArchive(archiveStream, ZipArchiveMode.Create, true)) {

                    var logArchive = zip.CreateEntry(Path.GetFileName(file.Name));

                    using (StreamWriter sw = new StreamWriter(logArchive.Open())) {

                        var logFileLines = await FileIO.ReadLinesAsync(file);

                        var logFileContent = string.Join(Environment.NewLine, logFileLines);

                        await sw.WriteAsync(logFileContent);


                    }


                }

                var archiveFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                                        destinationFolder, CreationCollisionOption.OpenIfExists);

                var zipFile = await archiveFolder.CreateFileAsync(
                                  string.Format("{0}_{1}.zip", DateTime.Now.Ticks, archiveName));

                archiveStream.Position = 0;

                using (Stream s = await zipFile.OpenStreamForWriteAsync()) {
                    archiveStream.CopyTo(s);

                }

            }
        }


        /// <summary>
        /// Archives the binary file.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="archiveName">Name of the archive.</param>
        /// <returns></returns>
        public static async Task ArchiveBinaryFile(string fullPath, string destinationFolder, string archiveName) {



            using (MemoryStream archiveStream = new MemoryStream()) {

                using (ZipArchive zip = new ZipArchive(archiveStream, ZipArchiveMode.Create, true)) {


                    var file = await StorageFile.GetFileFromPathAsync(fullPath);
                    var logArchive = zip.CreateEntry(Path.GetFileName(file.Name));

                    using (BinaryWriter sw = new BinaryWriter(logArchive.Open())) {

                        try {


                            IBuffer buffer = await FileIO.ReadBufferAsync(file);
                            byte[] bytes = buffer.ToArray();


                            sw.Write(bytes);


                        }

                        catch (Exception ex) {

                            throw;
                        }

                    }
                }

                var archiveFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                                        destinationFolder, CreationCollisionOption.OpenIfExists);

                var zipFile = await archiveFolder.CreateFileAsync(
                                  string.Format("{0}_{1}.zip", DateTime.Now.Ticks, archiveName));

                archiveStream.Position = 0;

                using (Stream s = await zipFile.OpenStreamForWriteAsync()) {
                    archiveStream.CopyTo(s);

                }

            }
        }





    }
}
