using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ExGrip.WinRT.Logging.Helpers {
    public static class StorageExtensions {

        /// <summary>
        /// Taken from
        /// http://stackoverflow.com/questions/19510449/disk-space-in-winrt-using-c-sharp-in-windows-8
        /// Calculates the free space of a IStorage item (a folder for example)
        /// </summary>
        /// <param name="sf">The IStorage Item</param>
        /// <returns>Available space left in IStorageItem (in bytes)</returns>
        public static async Task<ulong> GetFreeSpace(this IStorageItem sf) {

            var properties = await sf.GetBasicPropertiesAsync();
            var filteredProperties = await properties.RetrievePropertiesAsync(new[] { "System.FreeSpace" });
            var freeSpace = filteredProperties["System.FreeSpace"];

            return (UInt64)freeSpace;
        }


        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        /// <param name="sf">The sf.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Parameter cannot be null.;sf</exception>
        public static async Task<ulong> GetFileSizeInBytes(this IStorageItem sf) {

            if (sf != null) {

                var fileProperties = await sf.GetBasicPropertiesAsync();

                var size = fileProperties.Size;

                sf = null;

                return size;
            }

            else {
                throw new ArgumentException("Parameter cannot be null.", "sf");
            }
        }

    }
}
