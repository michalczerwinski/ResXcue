About ResXue
============
ResXue stands for Rescue for ResX and its goal is to solve the most pesky issues with ResX format.


Why?
===
ResX was introduced together with .NET Framework 1.1 in 2003 and except for minor details hasn't really changed from this time. 

Although it allows for storing localized text in .NET assemblies there are many issues making ResX challenging for every day use:
* doesn't play well with distributed version controls like Git or Mercurial when merging
* it's far from being a human-readable medium mostly due to 107-line header (sic!) - this is especially painful during code review
* every new resource element is added always at the end of the file which leads to many conflicts
* and to top it all of the attribute xml:space="preserve" is repeated for all data and metadata nodes!


ResXue
======
By installing this nuget package you simply introduce a new msbuild target which will reformat every resx file in the projects so that:
* all unnecessary header lines will be removed
* all ResX entries are combined to one line
* the entries will be sort alphabetically
* xml:space="preserve" is applied only once for the whole file


