#region

using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Domain
{
    public interface ICompanyService
    {
        Company GetCompany();
        void UpdateQuestionnaire(Questionnaire model);

        void UpdateAppleAppStoreCredentials(AppleStoreCredentials model);
        void UpdateGooglePlayCredentials(AndroidStoreCredentials model);
        void UpdateStoreSettings(StoreSettings model);
        void UpdateAppDescription(string appDescription);
        void ApproveLayouts();
    }
}