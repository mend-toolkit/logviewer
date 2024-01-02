# LogViewer
LogViewerConsole is an application thats designed to look at a file and highlight any issues that might be going on.  The configuration file is designed to be flexible so that any tweeks that need to be made can be done without having to rebuild the application.

The active configuration file exists in Github (this repository).  The LogViewer will automatically download the configuration file from Github if one is not provided as a parameter.  This enables the ability to update the configuration file globally and be used in several environment's without the need to update the LogViewer.

The configuration file is designed to identify which log file it's looking at.  Sometimes a partial file is provided and you need to tell it which file type it is.  The parameter for this is "useconfig". (See below example).

# Installation

The following commands will download the latest version onto your operating system.  For Macintosh and Linux, you will need to make the file executable.
```shell
curl -LJO https://github.com/mend-toolkit/logviewer/releases/latest/download/lvc_osx && chmod +x lvc_osx
curl -LJO https://github.com/mend-toolkit/logviewer/releases/latest/download/lvc_linux && chmod +x lvc_linux
curl -LJO https://github.com/mend-toolkit/logviewer/releases/latest/download/lvc_x64.exe
curl -LJO https://github.com/mend-toolkit/logviewer/releases/latest/download/lvc_x86.exe
```

You should consider renaming the file to lvc to make it easier.

# Usage Example
Basic Example:
```shell
./lvc_linux file="FileToReview.log"
```

Write the output to a file:
```shell
./lvc_linux file="FileToReview.log" output="results.log"
```
 
Specify the log file and which log file type it is:
- Only required if the autodetection did not resolve the correct file type
```shell
./lvc_linux file="FileToReview.log" useconfig="UnifiedAgent"
```



Complex Example:
- This assumes you downloaded the configuration file locally and named it "LogViewer.config"
```shell
./lvc_linux file="Whitesource.0.log" outputLog="findings-Whitesource.0.log" config="LogViewer.config" useconfig="UnifiedAgent"
```
# Parameters
- If the config parameter is empty, it will use the [configuration file](https://github.com/mend-toolkit/logviewer/blob/main/LogViewer.config) from this repository.
- useconfig potential values will be shown if autodetection fails
```shell
file="<File to review>"
outputLog="<results output filename>"
useconfig="<configuration section name>"  
config="<local configuration file path and filename>"
```
