#region

using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Services.Impl;

#endregion

namespace CustomerPortal.Web.Models
{
    public class CompanyViewModel
    {
        public Company Company { get; set; }


        public bool ShowLayoutTab { get; set; }
        public bool ShowLayoutTabFirst { get; set; }
        public string LayoutTabCheck { get; set; }

        public bool IsAppDescriptionReadOnly { get; set; }
        public string AppDescriptionTabCheck { get; set; }

        public QuestionnaireViewModel Questionnaire { get; set; }
        public bool ShowQuestionnaireInEditMode { get; set; }
        public string QuestionnaireTabCheck { get; set; }
        public bool IsQuestionnaireReadOnly { get; set; }

        public string StoreTabCheck { get; set; }
        public string GraphicsTabCheck { get; set; }

        public bool ShowVersionsTab { get; set; }
        public bool ShowVersionsTabFirst { get; set; }


        public static CompanyViewModel CreateFrom(Company company)
        {
            return new CompanyViewModel
            {
                Company = company,
                ShowLayoutTab = company.Status > AppStatus.Open,
                ShowLayoutTabFirst = company.Status == AppStatus.LayoutCompleted,
                LayoutTabCheck = company.LayoutsApprovedOn.HasValue ? "✔" : "",
                Questionnaire = QuestionnaireViewModel.CreateFrom(company),
                IsQuestionnaireReadOnly = company.Status > AppStatus.Open,
                ShowQuestionnaireInEditMode = string.IsNullOrEmpty(company.Application.AppName),
                QuestionnaireTabCheck = IsQuestionnaireCompleted(company) ? "✔" : "",
                IsAppDescriptionReadOnly = company.Status > AppStatus.Open,
                AppDescriptionTabCheck = IsAppDescriptionCompleted(company) ? "✔" : "",
                StoreTabCheck = IsStoreCompleted(company) ? "✔" : "",
                GraphicsTabCheck = IsGraphicsCompleted(company) ? "✔" : "",
                ShowVersionsTab = company.Status > AppStatus.LayoutCompleted,
                ShowVersionsTabFirst = company.Status == AppStatus.Test,
            };
        }


        private static bool IsStoreCompleted(Company company)
        {
            return AllPropertiesAreNotNull(company.Store)
                   && company.Store.UniqueDeviceIdentificationNumber.Any()
                   && AllPropertiesAreNotNull<StoreCredentials>(company.AppleAppStoreCredentials)
                   && AllPropertiesAreNotNull<StoreCredentials>(company.GooglePlayCredentials);
        }


        private static bool IsQuestionnaireCompleted(Company company)
        {
            return AllPropertiesAreNotNull(company.Application);
        }

        private static bool AllPropertiesAreNotNull<T>(T obj) where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return properties.All(x => x.GetValue(obj) != null);
        }

        private static bool IsAppDescriptionCompleted(Company company)
        {
            return !string.IsNullOrEmpty(company.AppDescription)
                   && !Regex.IsMatch(company.AppDescription, "{{[^}]+}}");
        }

        private static bool IsGraphicsCompleted(Company company)
        {
            return new GraphicsManager(company.Id).GetAll().Any();
        }
    }
}