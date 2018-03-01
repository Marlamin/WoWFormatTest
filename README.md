# WoW Format Test
Messing around with parsing WoW file formats for educational purposes.
Built from the ground up, but with lots of copy pasta from older projects (noted in thanks paragraph).

## Supported expansions
Built for (latest) World of Warcraft files. No backwards compatibility with anything older than current retail version.

## Configuration
If you have World of Warcraft (7.3.5 or higher) fully installed you can use your installed data instead of falling back to downloading stuff. To set this up, simply change the basedir setting in (executable).config to the directory that contains WoW.exe.

## Requirements
- [OpenTK](http://www.opentk.com/) (via NuGet)

## Projects
### Main projects
#### OBJExporterUI ([Official site](https://marlam.in/obj/))
Exports various WoW model formats to Wavefront .obj. Primary application.
#### WoWFormatLib
Does parsing of WoW's raw data files and returns them in a object that other applications can use. Handles CASC (WoW's filesystem), file parsing.
#### MinimapCompiler
Compiles minimaps. Not important, but still actively developed as I will need it for my other projects soon.
### Test projects
#### WoWFormatTest
App used to test some formats with.

## Thanks (in no particular order)
- Belvane
- TOM_RUS
- Schlumpf
- Warpten
- Thoorium
- Deamon87
- Miceiken
- Kirth
- relaxok
- Blizzard Entertainment
- Gredys
- flippy84
- justMaku
- Xalcon
- BoogieMan
- Everyone in #modcraft on QuakeNet
- WoWDev wiki authors (especially schlumpf!!!!)
- ..and all the people the above people base their work on

Last updated on March 1st 2018
