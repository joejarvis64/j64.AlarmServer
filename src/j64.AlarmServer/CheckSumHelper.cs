using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    public static class CheckSumHelper
    {
        private const string EndOfRequest = "\r\n"; // end of request

        public static string AppendCheckSumAndEndOfRequest(string value)
        {
            return value + GetChecksum(value) + EndOfRequest;
        }

        public static bool VerifyChecksum(ResponseCommand command, string payload, string checksum)
        {
            var calculatedChecksum = GetChecksum(command.GetStringValue() + payload);
            return calculatedChecksum.Equals(checksum);
        }

        private static string GetChecksum(string value)
        {
            //calculate the checksum as int
            var checksumValue = value.ToCharArray().Select(Convert.ToInt16).Aggregate(0, (current, intValue) => current + intValue);

            //convert to hex value as string
            var checksum = (String.Format("{0:x2}", checksumValue % 256).ToUpper());
            return checksum;
        }
    }
}
