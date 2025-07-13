namespace Ecommerce.Common.Models;

public class Cart
{
    public User User { get; set; } = new User();
    public PaymentType PaymentType { get; set; } = PaymentType.CashOnDelivery;  
    public List<Item> Items { get; set; } = new List<Item>();
}

public enum PaymentType
{
    CashOnDelivery,
    CreditCard,
    DebitCard,
    PayPal,
    BankTransfer
}

public static class PaymentTypeExtensions
{
   
    public static string GetDisplayName(PaymentType paymentType)
    {
        return paymentType switch
        {
            PaymentType.CashOnDelivery => "Cash On Delivery",
            PaymentType.CreditCard => "Credit Card",
            PaymentType.DebitCard => "Debit Card",
            PaymentType.PayPal => "PayPal",
            PaymentType.BankTransfer => "Bank Transfer",
            _ => paymentType.ToString()
        };
    }
}

