#region

using AutoMapper;
using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Models
{
    public class QuestionnaireViewModel : Questionnaire
    {
        public bool IsReadOnly { get; set; }

        internal static QuestionnaireViewModel CreateFrom(Company company)
        {
            var vm = Mapper.Map<QuestionnaireViewModel>(company.Application);
            vm.IsReadOnly = company.Status > AppStatus.Open;
            return vm;
        }
    }
}