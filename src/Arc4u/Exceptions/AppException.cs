//using System.Text;
//using FluentResults;

//namespace Arc4u;

///// <summary>
///// Represents an application exception.
///// <para>This class is not intented to be used client-side.</para>
///// </summary>
///// <remarks></remarks>
//public class AppException : Exception
//{

//    /// <summary>
//    /// Gets or sets the <see cref="Message"/> list of the current <see cref="AppException"/>.
//    /// </summary>
//    public Result Messages { get; set; }

//    private static string ToString(Result result)
//    {
//        //consider argument
//        if (result is null)
//        {
//            return string.Empty;
//        }

//        var builder = new StringBuilder();

//        foreach (var message in result.Reasons)
//        {
//            builder.AppendLine(message.Message);
//        }

//        //remove last Environment.NewLine
//        if (builder.Length != 0)
//        {
//            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
//        }

//        return builder.ToString();
//    }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="AppException"/> class.
//    /// </summary>
//    /// <param name="messages">The messages.</param>
//    public AppException(Result result)
//        : base(ToString(result))
//    {
//        Messages = result ?? new Result();
//    }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="AppException"/> class.
//    /// </summary>
//    /// <param name="messages">The messages.</param>
//    /// <param name="innerException">The inner exception.</param>
//    public AppException(Result result, Exception innerException)
//        : base(ToString(result), innerException)
//    {
//        Messages = result ?? new Result();
//    }

//    public AppException(string text)
//        : this(Result.Fail(text))
//    {
//    }

//    public AppException(string text, Exception innerException)
//        : this(Result.Fail(text), innerException)
//    {
//    }
//}

