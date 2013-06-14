/*
 * This file is part of SubLib.
 * Copyright (C) 2011 Pedro Castro
 *
 * SubLib is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * SubLib is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;

namespace SubLib.Exceptions {

/// <summary>The exception that is thrown when error in any of Translator processes <see cref="Translator" /> occurs.</summary>
public class TranslatorException : ApplicationException {
	private static string defaultMessage = "Translator request failed.";

	public TranslatorException (string message) : base(message) {
	}

	public TranslatorException() : base(defaultMessage) {
	}

}

}
