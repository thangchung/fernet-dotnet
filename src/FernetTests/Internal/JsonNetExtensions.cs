using Newtonsoft.Json;

namespace FernetTests.Internal
{
    public static class JsonNetExtensions
    {
        /// <summary>
        /// Parse data source with DateTime in ISO 8601 covered RFC 3339 standard
        /// https://stackoverflow.com/questions/33879036/convert-datetime-string-with-this-format-yyyy-mm-ddthhmmss-zzz
        /// </summary>
        /// <param name="jsonDataSource"></param>
        /// <returns></returns>
        public static TData DeserializeObject<TData>(this string jsonDataSource)
            where TData: class
        {
            var settings = new JsonSerializerSettings()
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            var data = JsonConvert.DeserializeObject<TData>(jsonDataSource, settings);
            return data;
        }
    }
}
