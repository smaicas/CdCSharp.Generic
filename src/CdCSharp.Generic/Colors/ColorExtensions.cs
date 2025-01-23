using System.Drawing;
using System.Globalization;

namespace CdCSharp.Generic.Colors;

/// <summary>
/// Provides extension methods for working with colors.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Converts a Color object to a hexadecimal representation.
    /// </summary>
    /// <param name="color">
    /// The Color object to convert.
    /// </param>
    /// <returns>
    /// A string representing the Color object in hexadecimal format (#RRGGBBAA).
    /// </returns>
    public static string GetHex(this Color color) => $"#{color.GetHexR()}{color.GetHexG()}{color.GetHexB()}{color.GetHexA()}";

    /// <summary>
    /// Gets the hexadecimal representation of the alpha channel of a color.
    /// </summary>
    /// <param name="color">
    /// The color whose alpha channel will be converted to hexadecimal.
    /// </param>
    /// <returns>
    /// A string representing the alpha channel of the color in hexadecimal format.
    /// </returns>
    public static string GetHexA(this Color color) => color.A.ToString("X2");

    /// <summary>
    /// Gets the hexadecimal representation of the blue component of a color.
    /// </summary>
    /// <param name="color">
    /// The color whose blue component's hexadecimal representation is to be retrieved.
    /// </param>
    /// <returns>
    /// The hexadecimal representation of the blue component of the color.
    /// </returns>
    public static string GetHexB(this Color color) => color.B.ToString("X2");

    /// <summary>
    /// Gets the hexadecimal representation of the green component of a color.
    /// </summary>
    /// <param name="color">
    /// The color whose green component's hexadecimal representation is to be retrieved.
    /// </param>
    /// <returns>
    /// The hexadecimal representation of the green component of the color.
    /// </returns>
    public static string GetHexG(this Color color) => color.G.ToString("X2");

    /// <summary>
    /// Gets the hexadecimal representation of the red component of a color.
    /// </summary>
    /// <param name="color">
    /// The color whose red component's hexadecimal representation is to be retrieved.
    /// </param>
    /// <returns>
    /// The hexadecimal representation of the red component of the color.
    /// </returns>
    public static string GetHexR(this Color color) => color.R.ToString("X2");

    /// <summary>
    /// Converts a Color object to an RGBA string representation.
    /// </summary>
    /// <param name="color">
    /// The Color object to convert.
    /// </param>
    /// <param name="alphaValueZeroTo255">
    /// The alpha value to use for the color (optional, default is the alpha value of the Color object).
    /// </param>
    /// <param name="computeAlphaPercent">
    /// Flag indicating whether the alpha value should be represented as a percentage (optional,
    /// default is true).
    /// </param>
    /// <returns>
    /// An RGBA string representation of the Color object.
    /// </returns>
    public static string ToStringRgba(this Color color, decimal? alphaValueZeroTo255 = null, bool? computeAlphaPercent = true)
    {
        alphaValueZeroTo255 ??= color.A;
        if (computeAlphaPercent == true)
            alphaValueZeroTo255 = Math.Round((decimal)alphaValueZeroTo255 / 255, 2);
        return $"rgba({(int)color.R},{(int)color.G},{(int)color.B},{((decimal)alphaValueZeroTo255).ToString(CultureInfo.InvariantCulture)})";
    }
}