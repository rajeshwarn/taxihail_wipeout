#region

using ExtendedMongoMembership;
using MongoDB.Bson;

#endregion

namespace CustomerPortal.Web.Security
{
    public static class MembershipAccountExtensions
    {
        public static T GetValue<T>(this MembershipAccountBase account, string name, T defaultValue) where T : BsonValue
        {
            if (account.CatchAll == null) return defaultValue;
            var value = account.CatchAll.GetValue(name, defaultValue);
            if (value is BsonNull) return defaultValue;
            return (T) value;
        }
    }
}