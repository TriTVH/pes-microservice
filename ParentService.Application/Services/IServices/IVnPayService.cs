using ParentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.Services.IServices
{
    public interface IVnPayService
    {
        Task<ResponseObject> GetPaymentUrl(string ipAdress, int formId);
        Task<ResponseObject> ConfirmPaymentUrl(
       string vnp_Amount,
       string vnp_OrderInfo,
       string vnp_PayDate,
       string vnp_TransactionStatus,
       string vnp_TxnRef);
    }
}
