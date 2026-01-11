namespace Svns.Services.Svn.Parsers;

/// <summary>
/// Interface for SVN output parsers
/// </summary>
public interface ISvnOutputParser<T>
{
    /// <summary>
    /// Parses the output from an SVN command
    /// </summary>
    /// <param name="output">The command output</param>
    /// <returns>The parsed result</returns>
    T Parse(string output);

    /// <summary>
    /// Parses the XML output from an SVN command
    /// </summary>
    /// <param name="xmlDocument">The XML document</param>
    /// <returns>The parsed result</returns>
    T ParseXml(System.Xml.XmlDocument xmlDocument);
}
