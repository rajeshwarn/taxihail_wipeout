using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Net
{
	[Serializable]
	public class CookieContainer
	{
		public const int DefaultCookieLengthLimit = 4096;
		public const int DefaultCookieLimit = 300;
		public const int DefaultPerDomainCookieLimit = 20;

		int capacity = DefaultCookieLimit;
		int perDomainCapacity = DefaultPerDomainCookieLimit;
		int maxCookieSize = DefaultCookieLengthLimit;
		CookieCollection2 cookies;

		// ctors
		public CookieContainer ()
		{
		}

		public CookieContainer (int capacity)
		{
			if (capacity <= 0)
				throw new ArgumentException ("Must be greater than zero", "Capacity");

			this.capacity = capacity;
		}

		public CookieContainer (int capacity, int perDomainCapacity, int maxCookieSize)
			: this (capacity)
		{
			if (perDomainCapacity != Int32.MaxValue && (perDomainCapacity <= 0 || perDomainCapacity > capacity))
				throw new ArgumentOutOfRangeException ("perDomainCapacity",
					string.Format ("PerDomainCapacity must be " +
						"greater than {0} and less than {1}.", 0,
						capacity));

			if (maxCookieSize <= 0)
				throw new ArgumentException ("Must be greater than zero", "MaxCookieSize");

			this.perDomainCapacity = perDomainCapacity;
			this.maxCookieSize = maxCookieSize;
		}

		// properties

		public int Count {
			get { return (cookies == null) ? 0 : cookies.Count; }
		}

		public int Capacity {
			get { return capacity; }
			set {
				if (value < 0 || (value < perDomainCapacity && perDomainCapacity != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value",
						string.Format ("Capacity must be greater " +
							"than {0} and less than {1}.", 0,
							perDomainCapacity));
				capacity = value;
			}
		}

		public int MaxCookieSize {
			get { return maxCookieSize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				maxCookieSize = value;
			}
		}

		public int PerDomainCapacity {
			get { return perDomainCapacity; }
			set {
				if (value != Int32.MaxValue && (value <= 0 || value > capacity))
					throw new ArgumentOutOfRangeException ("value");
				perDomainCapacity = value;
			}
		}

		public void Add (Cookie cookie)
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			AddCookie (cookie);
		}

		void AddCookie (Cookie cookie)
		{
			if (cookie.Domain.Length == 0)
				throw new ArgumentException ("Cookie domain not set.", "cookie.Domain");

			if (cookie.Value.Length > maxCookieSize)
				throw new Exception ("value is larger than MaxCookieSize.");
//
//			if ((cookie.Version == 1) && (cookie.Domain[0] != '.'))
//				throw new CookieException ("Invalid cookie domain: " + cookie.Domain);
//
//			if (cookie.HasDomain && !CheckPublicRoots (cookie.Domain))
//				throw new CookieException ("Invalid cookie domain: " + cookie.Domain);

			if (cookies == null)
				cookies = new CookieCollection2 ();

			if (cookies.Count >= capacity)
				RemoveOldest (null);

			// try to avoid counting per-domain
			if (cookies.Count >= perDomainCapacity) {
				if (CountDomain (cookie.Domain) >= perDomainCapacity)
					RemoveOldest (cookie.Domain);
			}

			// clone the important parts of the cookie
			Cookie c = new Cookie (cookie.Name, cookie.Value);
			c.Path = cookie.Path;
			c.Domain = cookie.Domain;
//			c.HasDomain = cookie.HasDomain;
			c.Version = cookie.Version;
			c.Expires = cookie.Expires;
			c.CommentUri = cookie.CommentUri;
			c.Comment = cookie.Comment;
			c.Discard = cookie.Discard;
			c.HttpOnly = cookie.HttpOnly;
			c.Secure = cookie.Secure;

			cookies.Add (c);
			CheckExpiration ();

		}

		int CountDomain (string domain)
		{
			int count = 0;
			foreach (Cookie c in cookies) {
				if (CheckDomain (domain, c.Domain, true))
					count++;
			}
			return count;
		}

		void RemoveOldest (string domain)
		{
			int n = 0;
			DateTime oldest = DateTime.MaxValue;
			for (int i = 0; i < cookies.Count; i++) {
				Cookie c = cookies [i];
				if ((c.TimeStamp < oldest) && ((domain == null) || (domain == c.Domain))) {
					oldest = c.TimeStamp;
					n = i;
				}
			}
			cookies.List.RemoveAt (n);
		}

		// Only needs to be called from AddCookie (Cookie) and GetCookies (Uri)
		void CheckExpiration ()
		{
			if (cookies == null)
				return;

			for (int i = cookies.Count - 1; i >= 0; i--) {
				Cookie cookie = cookies [i];
				if (cookie.Expired)
					cookies.List.RemoveAt (i);
			}
		}

		public void Add (CookieCollection cookies)
		{
			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie cookie in cookies)
				Add (cookie);
		}

		void Cook (Uri uri, Cookie cookie)
		{
			if (String.IsNullOrEmpty (cookie.Name))
				throw new Exception ("Invalid cookie: name");

			if (cookie.Value == null)
				throw new Exception ("Invalid cookie: value");

			if (uri != null) {
				if (cookie.Domain.Length == 0) {
					cookie.Domain = uri.Host;
//					cookie.HasDomain = false;
				}
//				} else if (cookie.HasDomain && !CheckSameOrigin (uri, cookie.Domain))
//					throw new CookieException ("Invalid cookie domain: " + cookie.Domain);
			}

			if (cookie.Version == 0 && String.IsNullOrEmpty (cookie.Path)) {
				if (uri != null) {
					cookie.Path = uri.AbsolutePath;
				} else {
					cookie.Path = "/";
				}
			}

			if (cookie.Version == 1 && cookie.Port.Length == 0 && uri != null && !uri.IsDefaultPort) {
				cookie.Port= uri.Port.ToString();
			}
		}

		public void Add (Uri uri, Cookie cookie)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			if (!cookie.Expired) {
				Cook (uri, cookie);
				AddCookie (cookie);
			}
		}

		public void Add (Uri uri, CookieCollection cookies)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie cookie in cookies) {
				if (!cookie.Expired) {
					Cook (uri, cookie);
					AddCookie (cookie);
				}
			}
		}

		public string GetCookieHeader (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			CookieCollection coll = GetCookies (uri);
			if (coll.Count == 0)
				return "";

			StringBuilder result = new StringBuilder ();
			foreach (Cookie cookie in coll) {
				// don't include the domain since it can be infered from the URI
				// include empty path as '/'
//				result.Append (cookie.ToString(uri));
//				result.Append ("; ");
			}

			if (result.Length > 0)
				result.Length -= 2; // remove trailing semicolon and space

			return result.ToString ();
		}

		internal static bool CheckPublicRoots (string domain)
		{
			if (string.IsNullOrEmpty (domain))
				return true;

			IPAddress address;
			if (IPAddress.TryParse (domain, out address))
				return domain[0] != '.';

			if (domain[0] == '.')
				domain = domain.Substring (1);

			if (string.Equals (domain, "localhost", StringComparison.InvariantCultureIgnoreCase))
				return true;

			var parts = domain.Split ('.');
			// Disallow TLDs
			if (parts.Length < 2)
				return false;

			// FIXME: Should probably use the public suffix list at
			// http://publicsuffix.org/list/ or something similar.
			return true;
		}

		internal static bool CheckSameOrigin (Uri uri, string domain)
		{
			if (!CheckPublicRoots (domain))
				return false;

			IPAddress address;
			if (IPAddress.TryParse (domain, out address)) {
				if (domain [0] == '.')
					return false;

				foreach (var ip in Dns.GetHostAddresses (uri.DnsSafeHost)) {
					if (address.Equals (ip))
						return true;
				}
				return false;
			}

			return CheckDomain (domain, uri.Host, false);
		}

		static bool CheckDomain (string domain, string host, bool exact)
		{
			if (domain.Length == 0)
				return false;

			var withoutDot = domain[0] == '.' ? domain.Substring (1) : domain;

			if (exact)
				return (String.Compare (host, withoutDot, StringComparison.InvariantCultureIgnoreCase) == 0);

			// check for allowed sub-domains - without string allocations
			if (!host.EndsWith (withoutDot, StringComparison.InvariantCultureIgnoreCase))
				return false;
			int p = host.Length - withoutDot.Length - 1;
			if (p < 0)
				return true;
			return (host [p] == '.');
		}

		static bool CheckDomain_RFC2109 (string domain, string host)
		{
			if (domain.Length == 0)
				return false;

			var withoutDot = domain[0] == '.' ? domain.Substring (1) : domain;
			return (String.Compare (host, withoutDot, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		public CookieCollection GetCookies (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			CheckExpiration ();
			CookieCollection2 coll = new CookieCollection2 ();
			if (cookies == null)
				return new CookieCollection();

			foreach (Cookie cookie in cookies) {
				string domain = cookie.Domain;
				if (cookie.Version == 1) {
					if (!CheckDomain_RFC2109 (domain, uri.Host))
						continue;
				} else {
					if (!CheckDomain (domain, uri.Host, false))
						continue;
				}


				string path = cookie.Path;
				string uripath = uri.AbsolutePath;
				if (path != "" && path != "/") {
					if (uripath != path) {
						if (!uripath.StartsWith (path))
							continue;

						if (path [path.Length - 1] != '/' && uripath.Length > path.Length &&
							uripath [path.Length] != '/')
							continue;
					}
				}

				if (cookie.Secure && uri.Scheme != "https")
					continue;

				coll.Add (cookie);
			}

			coll.Sort ();
			var result = new CookieCollection ();
			foreach (var item in coll.List) {
				result.Add (item);
			}

			return result;
		}

		public void SetCookies (Uri uri, string cookieHeader)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookieHeader == null)
				throw new ArgumentNullException ("cookieHeader");                        

			if (cookieHeader.Length == 0)
				return;

			// Cookies must be separated by ',' (like documented on MSDN)
			// but expires uses DAY, DD-MMM-YYYY HH:MM:SS GMT, so simple ',' search is wrong.
			// See http://msdn.microsoft.com/en-us/library/aa384321%28VS.85%29.aspx
			string [] jar = cookieHeader.Split (',');
			string tmpCookie;
			for (int i = 0; i < jar.Length; i++) {
				tmpCookie = jar [i];

				if (jar.Length > i + 1
					&& Regex.IsMatch (jar[i],
						@"													.*expires\s*=\s*(Mon|Tue|Wed|Thu|Fri|Sat|Sun)",
						RegexOptions.IgnoreCase)
					&& Regex.IsMatch (jar[i+1],
						@"													\s\d{2}-(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)-\d{4} \d{2}:\d{2}:\d{2} GMT",
						RegexOptions.IgnoreCase)) {
					tmpCookie = new StringBuilder (tmpCookie).Append (",").Append (jar [++i]).ToString ();
				}

				try {
					Cookie c = Parse (tmpCookie);

					// add default values from URI if missing from the string
					if (c.Path.Length == 0) {
						c.Path = uri.AbsolutePath;
					} else if (!uri.AbsolutePath.StartsWith (c.Path)) {
						string msg = String.Format ("'Path'='{0}' is invalid with URI", c.Path);
						throw new Exception (msg);
					}

					if (c.Domain.Length == 0) {
						c.Domain = uri.Host;
						// don't consider domain "a.b.com" as ".a.b.com"
						//c.HasDomain = false;
					} 
//					else if (c.HasDomain && !CheckSameOrigin (uri, c.Domain))
//						throw new CookieException ("Invalid cookie domain: " + c.Domain);

					AddCookie (c);
				}
				catch (Exception e) {
					string msg = String.Format ("Could not parse cookies for '{0}'.", uri);
					throw new Exception (msg, e);
				}
			}
		}

		static Cookie Parse (string s)
		{
			string [] parts = s.Split (';');
			Cookie c = new Cookie ();
			for (int i = 0; i < parts.Length; i++) {
				string key, value;
				int sep = parts[i].IndexOf ('=');
				if (sep == -1) {
					key = parts [i].Trim ();
					value = String.Empty;
				} else {
					key = parts [i].Substring (0, sep).Trim ();
					value = parts [i].Substring (sep + 1).Trim ();
				}

				switch (key.ToLowerInvariant ()) {
				case "path":
				case "$path":
					if (c.Path.Length == 0)
						c.Path = value;
					break;
				case "domain":
				case "$domain":
					if (c.Domain.Length == 0)
						c.Domain = value;
					break;
				case "expires":
				case "$expires":
					if (c.Expires == DateTime.MinValue)
						c.Expires = DateTime.SpecifyKind (DateTime.ParseExact (value,
							@"															ddd, dd-MMM-yyyy HH:mm:ss G\MT", CultureInfo.InvariantCulture), DateTimeKind.Utc);
					break;
				case "httponly":
					c.HttpOnly = true;
					break;
				case "secure":
					c.Secure = true;
					break;
				default:
					if (c.Name.Length == 0) {
						c.Name = key;
						c.Value = value;
					}
					break;
				}
			}
			return c;
		}
	}

	[Serializable]
	public class CookieCollection2 : ICollection, IEnumerable 
	{
		// not 100% identical to MS implementation
		sealed class CookieCollectionComparer : IComparer<Cookie> {
			public int Compare (Cookie x, Cookie y)
			{
				if (x == null || y == null)
					return 0;

				var ydomain = y.Domain.Length - (y.Domain[0] == '.' ? 1 : 0);
				var xdomain = x.Domain.Length - (x.Domain[0] == '.' ? 1 : 0);

				int result = ydomain - xdomain;
				return result == 0 ? y.Path.Length - x.Path.Length : result;
			}
		}

		static CookieCollectionComparer Comparer = new CookieCollectionComparer ();

		List<Cookie> list = new List<Cookie> ();

		public IList<Cookie> List {
			get { return list; }
		}
		// ICollection
		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public Object SyncRoot {
			get { return this; }
		}

		public void CopyTo (Array array, int index)
		{
			(list as IList).CopyTo (array, index);
		}

		public void CopyTo (Cookie [] array, int index)
		{
			list.CopyTo (array, index);
		}

		// IEnumerable
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		// This

		// LAMESPEC: So how is one supposed to create a writable CookieCollection
		// instance?? We simply ignore this property, as this collection is always
		// writable.
		public bool IsReadOnly {
			get { return true; }
		}                

		public void Add (Cookie cookie)
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			int pos = SearchCookie (cookie);
			if (pos == -1)
				list.Add (cookie);
			else
				list [pos] = cookie;
		}

		public void Sort ()
		{
			if (list.Count > 0)
				list.Sort (Comparer);
		}

		int SearchCookie (Cookie cookie)
		{
			string name = cookie.Name;
			string domain = cookie.Domain;
			string path = cookie.Path;

			for (int i = list.Count - 1; i >= 0; i--) {
				Cookie c = list [i];
				if (c.Version != cookie.Version)
					continue;

				if (0 != String.Compare (domain, c.Domain, true, CultureInfo.InvariantCulture))
					continue;

				if (0 != String.Compare (name, c.Name, true, CultureInfo.InvariantCulture))
					continue;

				if (0 != String.Compare (path, c.Path, true, CultureInfo.InvariantCulture))
					continue;

				return i;
			}

			return -1;
		}

		public void Add (CookieCollection cookies)
		{
			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie c in cookies)
				Add (c);
		}

		public Cookie this [int index] {
			get {
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("index");

				return list [index];
			}
		}

		public Cookie this [string name] {
			get {
				foreach (Cookie c in list) {
					if (0 == String.Compare (c.Name, name, true, CultureInfo.InvariantCulture))
						return c;
				}
				return null;
			}
		}

	}
}

