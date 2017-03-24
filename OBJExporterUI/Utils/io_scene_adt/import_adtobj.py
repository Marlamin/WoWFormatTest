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

        # Select the imported doodad
        obj = bpy.context.object

        # Make object active
        # bpy.context.scene.objects.active = obj

        # Read doodad definitions file
        with open(csvpath) as csvfile:
            reader = csv.DictReader(csvfile, delimiter=';')
            for row in reader:
                doodad_path, doodad_filename = os.path.split(filepath)
                newpath = os.path.join(doodad_path, row['ModelFile'])

                # Import the doodad
                bpy.ops.import_scene.obj(filepath=newpath)

                # Select the imported doodad
                obj = bpy.context.object

                # Make object active
                # bpy.context.scene.objects.active = obj

                # Set position
                # WARNING: WoW world coordinates, are Y and Z swapped?
                obj.location = (float(row['PositionX']) - offset_x, float(row['PositionY']), float(row['PositionZ']) - offset_y)

                print(float(row['PositionX']) - offset_x)
                print(obj.location[0])

                # Set scale
                if row['ScaleFactor']:
                    obj.scale = (float(row['ScaleFactor']), float(row['ScaleFactor']), float(row['ScaleFactor']))

                print(obj.scale[0])

                #break
                #print(newpath)
                #print(row['ModelFile'], row['PositionX'])
        #file = open(newpath, "r")
#
        #for line in file:
        #    print(line)
        #    splitted_line = line.split(";")
#
        #    modelname = splitted_line[0]
        #    type(modelname)
        #    position_x = float(splitted_line[1])
        #    type(position_x)
        #    position_y = float(splitted_line[2])
        #    position_z = float(splitted_line[3])
        #    rotation_x = float(splitted_line[4])
        #    rotation_y = float(splitted_line[5])
        #    rotation_z = float(splitted_line[6])
        #    scale = float(splitted_line[7])
        #    modelid = int(splitted_line[8])
        #    type(modelid)
#
        #    print ("Model name: %r, POS: %f %f %f, ROT: %f %f %f, Scale: %f, ModelID: %d" % (modelname, position_x, position_y, position_z, rotation_x, rotation_y, rotation_z, scale, modelid))
        #file.close()

        progress.leave_substeps("Finished importing: %r" % filepath)

    return {'FINISHED'}
