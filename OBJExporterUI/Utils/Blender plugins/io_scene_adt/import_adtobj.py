# ##### BEGIN GPL LICENSE BLOCK #####
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public License
#  as published by the Free Software Foundation; either version 2
#  of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program; if not, write to the Free Software Foundation,
#  Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
#
# ##### END GPL LICENSE BLOCK #####

# <pep8 compliant>

import array
import csv
import os
import time
import bpy
import mathutils
from math import radians
from mathutils import Quaternion
import string
from bpy_extras.io_utils import unpack_list
from bpy_extras.image_utils import load_image

from progress_report import ProgressReport, ProgressReportSubstep

# WoW Coordinate system


def load(context,
         filepath
         ):

    with ProgressReport(context.window_manager) as progress:
        progress.enter_substeps(1, "Importing ADT OBJ %r..." % filepath)

        csvpath = filepath.replace('.obj', '_ModelPlacementInformation.csv')

        # Coordinate setup

        ## WoW coordinate sytem
        # Max Size: 51200 / 3 = 17066,66666666667
        # Map Size: Max Size * 2 = 34133,33333333333
        # ADT Size: Map Size / 64 = 533,3333333333333

        max_size = 51200 / 3
        map_size = max_size * 2
        adt_size = map_size / 64

        base_folder, adtname = os.path.split(filepath)
        adtsplit = adtname.split("_")
        mapname = adtsplit[0]
        map_x = int(adtsplit[1])
        map_y = int(adtsplit[2].replace(".obj", ""))

        print(mapname)
        print(map_x)
        print(map_y)

        offset_x = adt_size * map_x
        offset_y = adt_size * map_y

        print(offset_x)
        print(offset_y)

        # Import ADT
        bpy.ops.import_scene.obj(filepath=filepath)

        bpy.ops.object.add(type='EMPTY')
        doodadparent = bpy.context.active_object
        doodadparent.parent = bpy.data.objects[mapname + '_' + str(map_x) + '_' + str(map_y)]
        doodadparent.name = "Doodads"
        doodadparent.rotation_euler = [0, 0, 0]
        doodadparent.rotation_euler.x = radians(-90)

        bpy.ops.object.add(type='EMPTY')
        wmoparent = bpy.context.active_object
        wmoparent.parent = bpy.data.objects[mapname + '_' + str(map_x) + '_' + str(map_y)]
        wmoparent.name = "WMOs"
        wmoparent.rotation_euler = [0, 0, 0]
        wmoparent.rotation_euler.x = radians(-90)
        # Make object active
        # bpy.context.scene.objects.active = obj

        # Read doodad definitions file
        with open(csvpath) as csvfile:
            reader = csv.DictReader(csvfile, delimiter=';')
            for row in reader:
                doodad_path, doodad_filename = os.path.split(filepath)
                newpath = os.path.join(doodad_path, row['ModelFile'])

                if row['Type'] == 'wmo':
                    bpy.ops.object.add(type='EMPTY')
                    parent = bpy.context.active_object
                    parent.name = row['ModelFile']
                    parent.parent = wmoparent
                    parent.location = (17066 - float(row['PositionX']), (17066 - float(row['PositionZ'])) * -1, float(row['PositionY']))
                    parent.rotation_euler = [0, 0, 0]
                    #obj.rotation_euler.x += (radians(90 + float(row['RotationX']))) # TODO
                    #obj.rotation_euler.y -= radians(float(row['RotationY']))        # TODO
                    parent.rotation_euler.z = radians((-90 + float(row['RotationY'])))
                    if row['ScaleFactor']:
                        parent.scale = (float(row['ScaleFactor']), float(row['ScaleFactor']), float(row['ScaleFactor']))

                    bpy.ops.import_scene.obj(filepath=newpath)
                    obj_objects = bpy.context.selected_objects[:]

                    # Put ADT rotations in here
                    for obj in obj_objects:
                        obj.parent = parent

                    wmocsvpath = newpath.replace('.obj', '_ModelPlacementInformation.csv')
                    # Read WMO doodads definitions file
                    with open(wmocsvpath) as wmocsvfile:
                        wmoreader = csv.DictReader(wmocsvfile, delimiter=';')
                        for wmorow in wmoreader:
                            wmodoodad_path, wmodoodad_filename = os.path.split(filepath)
                            wmonewpath = os.path.join(wmodoodad_path, wmorow['ModelFile'])
                            # Import the doodad
                            if(os.path.exists(wmonewpath)):
                                bpy.ops.import_scene.obj(filepath=wmonewpath)
                                # Select the imported doodad
                                wmoobj_objects = bpy.context.selected_objects[:]
                                for wmoobj in wmoobj_objects:
                                    # Prepend name
                                    wmoobj.name = "(" + wmorow['DoodadSet'] + ") " + wmoobj.name
                                    # Set parent
                                    wmoobj.parent = parent
                                    # Set position
                                    wmoobj.location = (float(wmorow['PositionX']) * -1, float(wmorow['PositionY']) * -1, float(wmorow['PositionZ']))
                                    # Set rotation
                                    rotQuat = Quaternion((float(wmorow['RotationW']), float(wmorow['RotationX']), float(wmorow['RotationY']), float(wmorow['RotationZ'])))
                                    rotEul = rotQuat.to_euler()
                                    rotEul.x += radians(90);
                                    rotEul.z += radians(180);
                                    wmoobj.rotation_euler = rotEul
                                    # Set scale
                                    if wmorow['ScaleFactor']:
                                        wmoobj.scale = (float(wmorow['ScaleFactor']), float(wmorow['ScaleFactor']), float(wmorow['ScaleFactor']))

                                    # Duplicate material removal script by Kruithne
                                    # Merge all duplicate materials
                                    for obj in bpy.context.scene.objects:
                                        if obj.type == 'MESH':
                                            i = 0
                                            for mat_slot in obj.material_slots:
                                                mat = mat_slot.material
                                                obj.material_slots[i].material = bpy.data.materials[mat.name.split('.')[0]]
                                                i += 1

                                    # Cleanup unused materials
                                    for img in bpy.data.images:
                                        if not img.users:
                                            bpy.data.images.remove(img)
                else:
                    if(os.path.exists(newpath)):
                        bpy.ops.import_scene.obj(filepath=newpath)
                        obj_objects = bpy.context.selected_objects[:]
                        for obj in obj_objects:
                            # Set parent
                            obj.parent = doodadparent

                            # Set location
                            obj.location.x = (17066 - float(row['PositionX']))
                            obj.location.y = (17066 - float(row['PositionZ'])) * -1
                            obj.location.z = float(row['PositionY'])
                            obj.rotation_euler.x += radians(float(row['RotationZ']))
                            obj.rotation_euler.y += radians(float(row['RotationX']))
                            obj.rotation_euler.z = radians(90 + float(row['RotationY'])) # okay

                            # Set scale
                            if row['ScaleFactor']:
                                obj.scale = (float(row['ScaleFactor']), float(row['ScaleFactor']), float(row['ScaleFactor']))


        # Set doodad and WMO parent to 0
        wmoparent.location = (0, 0, 0)
        doodadparent.location = (0, 0, 0)

        print("Deduplicating and cleaning up materials!")
        # Duplicate material removal script by Kruithne
        # Merge all duplicate materials
        for obj in bpy.context.scene.objects:
            if obj.type == 'MESH':
                i = 0
                for mat_slot in obj.material_slots:
                    mat = mat_slot.material
                    obj.material_slots[i].material = bpy.data.materials[mat.name.split('.')[0]]
                    i += 1

        # Cleanup unused materials
        for img in bpy.data.images:
            if not img.users:
                bpy.data.images.remove(img)
        progress.leave_substeps("Finished importing: %r" % filepath)

    return {'FINISHED'}
