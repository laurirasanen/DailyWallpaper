using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace DailyWallpaper
{
	public static class Downloader
	{
		private static string baseUrl = "https://bing.com";
		private static string jsonUrl = $"{baseUrl}/HPImageArchive.aspx?format=js";

		struct ImageInfo
		{
			public string Url;
			public string Copyright;

			public ImageInfo( string url, string copyright )
			{
				Url = url;
				Copyright = copyright;
			}
		}

		public static List<Image> GetImages( List<int> indices )
		{
			var images = new List<Image>();
			var info = GetImageInfo(indices);

			using ( var wc = new WebClient() )
			{
				for ( int i = 0; i < indices.Count; i++ )
				{
					if ( info.Count > i )
					{
						if ( indices[i] < 0 )
						{
							images.Add( null );
						}
						else
						{
							images.Add( ( Image )new ImageConverter().ConvertFrom( wc.DownloadData( info[i].Url ) ) );
						}
					}
				}
			}

			return images;
		}

		private static List<ImageInfo> GetImageInfo( List<int> indices )
		{
			var info = new List<ImageInfo>();

			var json = "";
			using ( var wc = new WebClient() )
			{
				int max = 0;
				indices.ForEach( i => max = i > max ? i : max );
				json = wc.DownloadString( $"{jsonUrl}&n={max + 1}" );
			}

			dynamic data = JObject.Parse(json);
			if ( data != null && data.images != null && data.images.Type == JTokenType.Array )
			{
				indices.ForEach( i =>
				{
					if ( data.images.Count > i )
					{
						if ( i < 0 )
						{
							info.Add( new ImageInfo( "", "" ) );
						}
						else
						{
							info.Add(
								new ImageInfo(
									$"{baseUrl}/{data.images[i].url.Value}",
									data.images[i].copyright.Value
								)
							);
						}
					}
				} );
			}

			return info;
		}
	}
}
