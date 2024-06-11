using DACK.Models;

namespace DACK.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context,VnPaymentResponseModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
