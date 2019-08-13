using System.Threading.Tasks;

namespace LeanCode.SmsSender
{
    public interface ISmsSender
    {
        Task Send(string message, string phoneNumber);
    }
}
