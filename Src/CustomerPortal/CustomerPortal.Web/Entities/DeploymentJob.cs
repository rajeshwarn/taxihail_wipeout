#region

using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Entities
{
    public enum DeploymentJobType
    {
        ServerPackage = 1,

        DeployServer = 2,
        DeployClient = 3,
    }

    [BsonIgnoreExtraElements]
    public class DeploymentJob : IEntity
    {
        public DeploymentJob()
        {
            ServerSide = true;
            Database = false;
            ClientSide = false;
            Android = false;
            CallBox = false;
            IosAdhoc = false;
            Status = "Requested";
        }

        [Display(Name = "Company")]
        public Company Company { get; set; }

        public string Status { get; set; }

        [Display(Name = "Server")]
        public Environment Server { get; set; }

        [Display(Name = "Revision")]
        public Revision Revision { get; set; }

        public DeploymentJobType Type { get; set; }
        public bool ServerSide { get; set; }

        [Display(Name = "Database")]
        public bool Database { get; set; }
        public bool ClientSide { get; set; }
        public bool Android { get; set; }
        public bool CallBox { get; set; }
        public bool IosAdhoc { get; set; }
        public bool IosAppStore { get; set; }
        public string ServerUrl { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; }
        public string Details { get; set; }

        public string Action
        {
            get
            {
                var action = "";
                if (ServerSide)
                {
                    action = "Server: ";
                    if (Database)
                    {
                        action += "Db, ";
                    }
                }
                else if (ClientSide)
                {
                    action = "Client: ";
                    if (Android)
                    {
                        action += "Android, ";
                    }
                    if (CallBox)
                    {
                        action += "Callbox, ";
                    }
                    if (IosAdhoc)
                    {
                        action += "iOS(Adhoc), ";
                    }
                    if (IosAppStore)
                    {
                        action += "iOS(AppStore) ";
                    }
                }
                return action;
            }
        }

        public string Id { get; set; }
    }

    public enum JobStatus
    {
        Requested,
        Inprogress,
        Success,
        Error
    }
}