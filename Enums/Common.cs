namespace Enums
{
    public class Common
    {

    }
    public enum Roles
    {
        SuperAdmin,
        thungan,
        phucvu,
        quanly
    }
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        COD = 0,
        BankTransfer = 1,
        VNPay = 2,
        MoMo = 3
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2
    }
}
