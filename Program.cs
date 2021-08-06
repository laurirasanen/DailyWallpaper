using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CommandLine;

namespace DailyWallpaper
{
	class Program
	{
		public class Options
		{
			[Option( 'i', "images", Required = false, Default = new int[] { }, HelpText = "The images to get.\n" +
				"  The number is an offset in days, where 0 = today's image.\n" +
				"  Indices are mapped to monitors in the same order.\n" +
				"  e.g. `0, 1` would map today's image to monitor 1 and yesterday's image to monitor 2." +
				"  Defaults to increasing list starting from 0 with length of monitor count." )]
			public IEnumerable<int> ImageIndices { get; set; }

			[Option( 's', "style", Required = false, Default = DesktopWallpaperPosition.Fill, HelpText = "The wallpaper style.\n" +
				"  0  = Center\n" +
				"  1  = Tile\n" +
				"  2  = Stretch\n" +
				"  3  = Fit\n" +
				"  4 = Fill\n" +
				"  5 = Span" )]
			public DesktopWallpaperPosition Style { get; set; }
		}

		static void Main( string[] args )
		{
			Parser.Default.ParseArguments<Options>( args )
				.WithParsed( Run )
				.WithNotParsed( HandleParseError );
		}

		static void Run( Options options )
		{
			var screenCount = SystemInformation.MonitorCount;
			var indices = new List<int>(options.ImageIndices);

			for ( int i = indices.Count; i < screenCount; i++ )
			{
				indices.Add( i );
			}

			var images = Downloader.GetImages(indices);
			for ( int i = 0; i < images.Count; i++ )
			{
				if ( images[i] != null )
				{
					Set( ( uint )i, images[i], options.Style );
				}
			}
		}

		public static void Set( uint monitor, Image image, DesktopWallpaperPosition position )
		{
			var filePath = Path.Combine( Path.GetTempPath(), $"dailywallpaper_{monitor}.{image.RawFormat}" );
			image.Save( filePath );

			var wallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();
			wallpaper.SetWallpaper( wallpaper.GetMonitorDevicePathAt( monitor ), filePath );
			wallpaper.SetPosition( position );
		}

		static void HandleParseError( IEnumerable<Error> errs )
		{
			foreach ( var err in errs )
			{
				Console.WriteLine( err.ToString() );
			}
		}
	}
}
