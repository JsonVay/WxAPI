using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxAPI.Model
{
    [JsonObject(MemberSerialization.OptOut)]
    public class WeChatModel
    {
        [JsonProperty("appid")]
        public string Appid { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("sqlConnStrings")]
        public string SqlConnStrings { get; set; }

        public string Session_key { get; set; }
        public string OpenId { get; set; }
        public string Errcode { get; set; }
        public string Errmsg { get; set; }
        public string RId { get; set; }

    }
}
