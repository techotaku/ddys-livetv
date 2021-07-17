using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

namespace DDRK.LiveTV
{
    public static class Extensions
    {
        public static string UrlSafeBase64EncodeUtf8String(this string str)
        {
            return str.UTF8Encode().UrlSafeBase64Encode();
        }

        public static string UrlSafeBase64DecodeUtf8String(this string str)
        {
            return str.UrlSafeBase64Decode().UTF8Decode();
        }

        public static string UrlSafeBase64Encode(this byte[] data)
        {
            return data.Base64Encode().Replace('+', '-').Replace('/', '_');
        }

        public static byte[] UrlSafeBase64Decode(this string str)
        {
            return str.Replace('-', '+').Replace('_', '/').Base64Decode();
        }

        public static string Base64Encode(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Base64Decode(this string str)
        {
            return Convert.FromBase64String(str);
        }

        public static byte[] UTF8Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string UTF8Decode(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static string UrlEncode(this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public static string UrlDecode(this string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        public static byte[] Inflate(this byte[] input)
        {
            var data = input;
            if (input.Length > 2 && input[0] == 0x78 && input[1] == 0x9C)
            {
                data = input.Skip(2).ToArray();
            }

            using var compressed = new MemoryStream(data);
            using var decompressed = new MemoryStream();
            using var z = new DeflateStream(compressed, CompressionMode.Decompress);
            z.CopyTo(decompressed);
            return decompressed.ToArray();
        }

        public static byte[] UnGzip(this byte[] input)
        {
            using var compressed = new MemoryStream(input);
            using var decompressed = new MemoryStream();
            using var gz = new GZipStream(compressed, CompressionMode.Decompress);
            gz.CopyTo(decompressed);
            return decompressed.ToArray();
        }

        public static string LastStack(this Exception ex)
        {
            var stack = ex.StackTrace;
            if (string.IsNullOrWhiteSpace(stack))
            {
                return string.Empty;
            }

            stack = stack.TrimStart();

            var lineEnd = stack.IndexOf(Environment.NewLine);
            if (lineEnd <= 0)
            {
                return stack;
            }

            return stack.Substring(0, lineEnd);
        }

        public static void LogException(this ILogger logger, Exception exception, string message, params object[] args)
        {
            if (exception is AggregateException ex && ex.InnerExceptions != null && ex.InnerExceptions.Count > 0)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    logger.LogException(e, message, args);
                }
            }
            else if (exception is AggregateException ex2 && ex2.InnerException != null)
            {
                logger.LogException(ex2.InnerException, message, args);
            }
            else
            {
                var stack = exception.LastStack();
                var parameters = new List<object>();
                parameters.AddRange(args);
                parameters.Add(exception.GetType().Name);
                parameters.Add(exception.Message);
                parameters.Add(stack);
                logger.LogError(message + " Exception<{exception}>: \"{error}\", last stack: \"{stackTrace}\".", parameters.ToArray());
            }
        }
    }
}
