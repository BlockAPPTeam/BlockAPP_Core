using BlockAPP_Core.Enums;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;

namespace BlockAPP_Core.Helpers
{
    public static class Logger
    {
        public static void InitLogger()
        {
            var _Config = new LoggingConfiguration();

            var _ConsoleTarget = new ColoredConsoleTarget();
            _ConsoleTarget.Layout = "${date:format=HH\\:mm\\:ss} | ${pad:padding=-6:inner=${level:uppercase=true}}| ${message}";

            var _FileTarget = new FileTarget() { FileName = "Logger.log" };
            _FileTarget.Layout = "${date:format=HH\\:mm\\:ss} | ${pad:padding=-6:inner=${level:uppercase=true}}| ${message}";

            _Config.AddTarget("console", new AsyncTargetWrapper(_ConsoleTarget));
            _Config.AddTarget("file", new AsyncTargetWrapper(_FileTarget));

            var _ConsoleRule = new LoggingRule("*", LogLevel.Trace, _ConsoleTarget);
            var _FileRule = new LoggingRule("*", LogLevel.Trace, _ConsoleTarget);

            _Config.LoggingRules.Add(_ConsoleRule);
            _Config.LoggingRules.Add(_FileRule);

            LogManager.Configuration = _Config;
            LogManager.ReconfigExistingLoggers();
        }
    }
}
