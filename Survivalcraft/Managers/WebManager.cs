using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Game.Handlers;

namespace Game
{
	public static class WebManager
	{

		public static IWebManagerHandler? WebManagerServicesCollection
		{
			get;
			set;
		}
		private static string HandlerNotInitializedWarningString
			=> $"{typeof(WebManager).FullName}.{nameof(WebManagerServicesCollection)} 未初始化";
		public class ProgressHttpContent : HttpContent
		{
			public Stream m_sourceStream;

			public CancellableProgress m_progress;

			public ProgressHttpContent(Stream sourceStream, CancellableProgress progress)
			{
				m_sourceStream = sourceStream;
				m_progress = progress ?? new CancellableProgress();
			}

			protected override bool TryComputeLength(out long length)
			{
				length = m_sourceStream.Length;
				return true;
			}

			protected override async Task SerializeToStreamAsync(Stream targetStream, TransportContext context)
			{
				var buffer = new byte[1024];
				var written = 0L;
				while (true)
				{
					m_progress.Total = m_sourceStream.Length;
					m_progress.Completed = written;
					if (m_progress.CancellationToken.IsCancellationRequested)
					{
						break;
					}
					int read = m_sourceStream.Read(buffer, 0, buffer.Length);
					if (read > 0)
					{
						await targetStream.WriteAsync(buffer, 0, read, m_progress.CancellationToken);
						written += read;
					}
					if (read <= 0)
					{
						return;
					}
				}
				throw new OperationCanceledException("Operation cancelled.");
			}
		}

		public static bool IsInternetConnectionAvailable()
		{
			if (WebManagerServicesCollection is null)
			{
				Log.Error(HandlerNotInitializedWarningString);
				return false;
			}

			return WebManagerServicesCollection.IsInternetConnectionAvailable();
		}

		public static void Get(string address, Dictionary<string, string> parameters, Dictionary<string, string> headers, CancellableProgress progress, Action<byte[]> success, Action<Exception> failure)
		{
			Exception e;
			Task.Run(async delegate
			{
				try
				{
					progress = progress ?? new CancellableProgress();
					if (!IsInternetConnectionAvailable())
					{
						throw new InvalidOperationException("Internet connection is unavailable.");
					}

					using var client = new HttpClient();
					
					Uri requestUri = parameters is { Count: > 0 } 
						? new Uri($"{address}?{UrlParametersToString(parameters)}") 
						: new Uri(address);
					client.DefaultRequestHeaders.Referrer = new Uri(address);
						
					if (headers != null)
					{
						foreach (KeyValuePair<string, string> header in headers)
						{
							client.DefaultRequestHeaders.Add(header.Key, header.Value);
						}
					}
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
					
					HttpResponseMessage responseMessage = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, progress.CancellationToken);
					await VerifyResponse(responseMessage);
					long? contentLength = responseMessage.Content.Headers.ContentLength;
					progress.Total = contentLength.GetValueOrDefault();
					await using Stream responseStream = await responseMessage.Content.ReadAsStreamAsync();
					using var targetStream = new MemoryStream();
					try
					{
						var written = 0L;
						var buffer = new byte[1024];
						int readCount;
						
						do
						{
							readCount = await responseStream.ReadAsync(buffer, progress.CancellationToken);
							if (readCount <= 0) continue;
							
							targetStream.Write(buffer, 0, readCount);
							written += readCount;
							progress.Completed = written;
						}
						while (readCount > 0);
						
						if (success != null)
						{
							byte[] result = targetStream.ToArray();
							Dispatcher.Dispatch(() =>
							{
								success(result);
							});
						}
					}
					finally
					{
						targetStream.Close();
					}
				}
				catch (Exception ex)
				{
					e = ex;
					Log.Error(ExceptionManager.MakeFullErrorMessage(e));
					Dispatcher.Dispatch(delegate
					{
						failure(e);
					});
				}
			});
		}

		public static void Put(string address, Dictionary<string, string> parameters, Dictionary<string, string> headers, Stream data, CancellableProgress progress, Action<byte[]> success, Action<Exception> failure)
		{
			PutOrPost(isPost: false, address, parameters, headers, data, progress, success, failure);
		}

