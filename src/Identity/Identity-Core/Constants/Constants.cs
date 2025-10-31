namespace Hesabdari_Core.Constants;

public abstract class Constants
{
    public abstract class RabbitMq
    {
        public const string Hostname = "localhost";
    }
    
    public abstract class Role
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }
    
    public abstract class Claims
    {
        public const string FullAccess = nameof(FullAccess);
    }
}