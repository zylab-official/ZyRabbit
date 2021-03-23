using System;
using System.Text.RegularExpressions;
using ZyRabbit.Instantiation;

namespace ZyRabbit.Enrichers.QueueSuffix
{
	public static class ApplicationQueueSuffixPlugin
	{
		private const string IisWorkerProcessName = "w3wp";
		private static readonly Regex DllRegex = new Regex(@"(?<ApplicationName>[^\\]*).dll", RegexOptions.Compiled);
		private static readonly Regex ConsoleOrServiceRegex = new Regex(@"(?<ApplicationName>[^\\]*).exe", RegexOptions.Compiled);
		private static readonly Regex IisHostedAppRegexVer1 = new Regex(@"-ap\s\\""(?<ApplicationName>[^\\]+)", RegexOptions.Compiled);
		private static readonly Regex IisHostedAppRegexVer2 = new Regex(@"\\\\apppools\\\\(?<ApplicationName>[^\\]+)", RegexOptions.Compiled);

		public static IClientBuilder UseApplicationQueueSuffix(this IClientBuilder builder)
		{
			var commandLine =  Environment.GetCommandLineArgs();
			
			var executableFileName = commandLine[0];
			var match = ConsoleOrServiceRegex.Match(executableFileName);
			var applicationName = string.Empty;
			
			if (match.Success && match.Groups["ApplicationName"].Value != IisWorkerProcessName)
			{
				applicationName = match.Groups["ApplicationName"].Value;
				if (applicationName.EndsWith(".vshost"))
					applicationName = applicationName.Remove(applicationName.Length - ".vshost".Length);
			}
			else
			{
				match = IisHostedAppRegexVer1.Match(executableFileName);
				if (match.Success)
				{
					applicationName = match.Groups["ApplicationName"].Value;
				}
				else
				{
					match = IisHostedAppRegexVer2.Match(executableFileName);
					if (match.Success)
					{
						applicationName = match.Groups["ApplicationName"].Value;
					}
					else
					{
						match = DllRegex.Match(executableFileName);
						if (match.Success)
						{
							applicationName = match.Groups["ApplicationName"].Value;
						}
					}
				}
			}

			var name =  applicationName.Replace(".", "_").ToLower();
			builder.UseQueueSuffix(new QueueSuffixOptions
			{
				CustomSuffixFunc = context => name,
				ActiveFunc = context => context.GetApplicationSuffixFlag()
			});
			return builder;
		}
	}
}