		public static void Post(string address, Dictionary<string, string> parameters, Dictionary<string, string> headers, Stream data, CancellableProgress progress, Action<byte[]> success, Action<Exception> failure)
		{
			PutOrPost(isPost: true, address, parameters, headers, data, progress, success, failure);
		}

		public static string UrlParametersToString(Dictionary<string, string> values)
		{
			var stringBuilder = new StringBuilder();
			var value = string.Empty;
			foreach (KeyValuePair<string, string> value2 in values)
			{
				stringBuilder.Append(value);
				value = "&";
				stringBuilder.Append(Uri.EscapeDataString(value2.Key));
				stringBuilder.Append('=');
				if (!string.IsNullOrEmpty(value2.Value))
				{
					stringBuilder.Append(Uri.EscapeDataString(value2.Value));
				}
			}
			return stringBuilder.ToString();
		}

		public static byte[] UrlParametersToBytes(Dictionary<string, string> values)
		{
			return Encoding.UTF8.GetBytes(UrlParametersToString(values));
		}

		public static MemoryStream UrlParametersToStream(Dictionary<string, string> values)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(UrlParametersToString(values)));
		}

		public static Dictionary<string, string> UrlParametersFromString(string s)
		{
			var dictionary = new Dictionary<string, string>();
			string[] array = s.Split('&', StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in array)
			{
				string[] array2 = Uri.UnescapeDataString(item).Split('=');
				if (array2.Length == 2)
				{
					dictionary[array2[0]] = array2[1];
				}
			}
			return dictionary;
		}

		public static Dictionary<string, string> UrlParametersFromBytes(byte[] bytes)
		{
			return UrlParametersFromString(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
		}

		public static object JsonFromString(string s)
		{
			return SimpleJson.SimpleJson.DeserializeObject(s);
		}

		public static object JsonFromBytes(byte[] bytes)
		{
			return JsonFromString(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
		}

		public static void PutOrPost(bool isPost, string address, Dictionary<string, string> parameters, Dictionary<string, string> headers, Stream data, CancellableProgress progress, Action<byte[]> success, Action<Exception> failure)
		{
			byte[] responseData = default;
			Task.Run(async () =>
			{
				try
				{
					if (!IsInternetConnectionAvailable())
					{
						throw new InvalidOperationException("Internet connection is unavailable.");
					}

					using var client = new HttpClient();
					var dictionary = new Dictionary<string, string>();
					if (headers != null)
					{
						foreach (KeyValuePair<string, string> header in 
						         headers.Where(header => !client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value)))
						{
							dictionary.Add(header.Key, header.Value);
						}
					}
					Uri uri = parameters is { Count: > 0 } ? new Uri(string.Format("{0}?{1}", new object[2]
					{
						address,
						UrlParametersToString(parameters)
					})) : new Uri(address);
					var content = new ProgressHttpContent(data, progress);
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						content.Headers.Add(item.Key, item.Value);
					}
					HttpResponseMessage responseMessage = (!isPost) ? (await client.PutAsync(uri, content, progress.CancellationToken)) : (await client.PostAsync(uri, content, progress.CancellationToken));
					await VerifyResponse(responseMessage);
					_ = responseData;
					responseData = await responseMessage.Content.ReadAsByteArrayAsync();
					Dispatcher.Dispatch(delegate
					{
						success(responseData);
					});
				}
				catch (Exception ex)
				{
					Log.Error(ExceptionManager.MakeFullErrorMessage(ex));
					Dispatcher.Dispatch(delegate
					{
						failure(ex);
					});
				}
			});
		}

		public static async Task VerifyResponse(HttpResponseMessage message)
		{
			if (!message.IsSuccessStatusCode)
			{
				var responseText = string.Empty;
				try
				{
					responseText = await message.Content.ReadAsStringAsync();
				}
				catch
				{
					// ignored
				}

				throw new InvalidOperationException($"{message.StatusCode.ToString()} ({(int)message.StatusCode})\n{responseText}");
			}
		}
	}
}
