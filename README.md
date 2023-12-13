# LogViewer
LogViewerConsole is an application thats designed to look at a file and highlight any issues that might be going on.  The configuration file is designed to be flexible so that any tweeks that need to be made can be done without having to rebuild the application.

The active configuration file exists in Github.  The LogViewer will automatically download the configuration file from Github if one is not provided as a parameter.  This enables the ability to update the configuration file globally and be used in several environment's without the need to update the LogViewer.

The configuration file is designed to identify which log file it's looking at.  Sometimes a partial file is provided and you need to tell it which file type it is.  The parameter for this is "useconfig". (See below example).

# Installation
For now, please either compile the binary or download it for your given operating system.  In the near future, the binaries will be included in the release area.  Sourcecode is located in the "dev" branch.
<br><br>If you need to compile this, it was written in .NET Core 6.

# Usage Example
Basic Example:<br>
LogViewerConsole file="FileToReview.log"<br>

Specify log file and which log file type it is:<br>
LogViewerConsole file="FileToReview.log" useconfig="UnifiedAgent"<br>

Write the output to a file:<br>
LogViewerConsole file="FileToReview.log" output="results.log"<br>

Complete Example:<br>
LogViewerConsole file="Whitesource.0.log" outputLog="findings-Whitesource.0.log" config="LogViewer.config" useconfig="UnifiedAgent" <br>(This assumes you downloaded the configuration file locally and named it "LogViewer.config" <br>
# Parameters
file="\<File to review\>"<br>
outputLog="\<results output filename\>"<br>
useconfig="\<configuration section name\>"  (IE: "Unified Agent")<br>
*config="\<local configuration file path and filename\>"<br><br>
*If the config parameter is empty, it will use the configuration file from this repository.
