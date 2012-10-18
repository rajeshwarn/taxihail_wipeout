using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MK.ConfigurationManager;
using MK.ConfigurationManager.Entities;

namespace MK.DeploymentService
{
    public class DeploymentJobService
    {
        private readonly Timer timer;
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        ConfigurationManagerDbContext DbContext { get; set; }

        public DeploymentJobService()
        {
            timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerOnElapsed(object state)
        {
            try
            {
                if (@lock.TryEnterWriteLock(0))
                {
                    CheckAndRunJob();
                }

            }
            catch (LockRecursionException)
            {
                Debug.WriteLine("LockRecursionException");
            }
            finally
            {
                if (@lock.IsWriteLockHeld) @lock.ExitWriteLock();
            }
        }

        private void CheckAndRunJob()
        {
            var job = DbContext.Set<DeploymentJob>().Where(x => x.Status == JobStatus.REQUESTED).FirstOrDefault();
            if(job != null)
            {
                try
                {
                    job.Status = JobStatus.INPROGRESS;
                    DbContext.SaveChanges();

                    if(job.Server)
                    {
                        
                    }



                    job.Status = JobStatus.SUCCESS;
                    DbContext.SaveChanges();

                }catch(Exception e)
                {
                    job.Status = JobStatus.ERROR;
                    job.Details = e.Message;
                    DbContext.SaveChanges();
                }
            }
        }

        public void Start()
        {
            Database.SetInitializer<ConfigurationManagerDbContext>(null);
            DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            timer.Change(0, 2000);
        }

        

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}