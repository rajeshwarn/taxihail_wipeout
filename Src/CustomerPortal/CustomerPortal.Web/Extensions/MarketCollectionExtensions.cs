using System.Linq;
using CustomerPortal.Web.Entities.Network;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoRepository;

namespace CustomerPortal.Web.Extensions
{
    public static class MarketCollectionExtensions
    {
        /// <summary>
        /// Searches (case insensitive) for a unique market and returns it if it exists
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public static Market GetMarket(this IRepository<Market> repo, string market)
        {
            if (market == null)
            {
                return null;
            }

            // this does a "like" search
            var nameQuery = Query<Market>.Matches(x => x.Name, new BsonRegularExpression(market, "i"));

            var results = repo.Collection.Find(nameQuery).ToList();
            return results.FirstOrDefault(x => x.Name.ToLowerInvariant() == market.ToLowerInvariant());
        }
    }
}