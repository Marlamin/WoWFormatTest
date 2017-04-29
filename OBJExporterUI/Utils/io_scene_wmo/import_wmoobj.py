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

def load(context,
         filepath
         ):

    with ProgressReport(context.window_manager) as progress:
        progress.enter_substeps(1, "Importing WMO OBJ %r..." % filepath)

        csvpath = filepath.replace('.obj', '_ModelPlacementInformation.csv')

        bpy.ops.import_scene.obj(filepath=filepath)

        # Select the imported WMO
        obj_objects = bpy.context.selected_objects[:]

        for obj in obj_objects:
            obj.rotation_euler = [0, 0, 0]
            obj.rotation_euler.x += radians(90)
            obj.rotation_euler.z -= radians(90)

        # Read doodad definitions file
        with open(csvpath) as csvfile:
            reader = csv.DictReader(csvfile, delimiter=';')
            for row in reader:
                doodad_path, doodad_filename = os.path.split(filepath)
                newpath = os.path.join(doodad_path, row['ModelFile'])

                # Import the doodad
                bpy.ops.import_scene.obj(filepath=newpath)

                # Select the imported doodad
                obj_objects = bpy.context.selected_objects[:]
                for obj in obj_objects:
                    # Print object name
                    # print (obj.name)

                    # Set position
                    obj.location = (float(row['PositionY']) * -1, float(row['PositionX']), float(row['PositionZ']))

                    # Set rotation
                    rotQuat = Quaternion((float(row['RotationW']), float(row['RotationX']), float(row['RotationY']), float(row['RotationZ'])))
                    rotEul = rotQuat.to_euler()
                    rotEul.x += radians(90);
                    rotEul.z += radians(90);
                    obj.rotation_euler = rotEul

                    # Set scale
                    if row['ScaleFactor']:
                        obj.scale = (float(row['ScaleFactor']), float(row['ScaleFactor']), float(row['ScaleFactor']))

        progress.leave_substeps("Finished importing: %r" % filepath)

    return {'FINISHED'}
