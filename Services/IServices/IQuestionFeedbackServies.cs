using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WxAPI.Services.IServices
{
    public interface IQuestionFeedbackServies
    {
        Task<DataTable> GetQuestionType();
        Task<DataTable> GetQuestionTypeDetaile(int pId);
        Task<DataTable> GetQuestionTypeInfoById(int id);
        Task<DataTable> GetPicInfoById(int id);
        Task<DataTable> GetQuestionTypeInfoJurisdiction();
    }
}
