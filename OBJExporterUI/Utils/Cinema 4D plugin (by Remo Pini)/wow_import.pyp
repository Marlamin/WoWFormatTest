import c4d
import os
import re
import glob
import time
import array
import csv
import string
import time
from c4d import gui, plugins, bitmaps, utils
from math import radians, atan2, asin, degrees, pi

PLUGIN_ID = 1039683
UI_LABEL = 10000
UI_FILENAME = 10001
UI_FILEDIALOG = 10002
UI_CHECKDEBUG = 10003
UI_CHECKLIMIT = 10004
UI_CHECKFULLADT = 10005
UI_GO = 10006
UI_PROGRESS = 10007
UI_PROGRESSSUB = 10008
UI_CHECKWMOM2 = 10009
UI_CHECKFIXMAT = 10010

WMO_SOURCE_ADT = 0
WMO_SOURCE_STANDALONE = 1
M2_SOURCE_ADT = 2
M2_SOURCE_WMO = 3
M2_SOURCE_STANDALONE = 4

#Some WOW Constants (coordinate sytem)
# Max Size: 51200 / 3 = 17066,66666666667
# Map Size: Max Size * 2 = 34133,33333333333
# ADT Size: Map Size / 64 = 533,3333333333333
    
WOW_MAX_SIZE =  51200.0 / 3
WOW_MAP_SIZE = 102400.0 / 3
WOW_ADT_SIZE =   1600.0 / 3

DEBUG = False
DIALOG = None
M2S = dict() # this holds a list of all models as they get added to avoid duplicating geometry (duplicates get added as instances)

def dedup(seq):
    seen = set()
    seen_add = seen.add
    return [x for x in seq if not (x in seen or seen_add(x))]

def file_len(fname):
    with open(fname) as f:
        for i, l in enumerate(f):
            pass
    return i + 1
    
def objfile_has_geometry(fname):
    with open(fname) as f:
        for l in iter(f):
            if(l.startswith("v")):
                return True
    return False
    
def png_has_transparency(fname):
    # Go through the PNG and try to find a transparent pixel
    # (found no better way so far to test for transparency)
    png = c4d.bitmaps.BaseBitmap()
    r, m = png.InitWith(fname)
    
    if r == c4d.IMAGERESULT_OK:
        a = png.GetInternalChannel()
        x,y = png.GetSize()
    
        if(x > 1000 and y > 1000): # if it's bigger than 1000x1000, we don't bother...
            return False
            
        for i in xrange(x):
            for j in xrange(y):
                if png.GetAlphaPixel(a, i, j) < 255:
                    return True
    else:
        if(DEBUG):
            print("Couldn't load PNG file {}!".format(fname))
        
    return False
    
def escape_pressed():
    bc = c4d.BaseContainer()
    rs = gui.GetInputState(c4d.BFM_INPUT_KEYBOARD, c4d.KEY_ESC, bc)
    if rs and bc[c4d.BFM_INPUT_VALUE]:
        if(gui.QuestionDialog("Stop Script and Abort Import?")):
            return True
        else:
            return False
    return False

