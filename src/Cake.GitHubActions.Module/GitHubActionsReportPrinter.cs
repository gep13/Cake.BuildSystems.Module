using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Module.Shared;

using JetBrains.Annotations;

namespace Cake.GitHubActions.Module
{
    /// <summary>
    /// The GitHub Actions report printer.
    /// </summary>
    [UsedImplicitly]
    public class GitHubActionsReportPrinter : CakeReportPrinterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubActionsReportPrinter"/> class.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="context">The context.</param>
        public GitHubActionsReportPrinter(IConsole console, ICakeContext context)
            : base(console, context)
        {
        }

        /// <inheritdoc />
        public override void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            try
            {
                if (_context.GitHubActions().IsRunningOnGitHubActions)
                {
                    WriteToMarkdown(report);
                }

                WriteToConsole(report);
            }
            finally
            {
                _console.ResetColor();
            }
        }

        /// <inheritdoc />
        public override void WriteLifeCycleStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }

        /// <inheritdoc />
        public override void WriteSkippedStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }

        /// <inheritdoc />
        public override void WriteStep(string name, Verbosity verbosity)
        {
            // Intentionally left blank
        }

        private static TimeSpan GetTotalTime(IEnumerable<CakeReportEntry> entries)
        {
            return entries.Select(i => i.Duration)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        private void WriteToMarkdown(CakeReport report)
        {
            var includeSkippedReasonColumn = report.Any(r => !string.IsNullOrEmpty(r.SkippedMessage));

            var sb = new StringBuilder();
            sb.AppendLine(string.Empty);

            if (includeSkippedReasonColumn)
            {
                sb.AppendLine("|Task|Duration|Skip Reason|");
                sb.AppendLine("|----|--------|-----------|");
            }
            else
            {
                sb.AppendLine("|Task|Duration|");
                sb.AppendLine("|----|--------|");
            }

            foreach (var item in report)
            {
                if (ShouldWriteTask(item))
                {
                    if (includeSkippedReasonColumn)
                    {
                        sb.AppendLine(string.Format("|{0}|{1}|{2}|", item.TaskName, FormatDuration(item), item.SkippedMessage));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("|{0}|{1}|", item.TaskName, FormatDuration(item)));
                    }
                }
            }

            if (includeSkippedReasonColumn)
            {
                sb.AppendLine("||||");
                sb.AppendLine(string.Format("|**_{0}_**|**_{1}_**||", "Total:", GetTotalTime(report)));
            }
            else
            {
                sb.AppendLine("|||");
                sb.AppendLine(string.Format("|**_{0}_**|**_{1}_**|", "Total:", GetTotalTime(report)));
            }

            _context.GitHubActions().Commands.SetStepSummary(sb.ToString());
        }
    }
}
