namespace GbbConnect2Protocol;

public class Header
{

    public Line[]? Lines { get; set; }

    /// <summary>
    /// Error: null -> no error
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Any string up to 256 characters returned in answer
    /// </summary>
    public string? OrderId { get; set; }

    // =======================================
    // GbbOptimizer -> Device
    // =======================================
    public const string LogLevel_ONLY_ERRORS = "OnlyErrors";
    public const string LogLevel_MIN = "Min";
    public const string LogLevel_MAX = "Max";
    public string? LogLevel { get; set; }

    public int? SendLastLog { get; set; }


    // =======================================
    // Device -> GbbOptimizer
    // =======================================
    public string? GbbVersion { get; set; }
    public string? GbbEnvironment { get; set; }

    public string? LastLog { get; set; }

}

public class Line
{
    /// <summary>
    /// 1,2,3,4...
    /// </summary>
    public int LineNo { get; set; }
    /// <summary>
    /// Any string up to 256 characters returned in anwer
    /// </summary>
    public string? Tag { get; set; }
    /// <summary>
    /// unix timestamp in seconds UTC
    /// </summary>
    public long? Timestamp { get; set; } 
    /// <summary>
    /// modbus command or response
    /// </summary>
    public string? Modbus { get; set; }
    /// <summary>
    /// on responce: error or null
    /// </summary>
    public string? Error { get; set; }


}

