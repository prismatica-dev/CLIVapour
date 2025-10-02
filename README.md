# CLIVapour
An improved cross-platform Command Line (CLI) port of [OpenVapour](https://github.com/prismatica-dev/OpenVapour), an advanced game torrent search engine.

Command line interface is based on [Yet another Yogurt (yay)](https://github.com/Jguer/yay)

## Usage
CLIVapour is intended to be used similar to terminal-based package managers. However it can be run without arguments for inexperienced users.

For a full list of arguments supported by CLIVapour, use `CLIVapour --help`.

### Examples
- `CLIVapour` will prompt the user for a search query and then search for it
- `CLIVapour Search Query` searches for the provided query automatically
- `CLIVapour Search Query --verbose` searches with verbose logging
- `CLIVapour Search Query -a -e` searches with the `-a` and `-e` arguments 

Single letter arguments can also be combined like so:
- `CLIVapour Search Query -a -v -e -U --timeout=60` is equivalent to `CLIVapour Search Query -aveU --timeout=60`

## Improvements 
CLIVapour is inherently faster to use than OpenVapour due to being a CLI port.<br>However it also provides several other improvements:
- Search filtering
- Newest -> oldest sorting
- Configurable timeouts
- Magnet multiple torrents at once
- Updated from .NET Framework 4.8.1 to .NET 9.0

## Legal Disclaimer
As CLIVapour utilises torrent hosting sites, some content accessible by the user may violate regional copyright laws. It is trusted that the user will not abuse this, and will only use this feature for content without [DRM](https://en.wikipedia.org/wiki/Digital_rights_management) or non-copyrighted material. Further, the tool also serves educational purpose, as it highlights several flaws in the URL "shorteners" used by torrent hosting sites. 
Please ensure you are aware of the implications of the provided [GNU General Public License v3.0](https://github.com/prismatica-dev/CLIVapour/blob/master/LICENSE.txt).
