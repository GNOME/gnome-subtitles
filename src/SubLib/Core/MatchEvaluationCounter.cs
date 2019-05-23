/*
 * This file is part of SubLib.
 * Copyright (C) 2006-2019 Pedro Castro
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
using System.Text.RegularExpressions;

namespace SubLib.Core {

internal class MatchEvaluationCounter {
	int count = 0; //The number of matches or times the evaluator was called
	bool evaluationOccurred = false; //Whether an evaluation occurred
	string replacement = String.Empty;

	internal MatchEvaluationCounter (string replacement) {
		this.replacement = replacement;
	}

	internal int Count {
		get { return count; }
	}

	internal bool EvaluationOccurred {
		get { return evaluationOccurred; }
		set { evaluationOccurred = value; }
	}

	internal string Evaluator (Match match) {
		count++;
		evaluationOccurred = true;
		return replacement;
	}

}

}