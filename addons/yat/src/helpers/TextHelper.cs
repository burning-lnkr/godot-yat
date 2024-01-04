using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YAT.Helpers
{
	/// <summary>
	/// Provides helper methods for working with text.
	/// </summary>
	public static class TextHelper
	{
		/// <summary>
		/// Escapes BBCode tags in the given text by replacing '[' with '[lb]'.
		/// </summary>
		/// <param name="text">The text to escape.</param>
		/// <returns>The escaped text.</returns>
		public static string EscapeBBCode(string text)
		{
			return text.Replace("[", "[lb]");
		}

		/// <summary>
		/// Makes the specified text bold by surrounding it with the appropriate tags.
		/// </summary>
		/// <param name="text">The text to make bold.</param>
		/// <returns>The specified text surrounded by bold tags.</returns>
		public static string MakeBold(string text)
		{
			return $"[b]{text}[/b]";
		}

		/// <summary>
		/// Sanitizes the given text by removing leading and trailing white spaces
		/// and empty entries.
		/// </summary>
		/// <param name="text">The text to sanitize.</param>
		/// <returns>An array of sanitized strings.</returns>
		public static string[] SanitizeText(string text)
		{
			return SplitClean(text, " ");
		}

		/// <summary>
		/// Concatenates sentences in an array of strings
		/// by removing quotation marks and
		/// merging consecutive strings enclosed in quotation marks.
		/// </summary>
		/// <param name="strings">The array of strings to concatenate.</param>
		/// <returns>An array of strings with concatenated sentences.</returns>
		public static string[] ConcatenateSentence(string[] strings)
		{
			List<string> modifiedStrings = new();

			for (ushort i = 0; i < strings.Length; i++)
			{
				string token = strings[i];
				bool startsWith = StartsWith(token, '"', '\'');

				// If the token starts and ends with quotation marks, remove them
				if (startsWith && EndsWith(token, '"', '\''))
				{
					modifiedStrings.Add(token.Length > 1 ? token[1..^1] : token[1..]);
					continue;
				}

				// Handle sentences in options (e.g. -name="John Doe")
				if (StartsWith(token, '-') &&
					(token.Contains('"') || token.Contains('\''))
					&& !EndsWith(token, '"', '\'')
				)
				{
					string sentence = token.Replace("\"", string.Empty).Replace("'", string.Empty);

					// Concatenate the next strings until the end of the sentence is reached
					while (!EndsWith(strings[i], '"', '\'') && i < strings.Length)
					{
						i++;
						if (i >= strings.Length) break;

						sentence += $" {strings[i]}";
					}

					sentence = i >= strings.Length ? sentence : sentence[..^1];
					modifiedStrings.Add(sentence);
					continue;
				}

				// If the token starts with a quotation mark, concatenate the next strings
				if (startsWith)
				{
					string sentence = strings[i][1..];

					// Concatenate the next strings until the end of the sentence is reached
					while (!EndsWith(strings[i], '"', '\'') && i < strings.Length)
					{
						i++;
						if (i >= strings.Length) break;
						sentence += $" {strings[i]}";
					}

					sentence = i >= strings.Length ? sentence : sentence[..^1];
					modifiedStrings.Add(sentence);
				}
				else modifiedStrings.Add(token);
			}

			return modifiedStrings.ToArray();
		}

		/// <summary>
		/// Determines whether the specified string starts with any of the specified characters.
		/// </summary>
		/// <param name="text">The string to check.</param>
		/// <param name="value">The characters to compare.</param>
		/// <returns><c>true</c> if the string starts with any of the specified characters; otherwise, <c>false</c>.</returns>
		public static bool StartsWith(string text, params char[] value)
		{
			return value.Any(text.StartsWith);
		}

		/// <summary>
		/// Determines whether the specified text ends with any of the specified characters.
		/// </summary>
		/// <param name="text">The text to check.</param>
		/// <param name="value">The characters to compare against the end of the text.</param>
		/// <returns><c>true</c> if the text ends with any of the specified characters; otherwise, <c>false</c>.</returns>
		public static bool EndsWith(string text, params char[] value)
		{
			return value.Any(text.EndsWith);
		}

		/// <summary>
		/// Splits the given text using the specified separator,
		/// trims each token, removes empty tokens,
		/// and returns an array of the resulting tokens.
		/// </summary>
		/// <param name="text">The text to split.</param>
		/// <param name="separator">The separator used to split the text.</param>
		/// <returns>An array of the resulting tokens.</returns>
		public static string[] SplitClean(string text, string separator)
		{
			return text.Split(separator, System.StringSplitOptions.TrimEntries |
									System.StringSplitOptions.RemoveEmptyEntries)
						.Select(token => token.Trim())
						.Where(token => !string.IsNullOrEmpty(token))
						.ToArray();
		}

		/// <summary>
		/// Shortens a file path to the specified length.
		///
		/// Adapted from https://stackoverflow.com/a/4638913
		/// </summary>
		/// <param name="path">The file path to shorten</param>
		/// <param name="maxLength">The max length of the output path (including the ellipsis if inserted)</param>
		/// <returns>
		/// The path with some of the middle directory paths replaced with an ellipsis
		/// (or the entire path if it is already shorter than maxLength)
		/// </returns>
		/// <remarks>
		/// Shortens the path by removing some of the "middle directories"
		/// in the path and inserting an ellipsis.
		/// If the filename and root path (drive letter or UNC server name)
		/// in itself exceeds the maxLength, the filename will be cut to fit.
		/// UNC-paths and relative paths are also supported.
		/// The inserted ellipsis is not a true ellipsis char, but a string of three dots.
		/// </remarks>
		/// <example>
		/// ShortenPath(@"c:\websites\myproject\www_myproj\App_Data\themegafile.txt", 50)
		/// Result: "c:\websites\myproject\...\App_Data\themegafile.txt"
		///
		/// ShortenPath(@"c:\websites\myproject\www_myproj\App_Data\theextremelylongfilename_morelength.txt", 30)
		/// Result: "c:\...gfilename_morelength.txt"
		///
		/// ShortenPath(@"\\myserver\theshare\myproject\www_myproj\App_Data\theextremelylongfilename_morelength.txt", 30)
		/// Result: "\\myserver\...e_morelength.txt"
		///
		/// ShortenPath(@"\\myserver\theshare\myproject\www_myproj\App_Data\themegafile.txt", 50)
		/// Result: "\\myserver\theshare\...\App_Data\themegafile.txt"
		///
		/// ShortenPath(@"\\192.168.1.178\theshare\myproject\www_myproj\App_Data\themegafile.txt", 50)
		/// Result: "\\192.168.1.178\theshare\...\themegafile.txt"
		///
		/// ShortenPath(@"\theshare\myproject\www_myproj\App_Data\", 30)
		/// Result: "\theshare\...\App_Data\"
		///
		/// ShortenPath(@"\theshare\myproject\www_myproj\App_Data\themegafile.txt", 35)
		/// Result: "\theshare\...\themegafile.txt"
		/// </example>
		public static string ShortenPath(string path, ushort maxLength, string ellipsisChar = "...")
		{
			if (string.IsNullOrEmpty(path) || maxLength <= ellipsisChar.Length) return ellipsisChar;
			if (path.Length <= maxLength) return path;

			int ellipsisLength = ellipsisChar.Length;
			var dirSeparatorChar = Path.DirectorySeparatorChar.ToString();
			bool isFirstPartsTurn = true;

			string[] pathParts = path.Split(dirSeparatorChar);
			StringBuilder result = new();

			int totalLength = 0;
			int startIdx = 0;
			int endIdx = pathParts.Length - 1;

			while (startIdx <= endIdx && totalLength + ellipsisLength <= maxLength)
			{
				if (isFirstPartsTurn)
				{
					string partToAdd = pathParts[startIdx] + dirSeparatorChar;
					if (totalLength + partToAdd.Length <= maxLength)
					{
						result.Append(partToAdd);
						totalLength += partToAdd.Length;
						startIdx++;
						if (partToAdd != dirSeparatorChar)
						{
							isFirstPartsTurn = false;
						}
					}
					else break;
				}
				else
				{
					string partToAdd = dirSeparatorChar + pathParts[endIdx];
					if (totalLength + partToAdd.Length <= maxLength)
					{
						result.Insert(0, partToAdd);
						totalLength += partToAdd.Length;
						endIdx--;
						if (partToAdd != dirSeparatorChar)
						{
							isFirstPartsTurn = true;
						}
					}
					else break;
				}
			}

			string lastPart = string.Join(dirSeparatorChar, pathParts, startIdx, endIdx - startIdx + 1);
			if (lastPart.Length + ellipsisLength > maxLength)
			{
				lastPart = lastPart.Substring(lastPart.Length - (maxLength - ellipsisLength));
			}

			return result.ToString() + ellipsisChar + lastPart;
		}
	}
}
