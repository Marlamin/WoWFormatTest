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

# <pep8-80 compliant>

bl_info = {
    "name": "Import WMO OBJ files with doodads",
    "author": "Marlamin",
    "version": (0, 1, 0),
    "blender": (2, 77, 0),
    "location": "File > Import-Export > WMO OBJ (.obj)",
    "description": "Import WMO OBJ files exported by Marlamin's OBJ Exporter with doodads",
    "warning": "",
    "wiki_url": "",
    "tracker_url": "",
    "category": "Import-Export"}



if "bpy" in locals():
    import importlib
    if "import_wmoobj" in locals():
        importlib.reload(import_wmoobj)

import bpy

from bpy.props import (
        BoolProperty,
        FloatProperty,
        StringProperty,
        EnumProperty,
        )
from bpy_extras.io_utils import (
        ImportHelper,
        ExportHelper,
        orientation_helper_factory,
        path_reference_mode,
        axis_conversion,
        )

IOOBJOrientationHelper = orientation_helper_factory("IOOBJOrientationHelper", axis_forward='-Z', axis_up='Y')

class ImportWMOOBJ(bpy.types.Operator, ImportHelper, IOOBJOrientationHelper):
    """Load a Wavefront OBJ File with additional WMO metadata"""
    bl_idname = "import_scene.wmoobj"
    bl_label = "Import WMOOBJ"
    bl_options = {'PRESET', 'UNDO'}

    filename_ext = ".obj"
    filter_glob = StringProperty(
            default="*.obj",
            options={'HIDDEN'},
            )


    def execute(self, context):

        #file = open('example.csv')
        #for line in file:
        #    print(line)
        from . import import_wmoobj
        return import_wmoobj.load(context, self.filepath)

    def draw(self, context):
        layout = self.layout

        row = layout.row(align=True)
        box = layout.box()


def menu_func_import(self, context):
    self.layout.operator(ImportWMOOBJ.bl_idname, text="WMO OBJ (.obj)")


def register():
    bpy.utils.register_module(__name__)

    bpy.types.INFO_MT_file_import.append(menu_func_import)


def unregister():
    bpy.utils.unregister_module(__name__)

    bpy.types.INFO_MT_file_import.remove(menu_func_import)

if __name__ == "__main__":
    register()
