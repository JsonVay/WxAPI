using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WxAPI.Model;
using WxAPI.Services.IServices;

namespace WxAPI.Services.Services
{
    public class QuestionFeedbackServies: IQuestionFeedbackServies
    {
        private readonly WeChatModel _weChatModel;

        public QuestionFeedbackServies(IOptions<WeChatModel> weChatModel)
        {
            _weChatModel = weChatModel.Value;
        }
        /// <summary>
        /// 获取问题类型
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> GetQuestionType()
        {
            DataTable dt = new DataTable();
            var sql = @"select Id,QuestionType,QuestionTypeCode from T_QualitySystem_QuestionType_Basics(nolock) 
                        where Status = 1 order by FIndex";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var result = await conn.ExecuteReaderAsync(sql);
            dt.Load(result);
            return dt;
        }

        /// <summary>
        /// 获取问题类型明细
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> GetQuestionTypeDetaile(int pId)
        {
            DataTable dt = new DataTable();
            var sql = @"select Id, QuestionDetail as QuestionType,PId,ReplyerDDId,Replyer,QuestionDetailCode
                        from T_QualitySystem_QuestionType_Basics_Detail(nolock)
			            where Status=1 and PId=@PId order by FIndex";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var result = await conn.ExecuteReaderAsync(sql,new { PId = pId });
            dt.Load(result);
            return dt;
        }

        public async Task<DataTable> GetQuestionTypeInfoById(int id)
        {
            DataTable dt = new DataTable();
            var sql = @"select a.Id,a.Fuserid,QuestionTypeName,a.CurHandlerDDId
                        ,d.Id as FId ,a.IsAnonymous ,a.FeedbackPerson,a.FeedbackTel ,a.QuestionTypeId
                        ,d.CreateOn ,d.CreateBy,d.CreateName,d.FContent,d.IsRefer,d.CreateName as ReferredBy,d.ReceiverBy
                        ,a.FactoryArea,a.SpecificLocation,IsComplete=isnull(a.IsComplete,0)
                        from T_QualitySystem_QuestionFeedback(nolock) a 
                        left join T_QualitySystem_QuestionType_Basics_Detail(nolock) b on a.QuestionTypeId=b.Id
                        left join T_QualitySystem_QuestionType_Basics(nolock) c on c.Id=b.PId
						left join T_QualitySystem_QuestionFeedbackAdd(nolock) d on a.Id=d.FeedbackId
			            where a.Status=1 and a.Id=@Id
						order by d.CreateOn asc";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var result = await conn.ExecuteReaderAsync(sql, new { Id = id });
            dt.Load(result);
            return dt;
        }

        public async Task<DataTable> GetPicInfoById(int id)
        {
            DataTable dt = new DataTable();
            var sql = @"select b.FeedbackAddId,a.Id,b.IsFeedback,b.FileName,b.Extension,b.FPicGuid from T_QualitySystem_QuestionFeedbackAdd(nolock) a
                       left join T_QualitySystem_QuestionFeedback_Pic(nolock) b on a.Id=b.FeedbackAddId
			            where a.Status=1 and b.Status=1 and a.Id=@Id";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var result = await conn.ExecuteReaderAsync(sql, new { Id = id });
            dt.Load(result);
            return dt;
        }

        /// <summary>
        /// 周报，详情权限名单
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> GetQuestionTypeInfoJurisdiction()
        {
            DataTable dt = new DataTable();
            var sql = @"select a.PersonnelJobNumber JobNumber, a.PersonnelDDId from T_QualitySystem_TypePermission(nolock) a 
                        left join T_QualitySystem_TypePermission_Detaile(nolock) b on a.Id = b.PId where b.PermissionId = @PermissionId";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var result = await conn.ExecuteReaderAsync(sql, new { PermissionId = 1 });
            dt.Load(result);
            return dt;
        }
    }
}