def import_adt_geometry(parent, filepath):
    # Import ADT from file, put under parent and move to correct spot in 3D space
    doc = c4d.documents.GetActiveDocument()
    base_folder, adtname = os.path.split(filepath)
    adtsplit = adtname.split("_")
    mapname = adtsplit[0]
    map_x = int(adtsplit[1])
    map_y = int(adtsplit[2].replace(".obj", ""))
    offset_x = (+31.5 - map_x) * WOW_ADT_SIZE
    offset_z = (-31.5 + map_y) * WOW_ADT_SIZE

    if(DEBUG):
        print("  Loading ADT {}".format(adtname))
        print("    Source: {}".format(filepath))
        print("    Coords: {}, {}".format(map_x, map_y))
        print("    Offset: {}, {}".format(offset_x, offset_z))
        
    c4d.documents.MergeDocument(doc, filepath, c4d.SCENEFILTER_OBJECTS + c4d.SCENEFILTER_MATERIALS)
    obj = doc.GetFirstObject()
    
    settings = c4d.BaseContainer()
    settings[c4d.MDATA_OPTIMIZE_TOLERANCE] = 0.01
    settings[c4d.MDATA_OPTIMIZE_POINTS] = True
    settings[c4d.MDATA_OPTIMIZE_POLYGONS] = True
    settings[c4d.MDATA_OPTIMIZE_UNUSEDPOINTS] = True
    c4d.utils.SendModelingCommand(command=c4d.MCOMMAND_OPTIMIZE, list=[obj], mode=c4d.MODELINGCOMMANDMODE_ALL, bc=settings, doc=doc)
    
    #c4d.CallCommand(14039) # Optimize... as there are unwelded points!!
            
    # do a reset of position (cleaner that way)
    for i,pt in enumerate(obj.GetAllPoints()):
        obj.SetPoint(i, c4d.Vector(pt.x - offset_x, pt.y, pt.z - offset_z))
    obj.Message (c4d.MSG_UPDATE)
    obj.InsertUnder(parent)
    obj.SetAbsPos(c4d.Vector(offset_x, 0, offset_z))
    c4d.EventAdd()

def import_m2(filename, fullpath, parent, px, py, pz, rx, ry, rz, rw, scale, source):
    global M2S
    doc = c4d.documents.GetActiveDocument()
    
    if(not objfile_has_geometry(filename)):
        if(DEBUG):
            print("      skipping (obj has no geometry and would crash C4D...")
                            
    else:
                            
        if(not filename in M2S):
            c4d.documents.MergeDocument(doc, fullpath, c4d.SCENEFILTER_OBJECTS + c4d.SCENEFILTER_MATERIALS)
            obj = doc.GetFirstObject()
            obj.InsertUnder(parent)
            M2S[filename] = obj
            if(DEBUG):
                print "      created as new object"

        else:
            obj = c4d.BaseObject(c4d.Oinstance)
            obj[c4d.INSTANCEOBJECT_LINK] = M2S[filename]
            obj[c4d.INSTANCEOBJECT_RENDERINSTANCE] = True
            obj.SetName(filename.replace('.obj', ' (Instance)'))
            obj.InsertUnder(parent)
            if(DEBUG):
                print "      created as new instance"
            
        #Set PSR
        if(source == M2_SOURCE_WMO):
            # do weird shit if the parent is a WMO...
            
            #Set PSR
            x = -px
            y = pz
            z = -py
                
            #Quaternion to Euler *gnaa*
            qx = -rx
            qy = -ry
            qz = -rz
            qw = rw
            
            #roll (x-axis rotation)
            t0 = 2.0 * (qw * qx + qy * qz)
            t1 = 1.0 - 2.0 * (qx * qx + qy * qy)
            ax = atan2(t0, t1)

            #pitch (y-axis rotation)
            t2 = 2.0 * (qw * qy - qz * qx)
            t2 = max(-1, min(1, t2))
            ay = asin(t2)

            #yaw (z-axis rotation)
            t3 = 2.0 * (qw * qz + qx * qy)
            t4 = 1.0 - 2.0 * (qy * qy + qz * qz)
            az = atan2(t3, t4)

            if(DEBUG):
                print("  Rot: {} {} {}".format(degrees(ax), degrees(ay), degrees(az)))
                                                        
            obj.SetRelRot(c4d.Vector(-az, ay - pi, ax - pi))
            
        elif(source == M2_SOURCE_ADT):
            x = 17066 - px
            y = py
            z = pz - 17066 
            
            obj.SetAbsRot(c4d.Vector(radians(ry + 90), radians(-rz), radians(rx)))
            
        elif(source == M2_SOURCE_STANDALONE):
            obj.SetAbsRot(c4d.Vector(0, 0, 0))
            
        else:
            c4d.gui.MessageDialog("M2 from UNKNOWN SOURCE")
        
        obj.SetAbsScale(c4d.Vector(scale, scale, scale))
        obj.SetRelPos(c4d.Vector(x, y, z))
   
