
namespace Banking.Application.Accounts;
public class OpenAccountRequest
{
    public Guid CustomerId { get; set; }
    public decimal InitialCredit { get; set; }
    public  OpenAccountRequest() { }


}