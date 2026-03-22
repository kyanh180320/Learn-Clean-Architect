using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Common.Extension
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert DateTime -> long (yyyyMMddHHmmss)
        /// </summary>
        public static long ToLongTimestamp(this DateTime dt)
        {
            return long.Parse(dt.ToString("yyyyMMddHHmmss"));
        }

        /// <summary>
        /// Convert long (yyyyMMddHHmmss) -> DateTime
        /// </summary>
        public static DateTime FromLongTimestamp(this long timestamp)
        {
            return DateTime.ParseExact(
                timestamp.ToString(),
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None
            );
        }
        /// <summary>Làm tròn lên đầu giờ kế tiếp. Nếu đã đúng HH:00:00 thì giữ nguyên.</summary>
        public static DateTime RoundUpToNextHour(this DateTime dt)
        {
            // Giữ nguyên Kind hiện tại (Unspecified/Local/Utc) để bạn tự kiểm soát
            var k = dt.Kind;
            var local = dt;
            if (k == DateTimeKind.Unspecified)
            {
                // Với timestamp của bạn không chứa timezone, Unspecified là hợp lý;
                // nếu muốn coi như Local thì thay bằng: dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            if (local.Minute == 0 && local.Second == 0 && local.Millisecond == 0)
                return local;

            return new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0, local.Kind)
                .AddHours(1);
        }
        public static int DaysBetween(long fromYmdHms, long toYmdHms)
        {
            var from = fromYmdHms.FromLongTimestamp();
            var to = toYmdHms.FromLongTimestamp();
            return (int)Math.Floor((to - from).TotalDays);
        }

        /// <summary>
        /// Parse chuỗi định dạng cố định "yyyy-MM-dd HH:mm:ss" → DateTime
        /// Cực nhanh, không cấp phát, không dùng TryParseExact.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTime(this string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length < 19)
                throw new FormatException("Invalid datetime format (expected yyyy-MM-dd HH:mm:ss)");

            ReadOnlySpan<char> text = s.AsSpan();

            // yyyy-MM-dd HH:mm:ss
            int year = (text[0] - '0') * 1000 +
                       (text[1] - '0') * 100 +
                       (text[2] - '0') * 10 +
                       (text[3] - '0');

            int month = (text[5] - '0') * 10 + (text[6] - '0');
            int day = (text[8] - '0') * 10 + (text[9] - '0');
            int hour = (text[11] - '0') * 10 + (text[12] - '0');
            int minute = (text[14] - '0') * 10 + (text[15] - '0');
            int second = (text[17] - '0') * 10 + (text[18] - '0');

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
        }
        public static long? DayStartToTs(string? ymd)
        {
            return TryParseDate(ymd, out var d) ? d.Date.ToLongTimestamp() : (long?)null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">yyyyMMdd hoặc yyyy-MM-dd</param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool TryParseDate(string? s, out DateTime d)
        {
            d = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s!.Length == 8 && long.TryParse(s, out _))
            {
                var y = int.Parse(s[..4]); var m = int.Parse(s.Substring(4, 2)); var day = int.Parse(s.Substring(6, 2));
                d = new DateTime(y, m, day); return true;
            }
            return DateTime.TryParse(s, out d);
        }
        /// <summary>
        /// Convert về Local time VietNam
        /// </summary>
        /// <param name="textValue"></param>
        /// <param name="endOfDay"></param>
        /// <returns></returns>
        public static DateTime? ConvertDateTimeLocal(string? textValue, bool? endOfDay = false)
        {
            if (string.IsNullOrWhiteSpace(textValue))
                return null;

            try
            {
                string[] formats =
                {
                    "yyyy-MM-dd",
                    "M/d/yyyy h:mm:ss tt",
                    "MM/dd/yyyy hh:mm:ss tt",
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "dd/MM/yyyy",
                    "dd/MM/yyyy HH:mm:ss",
                    "dd/MM/yyyy HH:mm",
                    "yyyy-MM-ddTHH:mm:ss",
                    "MM/dd/yyyy HH:mm:ss"
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(
                            textValue,
                            format,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeLocal, // ✅ Gỉa định giờ local, không chuyển sang UTC
                            out DateTime parsed))
                    {
                        if (endOfDay == true)
                        {
                            return parsed.Date.AddDays(1).AddSeconds(-1); // Trả về cuối ngày
                        }

                        return parsed;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Trả về long yyyyMMddHHmmss từ chuỗi, hoặc null nếu parse lỗi.
        /// </summary>
        public static long? ConvertDateTimeLocalToLongTs(string? textValue, bool? endOfDay = false)
        {
            var dt = ConvertDateTimeLocal(textValue, endOfDay);
            return dt.HasValue ? long.Parse(dt.Value.ToString("yyyyMMddHHmmss")) : (long?)null;
        }
        /// <summary>
        /// Trả về (fromTs, toTs) theo quy ước: from inclusive, to exclusive.
        /// - Nếu chỉ có from: to = from + 1 ngày.
        /// - Nếu from == to: tự mở rộng to = from + 1 ngày (fix case bạn gặp).
        /// </summary>
        public static (long? From, long? To) BuildDayRange(string? fromYmd, string? toYmd)
        {
            long? fromTs = ConvertDateTimeLocalToLongTs(fromYmd);
            long? toTs = ConvertDateTimeLocalToLongTs(toYmd);

            return (fromTs, toTs);
        }

        /// <summary>
        /// Chuỗi yyyyMMdd chuyển thành long yyyyMMdd000000
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryToYyyyMMdd000000(this ReadOnlySpan<char> s, out long result)
        {
            result = 0;

            // Độ dài phải là 8
            if (s.Length != 8)
                return false;

            // Kiểm tra từng ký tự phải là số
            for (int i = 0; i < 8; i++)
            {
                if (s[i] < '0' || s[i] > '9')
                    return false;
            }

            // Convert nhanh sang số (chưa thêm 000000)
            int year =
                (s[0] - '0') * 1000 +
                (s[1] - '0') * 100 +
                (s[2] - '0') * 10 +
                (s[3] - '0');

            int month =
                (s[4] - '0') * 10 +
                (s[5] - '0');

            int day =
                (s[6] - '0') * 10 +
                (s[7] - '0');

            // Validate tháng
            if (month < 1 || month > 12)
                return false;

            // Validate ngày
            int daysInMonth;
            try
            {
                daysInMonth = DateTime.DaysInMonth(year, month);
            }
            catch
            {
                return false; // năm không hợp lệ
            }

            if (day < 1 || day > daysInMonth)
                return false;

            // Tính số final yyyyMMdd000000
            long yyyyMMdd =
                (long)year * 10000 +
                (long)month * 100 +
                day;

            result = yyyyMMdd * 1_000_000;
            return true;
        }

        /// <summary>
        /// Chuỗi yyyyMMdd chuyển thành long yyyyMMdd000000
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ConvertStryyyyMMddToLongtimestamps(this string s, out long result)
            => s.AsSpan().TryToYyyyMMdd000000(out result);

        public static DateTime AddBusinessDays(DateTime start, int businessDays)
        {
            var due = start;
            var daysToAdd = businessDays;

            while (daysToAdd > 0)
            {
                due = due.AddDays(1);

                // bỏ T7/CN
                if (due.DayOfWeek != DayOfWeek.Saturday && due.DayOfWeek != DayOfWeek.Sunday)
                    daysToAdd--;
            }

            return due;
        }
    }
    public sealed class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly string[] Formats =
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.FFF"
        };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;

                if (DateTime.TryParseExact(s, Formats, null, DateTimeStyles.AssumeLocal, out var dt)) return dt;
                if (DateTime.TryParse(s, out dt)) return dt;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var epoch))
                return DateTimeOffset.FromUnixTimeSeconds(epoch).LocalDateTime;

            // Không parse được -> coi như null (hoặc ném JsonException tùy policy)
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

    }
}

