using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace dns_netcore
{
	class RecursiveResolver : IRecursiveResolver
	{
		private readonly IDNSClient _dnsClient;
		
		// cache of resolved domain names
		private readonly ConcurrentDictionary<string, IP4Addr> _cached = new();
		// caches of pending requests
		private readonly ConcurrentDictionary<string, Task<IP4Addr>> _resolving = new();
		private readonly ConcurrentDictionary<IP4Addr, Task<string>> _resolvingReverse = new();

		public RecursiveResolver(IDNSClient client)
		{
			this._dnsClient = client;
		}

		/// <summary>
		/// Check if a domain is either already cached or currently being resolved.
		/// If a cached entry is found, run a reverse query to confirm its validity.
		/// </summary>
		/// <param name="domain">The domain name to check against</param>
		/// <returns>(true, Address) if a corresponding address is found, (false, _) otherwise</returns>
		private async Task<(bool, IP4Addr)> CheckCache(string domain)
		{
			if (_resolving.TryGetValue(domain, out var resolver))
			{
				IP4Addr result = await resolver;
				return (true, result); // result will be moved to cache by the ResolveSubdomain task that created it
			}

			if (_cached.TryGetValue(domain, out IP4Addr address))
			{
				string reverse = await ResolveReverse(address);
				if (reverse == domain) return (true, address);
				
				// if reverse lookup doesn't match, the cache entry is outdated
				// could remove a newer valid entry, but that will at most reduce performance, not break integrity
				_cached.TryRemove(domain, out address);
			}
			
			return (false, new IP4Addr());
		}

		/// <summary>
		/// Resolves the address of a single subdomain, utilizing relevant caches
		/// </summary>
		/// <param name="subDomain">Name of the subdomain to resolve</param>
		/// <param name="resolver">IP of the parent resolver</param>
		/// <param name="resolverDomain">Domain name of <paramref name="resolver"/></param>
		/// <returns>IP address of <paramref name="subDomain"/></returns>
		private async Task<IP4Addr> ResolveSubdomain(string subDomain, IP4Addr resolver, string resolverDomain)
		{
			string fullDomain = string.IsNullOrEmpty(resolverDomain) ? subDomain : $"{subDomain}.{resolverDomain}";
			if (_resolving.TryGetValue(fullDomain, out var request))
			{
				return await request;
			}

			var result = new IP4Addr();
			Task<IP4Addr> req = _dnsClient.Resolve(resolver, subDomain);
			Task<IP4Addr> content = _resolving.GetOrAdd(fullDomain, req);
			try {
				result = await content;
				return result;
			}
			finally
			{
				// only the task that added the entry tries to move it
				if (content == req)
				{
					if (content.IsCompletedSuccessfully)
						_cached[fullDomain] = result;
					// make sure to remove a completed request even when it throws
					// the exception itself is propagated to caller
					_resolving.TryRemove(fullDomain, out _);
				}
			}
		}
		
		private async Task<string> ResolveReverse(IP4Addr address)
		{
			// same process as in ResolveSubdomain but for reverse lookups
			if (_resolvingReverse.TryGetValue(address, out var request))
			{
				return await request;
			}

			Task<string> req = _dnsClient.Reverse(address);
			Task<string> content = _resolvingReverse.GetOrAdd(address, req);
			try
			{
				return await content;
			}
			finally
			{
				if (content == req)
				{
					_resolvingReverse.TryRemove(address, out _);
				}
			}
		}
		
		public async Task<IP4Addr> ResolveRecursive(string domain)
		{
			(bool isCached, IP4Addr address) = await CheckCache(domain);
			if (isCached) return address;
			
			IP4Addr resolver = _dnsClient.GetRootServers()[0];
			string resolverDomain = "";
			// separate the first segment of the domain from the rest
			string[] split = domain.Split('.', 2);
			// don't recurse if domain is a TLD
			if (split.Length == 2 && !string.IsNullOrEmpty(split[1]))
			{
				resolver = await ResolveRecursive(split[1]);
				resolverDomain = split[1];
			}
			
			return await ResolveSubdomain(split[0], resolver, resolverDomain);
		}
	}
}
