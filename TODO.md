== TODO ==
[Fixed, needs more testing] * Alpha issues 
* Missing textures on huts (not missing, texture is transparent, uses shaders)
* Rotation issues (some small rotation issues remain)
* Water
* Holes, yo!
* Multiple texture layers (shader magic or rendering everything 4 times)
* Optimization
[Working temporarily] * Local file loading (hackfixed while TOM fixes his library to use different file stuff)
* WMO doodads
* Refactor WMO, M2 and BLP loading to be the same code in both TerrainWindow.cs and Render.cs
* Ortho camera 


pseudo code by Mirth
 (layer l : layers ) { float a = texture(l.alpha); vec4 col = texture(l.color); finalColor += col * a; }
