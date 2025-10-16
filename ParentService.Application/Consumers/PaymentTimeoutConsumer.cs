using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.Consumers
{
    public class PaymentTimeoutConsumer : IConsumer<PaymentTimeoutEvent>
    {
        private readonly IAdmissionFormRepo _admissionFormRepo;

        public PaymentTimeoutConsumer(IAdmissionFormRepo admissionFormRepo)
        {
            _admissionFormRepo = admissionFormRepo;
        }

        public async Task Consume(ConsumeContext<PaymentTimeoutEvent> context)
        {
            var message = context.Message;
  

            var form = await _admissionFormRepo.GetAdmissionFormByIdAsync(message.AdmissionFormId);
            if (form == null)
            {
                return;
            }
            // Only revert if the payment is still pending
            if (form.Status.Equals("payment_in_progress", StringComparison.OrdinalIgnoreCase))
            {
                form.Status = "waiting_for_payment";
                await _admissionFormRepo.UpdateAdmissionFormAsync(form);
            }
        }

        }
}
