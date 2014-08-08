using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExGrip.WinRT.Logging.Exceptions {
    public class StorageSpaceExceededException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageSpaceExceededException"/> class.
        /// </summary>
        public StorageSpaceExceededException() {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageSpaceExceededException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public StorageSpaceExceededException(string message) :base(message) {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageSpaceExceededException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public StorageSpaceExceededException(string message, Exception innerException) :base(message,innerException) {

        }
    }
}
