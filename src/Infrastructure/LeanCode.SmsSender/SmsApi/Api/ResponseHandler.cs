using System.Linq;
using LeanCode.SmsSender.SmsApi.Exceptions;
using Newtonsoft.Json;

namespace LeanCode.SmsSender.SmsApi.Api
{
    public class ResponseHandler
    {
        private readonly string response;

        private ResponseHandler(string response)
        {
            this.response = response;
        }

        public static void Handle(string response)
        {
            var handler = new ResponseHandler(response);
            handler.Handle();
        }

        public void Handle()
        {
            TryHandleError();
        }

        private void TryHandleError()
        {
            try
            {
                Responses.Error error = JsonConvert.DeserializeObject<Responses.Error>(response);

                if (isClientError(error.Code))
                {
                    throw new ClientException(error.Code, error.Message);
                }
                if (isHostError(error.Code))
                {
                    throw new HostException(error.Code, error.Message);
                }
                throw new ActionException(error.Code, error.Message);
            }
            catch (JsonSerializationException)
            { }
        }

        private bool isClientError(int code)
        {
            int[] clientErrors = {
                101,  // Niepoprawne lub brak danych autoryzacji
                102,  // Nieprawidłowy login lub hasło
                103,  // Brak punków dla tego użytkownika
                105,  // Błędny adres IP
                110,  // Usługa nie jest dostępna na danym koncie
                1000, // Akcja dostępna tylko dla użytkownika głównego
                1001  // Nieprawidłowa akcja
            };

            return clientErrors.Contains(code);
        }

        private bool isHostError(int code)
        {
            int[] hosErrors = {
                8,   // Błąd w odwołaniu
                666, // Wewnętrzny błąd systemu
                999, // Wewnętrzny błąd systemu
                201  // Wewnętrzny błąd systemu
            };

            return hosErrors.Contains(code);
        }
    }
}