using System;
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

        public async Task<int> LogInAsync(string name, int key, CancellationToken cancellationToken)
        {
            try
            {
                await _client.logInAsync(name, key, cancellationToken);
                return key;
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
            return key;
        }

        public Task LogOutAsync(CancellationToken cancellationToken)
        {
            return _client.logOutAsync(cancellationToken);
        }
    }
}