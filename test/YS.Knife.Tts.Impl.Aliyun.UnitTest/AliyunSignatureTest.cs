namespace YS.Knife.Tts.Impl.Aliyun.UnitTest
{
    public class AliyunSignatureTest
    {
        [Fact]
        public void ShouldGetCorrectSignature()
        {
            /**
             * 
AccessKeyId:my_access_key_id
Action:CreateToken
Version:2019-02-28
Timestamp:2019-04-18T08:32:31Z
Format:JSON
RegionId:cn-shanghai
SignatureMethod:HMAC-SHA1
SignatureVersion:1.0
SignatureNonce:b924c8c3-6d03-4c5d-ad36-d984d3116788
             * 
             */
            var values = new Dictionary<string, string>
            {
                { "AccessKeyId", "my_access_key_id" },
                { "Action", "CreateToken" },
                { "Version", "2019-02-28" },
                { "Timestamp", "2019-04-18T08:32:31Z" },
                { "Format", "JSON" },
                { "RegionId", "cn-shanghai" },
                { "SignatureMethod", "HMAC-SHA1" },
                { "SignatureVersion", "1.0" },
                { "SignatureNonce", "b924c8c3-6d03-4c5d-ad36-d984d3116788" }
            };

            AliyunSignature.CalcSignature("GET", "/", values, "my_access_key_secret").Should().Be("hHq4yNsPitlfDJ2L0nQPdugdEzM%3D");
        }
    }
}
