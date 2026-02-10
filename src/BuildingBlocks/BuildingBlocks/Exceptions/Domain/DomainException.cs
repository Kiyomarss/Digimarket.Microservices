namespace BuildingBlocks.Exceptions.Domain;

public class DomainException : Exception
{
    //TODO: در رابطه با شیوه هندل کردن خطا در میدل ویر و ارسال پیام در خروجی مورد بررسی قرار گیرد
    public DomainException(string message) : base(message) { }
}
