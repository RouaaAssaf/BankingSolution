
namespace Banking.Application.Accounts;
public class OpenAccountRequest
{
    public Guid CustomerId { get; set; }
    public decimal InitialDeposit { get; set; }
    public  OpenAccountRequest() { }


}