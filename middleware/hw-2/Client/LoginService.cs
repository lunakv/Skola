using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Client
{
    public class LoginService
    {
        private readonly Login.Client _client;

        public LoginService(TProtocol protocol)
        {
            var multiplex = new TMultiplexedProtocol(protocol, nameof(Login));
            _client = new Login.Client(multiplex);
        }

        public async Task LogInAsync(string name, int key, CancellationToken cancellationToken)
        {
            try
            {
                await _client.logInAsync(name, key, cancellationToken);
                return;
            }
            catch (InvalidKeyException e)
            {
                if (e.__isset.expectedKey)
                {
                    key = e.ExpectedKey;
                }
                else
                {
                    throw;
                }
            }

            await _client.logInAsync(name, key, cancellationToken);
        }

        public Task LogOutAsync(CancellationToken cancellationToken)
        {
            return _client.logOutAsync(cancellationToken);
        }
    }
}