def import_wmo(wmoparent, filepath, px, py, pz, rx, ry, rz, scale, source):
    global M2S
    
    doc = c4d.documents.GetActiveDocument()
    c4d.documents.MergeDocument(doc, filepath, c4d.SCENEFILTER_OBJECTS + c4d.SCENEFILTER_MATERIALS)
    obj = doc.GetFirstObject()
    obj.InsertUnder(wmoparent)
    base_folder, modelfile = os.path.split(filepath)
    
    #Set PSR
    if(source == WMO_SOURCE_ADT):
        x = 17066 - px  
        y = py
        z = pz - 17066
    
    elif(source == WMO_SOURCE_STANDALONE):
        x = px
        y = py
        z = pz
        
    else:
         c4d.gui.MessageDialog("WMO Weirdness!!!")

    obj.SetAbsScale(c4d.Vector(scale, scale, scale))
    obj.SetAbsRot(c4d.Vector(radians(ry - 90), radians(-rz), radians(rx)))
    obj.SetRelPos(c4d.Vector(x, y, z))
                                
    wmocsvpath = filepath.replace('.obj', '_ModelPlacementInformation.csv')
                            
    #set up progress
    totalsubentries = file_len(wmocsvpath)
    currentsubentry = 0
    
    # Read WMO doodads definitions file
    
    with open(wmocsvpath) as wmocsvfile:
                          
        wmoreader = csv.DictReader(wmocsvfile, delimiter=';')
        for wmorow in wmoreader:
        
            #update progress
            currentsubentry += 1
            val = 1.0 * currentsubentry/totalsubentries
            update_progress(True, val)
            
            if(escape_pressed()):
                return
        
            wmonewpath = os.path.join(base_folder, wmorow['ModelFile'])
            # Import the doodad
            
            if(DEBUG):
                print("    Load M2 {} from {}".format(wmorow['ModelFile'], wmonewpath))
            
            if wmorow['ScaleFactor']:
                s = float(wmorow['ScaleFactor'])
            else:
                s = 1.0
            
            import_m2(wmorow['ModelFile'], wmonewpath, obj, float(wmorow['PositionX']), float(wmorow['PositionY']), float(wmorow['PositionZ']),
                                                            float(wmorow['RotationX']), float(wmorow['RotationY']), float(wmorow['RotationZ']), float(wmorow['RotationW']), s, M2_SOURCE_WMO)
    
def fix_materials(base_folder):
    doc = c4d.documents.GetActiveDocument()
    c4d.CallCommand(12211) # Remove Duplicate Materials
    c4d.CallCommand(12168) # Remove Unused Materials
    
    mat = doc.GetFirstMaterial()
    
    while(mat):
        if(DEBUG):
            print("  Fixing {}".format(mat.GetName()))
            
        mat.SetChannelState(c4d.CHANNEL_REFLECTION, False)
        mat.SetChannelState(c4d.CHANNEL_TRANSPARENCY, False)
    
        shd = mat[c4d.MATERIAL_COLOR_SHADER]
        if(shd).GetType() == c4d.Xbitmap:
            
            #fix color shader path
            if(DEBUG):
                print("    Fixing color texture path for {}".format(shd[c4d.BITMAPSHADER_FILENAME]))
            
            shd[c4d.BITMAPSHADER_FILENAME] = os.path.join(base_folder, shd[c4d.BITMAPSHADER_FILENAME])

            #fix alpha shader path and config
            if(mat.GetChannelState(c4d.CHANNEL_ALPHA)):
                alp = mat[c4d.MATERIAL_ALPHA_SHADER]
                
                try:
                    fil = alp[c4d.BITMAPSHADER_FILENAME]
                except:
                    fil = ""
                
                if(fil):
                    if(DEBUG):
                        print("    Fixing alpha texture path for {}".format(alp[c4d.BITMAPSHADER_FILENAME]))
                    alp[c4d.BITMAPSHADER_FILENAME] = os.path.join(base_folder, alp[c4d.BITMAPSHADER_FILENAME])
            
            else:
                #alpha channel not set, check for color texture transparency just in case... (happens with some models)
                if(png_has_transparency(shd[c4d.BITMAPSHADER_FILENAME])):
                    mat.SetChannelState(c4d.CHANNEL_ALPHA, True)
                    alp = c4d.BaseList2D(c4d.Xbitmap)
                    alp[c4d.BITMAPSHADER_FILENAME] = shd[c4d.BITMAPSHADER_FILENAME]
                    alp[c4d.MATERIAL_ALPHA_SOFT] = True
                    alp[c4d.MATERIAL_ALPHA_IMAGEALPHA] = True
                    mat[c4d.MATERIAL_ALPHA_SHADER] = alp
                    mat.InsertShader(alp)
                    if(DEBUG):
                        print("    Color texture has alpha, adding alpha texture path for {}".format(alp[c4d.BITMAPSHADER_FILENAME]))
                
            #raise appropriate events
            mat.Message(c4d.MSG_UPDATE)
            mat.Update(True, True)
            c4d.EventAdd()
            
        mat = mat.GetNext()   
    
