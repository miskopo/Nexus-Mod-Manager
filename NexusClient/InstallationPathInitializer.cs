﻿using System;
using System.Diagnostics;
using System.IO;
using Nexus.Client.Games;

namespace Nexus.Client
{
	/// <summary>
	/// Gets a possible candidate for the installation path for the game mode represented by the given game mode descriptor.
	/// </summary>
	/// <param name="p_gmdGameModeInfo">The descriptor for the game mode for which the installation path is to be found.</param>
	/// <param name="p_strDefaultPath">The default installation path.</param>
	/// <param name="p_strPath">The installation path for the game mode represented by the given game mode descriptor.</param>
	/// <returns><c>true</c> if we should keep looking for the installation path, or <c>false</c> if we should stop.</returns>
	public delegate bool GetInstallationPathCandidateDelegate(IGameModeDescriptor p_gmdGameModeInfo, string p_strDefaultPath, out string p_strPath);

	/// <summary>
	/// Initializes the installation path for a game mode.
	/// </summary>
	public class InstallationPathInitializer
	{
		private GetInstallationPathCandidateDelegate m_fncFindInstallationPath = null;

		#region Properties

		/// <summary>
		/// Gets or sets the application's envrionment info.
		/// </summary>
		/// <value>The application's envrionment info.</value>
		protected IEnvironmentInfo EnvironmentInfo { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_eifEnvironmentInfo">The application's envrionment info.</param>
		/// <param name="p_fncFindInstallationPath">The delegate to call to get find the installation path.</param>
		public InstallationPathInitializer(IEnvironmentInfo p_eifEnvironmentInfo, GetInstallationPathCandidateDelegate p_fncFindInstallationPath)
		{
			EnvironmentInfo = p_eifEnvironmentInfo;
			m_fncFindInstallationPath = p_fncFindInstallationPath;
		}

		#endregion

		/// <summary>
		/// Initializes the installation path for the specified game mode.
		/// </summary>
		/// <remarks>
		/// This searches for the installation path. The user can enter a value, or
		/// an autodetection routine can be used.
		/// </remarks>
		/// <param name="p_gmfGameModeFactory">The descriptor of the game mode whose installation
		/// path is being initialized.</param>
		/// <returns><c>true</c> if the installation path was found;
		/// <c>false</c> otherwise.</returns>
		public bool InitializeInstallationPath(IGameModeFactory p_gmfGameModeFactory)
		{
			Trace.TraceInformation(String.Format("Looking for {0}.", p_gmfGameModeFactory.GameModeDescriptor.Name));
			Trace.Indent();

			if (EnvironmentInfo.Settings.InstallationPaths.ContainsKey(p_gmfGameModeFactory.GameModeDescriptor.ModeId))
			{
				Trace.TraceInformation("Found: " + EnvironmentInfo.Settings.InstallationPaths[p_gmfGameModeFactory.GameModeDescriptor.ModeId]);
				Trace.Unindent();
				return true;
			}

			string strGameFolder = p_gmfGameModeFactory.GetInstallationPath();
			while (!VerifyWorkingDirectory(p_gmfGameModeFactory.GameModeDescriptor, strGameFolder))
			{
				Trace.TraceInformation(String.Format("Cannot find in {0}.", strGameFolder));
				Trace.Indent();
				if (!String.IsNullOrEmpty(strGameFolder))
					foreach (string strFile in Directory.GetFiles(strGameFolder))
						Trace.TraceInformation(strFile);
				Trace.Unindent();
				if (!m_fncFindInstallationPath(p_gmfGameModeFactory.GameModeDescriptor, strGameFolder, out strGameFolder))
					return false;
			}

			EnvironmentInfo.Settings.InstallationPaths[p_gmfGameModeFactory.GameModeDescriptor.ModeId] = strGameFolder;
			EnvironmentInfo.Settings.Save();

			Trace.TraceInformation("Found: " + strGameFolder);
			Trace.Unindent();

			return true;
		}

		/// <summary>
		/// Verifies that the given path is a valid working directory for the game mode.
		/// </summary>
		/// <param name="p_gmdGameModeInfo">The descriptor of the game mode for which the given
		/// path is being verified as being the installation path.</param>
		/// <param name="p_strPath">The path to validate as a working directory.</param>
		/// <returns><c>true</c> if the path is a vlid working directory;
		/// <c>false</c> otherwise.</returns>
		private bool VerifyWorkingDirectory(IGameModeDescriptor p_gmdGameModeInfo, string p_strPath)
		{
			if (String.IsNullOrEmpty(p_strPath))
				return false;

			bool booFound = false;
			foreach (string strExe in p_gmdGameModeInfo.GameExecutables)
				if (File.Exists(Path.Combine(p_strPath, strExe)))
				{
					booFound = true;
					break;
				}
			return booFound;
		}
	}
}
