using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LongTaskApiExample.Data;
using LongTaskApiExample.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LongTaskApiExample.Providers
{
    public class LongTaskApiProvider
    {
        private readonly AppDbContext _db;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public LongTaskApiProvider(
            AppDbContext db,
            IBackgroundTaskQueue queue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LongTaskApiProvider> logger
            )
        {
            _db = db;
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task<List<Message>> GetJobList()
        {
            return _db.Messages.AsNoTracking().ToListAsync();
        }

        public async Task<string> RegistJob(string msg, int loop, double delay)
        {
            var requestKey = Guid.NewGuid().ToString();

            var requestJob = new Message();
            requestJob.Id = requestKey;
            requestJob.Text = msg;
            requestJob.UpdateTime = DateTime.Now;
            requestJob.Status = "Enqueue";
            requestJob.ProcessRate = 0.0f;

            _db.Messages.Add(requestJob);
            await _db.SaveChangesAsync();

            _queue.QueueBackgroundWorkItem(async token =>
            {
                var guid = requestKey;
                
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    // long task process
                    for (int i = 1; i <= loop; i++)
                    {
                        try
                        {
                            Message job = await db.FindAsync<Message>(requestKey);
                            job.UpdateTime = DateTime.Now;
                            job.ProcessRate = i / (float)loop;

                            var cancelJob = new CancelJob() { Id = guid };
                            var isAborted = await db.CancelJobs.ContainsAsync(cancelJob);
                            if (isAborted)
                            {
                                job.Status = "Aborted";
                            }
                            else
                            {
                                if (i == loop)
                                {
                                    job.Status = "Completed";
                                }
                                else
                                {
                                    job.Status = "Processing";
                                }
                            }
                            db.Update<Message>(job);
                            await db.SaveChangesAsync();


                            if (isAborted)
                            {
                                break;
                            }
                            
                            // do job
                            await Task.Delay(TimeSpan.FromSeconds(delay), token);
                            
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "An error occurred writing to the " +
                                "database. Error: {Message}", ex.Message);
                        }
                    }
                }

                _logger.LogInformation(
                    "Queued Background Task {Guid} is complete. 3/3", guid);
            });

            return requestKey;
        }

        public async Task<string> CancelJob(string id)
        {
            var job = new CancelJob { Id = id };
            var added = await _db.CancelJobs.ContainsAsync(job);
            if (added)
            {
                return $"already canceled. {id}";
            }

            await _db.CancelJobs.AddAsync(job);
            await _db.SaveChangesAsync();

            return "requst job abort is success.";
        }
    }
}