def update_progress(sub, val):
    msg = c4d.BaseContainer(c4d.BFM_SETSTATUSBAR)
    msg[c4d.BFM_STATUSBAR_PROGRESSON] = True
    msg[c4d.BFM_STATUSBAR_PROGRESS] = val
    
    if(sub):
        DIALOG.SendMessage(UI_PROGRESSSUB, msg)
    else:
        DIALOG.SendMessage(UI_PROGRESS, msg)

def isADT(filepath):
    # is the file provided an ADT?
    base_folder, filename = os.path.split(filepath)
    
    if(re.search("\d{2}_\d{2}\.", filename)):
         return True
    return False

def isWMO(filepath):
    # is the file provided a WMO?
    # current assumption: If it is not an ADT, it's a WMO (which will break if it's an M2)
    return not isADT(filepath)
    
def WOWImport(filepath, limit, fulladt, dowmom2, fixmat):

    # General Setup Stuff
    global M2S
    
    doc = c4d.documents.GetActiveDocument()
    base_folder, modelfile = os.path.split(filepath)
    ADT = isADT(filepath)
    WMO = isWMO(filepath)
        
    
    # Get Started
    if(DEBUG):
        print("Importing file {}...".format(filepath))

    # Setting up the basic Scene:
    #    adtname    (a NULL grouping everything together for easy manipulation and management)
    #    +-- Doodads (M2s)
    #    +-- WMOs    (WMOs and their sub-models)
    #    +-- ADTs    (terrain geometries)
   
    root = c4d.BaseObject(c4d.Onull)
    root.SetName(modelfile.replace('.obj', ''))
    doc.InsertObject(root)        
        
    doodadparent = c4d.BaseObject(c4d.Onull)
    doodadparent.SetName("Doodads")
    doc.InsertObject(doodadparent)
    doodadparent.InsertUnder(root)
    
    wmoparent = c4d.BaseObject(c4d.Onull)
    wmoparent.SetName("WMOs")
    doc.InsertObject(wmoparent)
    wmoparent.InsertUnder(root)

    adtparent = c4d.BaseObject(c4d.Osds)
    adtparent.SetName("ADTs")
    adtparent[c4d.SDSOBJECT_SUBEDITOR_CM]=1
    adtparent[c4d.SDSOBJECT_SUBRAY_CM]=1
    doc.InsertObject(adtparent)
    adtparent.InsertUnder(root)
    
    adtgroup = c4d.BaseObject(c4d.Oconnector)
    adtgroup.SetName("ADT Tiles")
    doc.InsertObject(adtgroup)
    adtgroup.InsertUnder(adtparent)
        
    # get ADTs

    if(ADT):
    
        if(fulladt):
            if(DEBUG):
                print "  doing ALL ADTs in directory"
            
            adtpattern = re.sub(r'_\d\d', r'_??', modelfile)
            files = glob.glob(os.path.join(base_folder, adtpattern))
        else:
            files = [os.path.join(base_folder, modelfile)]
            
        for file in files:
            import_adt_geometry(adtgroup, file)
            
        # Now deal with all WMOs and Doodads (consolidated across all ADTs that are to be imported) if wanted
        
        if(dowmom2):
        
            csvfile = []
            
            wmocount = 0
            m2count = 0
            
            for fl in files:
                fl = fl.replace('.obj', '_ModelPlacementInformation.csv')
                with open(fl) as f:
                    for line in iter(f):
                        line = line.strip()
                        csvfile.append(line)
        
            csvfile = dedup(csvfile)
            
            totalentries = len(csvfile)

            if( totalentries > 1):
            
                #set up progress
                totalentries = len(csvfile)
                currententry = 0
                
                reader = csv.DictReader(csvfile, delimiter=';')
                for row in reader:
                
                    #update progress
                    currententry += 1
                    val = 1.0 * currententry/totalentries
                    update_progress(False, val)
                    update_progress(True, 0)
                    
                    if(escape_pressed()):
                        return
                            
                    newpath = os.path.join(base_folder, row['ModelFile'])

                    if row['Type'] == 'wmo':
                    
                        wmocount += 1
                        
                        if(wmocount > 3 and limit):
                            if(DEBUG):
                                print("  WMO limit reached, skipping further WMOs")

                        else:
                            if(DEBUG):
                                print("  Load WMO {} from {}".format(row['ModelFile'], newpath))

                            if row['ScaleFactor']:
                                s = float(row['ScaleFactor'])
                            else:
                                s = 1.0
                                
                            import_wmo(wmoparent, newpath, float(row['PositionX']), float(row['PositionY']), float(row['PositionZ']),
                                                           float(row['RotationX']), float(row['RotationY']), float(row['RotationZ']), s, WMO_SOURCE_ADT)
                        
                    elif row['Type'] == 'm2':
                        
                        m2count += 1
                                
                        if(m2count > 10 and limit):
                            if(DEBUG):
                                print("  M2 limit reached, skipping further M2s")
                            
                        else:
                            if(DEBUG):
                                print("  Load M2 {} from {}".format(row['ModelFile'], newpath))
                            
                            import_m2(row['ModelFile'], newpath, doodadparent, float(row['PositionX']), float(row['PositionY']), float(row['PositionZ']),
                                                                               float(row['RotationX']), float(row['RotationY']), float(row['RotationZ']), 0, float(row['ScaleFactor']), M2_SOURCE_ADT)

                    else:
                        if(DEBUG):
                            print("  Encountered unknown item type <<{}>>".format(row['Type']))

    if(WMO):
    
        if(DEBUG):
            print("  Load WMO {} from {}".format(modelfile, filepath))

        import_wmo(wmoparent, filepath, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, WMO_SOURCE_STANDALONE)
                            
    if(DEBUG):
        print("Finished importing: {}".format(filepath))
        print("Fixing Textures")
    
    if(fixmat):
        fix_materials(base_folder)
        
    if(DEBUG):
        print("Done...")
    
    return True

