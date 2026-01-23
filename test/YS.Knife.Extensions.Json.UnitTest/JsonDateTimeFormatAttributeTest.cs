using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YS.Knife.Extensions.Json.UnitTest
{
    public class JsonDateTimeFormatAttributeTest
    {
        [Fact]
        public void ShouldDeserializeDateTime()
        {
            var content = "{\"Time\":\"2025-12-22 12:22:00\"}";
            var val = JsonSerializer.Deserialize<DateTimeModel>(content);
            val.Time.Should().Be(new DateTime(2025, 12, 22, 12, 22, 0));
        }
        [Fact]
        public void ShouldSerializeDateTime()
        {
            var data = new DateTimeModel { Time = new DateTime(2025, 12, 22, 12, 22, 0) };
            var val = JsonSerializer.Serialize(data);
            val.Should().Be("{\"Time\":\"2025-12-22 12:22:00\"}");
        }

        [Fact]
        public void ShouldDeserializeDateTimeOffset()
        {
            var content = "{\"Time\":\"2025-12-22 12:22:00 +08:00\"}";
            var val = JsonSerializer.Deserialize<DateTimeOffsetModel>(content);
            val.Time.Should().Be(new DateTimeOffset(2025, 12, 22, 12, 22, 0, TimeSpan.FromHours(8)));
        }
        [Fact]
        public void ShouldSerializeDateTimeOffset()
        {
            var data = new DateTimeOffsetModel { Time = new DateTimeOffset(2025, 12, 22, 12, 22, 0, TimeSpan.FromHours(8)) };
            var val = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            val.Should().Be("{\"Time\":\"2025-12-22 12:22:00 \u002B08:00\"}");
        }


        class DateTimeModel
        {
            [JsonDateTimeFormat("yyyy-MM-dd HH:mm:ss")]
            public DateTime Time { get; set; }
        }
        class DateTimeOffsetModel
        {
            [JsonDateTimeFormat("yyyy-MM-dd HH:mm:ss zzz")]
            public DateTimeOffset Time { get; set; }
        }
    }
}
