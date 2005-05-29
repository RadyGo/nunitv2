#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright � 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright � 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright � 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;

namespace NUnit.Util
{
	using System.Diagnostics;
	using System.Security.Policy;
	using System.Reflection;
	using System.Collections;
	using System.Configuration;
	using System.IO;

	using NUnit.Core;

	public class TestDomain : ProxyTestRunner
	{
		#region Instance Variables

		/// <summary>
		/// The appdomain used  to load tests
		/// </summary>
		private AppDomain domain; 

		/// <summary>
		/// The path to our cache
		/// </summary>
		private string cachePath;
		
		/// <summary>
		/// The remote runner loaded in the test appdomain
		/// </summary>
//		private TestRunner testRunner;

		/// <summary>
		/// Holds the event listener while we are running
		/// </summary>
		private EventListener listener;

		/// <summary>
		/// Indicate whether files should be shadow copied
		/// </summary>
		private bool shadowCopyFiles = true;

		private bool threaded = true;

		#endregion

		#region Properties

		public AppDomain AppDomain
		{
			get { return domain; }
		}

		public bool ShadowCopyFiles
		{
			get { return shadowCopyFiles; }
			set
			{
				if ( this.domain != null )
					throw new ArgumentException( "ShadowCopyFiles may not be set after domain is created" );
				shadowCopyFiles = value;
			}
		}

		#endregion

		#region Constructors

		public TestDomain( )
		{ 
			this.threaded = false;
		}

		public TestDomain( bool threaded )
		{
			this.threaded = threaded;
		}

		#endregion

		#region Loading and Unloading Tests

		public override Test Load( string assemblyFileName )
		{
			return Load( assemblyFileName, string.Empty );
		}

		public override Test Load(string assemblyFileName, string testFixture)
		{
			Unload();

			try
			{
				CreateDomain( assemblyFileName );
				string assemblyPath = Path.GetFullPath( assemblyFileName );

				testRunner = MakeRemoteTestRunner( domain );
				if ( testFixture != null && testFixture != string.Empty )
					return testRunner.Load( assemblyPath, testFixture );
				else
					return testRunner.Load( assemblyPath );
			}
			catch
			{
				Unload();
				throw;
			}
		}

		public override Test Load( TestProject testProject )
		{
			return Load( testProject, null );
		}

		public override Test Load( TestProject testProject, string testName )
		{
			Unload();

			try
			{
				FileInfo projectFile = new FileInfo( testProject.ProjectPath );
				string configFilePath = projectFile.Extension == ".dll" 
					? projectFile.FullName + ".config"
					: Path.ChangeExtension( projectFile.FullName, ".config" );
				CreateDomain( 
					testProject.ProjectPath,
					testProject.BasePath,
					configFilePath,
					GetBinPath( testProject.Assemblies ));

				testRunner = MakeRemoteTestRunner( domain );
				if ( testName != null )
					return testRunner.Load( testProject, testName );
				else
					return testRunner.Load( testProject );
			}
			catch
			{
				Unload();
				throw;
			}
		}

		public override void Unload()
		{
			testRunner = null;

			if(domain != null) 
			{
				try
				{
					AppDomain.Unload(domain);
					if ( this.ShadowCopyFiles )
						DeleteCacheDir( new DirectoryInfo( cachePath ) );
				}
				catch( CannotUnloadAppDomainException )
				{
					// TODO: Do something useful. For now we just
					// leave the orphaned AppDomain "out there"
					// rather than aborting the application.
				}
				finally
				{
					domain = null;
				}
			}
		}

		public static string GetBinPath( string[] assemblies )
		{
			ArrayList dirs = new ArrayList();
			string binPath = null;

			foreach( string path in assemblies )
			{
				string dir = Path.GetDirectoryName( Path.GetFullPath( path ) );
				if ( !dirs.Contains( dir ) )
				{
					dirs.Add( dir );

					if ( binPath == null )
						binPath = dir;
					else
						binPath = binPath + ";" + dir;
				}
			}

			return binPath;
		}

		#endregion

		#region Running Tests

		public override TestResult[] doRun(NUnit.Core.EventListener listener, string[] testNames)
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				this.listener = listener; // Save listener for unhandled exception event handler
				return base.doRun( listener, testNames );
			}
		}

#if STARTRUN_SUPPORT
		public override void doStartRun( EventListener listener, string[] testNames )
		{
			using( new TestExceptionHandler( new UnhandledExceptionEventHandler( OnUnhandledException ) ) )
			{
				base.doStartRun( listener, testNames );
			}
		}