class MainDialog(c4d.gui.GeDialog):
    
    def CreateLayout(self):
        self.AddStaticText(id=UI_LABEL, flags=c4d.BFH_LEFT, initw=400, name="ADT or WMO File to Import")
        self.AddEditText(id=UI_FILENAME, flags=c4d.BFH_SCALEFIT, initw=400, inith=10)
        self.AddButton(id=UI_FILEDIALOG, flags=c4d.BFH_CENTER|c4d.BFH_SCALEFIT, name="Open File Browser")
        self.AddCheckbox(id=UI_CHECKWMOM2, flags=c4d.BFH_LEFT|c4d.BFH_SCALEFIT, initw=200, inith=10, name="Include Doodads (WMOs and M2s for ADTs, M2s for WMOs)")
        self.AddCheckbox(id=UI_CHECKLIMIT, flags=c4d.BFH_LEFT|c4d.BFH_SCALEFIT, initw=200, inith=10, name="Limit import to 3 WMO and 10 M2 (for quick tests, only applies to ADTs)")
        self.AddCheckbox(id=UI_CHECKFULLADT, flags=c4d.BFH_LEFT|c4d.BFH_SCALEFIT, initw=200, inith=10, name="Try to load ALL ADT files in directory")
        self.AddCheckbox(id=UI_CHECKFIXMAT, flags=c4d.BFH_LEFT|c4d.BFH_SCALEFIT, initw=200, inith=10, name="Try to fix materials")
        self.AddCheckbox(id=UI_CHECKDEBUG, flags=c4d.BFH_LEFT|c4d.BFH_SCALEFIT, initw=200, inith=10, name="Debug Mode")
        self.AddButton(id=UI_GO, flags=c4d.BFH_CENTER|c4d.BFH_SCALEFIT, name="Start")
        self.AddCustomGui(id=UI_PROGRESS, pluginid=c4d.CUSTOMGUI_PROGRESSBAR, name="Progress Bar", flags=c4d.BFH_SCALEFIT, minw=200, minh=10)
        self.AddCustomGui(id=UI_PROGRESSSUB, pluginid=c4d.CUSTOMGUI_PROGRESSBAR, name="Progress Sub Bar", flags=c4d.BFH_SCALEFIT, minw=200, minh=10)
        return True

    def InitValues(self):
        self.SetBool(UI_CHECKWMOM2, True)
        self.SetBool(UI_CHECKDEBUG, False)
        self.SetBool(UI_CHECKLIMIT, False)
        self.SetBool(UI_CHECKFULLADT, False)
        self.SetBool(UI_CHECKFIXMAT, True)
        self.Enable(UI_GO, False)
        return True
          
    def Command(self, id, msg):
        global DEBUG
        global DIALOG
        
        if(id == UI_CHECKWMOM2):
            self.Enable(UI_CHECKLIMIT, self.GetBool(UI_CHECKWMOM2)) # You can only limit model loading if models are loaded at all...
            
        if(id == UI_FILENAME):
            self.Enable(UI_GO, self.GetString(UI_FILENAME) <> "")
    
        if(id == UI_FILEDIALOG):
            self.SetString(UI_FILENAME, c4d.storage.LoadDialog(c4d.FILESELECTTYPE_SCENES, "Select ADT or WMO to import", c4d.FILESELECT_LOAD, "", "", "*_??_??.obj"))
            self.Enable(UI_GO, self.GetString(UI_FILENAME) <> "")
        
        if(id == UI_GO):
            if(self.GetString(UI_FILENAME)):
                self.Enable(UI_GO, False)
                DEBUG = self.GetBool(UI_CHECKDEBUG)
                DIALOG = self
                WOWImport(self.GetString(UI_FILENAME), self.GetBool(UI_CHECKLIMIT), self.GetBool(UI_CHECKFULLADT), self.GetBool(UI_CHECKWMOM2), self.GetBool(UI_CHECKFIXMAT))
                self.Close()

        return True

class CMDData(c4d.plugins.CommandData):
    
    __dialog = None
    
    def Execute(self, doc):
        if self.__dialog is None:            
            self.__dialog = MainDialog()
        return self.__dialog.Open(c4d.DLG_TYPE_ASYNC, PLUGIN_ID)
    
    def RestoreLayout(self, sec_ref):
        if self.__dialog is None:            
            self.__dialog = MainDialog()
        return self.__dialog.Restore(PLUGIN_ID, sec_ref)
        
if __name__ == "__main__":
    icon = c4d.bitmaps.BaseBitmap()
    icon.InitWith(os.path.join(os.path.dirname(__file__), "res", "icon.tif"))
    c4d.plugins.RegisterCommandPlugin(PLUGIN_ID, "WOW Import", 0, icon, "WOW Import", CMDData())        