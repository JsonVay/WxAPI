using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WxAPI.Model
{
    [JsonObject(MemberSerialization.OptIn)]  
    public partial class JsonReturn
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty]
        public int Status { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        [JsonProperty]
        public object Data { get; set; }

        public JsonReturn()
        {
            this.Message = "请求成功";
            this.Status = (int)HttpStatusCode.OK;
        }
    }
}
