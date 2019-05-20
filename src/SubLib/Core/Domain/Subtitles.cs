/*
 * This file is part of SubLib.
 * Copyright (C) 2005-2019 Pedro Castro
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

using SubLib.IO.SubtitleFormats;
using System;

namespace SubLib.Core.Domain {

/// <summary>Represents the root class of all the subtitles.</summary>
/// <remarks>A <see cref="Subtitles" /> class is created using the <see cref="SubtitleFactory" />.</remarks>
public class Subtitles : ICloneable {
	private SubtitleCollection collection = null;
	private SubtitleProperties properties = null;

	/* Variable cache */
	private static SubtitleTypeInfo[] availableTypes = null;
	private static SubtitleTypeInfo[] availableTypesSorted = null;


	/* Public properties */

	/// <summary>A collection which contains the subtitles.</summary>
	public SubtitleCollection Collection {
		get { return collection; }
		set { collection = value; }
	}

	/// <summary>The properties of the subtitles.</summary>
	public SubtitleProperties Properties {
		get { return properties; }
		set { properties = value; }
	}

	/// <summary>Information about the available subtitle types.</summary>
	public static SubtitleTypeInfo[] AvailableTypes {
		get {
			if (availableTypes == null) {
				SubtitleFormat[] formats = BuiltInSubtitleFormats.SubtitleFormats;
				SubtitleTypeInfo[] types = new SubtitleTypeInfo[formats.Length];
				for (int index = 0 ; index < formats.Length ; index++) {
					types[index] = new SubtitleTypeInfo(formats[index]);
				}
				availableTypes = types;
			}
			return availableTypes;
		}
	}

	/// <summary>Information about the available subtitle types, sorted by their names.</summary>
	public static SubtitleTypeInfo[] AvailableTypesSorted {
		get {
			if (availableTypesSorted == null) {
				SubtitleTypeInfo[] types = AvailableTypes;
				Array.Sort(types);
				availableTypesSorted = types;
			}
			return availableTypesSorted;
		}
	}


	/* Public methods */

	/// <summary>Get information about an available subtitle type.</summary>
	/// <param name="type">The subtitle type.</param>
	/// <returns>The information about the specified subtitle type.</returns>
	public static SubtitleTypeInfo GetAvailableType (SubtitleType type) {
		SubtitleFormat format = BuiltInSubtitleFormats.GetFormat(type);
		return new SubtitleTypeInfo(format);
	}

	public static bool IsSubtitleExtension (string dottedExtension) {
		string extension = dottedExtension.Substring(1); //Remove the starting dot
		foreach (SubtitleTypeInfo type in AvailableTypes) {
			if (type.HasExtension(extension))
				return true;
		}
		return false;
	}

	public override string ToString(){
		return Collection.ToString() + "\n-------------------------------------------\n" + Properties.ToString();
	}

	public object Clone() {
		SubtitleProperties propertiesClone = this.properties.Clone() as SubtitleProperties;
		SubtitleCollection collectionClone = this.collection.Clone(propertiesClone);
		return new Subtitles(collectionClone, propertiesClone);
	}


	/* Internal members */

	/// <summary>Initializes a new instance of the <see cref="Subtitles" /> class.</summary>
	/// <param name="collection">A collection of subtitles.</param>
	/// <param name="properties">The subtitles' properties.</param>
	internal protected Subtitles (SubtitleCollection collection, SubtitleProperties properties) {
		this.collection = collection;
		this.properties = properties;
	}

	internal void UpdateFramesFromTimes (float frameRate) {
		foreach (Subtitle subtitle in collection) {
			subtitle.UpdateFramesFromTimes(frameRate);
		}
	}

	internal void UpdateTimesFromFrames (float frameRate) {
		foreach (Subtitle subtitle in collection) {
			subtitle.UpdateTimesFromFrames(frameRate);
		}
	}

	internal SubtitleText GetSubtitleText (int subtitleNumber, SubtitleTextType textType) {
		Subtitle subtitle = collection[subtitleNumber];
		if (textType == SubtitleTextType.Text)
			return subtitle.Text;
		else
			return subtitle.Translation;
	}


}

}