#endif

		// For now, just publish any unhandled exceptions and let the listener
		// figure out what to do with them.
		private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			if ( e.ExceptionObject.GetType() != typeof( System.Threading.ThreadAbortException ) )
				this.listener.UnhandledException( (Exception)e.ExceptionObject );
		}

		#endregion

		#region Helpers Used in AppDomain Creation and Removal

		/// <summary>
		/// Construct an application domain for testing a single assembly
		/// </summary>
		/// <param name="assemblyFileName">The assembly file name</param>
		private void CreateDomain( string assemblyFileName )
		{
			FileInfo testFile = new FileInfo( assemblyFileName );
			
			string domainName = string.Format( "domain-{0}", Path.GetFileName( assemblyFileName ) );

			domain = MakeAppDomain( domainName, testFile.DirectoryName, testFile.Name + ".config", testFile.DirectoryName );
		}

		/// <summary>
		/// Construct an application domain for testing multiple assemblies
		/// </summary>
		/// <param name="testFileName">The file name of the project file</param>
		/// <param name="appBase">The application base path</param>
		/// <param name="configFile">The configuration file to use</param>
		/// <param name="binPath">The private bin path</param>
		private void CreateDomain( string testFileName, string appBase, string configFile, string binPath)
		{
			domain = MakeAppDomain( testFileName, appBase, configFile, binPath );
		}

		/// <summary>
		/// This method creates appDomains for the framework.
		/// </summary>
		/// <param name="domainName">Name of the domain</param>
		/// <param name="appBase">ApplicationBase for the domain</param>
		/// <param name="configFile">ConfigurationFile for the domain</param>
		/// <param name="binPath">PrivateBinPath for the domain</param>
		/// <returns></returns>
		private AppDomain MakeAppDomain( string domainName, string appBase, string configFile, string binPath )
		{
			Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
			Evidence evidence = new Evidence(baseEvidence);

			AppDomainSetup setup = new AppDomainSetup();

			// We always use the same application name
			setup.ApplicationName = "Tests";
			// Note that we do NOT
			// set ShadowCopyDirectories because we  rely on the default
			// setting of ApplicationBase plus PrivateBinPath
			if ( this.ShadowCopyFiles )
			{
				setup.ShadowCopyFiles = "true";
				setup.ShadowCopyDirectories = appBase;
			}
			else
			{
				setup.ShadowCopyFiles = "false";
			}

			setup.ApplicationBase = appBase;
			setup.ConfigurationFile =  configFile;
			setup.PrivateBinPath = binPath;

			AppDomain runnerDomain = AppDomain.CreateDomain(domainName, evidence, setup);
			
			if ( this.ShadowCopyFiles )
				ConfigureCachePath(runnerDomain);

			return runnerDomain;
		}

		/// <summary>
		/// Set the location for caching and delete any old cache info
		/// </summary>
		/// <param name="domain">Our domain</param>
		private void ConfigureCachePath(AppDomain domain)
		{
			cachePath = ConfigurationSettings.AppSettings["shadowfiles.path"];
			if ( cachePath == "" || cachePath== null )
				cachePath = Path.Combine( Path.GetTempPath(), @"nunit20\ShadowCopyCache" );
			else
				cachePath = Environment.ExpandEnvironmentVariables(cachePath);
				
			cachePath = Path.Combine( cachePath, DateTime.Now.Ticks.ToString() ); 
				
			try 
			{
				DirectoryInfo dir = new DirectoryInfo(cachePath);		
				if(dir.Exists) dir.Delete(true);
			}
			catch( Exception ex)
			{
				throw new ApplicationException( 
					string.Format( "Invalid cache path: {0}",cachePath ),
					ex );
			}

			domain.SetCachePath(cachePath);

			return;
		}

		/// <summary>
		/// Helper method to delete the cache dir. This method deals 
		/// with a bug that occurs when files are marked read-only
		/// and deletes each file separately in order to give better 
		/// exception information when problems occur.
		/// 
		/// TODO: This entire method is problematic. Should we be doing it?
		/// </summary>
		/// <param name="cacheDir"></param>
		private void DeleteCacheDir( DirectoryInfo cacheDir )
		{
//			Debug.WriteLine( "Modules:");
//			foreach( ProcessModule module in Process.GetCurrentProcess().Modules )
//				Debug.WriteLine( module.ModuleName );
			

			if(cacheDir.Exists)
			{
				foreach( DirectoryInfo dirInfo in cacheDir.GetDirectories() )
					DeleteCacheDir( dirInfo );

				foreach( FileInfo fileInfo in cacheDir.GetFiles() )
				{
					fileInfo.Attributes = FileAttributes.Normal;
					try 
					{
						fileInfo.Delete();
						Debug.WriteLine( "Deleted " + fileInfo.Name );
					}
					catch( Exception ex )
					{
						Debug.WriteLine( string.Format( 
							"Error deleting {0}, {1}", fileInfo.Name, ex.Message ) );
					}
				}

				cacheDir.Attributes = FileAttributes.Normal;

				try
				{
					cacheDir.Delete();
				}
				catch( Exception ex )
				{
					Debug.WriteLine( string.Format( 
						"Error deleting {0}, {1}", cacheDir.Name, ex.Message ) );
				}
			}
		}
		
		private TestRunner MakeRemoteTestRunner( AppDomain runnerDomain )
		{
			// Inject assembly resolver into remote domain to help locate our assemblies
			AssemblyResolver assemblyResolver = (AssemblyResolver)runnerDomain.CreateInstanceFromAndUnwrap(
				typeof(AssemblyResolver).Assembly.CodeBase,
				typeof(AssemblyResolver).FullName);

			// Tell resolver to use our core assembly in the test domain
			assemblyResolver.AddFile( typeof( NUnit.Core.RemoteTestRunner ).Assembly.Location );

			Type runnerType = GetRunnerType();
			object obj = runnerDomain.CreateInstanceAndUnwrap(
				runnerType.Assembly.FullName, 
				runnerType.FullName,
				false, BindingFlags.Default,null,null,null,null,null);
			
			RemoteTestRunner runner = (RemoteTestRunner) obj;

			return runner;
		}

		private Type GetRunnerType ()
		{
			Type runnerType = null;
	
			if (threaded)
			{
				//runnerType = typeof(ThreadedRemoteRunner);
				runnerType = typeof(RemoteTestRunner);
			}
			else
			{
				runnerType = typeof(RemoteTestRunner);
			}
			return runnerType;
		}

		#endregion
	}
}
