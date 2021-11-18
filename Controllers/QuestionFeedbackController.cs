using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WxAPI.Helper;
using WxAPI.Model;
using WxAPI.Services.IServices;

namespace WxAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class QuestionFeedbackController : ControllerBase
    {
        private readonly IQuestionFeedbackServies _questionFeedbackServies;
        public QuestionFeedbackController(IQuestionFeedbackServies questionFeedbackServies)
        {
            this._questionFeedbackServies = questionFeedbackServies;
        }

        /// <summary>
        /// 获取问题类型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> GetQuestionType()
        {
            var data = await _questionFeedbackServies.GetQuestionType();
            return Ok(data);
        }

        /// <summary>
        /// 获取问题类型明细
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> GetQuestionTypeDetaile(int pId)
        {
            var data = await _questionFeedbackServies.GetQuestionTypeDetaile(pId);
            return Ok(data);
        }
    }
}
