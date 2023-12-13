# LogViewer
LogViewerConsole is an application thats designed to look at a file and highlight any issues that might be going on.  The configuration file is designed to be flexible so that any tweeks that need to be made can be done without having to rebuild the application.

The active configuration file exists in Github (this repository).  The LogViewer will automatically download the configuration file from Github if one is not provided as a parameter.  This enables the ability to update the configuration file globally and be used in several environment's without the need to update the LogViewer.

The configuration file is designed to identify which log file it's looking at.  Sometimes a partial file is provided and you need to tell it which file type it is.  The parameter for this is "useconfig". (See below example).

# Installation
No actual installation is needed, just copy the binary from the release.  For Macintosh and Linux, you will need to make the file executable.  This can be done by doing the following:
* chmod +x lvc_linux
* chmod +x lvc_osx

You should consider renaming the file to lvc to make it easier.

# Usage Example
Basic Example:<br>
lvc_linux file="FileToReview.log"<br>
 
Specify log file and which log file type it is:<br>
lvc_linux file="FileToReview.log" useconfig="UnifiedAgent"<br>

Write the output to a file:<br>
lvc_linux file="FileToReview.log" output="results.log"<br>

Complete Example:<br>
lvc_linux file="Whitesource.0.log" outputLog="findings-Whitesource.0.log" config="LogViewer.config" useconfig="UnifiedAgent" <br>(This assumes you downloaded the configuration file locally and named it "LogViewer.config" <br>
# Parameters
file="\<File to review\>"<br>
outputLog="\<results output filename\>"<br>
useconfig="\<configuration section name\>"  (IE: "Unified Agent")<br>
*config="\<local configuration file path and filename\>"<br><br>
*If the config parameter is empty, it will use the configuration file from this repository.
