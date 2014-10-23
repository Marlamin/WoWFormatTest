# WoW Format Test
Messing around with parsing WoW file formats for educational purposes.
Built from the ground up, but with lots of copy pasta from older projects (noted in thanks paragraph).

## Supported expansions
Built for (latest) Warlords of Draenor files. Backwards compatibility with Mists of Pandaria not guaranteed. Cataclysm and lower probably won't work at all.

MPQ support is currently not planned. Very basic support for the new CASC file format introduced in Warlords of Draenor is present. It can only download content from Blizzard's servers, not load storages already present on HDD (yet).

## Configuration
Should be no configuration necessary at the time of writing. 

## Requirements
- [OpenTK](http://www.opentk.com/) (recent version already included)

## Projects
### Main projects
#### WoWFormatLib 
Does parsing of WoW's raw data files and returns them in a object that other applications can use. Handles CASC (WoW's filesystem), file parsing, DBC reading.
#### WoWOpenGL 
After some issues and annoyances with DirectX I decided to switch over to OpenTK/OpenGL.
#### WMOMapCompiler 
Not important, but still developed as I will need it for my other projects soon.
	
### Test projects
#### CASCtest 
App used to test CASC (file system) related things.
#### DBCtest 
App used to test DBC structures.
#### WoWFormatTest  
First app used to test some formats with.

### Discontinued/archived projects
#### WoWRenderLib 
Got data from WoWFormatLib and did some preprocessing before going to WoWFormatUI.
#### WoWFormatUI 
Initial test of rendering, gets data from WoWRenderLib and tries to display it in DirectX. No longer actively developed.
#### RenderTestWPF 
Rendering test with WPF.
	
## Thanks
- TOM_RUS
- flippy84
- JustMak
- Xalcon
- BoogieMan 
- Everyone in #modcraft on QuakeNet
- WoWDev wiki authors (especially schlumpf!!!!)
- Miceiken
- ..and all the people the above people base their work on
- also Thoorium

Last updated on October 23rd 2014
