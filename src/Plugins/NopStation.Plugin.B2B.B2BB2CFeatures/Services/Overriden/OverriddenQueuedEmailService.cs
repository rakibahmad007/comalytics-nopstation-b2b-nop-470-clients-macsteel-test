using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Messages;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverriddenQueuedEmailService : QueuedEmailService
{
    public OverriddenQueuedEmailService(IRepository<QueuedEmail> queuedEmailRepository) : 
        base(queuedEmailRepository)
    {

    }

    /// <summary>
    /// Inserts a queued email
    /// </summary>
    /// <param name="queuedEmail">Queued email</param>        
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InsertQueuedEmailAsync(QueuedEmail queuedEmail)
    {
        if (!string.IsNullOrWhiteSpace(queuedEmail.To))
        {
            var emailParts = queuedEmail.To.Split('@');
            var username = emailParts[0].Split('+')[0];
            queuedEmail.To = $"{username}@{emailParts[1]}";
        }
        await _queuedEmailRepository.InsertAsync(queuedEmail);
    }

    /// <summary>
    /// Updates a queued email
    /// </summary>
    /// <param name="queuedEmail">Queued email</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UpdateQueuedEmailAsync(QueuedEmail queuedEmail)
    {
        if (!string.IsNullOrWhiteSpace(queuedEmail.To))
        {
            var emailParts = queuedEmail.To.Split('@');
            var username = emailParts[0].Split('+')[0];
            queuedEmail.To = $"{username}@{emailParts[1]}";
        }
        await _queuedEmailRepository.UpdateAsync(queuedEmail);
    }
}
