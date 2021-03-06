// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;
using NUnit.Util;
using NUnit.Core;

namespace NUnit.UiKit
{
	/// <summary>
	/// The TextDisplay interface is implemented by object - generally
	/// controls - that display text.
	/// </summary>
	public interface TextDisplay : TestObserver
	{
		/// <summary>
		///  The output types handled by this display
		/// </summary>
		TextDisplayContent Content { get; set; }

		/// <summary>
		/// Clears the display
		/// </summary>
		void Clear();

		/// <summary>
		/// Appends text to the display
		/// </summary>
		/// <param name="text">The text to append</param>
		void Write( string text );

		/// <summary>
		/// Appends text to the display followed by a newline
		/// </summary>
		/// <param name="text">The text to append</param>
		void WriteLine( string text );

		void Write( NUnit.Core.TestOutput output );

		/// <summary>
		/// Gets the current text - used mainly for testing
		/// </summary>
		string GetText();
	}
}
