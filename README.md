# WoW Format Test
Messing around with parsing WoW file formats for educational purposes.
Built from the ground up, but with lots of copy pasta from older projects (noted in thanks paragraph).

## Supported expansions
Built for (latest) Warlords of Draenor files. Backwards compatibility with Mists of Pandaria not guaranteed. Cataclysm and lower probably won't work at all.

MPQ support is currently not planned. Very basic support for the new CASC file format introduced in Warlords of Draenor is present. It can only download content from Blizzard's servers, not load storages already present on HDD (yet).

## Configuration
If you have World of Warcraft (6.0 or higher) fully installed you can use your installed data instead of falling back to downloading stuff. To set this up, simply change the basedir setting in WoWOpenGL.exe.config to the directory that contains WoW.exe.

## Requirements
- [OpenTK](http://www.opentk.com/) (recent version already included)

## Projects
### Main projects
#### OBJExporterUI ([Official site](https://marlam.in/obj/))
Exports various WoW model formats to Wavefront .obj. 
#### WoWOpenGL ([Official site](https://marlam.in/mv/))
After some issues and annoyances with DirectX I decided to switch over to OpenTK/OpenGL.
#### WoWFormatLib 
Does parsing of WoW's raw data files and returns them in a object that other applications can use. Handles CASC (WoW's filesystem), file parsing, DBC reading.
#### MinimapCompiler 
Compiles minimaps. Not important, but still actively developed as I will need it for my other projects soon.

### Test projects
#### CASCtest 
App used to test CASC (file system) related things.
#### DBCtest 
App used to test DBC structures.
#### WoWFormatTest  
First app used to test some formats with.
#### WoWShaderTest
Eventual replacement project for WoWOpenGL.
#### HeightmapGenerator
Generates heightmaps. ¯\_(ツ)_/¯
#### WDLtextopentk
Making a color heightmap from a WDL file, code ported from Noggit to C#. Based on a simple OpenTK example.

### Discontinued/archived projects
#### WoWRenderLib 
Got data from WoWFormatLib and did some preprocessing before going to WoWFormatUI.
#### WoWFormatUI 
Initial test of rendering, gets data from WoWRenderLib and tries to display it in DirectX. No longer actively developed.
#### RenderTestWPF 
Rendering test with WPF.
	
## Thanks (in no particular order)
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

Last updated on December 14th 2015
